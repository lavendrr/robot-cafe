using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public void StartGame()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.UIClick);
        StateManager.Instance.ChangeState(new ShiftState());
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.UIClick);
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
