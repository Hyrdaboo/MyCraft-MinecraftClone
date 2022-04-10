using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private void Update()
    {       
        if (Input.GetKeyDown(KeyCode.Escape) && EndlessTerrain.finished)
        {
           Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.R) && EndlessTerrain.finished)
        {
            EndlessTerrain.finished = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}
