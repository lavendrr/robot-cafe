using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

namespace Orders
{
    public class Customer : MonoBehaviour
    {
        [SerializeField]
        private List<EmotionMaterialMap> emotionMaterialMaps = new List<EmotionMaterialMap>();
        private Animator animator;
        private Order order;
        private MeshRenderer headMesh;

        void Start()
        {
            animator = GetComponent<Animator>();
            headMesh = gameObject.transform.Find("SM_robot/robot_head")?.GetComponent<MeshRenderer>();

            SetEmotion(Emotion.Happy);
        }

        public Order GetOrder()
        {
            return order;
        }

        public void PlaceOrder()
        {
            order = new Order();
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
            Destroy(this.gameObject);
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
}