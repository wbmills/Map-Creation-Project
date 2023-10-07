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
    public Vector3 direction;
    public Road prev;
    public Road pointer;

    // width and length of road, where 
    public float width;
    public float length;

    public List<Vector3> road = new List<Vector3>();
}

public class MapConfig
{
    public Terrain terrain;
    public float density;
    public float uniformity;
    public string theme;
    public float maxRoadLength;
    public float maxRoadWidth;
    public float minRoadLength;
    public float minRoadWidth;
    public GameObject[] obsOfTheme;
    public Vector3 initPoint;
}

public class MapGeneration : MonoBehaviour
{
    private Terrain mapTerrain;
    private Vector3 terrainCentre;
    private List<GameObject> allObjectsInMap;
    private List<Road> allRoads;
    private GameObject[] obsOfTheme;
    private MapConfig curConfig;

    void Start()
    {
        SetMapTerrain();
        SetDefaults();
        obsOfTheme = GetObjectsOfTheme("default");
        curConfig = SetCurrentMapConfig();
        GenerateMap();
    }

    private void ResetTerrainPaint()
    {
        foreach(Road r in allRoads)
        {
            PaintTerrain(r.road, true);
        }
    }

    private void SetDefaults()
    {
        allRoads = new List<Road>();
        allObjectsInMap = new List<GameObject>();
    }

    void Update()
    {
        
    }

    // Generate map (in progress)
    /*private void GenerateMap(float mapDensity, float uniformity, float roadAngularity, float maxRoadWidth,
        float minRoadWidth, float maxRoadLength, float minRoadLength, Vector3 centre = default, float randomness = 0f, float angleResolution = 1f, string theme = "Default")*/
    private void GenerateMap()
    {
        string theme = curConfig.theme;
        float maxRoadLength = curConfig.maxRoadLength;
        float maxRoadWidth = curConfig.maxRoadWidth;
        float minRoadWidth = curConfig.minRoadWidth;
        float minRoadLength = curConfig.minRoadLength;
        GameObject[] allObjects = GetObjectsOfTheme(theme);
        List<Vector3> directionOptions = new List<Vector3>() { Vector3.forward, Vector3.right, Vector3.left };

        int i = 0;
        bool end = false;
        float maxRoads = 20;
        Vector3 newDir;
        while (!end && allRoads.Count < maxRoads && i < 100)
        {
            newDir = directionOptions[Random.Range(0, directionOptions.Count)];
            float newWidth = Random.Range(minRoadWidth, maxRoadWidth);
            float newLength = Random.Range(minRoadLength, maxRoadLength);

            GenerateRoad(20, 60, newDir);
            i++;
        }
        print($"Road Count: {allRoads.Count}");
    }


    private List<Vector3> SetRoadVectors(Road road)
    {
        List<Vector3> positionsToPaint = new List<Vector3>();
        
        int tries = 0;
        if (road.prev != null)
        {
            Vector3 prevRoadWithWidth = road.prev.pointB + (road.prev.direction * (road.width / 2));
            positionsToPaint.Add(prevRoadWithWidth);
        }
        else
        {
            positionsToPaint.Add(road.pointA);
        }

        while (tries < 200 && (road.pointB - positionsToPaint[positionsToPaint.Count - 1]).normalized == road.direction)
        {
            positionsToPaint.Add(positionsToPaint[positionsToPaint.Count - 1] + (road.direction * 2));
        }
        positionsToPaint.Add(positionsToPaint[positionsToPaint.Count - 1] + (road.direction * 2));

        return positionsToPaint;
    }

    private void OnApplicationQuit()
    {
        ResetTerrainPaint();
    }

    private GameObject[] GetObjectsOfTheme(string theme)
    {
        GameObject[] obsOfTheme = Resources.LoadAll<GameObject>($"Map Generation/{theme}");
        if (!CheckCompletePrefabSet(theme, obsOfTheme))
        {
            throw new Exception("Not all the required prefabs are present");
        }
        return obsOfTheme;
    }

    private MapConfig SetCurrentMapConfig()
    {
        MapConfig tempConfig = new MapConfig();
        tempConfig.minRoadWidth = 20f;
        tempConfig.maxRoadWidth = 60f;
        tempConfig.minRoadLength = 40f;
        tempConfig.maxRoadLength = 80f;
        tempConfig.theme = "default";
        tempConfig.initPoint = new Vector3(60f, 0f, 60f);
        return tempConfig;
    }

