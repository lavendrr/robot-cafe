using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Orders
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance { get; private set; }

        private Crosshair crosshair;
        private TextMeshProUGUI orderInfo;

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.
            if (instance != null && instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                instance = this; 
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            crosshair = new Crosshair(GameObject.Find("Crosshair"));
            orderInfo = GameObject.Find("OrderInfo").GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateCrosshair();
        }

        private void UpdateCrosshair()
        {
            var (type, _) = PlayerInteractions.instance.InteractionCheck();
            if (type == InteractableType.Grabbable)
            {
                crosshair.SetGrab();
            }
            else if (type == InteractableType.Slottable && PlayerInteractions.instance.GetGrabStatus())
            {
                crosshair.SetSlot();
            }
            else if (type == InteractableType.Usable)
            {
                crosshair.SetUse();
            }
            else
            {
                crosshair.SetNeutral();
            }
        }

        public void SetOrderInfo(string order)
        {
            orderInfo.text = "Current Order: " + order;
        }
    }

    public class Crosshair
    {
        private GameObject obj;
        private UnityEngine.UI.Image img;
        private TextMeshProUGUI tooltip;

        public Crosshair(GameObject gameObject)
        {
            obj = gameObject;
            img = obj.GetComponent<UnityEngine.UI.Image>();
            tooltip = obj.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetNeutral()
        {
            img.color = Color.white;
            tooltip.text = "";
        }

        public void SetGrab()
        {
            img.color = Color.red;
            tooltip.text = "(E)";
        }

        public void SetSlot()
        {
            img.color = Color.green;
            tooltip.text = "(E)";
        }

        public void SetUse()
        {
            img.color = Color.blue;
            tooltip.text = "(Q)";
        }
    }
}
