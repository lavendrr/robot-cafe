using UnityEngine;

[ExecuteInEditMode]
public class Liquid : MonoBehaviour
{
    public enum UpdateMode { Normal, UnscaledTime }
    public UpdateMode updateMode;

    [Tooltip("Maximum tilt of the liquid surface, in the shader's wobble units.")]
    [SerializeField]
    float MaxWobble = 0.3f;
    [Tooltip("Oscillation frequency of the slosh, in Hz. Higher = faster back-and-forth.")]
    [SerializeField]
    float WobbleSpeedMove = 1.2f;
    [Tooltip("Damping of the slosh. Higher settles faster; low values ring for longer. Keep below ~2*2*PI*WobbleSpeedMove to stay underdamped (visible overshoot).")]
    [SerializeField]
    float Recovery = 2.5f;
    [Tooltip("How much container acceleration tilts the liquid. Higher = more responsive splash.")]
    [SerializeField]
    float WobbleStrength = 0.5f;
    [Tooltip("Liquid inertia. Higher = thicker, sluggish liquid that smooths out sharp acceleration spikes.")]
    [SerializeField]
    float Thickness = 0.5f;
    [Tooltip("Adds the shader's rippling sine wave to the surface. Turn off to see a clean flat tilt while tuning direction.")]
    [SerializeField]
    bool SurfaceWave = true;
    [Tooltip("Height of the surface ripple when SurfaceWave is on (the shader's _Amplitude).")]
    [SerializeField]
    float SurfaceWaveAmplitude = 0.2f;
    [SerializeField]
    float fillAmount = 0.56f;
    [Range(0, 1)]
    public float CompensateShapeAmount;
    [SerializeField]
    Mesh mesh;
    [SerializeField]
    Renderer rend;

    static readonly int WobbleXID = Shader.PropertyToID("_WobbleX");
    static readonly int WobbleZID = Shader.PropertyToID("_WobbleZ");
    static readonly int FillAmountID = Shader.PropertyToID("_FillAmount");
    static readonly int AmplitudeID = Shader.PropertyToID("_Amplitude");

    MaterialPropertyBlock mpb;

    Vector3 pos;
    Vector3 lastPos;
    Vector3 worldVelocity;
    Vector3 smoothedAccel;
    Quaternion lastRot;
    Vector3 angularVelocity;

    // Damped-spring state, one scalar oscillator per horizontal axis.
    float wobbleAmountX;   // = _WobbleX sent to the shader
    float wobbleAmountZ;   // = _WobbleZ sent to the shader
    float springVelX;
    float springVelZ;

    Vector3 comp;

    void Start()
    {
        GetMeshAndRend();
        // Seed last-frame state so the first frame doesn't produce a huge phantom acceleration.
        lastPos = transform.position;
        lastRot = transform.rotation;
        worldVelocity = Vector3.zero;
        smoothedAccel = Vector3.zero;
    }

    private void OnValidate()
    {
        GetMeshAndRend();
    }

#if UNITY_EDITOR
    double lastEditorTime;

    void OnEnable()
    {
        // Drive the sim off a continuous editor clock so it animates while editing,
        // instead of relying on Update's sporadic edit-mode ticks.
        if (!Application.isPlaying)
        {
            GetMeshAndRend();
            lastPos = transform.position;
            lastRot = transform.rotation;
            worldVelocity = Vector3.zero;
            smoothedAccel = Vector3.zero;
            lastEditorTime = UnityEditor.EditorApplication.timeSinceStartup;
            UnityEditor.EditorApplication.update += EditorTick;
        }
    }

    void OnDisable()
    {
        UnityEditor.EditorApplication.update -= EditorTick;
    }

    void EditorTick()
    {
        if (Application.isPlaying || this == null)
        {
            return;
        }

        double now = UnityEditor.EditorApplication.timeSinceStartup;
        float deltaTime = (float)(now - lastEditorTime);
        lastEditorTime = now;

        Tick(deltaTime);

        // Repaint the Scene/Game views so motion and ring-down are actually visible while
        // editing. Only while something is happening, to avoid constant idle repaints.
        float activity = Mathf.Abs(wobbleAmountX) + Mathf.Abs(wobbleAmountZ)
                       + Mathf.Abs(springVelX) + Mathf.Abs(springVelZ)
                       + worldVelocity.magnitude;
        if (activity > 0.0001f)
        {
            UnityEditor.SceneView.RepaintAll();
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
    }
#endif

    void GetMeshAndRend()
    {
        if (mesh == null)
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }
        if (rend == null)
        {
            rend = GetComponent<Renderer>();
        }
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
    }

    void Update()
    {
        // In the editor the sim is driven continuously by EditorTick (via
        // EditorApplication.update), so MonoBehaviour.Update only runs it in play mode.
        if (!Application.isPlaying)
        {
            return;
        }

        float deltaTime = 0;
        switch (updateMode)
        {
            case UpdateMode.Normal:
                deltaTime = Time.deltaTime;
                break;

            case UpdateMode.UnscaledTime:
                deltaTime = Time.unscaledDeltaTime;
                break;
        }

        Tick(deltaTime);
    }

