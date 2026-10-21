// Hand-written URP Unlit replacement for the LiquidEffect shader graph.
//
// Preserves the original look (wobble-tilted fill cut, scrolling surface wave, foam band,
// fresnel rim, two-sided front/back shading) and adds layered-drink support:
//
//   * _FillRamp : a 1xN vertical gradient texture built on the CPU from the drink's bases
//                 (bottom .. top, in pour order). Sampled by normalized height to colour the body.
//   * The liquid SURFACE and FOAM colours are derived in-shader from the ramp's TOP band
//     (a slightly / strongly lightened version of it), so there is a single source of truth.
//   * Layer boundaries follow the wobble tilt (they use the same wobble-rotated fill coordinate
//     as the surface cut), but do NOT ripple with the surface wave.
//
// Property names (_WobbleX/_WobbleZ/_FillAmount/_Amplitude/...) match the old shader so
// Liquid.cs keeps driving it through its MaterialPropertyBlock unchanged.
Shader "RobotCafe/Liquid"
{
    Properties
    {
        [HideInInspector] _WobbleX ("Wobble X", Float) = 0
        [HideInInspector] _WobbleZ ("Wobble Z", Float) = 0
        [HideInInspector] _FillAmount ("Fill Amount", Vector) = (0, 0, 0, 0)

        _LiquidDepth ("Liquid Depth (layer span)", Float) = 1.0
        _Tint ("Body Tint", Color) = (1, 1, 1, 1)

        _LayerSoftness ("Layer Softness", Range(0, 0.5)) = 0.02
        _SurfaceLighten ("Surface Lighten (from top band)", Range(0, 1)) = 0.2
        _FoamLighten ("Foam Lighten (from top band)", Range(0, 1)) = 0.5

        [NoScaleOffset] _Main_Texture ("Main Texture", 2D) = "white" {}
        _Foam_Smoothness ("Foam Smoothness", Float) = 0
        _FoamWidth ("Foam Width", Range(0, 0.5)) = 0.04
        _Frequency ("Wave Frequency", Float) = 3
        _Amplitude ("Wave Amplitude", Float) = 0.5
        _Rim_Power ("Rim Power", Float) = 5
        [HDR] _Rim_Color ("Rim Color", Color) = (0.19, 0.086, 0.0098, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float  _WobbleX;
            float  _WobbleZ;
            float3 _FillAmount;
            float  _LiquidDepth;
            float4 _Tint;
            float  _LayerSoftness;
            float  _SurfaceLighten;
            float  _FoamLighten;
            float  _Foam_Smoothness;
            float  _FoamWidth;
            float  _Frequency;
            float  _Amplitude;
            float  _Rim_Power;
            float4 _Rim_Color;
            float4 _Main_Texture_ST;
        CBUFFER_END

        // Layer bands are passed straight in as uniforms (set per-cup via MaterialPropertyBlock)
        // rather than baked into a gradient texture. _LayerHeights[i] is the cumulative TOP of
        // band i in normalized height (0..1); the last used entry is 1. The body colour is picked
        // analytically from the normalized fill height, so a composition change only writes a few
        // arrays — nothing is regenerated on the CPU.
        #define MAX_LAYERS 10
        int    _LayerCount;
        float4 _LayerColors[MAX_LAYERS];
        float  _LayerHeights[MAX_LAYERS];

        TEXTURE2D(_Main_Texture);   SAMPLER(sampler_Main_Texture);

        // Rodrigues rotation, matching the graph's "Rotate About Axis (Degrees)" node.
        float3 RotateAboutAxisDegrees(float3 v, float3 axis, float degrees)
        {
            float rad = radians(degrees);
            float s, c;
            sincos(rad, s, c);
            axis = normalize(axis);
            return v * c + cross(axis, v) * s + axis * dot(axis, v) * (1.0 - c);
        }

        // Wobble-rotated fill coordinate, in object-origin-relative world space.
        // fillPos.y is the tilted vertical coordinate the surface cut and the layers key off.
        float3 ComputeFillPosition(float3 positionWS)
        {
            float3 originWS = TransformObjectToWorld(float3(0, 0, 0));
            float3 rel = positionWS - originWS - _FillAmount;
            float3 rotForZ = RotateAboutAxisDegrees(rel, float3(1, 0, 0), 90.0); // scaled by WobbleZ
            float3 rotForX = RotateAboutAxisDegrees(rel, float3(0, 0, 1), 90.0); // scaled by WobbleX
            return rel + _WobbleX * rotForX + _WobbleZ * rotForZ;
        }

        // Scrolling surface wave added to the vertical coordinate for the cut & foam (not the layers).
        float SurfaceWaveOffset(float3 fillPos)
        {
            float amp = _Amplitude * (abs(_WobbleX) + abs(_WobbleZ));
            float phase = _Time.y + fillPos.x * _Frequency + fillPos.z * _Frequency;
            return sin(phase) * amp;
        }

        // Pick the body colour from the band uniforms, blending across each interior boundary.
        // t is the normalized fill height (0 at the liquid bottom, 1 at the surface).
        float3 LayerColor(float t)
        {
            int count = clamp(_LayerCount, 1, MAX_LAYERS);
            float3 col = _LayerColors[0].rgb;
            float w = max(_LayerSoftness, 1e-4);
            for (int i = 1; i < count; i++)
            {
                float edge = _LayerHeights[i - 1];          // bottom of band i = top of band i-1
                float blend = smoothstep(edge - 0.5 * w, edge + 0.5 * w, t);
                col = lerp(col, _LayerColors[i].rgb, blend);
            }
            return col;
        }
        ENDHLSL

        // ------------------------------------------------------------------
        // Forward unlit pass
        // ------------------------------------------------------------------
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest LEqual
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 fillPos    : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3 positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.positionWS = positionWS;
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.fillPos = ComputeFillPosition(positionWS);
                return OUT;
            }

            // Lighten toward white; used to derive surface/foam from the ramp's top band.
            float3 Lighten(float3 c, float amount)
            {
                return lerp(c, float3(1, 1, 1), amount);
            }

            half4 frag(Varyings IN, FRONT_FACE_TYPE faceSign : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float wave = SurfaceWaveOffset(IN.fillPos);
                float surfaceCoord = IN.fillPos.y + wave;   // for cut & foam (ripples)
                float fillY = IN.fillPos.y;                 // for layers (flat, tilted only)

                // Liquid exists below the cut plane at 0.5.
                float inside = step(surfaceCoord, 0.5);
                clip(inside - 0.01);

                // Normalized height up the liquid column, 0 at the bottom, 1 at the surface.
                float t = saturate((fillY - 0.5) / max(_LiquidDepth, 1e-4) + 1.0);

                float3 rampCol = LayerColor(t);
                int topIdx = clamp(_LayerCount - 1, 0, MAX_LAYERS - 1);
                float3 topBand = _LayerColors[topIdx].rgb;

                float3 surfaceColor = Lighten(topBand, _SurfaceLighten);
                float3 foamColor    = Lighten(topBand, _FoamLighten);

                // Body colour: ramp * main texture (subtle variation) * tint.
                float4 tex = SAMPLE_TEXTURE2D(_Main_Texture, sampler_Main_Texture, surfaceCoord.xx);
                float3 body = rampCol * tex.rgb * _Tint.rgb;

                // Foam band just below the surface.
                float foamInner = 0.5 - _FoamWidth;
                float foamOuter = foamInner - _Foam_Smoothness;
                float foamBand = smoothstep(foamOuter, foamInner, surfaceCoord) * inside;

                // Fresnel rim.
                float3 N = normalize(IN.normalWS);
                float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                float fresnel = pow(1.0 - saturate(dot(N, V)), _Rim_Power);
                float3 rim = _Rim_Color.rgb * fresnel;

                // Front faces: body with foam carved in + foam + rim.
                float3 frontColor = rim + body * (1.0 - foamBand) + foamColor * foamBand;

                // Back faces (the top surface disk seen from above): lightened top band + rim of foam.
                float topFoamMask = step(0.5 - 0.1 * _FoamWidth, surfaceCoord) * inside;
                float3 backColor = surfaceColor + foamColor * topFoamMask;

                bool isFront = IS_FRONT_VFACE(faceSign, true, false);
                float3 color = isFront ? frontColor : backColor;

                return half4(color, inside);
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------
        // Depth prepass (matches the clip so depth lines up with the visible liquid)
        // ------------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            Cull Off
            ZTest LEqual
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float3 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 fillPos    : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3 positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.fillPos = ComputeFillPosition(positionWS);
                return OUT;
            }

            half frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                float surfaceCoord = IN.fillPos.y + SurfaceWaveOffset(IN.fillPos);
                clip(step(surfaceCoord, 0.5) - 0.01);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
