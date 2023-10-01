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
        Vector3 init_point = new Vector3(50f, 2.5f, 20);
        float maxRoadLength = 60f;
        float maxRoadWidth = 5f;
        GameObject[] obsOfTheme = Resources.LoadAll<GameObject>($"Map Generation/{theme}");
        if (!CheckCompletePrefabSet(theme, obsOfTheme))
        {
            throw new Exception("Not all the required prefabs are present");
        }

        int i = 0;
        bool end = false;
        float curPos = init_point.z;

        while (!end && i < 100)
        {

            Road tempRoad = new Road();
            if (allRoads.Count == 0)
            {
                tempRoad.pointA = init_point;
            }
            else
            {
                tempRoad.pointA = allRoads[allRoads.Count - 1].pointB;
            }
            
            tempRoad.centre = new Vector3(tempRoad.pointA.x, tempRoad.pointA.y, tempRoad.pointA.z + (maxRoadLength / 2));
            tempRoad.pointB = new Vector3(tempRoad.centre.x, tempRoad.centre.y, tempRoad.centre.z + (maxRoadLength / 2));
            tempRoad.length = maxRoadLength;
            tempRoad.width = maxRoadWidth;

            if (tempRoad.pointB.z + maxRoadLength >= mapTerrain.terrainData.size.z)
            {
                end = true;
            }
            SetRoadObjects(tempRoad, .7f, .9f, "Building", obsOfTheme);
            allRoads.Add(tempRoad);
            curPos += maxRoadLength * Random.Range(10, 100) * 0.01f;
            i++;
            //end = true;
        }
    }


    // the lower uniformity, the more equal distance objects will be from each other
    // the lower density, the further away objects will be 
    private void SetRoadObjects(Road road, float uniformity, float density, string objectTag, GameObject[] allPrefabs)
    {
        density = 1 - density;
        uniformity = 1 - uniformity;
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

        Vector3 initPoint = road.pointA; // set initial position to the beginning of the first road in the array
        float distanceBetweenObjects = (road.length * density);
        Vector3 furthestBoundLeft = new Vector3(road.pointA.x, road.pointA.y, road.pointA.z + distanceBetweenObjects); // last point is the furthest point where there is an object placed on the road
        int i = 0; // iterate to prevent accidental 'forever loop'
        while (i < 100 && furthestBoundLeft.z < road.pointB.z)
        {
            RaycastHit furthestPointInfo;
            Physics.Raycast(road.pointB, Vector3.back, out furthestPointInfo, maxDistance: road.length);
            if (furthestPointInfo.point == Vector3.zero)
            {
                Vector3 point = road.pointA;
                furthestBoundLeft = new Vector3(point.x, point.y, point.z + distanceBetweenObjects);
            }
            else
            {
                Vector3 point = furthestPointInfo.point;
                furthestBoundLeft = new Vector3(point.x, point.y, point.z + distanceBetweenObjects);
                if (furthestBoundLeft.z > road.pointB.z)
                {
                    break;
                }
            }

            // set new object and its position
            GameObject newOb = prefabsOfTag[Random.Range(0, prefabsOfTag.Count)];
            float newYPos = mapTerrain.SampleHeight(furthestBoundLeft);
            newOb.transform.position = furthestBoundLeft;
            //newOb.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
            road.buildings.Add(newOb);
            Instantiate(newOb);
            BuildWall(wall, newOb, road);
            i++;
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
                //Gizmos.DrawCube(r.centre, Vector3.one);
                Gizmos.DrawSphere(r.pointB, 1f);
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
    private void BuildWall(GameObject wallObject, GameObject ob, Road road)
    {
        // set left and right bound values
        Vector3 leftBounds;
        Vector3 rightBounds;
        RaycastHit check = new RaycastHit();
        Physics.Raycast(ob.transform.position, Vector3.back, out check);
        if (isCollisionAChild(check.collider, ob))
        {
            return;
        }
        else if (check.collider && check.collider.gameObject != ob.gameObject)
        {
            leftBounds = check.point;
        }
        else
        {
            leftBounds = road.pointA;
        }

        Physics.Raycast(leftBounds, Vector3.forward, out check);
        if (check.collider)
        {
            rightBounds = check.point;
        }
        else
        {
            return;
            //rightBounds = road.pointB;
        }

        GameObject tempObj = CreateNewObject(wallObject, Vector3.zero, Quaternion.Euler(0, 0, 0), null);
        tempObj.transform.Translate(Vector3.up * mapTerrain.SampleHeight(tempObj.transform.position));

        // sqrt((x1 - x2)^2 + (z1 - z2)^2) = length of wall
        float tempX = Mathf.Sqrt(squareNum(rightBounds.x - leftBounds.x) + squareNum(rightBounds.z - leftBounds.z));

        tempObj.transform.localScale = new Vector3(tempX, tempObj.transform.localScale.y, tempObj.transform.localScale.z);

        Vector3 difference = (leftBounds - rightBounds) / 2;
        Vector3 newPos = new Vector3(rightBounds.x + difference.x, rightBounds.y, rightBounds.z + difference.z);
        tempObj.transform.position = newPos;
        Vector3 dir = leftBounds - rightBounds;
        var rot = Quaternion.LookRotation(dir, Vector3.up);
        tempObj.transform.rotation = rot;
        tempObj.transform.Rotate(0, 90, 0);
    }

    // method to square a given number, returning num * num
    private float squareNum(float num)
    {
        return num * num;
    }

    private bool isCollisionAChild(Collider c, GameObject ob)
    {
        if (!c)
        {
            return true;
        }
        foreach(Transform t in ob.GetComponentInChildren<Transform>())
        {
            if (t.gameObject == c.gameObject)
            {
                return true;
            }
        }
        return false;
    }
}
