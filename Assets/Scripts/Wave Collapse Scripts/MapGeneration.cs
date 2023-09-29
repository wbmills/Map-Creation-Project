using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


// Map Gen 2.0 
public class Road
{
    public int id;
    public Vector3 centre;
    public List<GameObject> buildings = new List<GameObject>();
    public GameObject[] walls;
    public GameObject[] details;
    public bool hasGround;

    public Vector3 pointA;
    public Vector3 pointB;

    // width and length of road, where 
    public float width;
    public float length;
}
public class MapGeneration : MonoBehaviour
{
    private Terrain mapTerrain;
    private Vector3 terrainCentre;
    private List<GameObject> allObjectsInMap;
    private List<Road> allRoads;

    void Start()
    {
        SetMapTerrain();
        allRoads = new List<Road>();
        allObjectsInMap = new List<GameObject>();
        //GenerateMap(1f, 1f, 1f, 10f, 5f, 10f, 5f);
        
        GenerateMap();
    }

    void Update()
    {
        
    }

    // Generate map (in progress)
    /*private void GenerateMap(float mapDensity, float uniformity, float roadAngularity, float maxRoadWidth,
        float minRoadWidth, float maxRoadLength, float minRoadLength, Vector3 centre = default, float randomness = 0f, float angleResolution = 1f, string theme = "Default")*/
    private void GenerateMap()
    {
        string theme = "default";
        Vector3 centre = new Vector3(terrainCentre.x, 0, 0);
        float maxRoadLength = 20f;
        float maxRoadWidth = 5f;
        GameObject[] obsOfTheme = Resources.LoadAll<GameObject>($"Map Generation/{theme}");
        if (!CheckCompletePrefabSet(theme, obsOfTheme))
        {
            throw new Exception("Not all the required prefabs are present");
        }

        int i = 0;
        bool end = false;
        float curPos = centre.z;

        while (!end && i < 100)
        {
            if (allRoads.Count == 0)
            {
                Road initRoad = new Road();
                initRoad.centre = centre;
                initRoad.pointB = new Vector3(centre.x, centre.y, centre.z + (maxRoadLength / 2));
                initRoad.pointA = new Vector3(centre.x, centre.y, centre.z - (maxRoadLength / 2));
                allRoads.Add(initRoad);
            }

            Road tempRoad = new Road();
            tempRoad.pointA = allRoads[allRoads.Count-1].pointB;
            tempRoad.centre = new Vector3(tempRoad.pointA.x, tempRoad.pointA.y, tempRoad.pointA.z + (maxRoadLength / 2));
            tempRoad.pointB = new Vector3(tempRoad.centre.x, tempRoad.centre.y, tempRoad.centre.z + (maxRoadLength / 2));
            tempRoad.length = maxRoadLength;
            tempRoad.width = maxRoadWidth;

            if (tempRoad.pointB.z + maxRoadLength >= mapTerrain.terrainData.size.z)
            {
                end = true;
            }
            SetRoadObjects(tempRoad, 1, .5f, "Building", obsOfTheme);
            allRoads.Add(tempRoad);
            curPos += maxRoadLength * Random.Range(10, 100) * 0.01f;
            i++;
        }
        print(allRoads.Count);
    }

    // the lower uniformity, the more equal distance objects will be from each other
    // the lower density, the further away objects will be 
    private void SetRoadObjects(Road road, float uniformity, float density, string objectTag, GameObject[] allPrefabs)
    {
        density = 1 - density;
        uniformity = 1 - uniformity;
        float minDistance = 20;
        GameObject wall = null;
        List<GameObject> prefabsOfTag = new List<GameObject>();
        foreach (GameObject ob in allPrefabs)
        {
            if (ob.tag == objectTag)
            {
                prefabsOfTag.Add(ob);
            }
            else if (ob.tag == "Walls")
            {
                wall = ob;
            }
        }

        Vector3 initPoint = road.pointA;
        Vector3 lastPoint = road.pointA;
        float distanceBetweenObjects = 0;
        int i = 0;
        while (i < 100 && lastPoint.z + 5 < road.pointB.z)
        {
            distanceBetweenObjects = minDistance * (density * (1 + Random.Range(0, uniformity) * 10));
            print(distanceBetweenObjects);
            GameObject tempOb = prefabsOfTag[Random.Range(0, prefabsOfTag.Count)];
            float newZPos = lastPoint.z + distanceBetweenObjects + tempOb.GetComponent<Renderer>().bounds.size.z;
            float newYPos = mapTerrain.SampleHeight(new Vector3(lastPoint.x, lastPoint.y, newZPos));
            tempOb.transform.position = new Vector3(lastPoint.x, newYPos + (tempOb.GetComponent<Renderer>().bounds.size.y / 2), newZPos);
            Vector3 newLastPoint = new Vector3(lastPoint.x, lastPoint.y, lastPoint.z + distanceBetweenObjects + (tempOb.GetComponent<Renderer>().bounds.size.z));
            BuildWall(wall, lastPoint, newLastPoint);
            lastPoint = newLastPoint;
            road.buildings.Add(tempOb);
            Instantiate(tempOb);
        }
    }