    public void PaintTerrain(List<Vector3> positionsToPaint, bool reset=false)
    {
        Terrain terrain = mapTerrain;
        foreach (Vector3 tile in positionsToPaint)
        {
            Vector3 terrainPos = tile - terrain.transform.position;
            Vector3 mapPos = new Vector3(terrainPos.x / terrain.terrainData.size.x, 0, terrainPos.z / terrain.terrainData.size.z);
            float xCoord = mapPos.x * terrain.terrainData.alphamapWidth;
            float zCoord = mapPos.z * terrain.terrainData.alphamapHeight;
            int posX = (int)xCoord;
            int posZ = (int)zCoord;
            if (posX > 0 && posX < terrain.terrainData.alphamapWidth && posZ > 0 && posZ < terrain.terrainData.alphamapHeight)
            {
                int c = 7;
                float[,,] splatmapData = terrain.terrainData.GetAlphamaps(posX, posZ, c, c);
                for (int y = 0; y < c; y++)
                {
                    for (int x = 0; x < c; x++)
                    {
                        if (reset)
                        {
                            splatmapData[x, y, 0] = 1;
                            splatmapData[x, y, 1] = 0;
                        }
                        else
                        {
                            splatmapData[x, y, 0] = 0;
                            splatmapData[x, y, 1] = 1;
                        }
                    }
                }
                terrain.terrainData.SetAlphamaps(posX, posZ, splatmapData);
            }
        }
    }

    private Road GenerateRoad(float maxRoadWidth, float maxRoadLength, Vector3 relativeDirection, bool blank=false)
    {
        Road tempRoad = new Road();
        Vector3 newDir = relativeDirection;
        Vector3 newPointA;
        Road prevRoad = null;
        if (allRoads.Count == 0)
        {
            newPointA = curConfig.initPoint;
        }
        else
        {
            prevRoad = allRoads[allRoads.Count - 1];
            Vector3 prevRoadDir = (prevRoad.pointB - prevRoad.pointA).normalized;
            if (newDir != prevRoadDir)
            {
                newPointA = prevRoad.pointB + (prevRoadDir * (maxRoadWidth / 2)) + (newDir * (prevRoad.width / 2));
            }
            else
            {
                newPointA = allRoads[allRoads.Count - 1].pointB;
            }
            allRoads[allRoads.Count - 1].pointer = tempRoad;
        }

        tempRoad.pointA = newPointA;
        tempRoad.centre = tempRoad.pointA + newDir * (maxRoadLength / 2);
        tempRoad.pointB = tempRoad.centre + newDir * (maxRoadLength / 2);
        tempRoad.length = maxRoadLength;
        tempRoad.width = maxRoadWidth;
        tempRoad.direction = (tempRoad.pointB - tempRoad.pointA).normalized;
        tempRoad.pointer = null;
        tempRoad.prev = prevRoad;
        tempRoad.road = SetRoadVectors(tempRoad);

        if (!blank && checkInTerrain(tempRoad))
        {
            SetRoadObjects(tempRoad, .7f, .9f, "Building", obsOfTheme);
            PaintTerrain(tempRoad.road);
            allRoads.Add(tempRoad);
        }
        else if (blank)
        {
            allRoads.Add(tempRoad);
        }
        else
        {
            tempRoad = null;
        }

        return tempRoad;
    }

    private bool checkInTerrain(Road road)
    {
        Terrain t = GameObject.FindFirstObjectByType<Terrain>();
        Bounds bounds = t.terrainData.bounds;
        Vector3 roadDir = (road.pointB - road.pointA).normalized;
        List<Vector3> allPoints = new List<Vector3>(){ road.pointA, road.pointB, road.pointA + (roadDir * road.width/2),
        road.pointA - (roadDir * road.width/2), road.pointB + (roadDir * road.width / 2), road.pointB - (roadDir * road.width / 2)};
        
        foreach (Vector3 v in allPoints)
        {
            if (!bounds.Contains(new Vector3(v.x, 0, v.z)))
            {
                return false;
            }
        }
        return true;
    }

