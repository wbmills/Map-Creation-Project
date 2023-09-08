using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using System.IO;

public class exportScene : MonoBehaviour
{
    public bool export;
    void Start()
    {
        export = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (export)
        {
            ExportGameObjects(); 
        }
    }

    // from https://docs.unity3d.com/Packages/com.unity.formats.fbx@2.0/manual/devguide.html
    public void ExportGameObjects()
    {
        export = false;
        GameObject allObjs = GameObject.FindGameObjectWithTag("Object Parent");
        string filePath = Path.Combine(Application.dataPath, "MyGame.fbx");
        ModelExporter.ExportObject(filePath, allObjs);
    }

}
