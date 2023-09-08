using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class loadTown : MonoBehaviour
{
    public ObjectInformation[] objectInformation;
    public bool autosave;
    private townGeneration tgScript;
    public string file;
    private List<Vector3> allTilePositions;
    private List<string> saveOptions;
    public Dropdown saveOptionsDropdown;
    public class ObjectInformation
    {
        // all columns for file
        public string name;

        public float positionX;
        public float positionY;
        public float positionZ;

        public float rotationX;
        public float rotationY;
        public float rotationZ;
    }

    void Start()
    {
        autosave = false;
        saveOptions = new List<string>() { "Save 1", "Save 2", "Save 3" };
        file = saveOptions[0];
        tgScript = transform.GetComponent<townGeneration>();
    }

    private void OnApplicationQuit()
    {
        if (autosave)
        {
            saveObjects();
        }
    }
    private void writeCSV(string data)
    {
        setFile();
        string path = $"Assets/Files/{file}.csv";
        using (StreamWriter writer = File.AppendText(path))
        {
            writer.WriteLine(data);
        }
    }
    public void readCSV()
    {
        setFile();
        string path = $"Assets/Files/{file}.csv";
        string[] data = File.ReadAllLines(path);
        if (data.Length != 0)
        {
            // table size is data length divided by number of columns, then -1 to ignore the first row of headings
            int tableSize = data.Length;
            objectInformation = new ObjectInformation[tableSize-1];

            for (int i = 1; i < tableSize; i++)
            {
                string[] dataSubset = data[i].Split(",");
                objectInformation[i-1] = new ObjectInformation();
                objectInformation[i-1].name = dataSubset[0];
                objectInformation[i-1].positionX = float.Parse(dataSubset[1]);
                objectInformation[i-1].positionY = float.Parse(dataSubset[2]);
                objectInformation[i-1].positionZ = float.Parse(dataSubset[3]);

                objectInformation[i-1].rotationX = float.Parse(dataSubset[4]);
                objectInformation[i-1].rotationY = float.Parse(dataSubset[5]);
                objectInformation[i-1].rotationZ = float.Parse(dataSubset[6]);
            }
        }
    }
    private void clearCSV()
    {
        print("Clearing Previous Save...");
        string path = $"Assets/Files/{file}.csv";
        File.WriteAllText(path, "");
        print("Save Cleared.");
    }

    public void setFile()
    {
        file = saveOptions[saveOptionsDropdown.value];
    }
    public void saveObjects()
    {
        print("Saving...");
        setFile();
        clearCSV();
        GameObject[] allBuldings = GameObject.FindGameObjectsWithTag("Building");
        GameObject[] allTrees = GameObject.FindGameObjectsWithTag("Tree");
        GameObject[] allExtras = GameObject.FindGameObjectsWithTag("Details");
        GameObject[] allWalls = GameObject.FindGameObjectsWithTag("Walls");
        List<GameObject[]> allObjs = new List<GameObject[]>() {allBuldings, allTrees, allExtras, allWalls};

        writeCSV("name,positionX,positionY,positionZ,rotationX,rotationY,rotationZ");
        foreach(GameObject[] objArr in allObjs)
        {
            foreach (GameObject obj in objArr)
            {
                Vector3 pos = obj.transform.position;
                Vector3 rot = obj.transform.rotation.eulerAngles;
                string objInfoString = $"{obj.name},{pos.x},{pos.y},{pos.z},{rot.x},{rot.y},{rot.z}";
                writeCSV(objInfoString);
            }
        }

        Terrain t = Terrain.activeTerrain;
        

        foreach(Vector3 tilePos in allTilePositions)
        {
            string namePosString = $"Floor,{tilePos.x},{tilePos.y},{tilePos.z},{0},{0},{0}";
            writeCSV(namePosString);
        }
        print("Save Complete.");
    }

    public void loadObjects()
    {
        print("loading...");
        tgScript.killMap();
        GameObject[] allBuldings = GameObject.FindGameObjectsWithTag("Building");
        GameObject[] allTrees = GameObject.FindGameObjectsWithTag("Tree");
        GameObject[] allExtras = GameObject.FindGameObjectsWithTag("Details");
        GameObject[] allWalls = GameObject.FindGameObjectsWithTag("Walls");
        List<GameObject[]> allObjs = new List<GameObject[]>() { allBuldings, allTrees, allExtras, allWalls };
        foreach(GameObject[] objs in allObjs)
        {
            foreach(GameObject obj in objs)
            {
                Destroy(obj);
            }
        }

        GameObject[] objectPrefabs = tgScript.objectPrefabs;
        Dictionary<string, GameObject> obDict = new Dictionary<string, GameObject>();
        allTilePositions = new List<Vector3>();
        foreach (GameObject obj in objectPrefabs)
        {
            obDict.Add(obj.name, obj);
        }

        readCSV();
        print(objectInformation.Length);
        foreach(var ob in objectInformation)
        {
            string n = ob.name.Replace("(Clone)", "");
            if (n == "Floor")
            {
                allTilePositions.Add(new Vector3(ob.positionX, ob.positionY, ob.positionZ));
                tgScript.allTilePositions.Add(new Vector3(ob.positionX, ob.positionY, ob.positionZ));
            }
            else
            {
                GameObject tempOb = obDict[n];
                Vector3 tempPos = new Vector3(ob.positionX, ob.positionY, ob.positionZ);
                Quaternion tempRot = Quaternion.Euler(ob.rotationX, ob.rotationY, ob.rotationZ);
                Instantiate(tempOb, tempPos, tempRot);
            }
        }
        tgScript.terrainPainter(allTilePositions);
        gameObject.GetComponent<playerMovement>().invokeChangePlayer();
        print("Load Complete.");
    }
}
