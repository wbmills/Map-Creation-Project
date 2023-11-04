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

    public Vector3 road;
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
    private List<Vector3> unusedPoints;
    public bool debug = true;
    Vector3[,] allPoints;

    void Start()
    {
        SetMapTerrain();
        SetDefaults();
        if (debug)
        {
            NewMap();
        }
    }

    public void NewMap()
    {
        curConfig = SetCurrentMapConfig();
        obsOfTheme = GetObjectsOfTheme(curConfig.theme);
        GenerateMap();
    }

    private void ResetTerrainPaint()
    {
        foreach (Road r in allRoads)
        {
            PaintTerrain(r.road, r.direction, true);
        }
        
    }

    private void SetDefaults()
    {
        allRoads = new List<Road>();
        allObjectsInMap = new List<GameObject>();
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

        int i = 0;
        float maxRoads = 4000;
        Vector3 newDir;
        Road temp;

        int arrWidth = Mathf.RoundToInt((mapTerrain.terrainData.size.x - maxRoadLength)/maxRoadLength);
        int arrHeight = Mathf.RoundToInt((mapTerrain.terrainData.size.z - maxRoadLength) / maxRoadLength);
        allPoints = new Vector3[arrWidth, arrHeight];
        unusedPoints = new List<Vector3>();
        Vector3[,] unusedPoints2D = new Vector3[arrWidth, arrHeight];
        int w = 0;
        int h = 0;
        for (float x = maxRoadLength; x < mapTerrain.terrainData.size.x - maxRoadLength; x += maxRoadLength)
        {
            for (float z = maxRoadLength; z < mapTerrain.terrainData.size.z - maxRoadLength; z += maxRoadLength)
            {
                Vector3 newPos = new Vector3(x, 0, z);
                unusedPoints.Add(newPos);
                allPoints[w, h] = newPos;
                unusedPoints2D[w, h] = newPos;
                w += 1;
            }
            h += 1;
            w = 0;
        }
        curConfig.initPoint = unusedPoints[Random.Range(0, unusedPoints.Count)];
        unusedPoints.Remove(curConfig.initPoint);
        while (allRoads.Count < maxRoads && i < maxRoads && unusedPoints.Count > 2)
        {
            temp = null;
            float tempWidth = maxRoadWidth;

            Vector3 newPointA = Vector3.one;
            Vector3 newPointB = Vector3.one;
            List<Vector3> directionOptions = new List<Vector3>() { Vector3.forward, Vector3.right, Vector3.left };
            while (temp == null && directionOptions.Count > 0)
            {
                newDir = directionOptions[Random.Range(0, directionOptions.Count)];
                directionOptions.Remove(newDir);
                if (allRoads.Count == 0)
                {
                    newPointA = curConfig.initPoint;
                }
                else
                {
                    Road prevRoad = allRoads[allRoads.Count-1];
                    newPointA = prevRoad.pointB;
                }
               

                newPointB = newPointA + (newDir * maxRoadLength);
                temp = GenerateRoad(newPointA, newPointB, newDir);
                
                if ((temp == null && directionOptions.Count == 0) | !unusedPoints.Contains(newPointB))
                {
                    newPointA = unusedPoints[Random.Range(0, unusedPoints.Count)];
                    newPointB = newPointA + (newDir * maxRoadLength);
                    temp = GenerateRoad(newPointA, newPointB, newDir);
                }
                if (temp != null)
                {
                    PaintTerrain(temp.road, temp.direction);
                }
            }
            unusedPoints.Remove(newPointA);
            unusedPoints.Remove(newPointB);
            i++;
        }
/*        List<Vector3> finalPaint = new List<Vector3>();
        foreach (Road r in allRoads)
        {
            foreach(Vector3 v in r.road)
            {
                finalPaint.Add(v);
            } 
        }
        PaintTerrain(finalPaint);*/
        print($"Road Count: {allRoads.Count}");
    }

    private Vector3 SetRoadVectors(Road road)
    {

        float angleFromCentre = Vector3.Angle(Vector3.forward, road.direction) * (Mathf.PI / 180);
        float addX = road.width * Mathf.Sin(angleFromCentre + (90 * (Mathf.PI / 180)));
        float addZ = road.width * Mathf.Cos(angleFromCentre + (90 * (Mathf.PI / 180)));
        Vector3 rotateDir = new Vector3(Mathf.Sin(angleFromCentre + (90 * (Mathf.PI / 180))), 0, Mathf.Cos(angleFromCentre + (90 * (Mathf.PI / 180))));
        Vector3 A = new Vector3(road.pointA.x - addX, road.pointA.y, road.pointA.z - addZ);
        Vector3 B = new Vector3(road.pointA.x + addX, road.pointA.y, road.pointA.z + addZ);
        Vector3 C = new Vector3(road.pointB.x - addX, road.pointB.y, road.pointB.z - addZ);
        Vector3 D = new Vector3(road.pointB.x + addX, road.pointB.y, road.pointB.z + addZ);
        Vector3 smallest = A;
        foreach (Vector3 i in new List<Vector3>() { A, B, C, D })
        {
            if (i.x < smallest.x | i.z < smallest.z)
            {
                smallest = i;
            }
        }
        return smallest;
        /*        //Vector3 A = road.pointA;
                //Vector3 B = road.pointB;
                List<Vector3> positionsToPaint = new List<Vector3>();
                Vector3 tempStartPos = A;
                int scale = 6;
                positionsToPaint.Add(tempStartPos);
                while (((tempStartPos + (road.direction * road.length)) - positionsToPaint[positionsToPaint.Count - 1]).normalized == road.direction)
                {
                    positionsToPaint.Add(positionsToPaint[positionsToPaint.Count - 1] + (road.direction * scale));
                }*/
        /*
                for (float i = (road.width * 2); i > 0; i -= scale)
                {
                    tempStartPos = new Vector3(A.x, A.y, A.z) - (i * rotateDir);
                    positionsToPaint.Add(tempStartPos);
                    while (((tempStartPos + (road.direction * road.length)) - positionsToPaint[positionsToPaint.Count - 1]).normalized == road.direction)
                    {
                        positionsToPaint.Add(positionsToPaint[positionsToPaint.Count - 1] + (road.direction * scale));
                    }
                }*/
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
        tempConfig.maxRoadWidth = 5f;
        tempConfig.minRoadLength = 60f;
        tempConfig.maxRoadLength = 20f;
        tempConfig.theme = "Default";
        tempConfig.uniformity = .7f;
        tempConfig.density = 0f;
        tempConfig.initPoint = new Vector3(60f, 0f, 60f);
        return tempConfig;
    }

    public void PaintTerrain(Vector3 positionToPaint, Vector3 dir, bool reset=false)
    {
        Terrain terrain = mapTerrain;
        dir = new Vector3(MathF.Abs(dir.x), MathF.Abs(dir.y), MathF.Abs(dir.z));
        Vector3 tile = positionToPaint;

        Vector3 terrainPos = tile - terrain.transform.position;
        Vector3 mapPos = new Vector3(terrainPos.x / terrain.terrainData.size.x, 0, terrainPos.z / terrain.terrainData.size.z);
        float xCoord = mapPos.x * terrain.terrainData.alphamapWidth;
        float zCoord = mapPos.z * terrain.terrainData.alphamapHeight;
        int posX = (int)xCoord;
        int posZ = (int)zCoord;
        if (posX > 0 && posX < terrain.terrainData.alphamapWidth && posZ > 0 && posZ < terrain.terrainData.alphamapHeight)
        {
            //int c = Mathf.RoundToInt(curConfig.maxRoadWidth) * 2;
            int c = (int)curConfig.maxRoadWidth;
            int b = (int)curConfig.maxRoadLength;
            int xMax = (int)(c * dir.z) * 2 + (int)(b * dir.x);
            int yMax = (int)(c * dir.x) * 2 + (int)(b * dir.z);
            float[,,] splatmapData = terrain.terrainData.GetAlphamaps(posX, posZ, xMax, yMax);
            for (int y = 0; y < xMax; y++)
            {
                for (int x = 0; x < yMax; x++)
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

    private Road GenerateRoad(Vector3 pointA, Vector3 pointB, Vector3 relativeDirection, bool blank=false)
    {
        Road tempRoad = new Road();
        Vector3 newDir = relativeDirection;
        Road prevRoad = null;
        if (allRoads.Count > 0)
        {
            prevRoad = allRoads[allRoads.Count - 1];
            prevRoad.pointer = tempRoad;
        }

        tempRoad.pointA = pointA;
        tempRoad.pointB = pointB;
        tempRoad.length = Vector3.Distance(pointA, pointB);
        tempRoad.width = curConfig.maxRoadWidth;
        tempRoad.direction = (tempRoad.pointB - tempRoad.pointA).normalized;
        tempRoad.pointer = null;
        tempRoad.prev = prevRoad;
        tempRoad.road = SetRoadVectors(tempRoad);

        if (!blank && checkInTerrain(tempRoad))
        {
            SetRoadObjects(tempRoad, "Building", obsOfTheme, false);
            //PaintTerrain(tempRoad.road);
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

    public Terrain GetCurrentTerrain()
    {
        return mapTerrain;
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
    private void SetRoadObjects(Road road, string objectTag, GameObject[] allPrefabs, bool generateWalls=false)
    {
        float density = 1 - curConfig.density;
        float uniformity = 1 - curConfig.uniformity;
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
            if (density == 1 && generateWalls)
            {
                BuildWallSimple(wall, relativeSideA, relativeSideB);
            }
            else
            {
                int i = 0; // iterate to prevent accidental 'forever loop'
                GameObject lastOb = null;
                while (i < 5 && (relativeSideB - furthestBoundLeft).normalized == direction)
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
                    if (lastOb == null | (lastOb && lastOb.transform.position != newOb.transform.position))
                    {
                        road.buildings.Add(newOb);
                        Instantiate(newOb);
                        if (generateWalls)
                        {
                            BuildWall(wall, newOb, road);
                        }
                        lastOb = newOb;
                    }
                    i++;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        RoadDebug();
        //pointsDebug();
    }

    private void pointsDebug()
    {
        if (allPoints != null)
        {
            foreach (Vector3 point in allPoints)
            {
                Gizmos.DrawWireCube(new Vector3(point.x, 10, point.z), Vector3.one * 2);
            }
        }
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

                // x = r(cos(degrees°)), y = r(sin(degrees°)).
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
        ResetTerrainPaint();
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


    private void BuildWallSimple(GameObject wallObject, Vector3 a, Vector3 b)
    {
        Vector3 leftBounds;
        Vector3 rightBounds;
        leftBounds = a;
        rightBounds = b;

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
            //leftBounds = defaultPoint;
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
