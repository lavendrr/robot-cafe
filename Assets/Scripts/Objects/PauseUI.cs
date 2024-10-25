using UnityEngine;

public class PauseUI : MonoBehaviour
{
    public void B_Resume()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.UIClick);
        StateManager.Instance.SetGamePaused(false);
    }

    public void B_QuitToMenu()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.UIClick);
        StateManager.Instance.ChangeState(new MainMenuState());
    }

    public void B_QuitGame()
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