    private void DebugShowRoads()
    {

    }

    private void OnDrawGizmos()
    {
        if (allRoads != null)
        {
            foreach (Road r in allRoads)
            {
                Gizmos.DrawCube(r.pointA, Vector3.one);
                Gizmos.DrawCube(r.pointB, Vector3.one);
            }
        }
    }

    private bool CheckCompletePrefabSet(string theme, GameObject[] prefabsOfTheme)
    {
        Dictionary<string, int> requiredPrefabs = new Dictionary<string, int>() {
            {"Building", 0 },
            {"Walls", 0 } 
        };

        foreach(GameObject ob in prefabsOfTheme)
        {
            if (requiredPrefabs.ContainsKey(ob.tag))
            {
                requiredPrefabs[ob.tag] += 1;
            }
        }

        foreach (KeyValuePair<string, int> pair in requiredPrefabs)
        {
            if (pair.Value == 0)
            {
                return false;
            }
        }

        return true;
    }

    // empty scene, including terrain if killTerrain = true
    private void KillMap(bool killTerrain = false)
    {
        foreach(GameObject ob in allObjectsInMap)
        {
            Destroy(ob);
            allObjectsInMap.Remove(ob);
        }

        if (killTerrain)
        {
            Destroy(mapTerrain.gameObject);
            SetMapTerrain(null);
        }
    }

    // set terrain to first active terrain in scene, specified terrain, or nothing.
    private void SetMapTerrain(GameObject newTerrain=null)
    {
        GameObject[] allTerrains = GameObject.FindGameObjectsWithTag("Terrain");
        if (newTerrain == null && allTerrains.Length > 0)
        {
            mapTerrain = allTerrains[0].GetComponent<Terrain>();
            terrainCentre = new Vector3(mapTerrain.transform.position.x + (mapTerrain.terrainData.size.x / 2),
                mapTerrain.transform.position.y, mapTerrain.transform.position.z + (mapTerrain.terrainData.size.z / 2));
        }
        else if (newTerrain != null)
        {
            mapTerrain = newTerrain.GetComponent<Terrain>();
            terrainCentre = new Vector3(mapTerrain.transform.position.x + (mapTerrain.terrainData.size.x / 2),
                mapTerrain.transform.position.y, mapTerrain.transform.position.z + (mapTerrain.terrainData.size.z / 2));
        }
        else
        {
            mapTerrain = null;
        }
    }

    // Instantiate new GameObject, applying necessary extra steps for organisation and function
    private GameObject CreateNewObject(GameObject obj, Vector3 pos, Quaternion rot, string tagTemp)
    {
        if (tagTemp == null)
        {
            tagTemp = obj.tag;
        }
        GameObject tempObj = Instantiate(obj, pos, rot);
        tempObj.tag = tagTemp;
        allObjectsInMap.Add(tempObj);
        return tempObj;
    }

    // instantiate wall between two points, setting wall scale and angle to fit exactly between points
    private void BuildWall(GameObject wallObject, Vector3 left, Vector3 right)
    {
        GameObject tempObj = CreateNewObject(wallObject, Vector3.zero, Quaternion.Euler(0, 0, 0), null);
        tempObj.transform.Translate(Vector3.up * mapTerrain.SampleHeight(tempObj.transform.position));
        GameObject leftBounds = new GameObject();
        leftBounds.transform.position = left;
        GameObject rightBounds = new GameObject();
        rightBounds.transform.position = right;

        // sqrt((x1 - x2)^2 + (z1 - z2)^2) = length of wall
        float tempX = Mathf.Sqrt(squareNum(rightBounds.transform.position.x - leftBounds.transform.position.x) + squareNum(rightBounds.transform.position.z - leftBounds.transform.position.z));

        tempObj.transform.localScale = new Vector3(tempX, tempObj.transform.localScale.y, tempObj.transform.localScale.z);

        Vector3 difference = (leftBounds.transform.position - rightBounds.transform.position) / 2;
        Vector3 newPos = new Vector3(rightBounds.transform.position.x + difference.x, rightBounds.transform.position.y, rightBounds.transform.position.z + difference.z);
        tempObj.transform.position = newPos;
        Vector3 dir = leftBounds.transform.position - rightBounds.transform.position;
        var rot = Quaternion.LookRotation(dir, Vector3.up);
        tempObj.transform.rotation = rot;
        tempObj.transform.Rotate(0, 90, 0);
    }

    // method to square a given number, returning num * num
    private float squareNum(float num)
    {
        return num * num;
    }
}
