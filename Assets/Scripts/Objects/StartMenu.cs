using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public void StartGame()
    {
        StateManager.Instance.ChangeState(new ShiftState());
    }

    public void QuitGame()
    {
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
