using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseUI;
    bool paused = false;

    void Start()
    {
        paused = false;
        if (pauseUI != null ) pauseUI.SetActive( false );
        Time.timeScale = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        paused = !paused;
        pauseUI.SetActive(paused);
        Time.timeScale = paused ? 0 : 1;
    }
}
