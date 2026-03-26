using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESCcontrol : MonoBehaviour
{
    public GameObject Canvas;
    public bool isESCopen = false;
    // Start is called before the first frame update
    void Start()
    {
        Canvas.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            
            switch (isESCopen) {
                case false:
                    Canvas.SetActive(true);
                    Time.timeScale = 0f;
                    isESCopen = !isESCopen;
                    Debug.Log("openESC");
                    break;
                case true:
                    Canvas.SetActive(false);
                    Time.timeScale = 1f;
                    isESCopen = !isESCopen;
                    Debug.Log("closeESC");
                    break;
            }
        }
    }

    public void ContinueGame()
    {
        Canvas.SetActive(false);
        Time.timeScale = 1f;
        isESCopen = !isESCopen;
        Debug.Log("Continue");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