    // still deciding how 'modular' this approach would be 
    private GameObject[] GetGameObjectOfType(string objectType)
    {
        if (obsOfTheme == null)
        {
            return null;
        }
        else
        {
            return null;
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
        Vector3 direction = (road.pointB - road.pointA).normalized;
        float angleFromCentre = Vector3.Angle(Vector3.forward, direction) * (Mathf.PI / 180);
        float addX = road.width * Mathf.Sin(angleFromCentre + (90 * (Mathf.PI / 180)));
        float addY = road.width * Mathf.Cos(angleFromCentre + (90 * (Mathf.PI / 180)));
        Vector3 pointALeft = new Vector3(road.pointA.x + addX, road.pointA.y, road.pointA.z + addY);
        Vector3 pointARight = new Vector3(road.pointA.x - addX, road.pointA.y, road.pointA.z - addY);

        Vector3[] sides = new Vector3[2] {pointALeft, pointARight };
        foreach(Vector3 side in sides)
        {
            Vector3 relativeSideA = side;
            Vector3 relativeSideB = relativeSideA + (direction * road.length);
            Vector3 initPoint = relativeSideA; // set initial position to the beginning of the first road in the array
            float distanceBetweenObjects = (road.length * density);
            //Vector3 furthestBoundLeft = new Vector3(road.pointA.x, road.pointA.y, road.pointA.z + distanceBetweenObjects); // last point is the furthest point where there is an object placed on the road
            Vector3 furthestBoundLeft = relativeSideA + (distanceBetweenObjects * direction);
            int i = 0; // iterate to prevent accidental 'forever loop'

            while (i < 100 && (relativeSideB - furthestBoundLeft).normalized == direction)
            {
                RaycastHit furthestPointInfo;
                Physics.Raycast(relativeSideB, -direction, out furthestPointInfo, maxDistance: road.length);
                if (furthestPointInfo.point == Vector3.zero)
                {
                    Vector3 point = relativeSideA;
                    furthestBoundLeft = point + (distanceBetweenObjects * direction);
                }
                else
                {
                    Vector3 point = furthestPointInfo.point;
                    furthestBoundLeft = point + (distanceBetweenObjects * direction); ;
                    if ((relativeSideB - furthestBoundLeft).normalized != direction)
                    {
                        break;
                    }
                }

                // set new object and its position
                GameObject newOb = prefabsOfTag[Random.Range(0, prefabsOfTag.Count)];
                float newYPos = mapTerrain.SampleHeight(furthestBoundLeft);
                newOb.transform.position = furthestBoundLeft;
                Vector3 relDir = (side - road.pointA).normalized;
                newOb.transform.rotation = Quaternion.LookRotation(relDir, Vector3.up);
                //newOb.transform.Rotate(new Vector3(0, 90, 0));
                road.buildings.Add(newOb);
                Instantiate(newOb);
                BuildWall(wall, newOb, road);
                i++;
            }
        }
    }

    private void OnDrawGizmos()
    {
        RoadDebug();
    }

    private void RoadDebug()
    {
        if (allRoads != null)
        {
            foreach (Road r in allRoads)
            {
                Gizmos.DrawCube(r.pointA, Vector3.one);
                //Gizmos.DrawCube(r.centre, Vector3.one);
                Gizmos.DrawSphere(r.pointB, 1f);

                // x = r(cos(degrees�)), y = r(sin(degrees�)).
                Vector3 direction = (r.pointB - r.pointA).normalized;
                float angleFromCentre = Vector3.Angle(Vector3.forward, direction) * (Mathf.PI / 180);
                float addX = r.width * Mathf.Sin(angleFromCentre + (90 * (Mathf.PI / 180)));
                float addY = r.width * Mathf.Cos(angleFromCentre + (90 * (Mathf.PI / 180)));
                Vector3 pointALeft = new Vector3(r.pointA.x + addX, r.pointA.y, r.pointA.z + addY);
                Vector3 pointARight = new Vector3(r.pointA.x - addX, r.pointA.y, r.pointA.z - addY);
                Gizmos.DrawWireCube(pointALeft, Vector3.one * 2);
                Gizmos.DrawWireCube(pointARight, Vector3.one * 2);
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
        Vector3 direction = (road.pointB - road.pointA).normalized;
        Physics.Raycast(ob.transform.position, -direction, out check, maxDistance:Vector3.Distance(road.pointA, road.pointB));
        if (isCollisionAChild(check.collider, ob) | (road.pointA - check.point).normalized == direction)
        {
            return;
        }
        else if (check.collider && check.collider.gameObject != ob.gameObject && check.collider.transform.rotation == ob.transform.rotation)
        {
            leftBounds = check.point;
        }
        else
        {
            leftBounds = road.pointA;
        }

        Physics.Raycast(leftBounds, direction, out check);
        if (check.collider && check.collider.transform.rotation == ob.transform.rotation)
        {
            rightBounds = check.point;
        }
        else
        {
            return;
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
