using UnityEngine;

public class ShiftEndUI : MonoBehaviour
{
    public void B_AdvanceDay()
    {
        StateManager.Instance.ChangeState(new ShiftState());
    }

    public void B_QuitToMenu()
    {
        StateManager.Instance.ChangeState(new MainMenuState());
    }
}