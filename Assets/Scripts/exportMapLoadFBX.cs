using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class exportMapLoadFBX : MonoBehaviour
{
    public bool loadMap = true;

    // will execute during edit and runtime
    [ExecuteAlways]
    void Start()
    {
        if (loadMap)
        {
            // load fbx function
            // if file not found, open menu to select file location
        }
    }

    private void loadFBX()
    {
        // load fbx file
    }
}
