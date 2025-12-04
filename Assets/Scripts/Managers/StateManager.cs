using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using GameConsts;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    private State currentState;
    public delegate void StateChangeHandler(State newState);
    public event StateChangeHandler OnStateChanged;
    public delegate void PauseChangeHandler(bool gamePaused);
    public event PauseChangeHandler OnGamePausedChanged;
    private bool gamePaused = false;

    public Player player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        ChangeState(new MainMenuState());
    }

    public State GetCurrentState()
    {
        return currentState;
    }

    public void ChangeState(State newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();

        // Broadcast the state change event
        OnStateChanged?.Invoke(newState);
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
    }

    public bool GetGamePaused()
    {
        return gamePaused;
    }

    public void SetGamePaused(bool _gamePaused)
    {
        if (currentState != null && currentState.Pausable)
        {
            gamePaused = _gamePaused;

            // Notify subscribers of the event
            OnGamePausedChanged?.Invoke(gamePaused);
        }
    }

    public void RegisterPlayer(Player newPlayer)
    {
        player = newPlayer;
    }

    public IEnumerator Delay(float time, System.Action<bool> done)
    {
        yield return new WaitForSeconds(time);
        done(true);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Start" || scene.name == "Shift" || scene.name == "Planning")
        {
            SceneManager.SetActiveScene(scene);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

public abstract class State
{
    public abstract bool Pausable { get; }
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

public class MainMenuState : State
{
    public override bool Pausable => false;
    public override void Enter()
    {
        if (!SceneManager.GetSceneByName("Start").isLoaded)
        {
            SceneManager.LoadScene("Start", LoadSceneMode.Additive);
        }

        if (!SceneManager.GetSceneByName("LevelGeo").isLoaded)
        {
            SceneManager.LoadScene("LevelGeo", LoadSceneMode.Additive);
        }

        if (SceneManager.GetSceneByName("Shift").isLoaded)
        {
            SceneManager.UnloadSceneAsync("Shift");
        }
        if (SceneManager.GetSceneByName("Planning").isLoaded)
        {
            SceneManager.UnloadSceneAsync("Planning");
        }
        if (SceneManager.GetSceneByName("Pause").isLoaded)
        {
            SceneManager.UnloadSceneAsync("Pause");
        }
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
        SceneManager.UnloadSceneAsync("Start");
    }
}

public class PlanningState : State
{
    public override bool Pausable => false;
    public override void Enter()
    {
        // Update day count
        SaveManager.Instance.SetDayCount(SaveManager.Instance.GetDayCount() + 1);
        if (!SceneManager.GetSceneByName("Planning").isLoaded)
        {
            SceneManager.LoadScene("Planning", LoadSceneMode.Additive);
        }
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
        // Save game
        LevelLayout newLayout = SaveManager.Instance.GetCafeLayout();
        newLayout.elements = PlanningManager.Instance.GetFinalGrid();
        SaveManager.Instance.SaveCafeLayout(newLayout);
        SaveManager.Instance.Save();
        SceneManager.UnloadSceneAsync("Planning");
    }
}

public class ShiftState : State
{
    public override bool Pausable => true;
    private float shiftTimer = GameConsts.GameConsts.ShiftLengthInSec;
    private bool paused = false;
    public override void Enter()
    {
        if (!SceneManager.GetSceneByName("Shift").isLoaded)
        {
            SceneManager.LoadScene("Shift", LoadSceneMode.Additive);
        }
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Subscribe to game paused events
        StateManager.Instance.OnGamePausedChanged += OnGamePausedChanged;
        // Capture the player cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Shift")
        {
            // Spawn first cup
            //OrderManager.Instance.SpawnCup();
        }
    }

    public override void Update()
    {
        if (!paused)
        {
            UpdateTimer();
        }
    }

    public override void Exit()
    {
        // Delete all stray cups
        Cup[] cups = Object.FindObjectsOfType<Cup>();
        foreach (Cup cup in cups)
        {
            cup.Destroy();
        }

        // Delete all stray customers
        Customer[] customers = Object.FindObjectsOfType<Customer>();
        foreach (Customer customer in customers)
        {
            customer.Destroy();
        }

        // Unsubscribe from the scene load event
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // Unsubscribe to game paused events
        StateManager.Instance.OnGamePausedChanged -= OnGamePausedChanged;
    }

    private void UpdateTimer()
    {
        // Update the timer each frame until it reaches 0, and format the string accordingly for the UI text
        if (shiftTimer > 0f)
        {
            shiftTimer -= Time.deltaTime;
            if (UIManager.Instance)
            {
                UIManager.Instance.UpdateTimerText(shiftTimer);
            }
        }
        else
        {
            StateManager.Instance.ChangeState(new ShiftEndState());
        }
    }

    private void OnGamePausedChanged(bool gamePaused)
    {
        paused = gamePaused;
        if (paused)
        {
            // Disable player movement
            GameObject.Find("PlayerCapsule").GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerPaused");
        }
        else
        {
            // Enable player movement
            GameObject.Find("PlayerCapsule").GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
        }
    }
}

public class ShiftEndState : State
{
    public override bool Pausable => false;
    public override void Enter()
    {
        // Release the player cursor
        Cursor.lockState = CursorLockMode.None;
        // Update the high score
        int score = OrderManager.Instance.completedCounter;
        if (SaveManager.Instance.GetHighScore() < score)
        {
            SaveManager.Instance.SetHighScore(score);
        }
        // Switch the UI to the game end UI
        UIManager.Instance.UpdateShiftEndUI(score);
        // Save game
        SaveManager.Instance.Save();

        // Leaderboard
        if(SaveManager.Instance.GetCurrentDay() == 0)
        {
            Debug.Log("Saving leaderboard stuff");
            SaveManager.Instance.SaveLeaderboardEntry(score);
            SaveManager.Instance.SaveLeaderboard();
        }
        Debug.Log("Saving leaderboard stuff 2");
        SaveManager.Instance.SaveLeaderboardEntry(score + 1);
        SaveManager.Instance.SaveLeaderboard();
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
        SceneManager.UnloadSceneAsync("Shift");
    }
}
