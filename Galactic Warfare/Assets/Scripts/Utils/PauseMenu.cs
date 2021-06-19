using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuCanvas;
    public PlayerMovement movement;
    public PlayerInventoryManager inventory;

    private CursorLockMode lastLockMode;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCanvas();
        }
    }

    private void ToggleCanvas()
    {
        bool canvasActive = !pauseMenuCanvas.activeSelf;
        pauseMenuCanvas.SetActive(canvasActive);

        if(canvasActive)
        {
            lastLockMode = Cursor.lockState;
            Cursor.lockState = CursorLockMode.None;

            movement.InputEnabled = false;
            inventory.InputEnabled = false;
        }
        else
        {
            Cursor.lockState = lastLockMode;
            movement.InputEnabled = true;
            inventory.InputEnabled = true;
        }
    }

    public void MainMenu()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }
    }
}
