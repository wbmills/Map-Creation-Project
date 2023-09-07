using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class townGeneration : MonoBehaviour
{
    // input fields for menu
    public TMP_InputField houseNum;
    public TMP_InputField treeNum;
    public TMP_InputField extrasNum;
    public TMP_InputField roadSize;
    public GameObject emptyPrefab;

    public List<Vector3> allTilePositions;

    private List<List<Vector3>> allRays;

    // GameObjects used in object spawning and road generation
    public GameObject floorPrefab;
    public GameObject currentTile;
    public GameObject curObjectSpawning;
    private GameObject objectPlacer;
    private GameObject finalFloor;

    //ray for house rotation
    private Ray houseToRoadRay;
    private RaycastHit htrrInfo;

    // Road spawning: ray, ray information, and ray direction
    private Ray tileRay;
    private RaycastHit tileRayHit;
    private Vector3 currentRayDir;

    // all rays for object spawning 
    private Ray rayX1;
    private Ray rayX2;
    private Ray rayZ1;
    private Ray rayZ2;

    // all ray information for object spawning
    private RaycastHit[] rayHitX1;
    private RaycastHit[] rayHitX2;
    private RaycastHit[] rayHitZ1;
    private RaycastHit[] rayHitZ2;

    // Terrain, upper and lower bounds of X and Z terrain size
    public Terrain terrain;
    private List<float> boundsX;
    private List<float> boundsZ;

    // the size of the object to be spawned and the list of objects that can be spawned
    public GameObject[] objectPrefabs;
    private Vector3 objectBounds;
    private List<GameObject> allObjects;
    

    void Start()
    {
        allObjects = new List<GameObject>();
        allRays = new List<List<Vector3>>();
        objectPlacer = GameObject.FindGameObjectWithTag("Object Placer");

        // getting terrain bounds
        terrain = GameObject.FindAnyObjectByType<Terrain>();
        boundsX = new List<float> {terrain.transform.position.x, terrain.transform.position.x + terrain.terrainData.size.x };
        boundsZ = new List<float> {terrain.transform.position.z, terrain.transform.position.z + terrain.terrainData.size.z };
        

        // set default values for menu, or create new ones if none exist
        if (PlayerPrefs.HasKey("houseNum"))
        {
            int[] prefs = getPlayerPrefs();
            houseNum.text = prefs[0].ToString();
            treeNum.text = prefs[1].ToString();
            extrasNum.text = prefs[2].ToString();
            roadSize.text = prefs[3].ToString();
        }
        else
        {
            setPlayerPrefs(0, 0, 0, 0);
        }
    }
    public void killMap()
    {
        float[,,] splatmapData = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        for (int y = 0; y < terrain.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrain.terrainData.alphamapWidth; x++)
            {
                splatmapData[x, y, 0] = 1;
                splatmapData[x, y, 1] = 0;
            }
        }
        terrain.terrainData.SetAlphamaps(0, 0, splatmapData);

        foreach (GameObject obj in allObjects)
        {       
                Destroy(obj);
        }
    }

    public void addToAllObjects(GameObject obj)
    {
        allObjects.Add(obj);
    }
    private int[] getPlayerPrefs()
    {
        int[] all = new int[] { PlayerPrefs.GetInt("houseNum"), PlayerPrefs.GetInt("treeNum"),
        PlayerPrefs.GetInt("extrasNum"), PlayerPrefs.GetInt("roadSize")};

        return all;
    }

    private void setPlayerPrefs(int houseNum, int treeNum, int extrasNum, int roadSize)
    {
        PlayerPrefs.SetInt("houseNum", houseNum);
        PlayerPrefs.SetInt("treeNum", treeNum);
        PlayerPrefs.SetInt("extrasNum", extrasNum);
        PlayerPrefs.SetInt("roadSize", roadSize);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        killMap();
    }
    public void generate()
    {
        killMap();
        allTilePositions = new List<Vector3>();
        int houseNumString = Int32.Parse(houseNum.text);
        int treeNumString = Int32.Parse(treeNum.text);
        int extrasNumString = Int32.Parse(extrasNum.text);
        int roadSizeString = Int32.Parse(roadSize.text);
        setPlayerPrefs(houseNumString, treeNumString, extrasNumString, roadSizeString);

        spawnObjects(houseNumString, houseNumString, "Building");
        spawnWhereSpace("Tree", treeNumString);
        spawnWhereSpace("Details", extrasNumString);
        generateRoad(roadSizeString);
        rotateBuildings();
        spawnWalls(objectPrefabs[2], 1f);

        //finalFloor.SetActive(false);
        gameObject.GetComponent<playerMovement>().invokeChangePlayer();
    }

    void Update()
    {
        drawObjectRay(objectPrefabs[0], Quaternion.identity);
    }


    private void drawRoadRay()
    {
        currentRayDir = currentTile.transform.forward * currentTile.GetComponent<Renderer>().bounds.size.x;
        tileRay = new Ray(currentTile.transform.position, currentRayDir);
        Physics.Raycast(tileRay, out tileRayHit, LayerMask.GetMask("Floor"));
    }

    private void drawObjectRay(GameObject obj, Quaternion rotation)
    {
        Vector3 bounds = obj.GetComponent<Renderer>().bounds.size;

        if (rotation.eulerAngles.y == 90 | rotation.eulerAngles.y == 270)
        {
            float tempBound = bounds.x;
            bounds.x = bounds.z;
            bounds.z = tempBound;
        }

        rayHitZ1 = Physics.RaycastAll(new Vector3(objectPlacer.transform.position.x - (bounds.x / 2), // new x position - half the size of the object
            objectPlacer.transform.position.y,
            boundsZ[0]), // the bottom of the bounds of terrain
            objectPlacer.transform.forward * boundsZ[1]);


        rayHitZ2 = Physics.RaycastAll(new Vector3(objectPlacer.transform.position.x + (bounds.x / 2),
            objectPlacer.transform.position.y,
            boundsZ[0]),
            objectPlacer.transform.forward * boundsZ[1]);
        
        rayHitX1 = Physics.RaycastAll(new Vector3(boundsX[0],
            objectPlacer.transform.position.y,
            objectPlacer.transform.position.z - (bounds.z / 2)),
            objectPlacer.transform.right * boundsX[1]);

        rayHitX2 = Physics.RaycastAll(new Vector3(boundsX[0], 
            objectPlacer.transform.position.y,
            objectPlacer.transform.position.z + (bounds.z / 2)),
            objectPlacer.transform.right * boundsX[1]);
        
    }

    private bool isRaycastColliding()
    {
        // return true if there is another object in new object space, false if not
        // gets all gameobjects that have been intersected by raycasts
        // iterates through lists and adds to frequency dictionary
        // if gameobject is found in more than one raycast hit, then there is an object in the space that has been collided with

        List<List<RaycastHit[]>> all = new List<List<RaycastHit[]>>();
        // clean up later when you have energy
        List<RaycastHit[]> x1z1 = new List<RaycastHit[]>() {rayHitX1, rayHitZ1 };
        List<RaycastHit[]> x1z2 = new List<RaycastHit[]>() { rayHitX1, rayHitZ2 };
        List<RaycastHit[]> x2z1 = new List<RaycastHit[]>() { rayHitX2, rayHitZ1 };
        List<RaycastHit[]> x2z2 = new List<RaycastHit[]>() { rayHitX2, rayHitZ2 };

        all.Add(x1z1);
        all.Add(x1z2);
        all.Add(x2z1);
        all.Add(x2z2);

        Dictionary<GameObject, float> freq;


        foreach(List<RaycastHit[]> combo in all)
        {
            freq = new Dictionary<GameObject, float>();
            foreach (RaycastHit[] twoList in combo)
            { 
                foreach (RaycastHit r in twoList)
                {
                    if (!freq.ContainsKey(r.collider.gameObject))
                    {
                        freq.Add(r.collider.gameObject, 1);
                    }
                    else
                    {
                        freq[r.collider.gameObject] += 1;
                    }
                }
            }

            foreach (var item in freq)
            {
                if (item.Value > 1)
                {
                    // could get gameobject.transform.position to see if it is within bounds
                    return true;
                }
            }
        }
        return false;
    }
    

    private void rotateBuildings()
    {
        GameObject[] allBuildings = GameObject.FindGameObjectsWithTag("Building");
        GameObject[] allTiles = GameObject.FindGameObjectsWithTag("Floor");

        foreach (GameObject building in allBuildings)
        {
            List<Vector3> directions = new List<Vector3>() { building.transform.forward, building.transform.right, -building.transform.forward, -building.transform.right };
            foreach (Vector3 dir in directions)
            {
                Vector3 rayPos = new Vector3(building.transform.position.x, allTiles[1].transform.position.y, building.transform.position.z);
                houseToRoadRay = new Ray(rayPos, dir * 1.5f);
                Physics.Raycast(houseToRoadRay, out htrrInfo);
                if (htrrInfo.collider && htrrInfo.distance > 0.1f && htrrInfo.collider.tag == "Floor")
                {
                    building.transform.LookAt(htrrInfo.transform.position);
                    building.transform.rotation = Quaternion.Euler(0, building.transform.rotation.eulerAngles.y, 0);
                }
            }
        }
    }

    public void spawnWalls(GameObject wallObject, float freq)
    {
        GameObject[] allBuildings = GameObject.FindGameObjectsWithTag("Building");
        foreach(GameObject building in allBuildings)
        {
            GameObject conLeft = building.transform.Find("conLeft").gameObject;
            GameObject conRight = building.transform.Find("conRight").gameObject;
            Vector3 objSize = wallObject.GetComponent<Renderer>().bounds.size;
            Vector3 rayPos = conRight.transform.position;
            Vector3 endPos = conRight.transform.right * 5;

            Ray tempRay = new Ray(rayPos, endPos);
            RaycastHit tempRayInfo = new RaycastHit();
            Physics.Raycast(tempRay, out tempRayInfo);

            float chance = Random.Range(0, 100) / 100;
            
            if (tempRayInfo.collider && tempRayInfo.collider.tag == "Building" && tempRayInfo.distance >= objSize.x && tempRayInfo.distance < 15)
            {
                GameObject conLeft2 = tempRayInfo.collider.transform.Find("conLeft").gameObject;
                allRays.Add(new List<Vector3>() { conRight.transform.position, conLeft2.transform.position });
                GameObject tempObj = Instantiate(wallObject, Vector3.zero, Quaternion.Euler(0, 0, 0));
                tempObj.transform.Translate(Vector3.up * terrain.SampleHeight(tempObj.transform.position));
                allObjects.Add(tempObj);
                
                // sqrt((x1 - x2)^2 + (z1 - z2)^2) = length of wall
                float tempX = Mathf.Sqrt(squareNum(conRight.transform.position.x - conLeft2.transform.position.x) + squareNum(conRight.transform.position.z - conLeft2.transform.position.z));
             
                tempObj.transform.localScale = new Vector3(tempX, tempObj.transform.localScale.y, tempObj.transform.localScale.z);

                Vector3 difference = (conLeft2.transform.position - conRight.transform.position) / 2;
                Vector3 newPos = new Vector3(conRight.transform.position.x + difference.x, conRight.transform.position.y, conRight.transform.position.z + difference.z);
                tempObj.transform.position = newPos;
                Vector3 dir = conLeft2.transform.position - conRight.transform.position;
                var rot = Quaternion.LookRotation(dir, Vector3.up);
                tempObj.transform.rotation = rot;
                tempObj.transform.Rotate(0, 90, 0);
            }
        }
    }

    private void spawnWhereSpace(string filterTag, int maxNum)
    {
        int chance = 50;
        GameObject obj;
        List<GameObject> obList = new List<GameObject>();
        foreach(GameObject ob in objectPrefabs)
        {
            if (ob.tag == filterTag)
            {
                obList.Add(ob);
            }
        }
        Vector3 objBounds;
        Vector3 curPos = new Vector3(0, 0, 0);
        int layerMask = ~(1 << 2);

        int step = (int)((boundsX[1] - boundsX[0]) / maxNum) * 5;
        int curNum = 0;
        for (int y=0; y < terrain.terrainData.bounds.size.z; y+= step)
        {
            for (int x = 0; x < terrain.terrainData.bounds.size.x; x+= step)
            {
                Collider[] all = Physics.OverlapSphere(curPos, 1, layerMask);
                if (all.Length == 0 && curNum < maxNum && !isOutofBounds(curPos) && Random.Range(0, 100) > chance)
                {
                    obj = obList[Random.Range(0, obList.Count)];
                    objBounds = obj.GetComponent<Renderer>().bounds.size;
                    GameObject tempObj = Instantiate(obj, curPos, Quaternion.identity);
                    tempObj.transform.Translate(Vector3.up * terrain.SampleHeight(tempObj.transform.position));
                    allObjects.Add(tempObj);
                    curNum++;
                }
                else
                {
                    //print(all.Length);
                }
                curPos = new Vector3(x, 0, y);
            }
        }
    }

    private float squareNum(float num)
    {
        return num * num;
    }
    public void spawnObjects(int objectCount, int rows, string selectedTag)
    {
        int maxCols = objectCount;
        int maxRows = rows;
        Dictionary<GameObject, float> obFreq = new Dictionary<GameObject, float>();
        Dictionary<GameObject, float> obsInUse = new Dictionary<GameObject, float>();
        obFreq.Add(objectPrefabs[0], 20);
        obFreq.Add(objectPrefabs[1], 20);
        obFreq.Add(objectPrefabs[3], 100);
        obFreq.Add(objectPrefabs[4], 100);

        foreach (KeyValuePair<GameObject, float> ob in obFreq)
        {
            if (ob.Key.tag == selectedTag)
            {
                obsInUse.Add(ob.Key, ob.Value);
            }
        }
        foreach (KeyValuePair<GameObject, float> ob in obsInUse)
        {
            float x = 0;
            float y = 0;
           
            for (int c = 0; c <= maxCols; c++)
            {
                x = 0;
                for (int r = 0; r <= maxRows; r++)
                {
                    // set temporary rotation and position to revert back to at end of loop
                    Quaternion tempSpawnerRotation = objectPlacer.transform.rotation;
                    Vector3 tempSpawnerPosition = objectPlacer.transform.position;

                    // Offset spawner by rotating then moving in that direction around central point
                    Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0,360), 0);
                    float offset = Random.Range(0, 5);
                    objectPlacer.transform.rotation = randomRotation;
                    objectPlacer.transform.Translate(objectPlacer.transform.forward * offset);
                    // if chance is within object frequency, then spawn
                    int chance = Random.Range(0, 100);

                    // random object rotation set to multiples of 90 degrees on y axes
                    // new object position is where the object placer is located
                    Quaternion newObjectRotation = Quaternion.Euler(0, Random.Range(-1, 3) * 90, 0);
                    drawObjectRay(ob.Key, newObjectRotation);
                    
                    if (ob.Value >= chance && !isRaycastColliding())
                    {
                        GameObject tempObj = Instantiate(ob.Key, objectPlacer.transform.position, newObjectRotation);
                        tempObj.transform.Translate(Vector3.up * terrain.SampleHeight(tempObj.transform.position));
                        allObjects.Add(tempObj);
                        
                    }
                    x += boundsZ[1] / maxRows;
                    objectPlacer.transform.rotation = tempSpawnerRotation;
                    objectPlacer.transform.position = tempSpawnerPosition;
                    objectPlacer.transform.position = new Vector3(x, objectPlacer.transform.position.y, y);
                }
                y += boundsX[1] / maxCols;
            }
        }
    }

   
    public void generateRoad(int maxTiles)
    {
        Vector3 initPos;
        if (GameObject.FindGameObjectWithTag("Floor") == null)
        {
            //Vector3 initPos = new Vector3(Random.Range(boundsX[0], boundsX[1]), .003f, Random.Range(boundsZ[0], boundsZ[1]));
            initPos = new Vector3(boundsX[1] / 2, .003f, boundsZ[1] / 2);
            currentTile = Instantiate(floorPrefab, initPos, Quaternion.identity);
            allObjects.Add(currentTile);
        }
        else
        {
            currentTile = GameObject.FindGameObjectWithTag("Floor");
            initPos = currentTile.transform.position;
        }
        

        drawRoadRay();
        int attempts = 0;
        int tileCount = 0;
        float tileSizeX = floorPrefab.GetComponent<Renderer>().bounds.size.x;

        while (attempts < maxTiles && tileCount < maxTiles)
        {
            List<int> rotationOptions = new List<int> { -1, 0, 1, 2 };
            bool tilePlaced = false;

            while (!tilePlaced && rotationOptions.Count > 0)
            {
                Vector3 curPos = currentTile.transform.position;
                int randChoice = Random.Range(0, rotationOptions.Count);
                int yRotation = rotationOptions[randChoice] * 90;
                rotationOptions.RemoveAt(randChoice);
                bool posOutOfBounds = isOutofBounds(curPos);

                if (!posOutOfBounds && (!tileRayHit.collider && tileRayHit.distance == 0) | tileRayHit.distance > tileSizeX)
                {
                    Vector3 tempPos = curPos + (currentTile.transform.forward * tileSizeX);
                    GameObject tempObject = Instantiate(floorPrefab, tempPos, Quaternion.Euler(0, yRotation, 0));
                    allObjects.Add(tempObject);

                    if (yRotation == -90 | yRotation == 90)
                    {
                        currentTile = tempObject;
                    }
                    tileCount++;
                    tilePlaced = true;
                }
                else if (!posOutOfBounds && tileRayHit.collider.tag == "Floor" && tileRayHit.collider.gameObject != currentTile.gameObject)
                {
                    while(tileRayHit.collider && tileRayHit.collider.tag == "Floor")
                    {
                        tileRayHit.collider.gameObject.transform.rotation = currentTile.transform.rotation;
                        currentTile = tileRayHit.collider.gameObject;
                        drawRoadRay();
                    }
                    currentTile.transform.Rotate(0, Random.Range(-1, 2) * 90, 0);
                    rotationOptions = new List<int> { -1, 0, 1, 2 };
                }
                else
                {
                    currentTile.transform.Rotate(0, Random.Range(-1, 2) * 90, 0);
                }
                drawRoadRay();
            }

            if (!tilePlaced)
            {
                GameObject[] all = GameObject.FindGameObjectsWithTag("Floor");
                currentTile = all[Random.Range(0, all.Length)];
                currentTile.transform.Rotate(0, Random.Range(-1,3) * 90, 0);
            }
            attempts++;
        }

        GameObject[] allTiles = GameObject.FindGameObjectsWithTag("Floor");
        CombineInstance[] tileMeshInstances = new CombineInstance[allTiles.Length];

        //float[,,] splatmapData = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight, 0];
        
        //print(terrain.terrainData.alphamapLayers);
        int i = 0;
        foreach (GameObject tile in allTiles)
        {
            tile.transform.Translate(Vector3.up * terrain.SampleHeight(tile.transform.position));
            allTilePositions.Add(tile.transform.position);
            terrainPainter(new List<Vector3>() { tile.transform.position });
            tileMeshInstances[i].mesh = tile.GetComponent<MeshFilter>().sharedMesh;
            tileMeshInstances[i].transform = tile.transform.localToWorldMatrix;
            Destroy(tile);
            i++;
        }

        Mesh tileMesh = new Mesh();
        //tileMesh.CombineMeshes(tileMeshInstances);
        //finalFloor = Instantiate(emptyPrefab, Vector3.zero, Quaternion.identity);
        //finalFloor.tag = "Floor";
        //finalFloor.transform.GetComponent<MeshFilter>().sharedMesh = tileMesh;
        //finalFloor.transform.gameObject.SetActive(true);
    }

    public void terrainPainter(List<Vector3> tilePositions)
    {
        foreach (Vector3 tile in tilePositions)
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
                        //float tempLerp = Random.Range(30, 100) * .001f;
                        splatmapData[x, y, 0] = 0;
                        splatmapData[x, y, 1] = 1;
                    }
                }
                terrain.terrainData.SetAlphamaps(posX, posZ, splatmapData);
            }
        }
    }

    private bool isOutofBounds(Vector3 position)
    {
        if (position.x > boundsX[0] && position.x < boundsX[1] && position.z > boundsZ[0] && position.z < boundsZ[1])
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}