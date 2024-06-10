using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    private State currentState;
    public delegate void StateChangeHandler(State newState);
    public event StateChangeHandler OnStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Level")
        {
            ChangeState(new ShiftState());
        }
        if (scene.name == "Start")
        {
            ChangeState(new MainMenuState());
        }
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

    private void OnDestroy()
    {
        // Unsubscribe from the scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

public abstract class State
{
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}

public class MainMenuState : State
{
    public override void Enter()
    {
        Debug.Log("Entering MainMenu state");
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
        Debug.Log("Exiting MainMenu state");
    }
}

public class PlanningState : State
{
    public override void Enter()
    {
        Debug.Log("Entering Planning state");
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
        Debug.Log("Exiting Planning state");
    }
}

public class ShiftState : State
{
    private float shiftTimer = 15f;
    public override void Enter()
    {
        Debug.Log("Entering Shift state");
    }

    public override void Update()
    {
        UpdateTimer();
    }

    public override void Exit()
    {
        Debug.Log("Exiting Shift state");
    }

    private void UpdateTimer()
    {
        // Update the timer each frame until it reaches 0, and format the string accordingly for the UI text
        if (shiftTimer > 0f)
        {
            shiftTimer -= Time.deltaTime;
            UIManager.instance.UpdateTimerText(shiftTimer);
        }
        else
        {
            StateManager.Instance.ChangeState(new ShiftEndState());
        }
    }
}

public class PauseState : State
{
    public override void Enter()
    {
        Debug.Log("Entering Pause state");
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
        Debug.Log("Exiting Pause state");
    }
}

public class ShiftEndState : State
{
    public override void Enter()
    {
        Debug.Log("Entering ShiftEnd state");
        // Disable player input
        GameObject.Find("PlayerCapsule").GetComponent<PlayerInput>().DeactivateInput();
        // Release the player cursor
        Cursor.lockState = CursorLockMode.None;
        // Update the high score and day count
        int score = OrderManager.instance.completedCounter;
        if (SaveManager.instance.GetHighScore() < score)
        {
            SaveManager.instance.SetHighScore(score);
        }
        SaveManager.instance.SetDayCount(SaveManager.instance.GetDayCount() + 1);
        // Switch the UI to the game end UI
        UIManager.instance.EndGame(score);
        // Save game
        SaveManager.instance.Save();
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
        Debug.Log("Exiting ShiftEnd state");
    }
}
