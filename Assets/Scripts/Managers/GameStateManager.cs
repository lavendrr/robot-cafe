using UnityEngine;


        public class StateManager : MonoBehaviour
    {
        public static StateManager Instance { get; private set; }

        private State currentState;

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
        }

        private void Start()
        {
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
        }

        private void Update()
        {
            if (currentState != null)
            {
                currentState.Update();
            }
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
        public override void Enter()
        {
            Debug.Log("Entering Shift state");
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
            Debug.Log("Exiting Shift state");
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

