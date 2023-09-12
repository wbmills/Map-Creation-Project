using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using System.IO;

public class exportScene : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject sceneTemplate;

    // from https://docs.unity3d.com/Packages/com.unity.formats.fbx@2.0/manual/devguide.html
    public void ExportMapAsFBX(GameObject parentObj)
    {
        Terrain tOb = FindAnyObjectByType<Terrain>();
        GameObject allObjs = GameObject.FindGameObjectWithTag("Object Parent");
        string filePath = Path.Combine(Application.dataPath, "Map.fbx");
        //ModelExporter.ExportObject(filePath, allObjs);
        ModelExporter.ExportObject(filePath, tOb);
        Debug.Log($"Successful Save in {filePath}");
    }

    // Add extras not already in parent object to parent, add player if chosen, export as scene if chosen
    public GameObject CompileIntoParent(GameObject parentObj, List<GameObject> objsToAdd, bool withPlayer)
    {
        foreach(GameObject obj in objsToAdd)
        {
            obj.transform.SetParent(parentObj.transform);
        }

        if (withPlayer)
        {
            // export with player controller too
            GameObject playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            playerInstance.transform.SetParent(parentObj.transform);
        }
        return parentObj;
    }

    public SceneAsset ExportAsScene(GameObject parentObj)
    {
        // create empty scene, add all assets from map, and return new scene
        return null;
    }
}
