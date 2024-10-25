using UnityEngine;

public class ShiftEndUI : MonoBehaviour
{
    public void B_AdvanceDay()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.UIClick);
        StateManager.Instance.ChangeState(new PlanningState());
    }

    public void B_QuitToMenu()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.UIClick);
        StateManager.Instance.ChangeState(new MainMenuState());
    }
}