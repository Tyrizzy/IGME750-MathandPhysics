using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Follow : MonoBehaviour
{
    [SerializeField] Transform MainCube;
    [SerializeField] Transform Camera;
    [SerializeField] GameObject Canvas;
    private bool IsPaused;

    [SerializeField] private Vector3 Offset;
    private Vector3 Vec = Vector3.zero;

    private void Start()
    {
        
    }

    private void Update()
    {
        Vector3 TargetPos = MainCube.position + Offset;

        transform.position = Vector3.SmoothDamp(Camera.position, TargetPos, ref Vec, 0);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        Canvas.SetActive(true);
        IsPaused = true;
        Debug.Log("PASUED");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Canvas.SetActive(false);
        IsPaused = false;
        Debug.Log("UNPASUED");
    }

    public void Restart()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
