
using Project.Core;
using Project.Core.States;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class _TEMP_DebugLoader : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            GameManager.Instance.ChangeState(GameStateType.Gameplay);
            SceneManager.LoadScene("Level_01", LoadSceneMode.Single);
        }
    }
}