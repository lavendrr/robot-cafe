using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Orders
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance { get; private set; }
        private AudioManager Audio;

        private GameObject gameUI, endMenu;
        private Crosshair crosshair;
        private TextMeshProUGUI orderInfo, ordersCompleted, timerText;
        private float timer = 60f;
        private int minutes;
        private float seconds;

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
            Audio = AudioManager.instance;
            gameUI = GameObject.Find("GameUI");
            endMenu = GameObject.Find("EndMenu");
            endMenu.SetActive(false);
            crosshair = new Crosshair(GameObject.Find("Crosshair"));
            orderInfo = GameObject.Find("OrderInfo").GetComponent<TextMeshProUGUI>();
            timerText = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
            ordersCompleted = GameObject.Find("OrdersCompleted").GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateCrosshair();
            UpdateTimer();
        }

        private void UpdateCrosshair()
        {
            // Update crosshair state depending on what the player is looking at
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

        private void UpdateTimer()
        {
            // Update the timer each frame until it reaches 0, and format the string accordingly for the UI text
            if(!(timer < 0f))
            {
                timer -= Time.deltaTime;
                seconds = timer % 60;
                minutes = (int) timer / 60;
                timerText.text = "Time: " + string.Format("{0:00}:{1:00.0}", minutes, seconds);
            }
            else
            {
                EndGame();
            }
        }

        public void SetOrderInfo(string order)
        {
            orderInfo.text = "Current Order: " + order;
        }

        public void CompleteOrder(int completed)
        {
            ordersCompleted.text = "Orders Completed: " + completed.ToString();
        }

        public void EndGame()
        {
            // Disables player input, releases the player cursor, switches the UI to the game end UI, updates the high score, and sets the game end text
            GameObject.Find("PlayerCapsule").GetComponent<PlayerInput>().DeactivateInput();
            Cursor.lockState = CursorLockMode.None;
            gameUI.SetActive(false);
            endMenu.SetActive(true);

            int score = OrderManager.instance.completedCounter;

            if (PlayerPrefs.GetInt("highScore") < OrderManager.instance.completedCounter)
            {
                PlayerPrefs.SetInt("highScore", OrderManager.instance.completedCounter);
            }

            GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = "Orders Completed: " + score.ToString() + "\nHigh Score: " + PlayerPrefs.GetInt("highScore").ToString();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene("Level");
        }

        public void LoadStartScene()
        {
            SceneManager.LoadScene("Start");
        }
    }

    // Class for the Crosshair UI element, containing methods to easily swap its tooltip text and color
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
