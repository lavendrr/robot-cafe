using UnityEngine;

public class PauseUI : MonoBehaviour
{
    public void B_Resume()
    {
        StateManager.Instance.SetGamePaused(false);
    }

    public void B_QuitToMenu()
    {
        StateManager.Instance.ChangeState(new MainMenuState());
    }

    public void B_QuitGame()
    {
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}