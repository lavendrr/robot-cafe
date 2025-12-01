using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;

public enum Emotion
{
    Happy,
    Shocked,
    Annoyed,
    Starry
}

[System.Serializable]
public class EmotionMaterialMap
{
    public Emotion emotion;
    public Material material;
}

public class Customer : MonoBehaviour
{
    [SerializeField]
    private List<EmotionMaterialMap> emotionMaterialMaps = new List<EmotionMaterialMap>();
    private Animator animator;
    private Order order;
    private MeshRenderer headMesh;
    public EventInstance hoverAudio;

    void Start()
    {
        animator = GetComponent<Animator>();
        headMesh = transform.Find("Customer_head")?.GetComponent<MeshRenderer>();
        hoverAudio = (EventInstance)AudioManager.Instance.PlaySFX(AudioManager.Instance.customerHover, transform.position, gameObject, true);

        SetEmotion(Emotion.Happy);
    }

    public Order GetOrder()
    {
        return order;
    }

    public void PlaceOrder()
    {
        // If the order is null, place a new order. This stops repeated order taking
        order ??= new Order();
    }

    public void WrongOrder()
    {
        StartCoroutine(WrongOrderAnim());
    }

    private IEnumerator WrongOrderAnim()
    {
        SetEmotion(Emotion.Shocked);
        yield return new WaitForSeconds(1f);
        SetEmotion(Emotion.Annoyed);
        yield break;
    }

    public void Leave()
    {
        animator.SetTrigger("Leave");
        SetEmotion(Emotion.Starry);
    }

    public void Destroy()
    {
        AudioManager.Instance.RemoveInstance(hoverAudio);
        Destroy(gameObject);
    }

    private Material GetEmotionMaterial(Emotion emotion)
    {
        foreach (var map in emotionMaterialMaps)
        {
            if (map.emotion == emotion)
            {
                return map.material;
            }
        }
        return null;
    }

    private void SetEmotion(Emotion emotion)
    {
        Material emotionMaterial = GetEmotionMaterial(emotion);
        if (emotionMaterial == null)
        {
            Debug.LogWarning("Material for emotion " + emotion.ToString() + " is null.");
            return;
        }

        if (headMesh != null && headMesh.materials.Length > 3)
        {
            // Get the current materials array
            List<Material> currentMaterials = new List<Material>();
            headMesh.GetMaterials(currentMaterials);

            // Set the emotion material to the fourth slot (face material)
            currentMaterials[3] = emotionMaterial;

            // Apply the updated materials array
            headMesh.SetMaterials(currentMaterials);
        }
        else
        {
            Debug.LogWarning("Error setting face material.");
        }
    }
}