    void Tick(float deltaTime)
    {
        // Clamp so an editor repaint gap or an in-game frame hitch can't inject a huge
        // dt that destabilizes the spring integration.
        deltaTime = Mathf.Clamp(deltaTime, 0f, 0.05f);

        if (deltaTime > 0)
        {
            // Linear velocity and acceleration of the container, in world space.
            Vector3 newWorldVelocity = (transform.position - lastPos) / deltaTime;
            Vector3 rawAccel = (newWorldVelocity - worldVelocity) / deltaTime;
            worldVelocity = newWorldVelocity;

            // Low-pass the acceleration; thicker liquid responds more slowly and ignores jitter.
            float accelSmoothing = deltaTime * (10f / Mathf.Max(Thickness, 0.01f));
            smoothedAccel = Vector3.Lerp(smoothedAccel, rawAccel, Mathf.Clamp01(accelSmoothing));

            angularVelocity = GetAngularVelocity(lastRot, transform.rotation, deltaTime);

            // Inertial (pseudo) force on the liquid points OPPOSITE the container's acceleration,
            // so the surface piles up on the trailing side — the physically correct direction.
            // A little vertical acceleration and the container's spin feed in too.
            // Note: the shader maps _WobbleX to a rotation about Z and _WobbleZ to a rotation
            // about X — opposite handedness — so the X drive takes the opposite sign of the Z
            // drive for both axes to slosh away from the direction of movement.
            float driveX = -(smoothedAccel.x + smoothedAccel.y * 0.2f) - angularVelocity.z - angularVelocity.y;
            float driveZ = (smoothedAccel.z + smoothedAccel.y * 0.2f) - angularVelocity.x - angularVelocity.y;

            // Equilibrium tilt the liquid is currently being pushed toward, capped at MaxWobble.
            float targetX = Mathf.Clamp(driveX * WobbleStrength, -MaxWobble, MaxWobble);
            float targetZ = Mathf.Clamp(driveZ * WobbleStrength, -MaxWobble, MaxWobble);

            // Damped harmonic oscillator per axis (semi-implicit Euler): the surface chases the
            // target, overshoots, swings back slightly less, and rings down until it settles.
            float omega = 2f * Mathf.PI * WobbleSpeedMove;
            float k = omega * omega;   // stiffness -> frequency
            float c = Recovery;        // damping -> decay rate

            springVelX += (k * (targetX - wobbleAmountX) - c * springVelX) * deltaTime;
            springVelZ += (k * (targetZ - wobbleAmountZ) - c * springVelZ) * deltaTime;
            wobbleAmountX += springVelX * deltaTime;
            wobbleAmountZ += springVelZ * deltaTime;

            // Allow overshoot past the target, but keep it bounded for the shader.
            float limit = MaxWobble * 2f;
            wobbleAmountX = Mathf.Clamp(wobbleAmountX, -limit, limit);
            wobbleAmountZ = Mathf.Clamp(wobbleAmountZ, -limit, limit);
        }

        // Compute fill offset, then push everything to the shader via a per-instance property block
        // so every cup wobbles independently without mutating the shared material asset.
        UpdatePos(deltaTime);
        ApplyToRenderer();

        // keep last frame's transform
        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    void ApplyToRenderer()
    {
        if (rend == null)
        {
            return;
        }
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(WobbleXID, wobbleAmountX);
        mpb.SetFloat(WobbleZID, wobbleAmountZ);
        mpb.SetVector(FillAmountID, pos);
        mpb.SetFloat(AmplitudeID, SurfaceWave ? SurfaceWaveAmplitude : 0f);
        rend.SetPropertyBlock(mpb);
    }

    void UpdatePos(float deltaTime)
    {
        Vector3 worldPos = transform.TransformPoint(new Vector3(mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z));
        if (CompensateShapeAmount > 0)
        {
            // only lerp if not paused/normal update
            if (deltaTime != 0)
            {
                comp = Vector3.Lerp(comp, (worldPos - new Vector3(0, GetLowestPoint(), 0)), deltaTime * 10);
            }
            else
            {
                comp = (worldPos - new Vector3(0, GetLowestPoint(), 0));
            }

            pos = worldPos - transform.position - new Vector3(0, fillAmount - (comp.y * CompensateShapeAmount), 0);
        }
        else
        {
            pos = worldPos - transform.position - new Vector3(0, fillAmount, 0);
        }
    }

    //https://forum.unity.com/threads/manually-calculate-angular-velocity-of-gameobject.289462/#post-4302796
    Vector3 GetAngularVelocity(Quaternion foreLastFrameRotation, Quaternion lastFrameRotation, float deltaTime)
    {
        var q = lastFrameRotation * Quaternion.Inverse(foreLastFrameRotation);
        // no rotation?
        // You may want to increase this closer to 1 if you want to handle very small rotations.
        // Beware, if it is too close to one your answer will be Nan
        if (Mathf.Abs(q.w) > 1023.5f / 1024.0f)
            return Vector3.zero;
        float gain;
        // handle negatives, we could just flip it but this is faster
        if (q.w < 0.0f)
        {
            var angle = Mathf.Acos(-q.w);
            gain = -2.0f * angle / (Mathf.Sin(angle) * deltaTime);
        }
        else
        {
            var angle = Mathf.Acos(q.w);
            gain = 2.0f * angle / (Mathf.Sin(angle) * deltaTime);
        }
        Vector3 angularVelocity = new Vector3(q.x * gain, q.y * gain, q.z * gain);

        if (float.IsNaN(angularVelocity.z))
        {
            angularVelocity = Vector3.zero;
        }
        return angularVelocity;
    }

    float GetLowestPoint()
    {
        float lowestY = float.MaxValue;
        Vector3 lowestVert = Vector3.zero;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {

            Vector3 position = transform.TransformPoint(vertices[i]);

            if (position.y < lowestY)
            {
                lowestY = position.y;
                lowestVert = position;
            }
        }
        return lowestVert.y;
    }
}
