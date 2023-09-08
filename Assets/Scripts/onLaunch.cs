using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onLaunch : MonoBehaviour
{
    private GameObject sceneManager;
    private cameraController camCon;
    private menuController menuCon;

    void Start()
    {
        sceneManager = GameObject.Find("SceneManager");
        camCon = sceneManager.GetComponent<cameraController>();
        menuCon = sceneManager.GetComponent<menuController>();
    }
}
