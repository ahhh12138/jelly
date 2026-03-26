using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SampleSceneStart : MonoBehaviour
{
    public void Awake()
    {
        SceneManager.LoadScene("StartScene");
        Debug.Log("push");
    }
}
