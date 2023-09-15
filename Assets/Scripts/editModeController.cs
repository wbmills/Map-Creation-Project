using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System.Reflection;

public class editModeController : MonoBehaviour
{
    private Canvas c;
    public Camera cam;
    public GameObject spotlight;
    private Vector3 mousePosinWorld;
    private Terrain terrain;
    private GameObject curSceneObject;
    public Dropdown prefabList;
    private GameObject sceneController;
    private GameObject tempCollision;

    private float scrollSensitivity = 80f;
    private loadTown ltScript;
    private playerMovement pmScript;
    private townGeneration tgScript;
    private Dictionary<KeyCode, string> buttons;
    private GameObject[] objectPrefabs;
    private Vector3 curMousePos;

    void Start()
    {
        terrain = GameObject.FindAnyObjectByType<Terrain>();
        curSceneObject = null;
        sceneController = GameObject.Find("SceneManager");
        pmScript = sceneController.GetComponent<playerMovement>();
        tgScript = gameObject.GetComponent<townGeneration>();
        ltScript = sceneController.GetComponent<loadTown>();
        updatePrefabs();
        setButtons();
    }

    private void updatePrefabs()
    {
        objectPrefabs = tgScript.objectPrefabs;
        var tempList = new List<string>();
        foreach (GameObject ob in objectPrefabs)
        {
            tempList.Add(ob.name);
        }
        prefabList.ClearOptions();
        prefabList.AddOptions(tempList);
    }

    private void setButtons()
    {
        buttons = new Dictionary<KeyCode, string>(){
            {KeyCode.Equals, "zoomIn" },
            {KeyCode.Minus, "zoomOut" },
            {KeyCode.R, "rotateObject" },
            {KeyCode.Q, "deleteCurrentObject" },
            {KeyCode.Mouse1, "outputCurHit" },
            {KeyCode.E, "spawnObject" },
            {KeyCode.Mouse2, "saveToFBX" },
            {KeyCode.Slash, "updatePrefabs" },
            {KeyCode.Alpha1, "changePrefabSelection" },
            {KeyCode.Comma, "makeObjectSmaller" },
            {KeyCode.Period, "makeObjectBigger" },
            {KeyCode.Backspace, "playerSwitch" }
        };
    }

    private void playerSwitch()
    {
        cameraController camCon = sceneController.GetComponent<cameraController>();
        if (camCon.currentCamera.name == "EditModeCamera")
        {
            camCon.setCamera("Main Camera");
        }
        else if (camCon.currentCamera.name == "Main Camera")
        {
            camCon.setCamera("EditModeCamera");
        }
        
    }

    private void makeObjectBigger()
    {
        var scale = curSceneObject.transform.localScale;
        if (curSceneObject.name.Contains("Wall"))
        {
            scale = new Vector3(scale.x + 2, scale.y, scale.z);
        }
        else if (curSceneObject != null)
        {
            scale = new Vector3(scale.x + .5f, scale.y + .5f, scale.z + .5f);
        }
        curSceneObject.transform.localScale = scale;
        moveObject();
    }

    private void makeObjectSmaller()
    {
        var scale = curSceneObject.transform.localScale;
        if (curSceneObject.name.Contains("Wall"))
        {
            scale = new Vector3(scale.x - 2, scale.y, scale.z);
        }
        else if (curSceneObject != null)
        {
            scale = new Vector3(scale.x - .5f, scale.y - .5f, scale.z - .5f);
        }
        if (scale.x > 0 && scale.y > 0 && scale.z > 0)
        {
            curSceneObject.transform.localScale = scale;
        }

        moveObject();
    }

    private void changePrefabSelection()
    {
        if (prefabList.value < prefabList.options.Count)
        {
            prefabList.value = prefabList.value + 1;
        }
        else
        {
            prefabList.value = 0;
        }
    }

    private void saveToFBX()
    {
        GameObject parentOb = GameObject.FindGameObjectWithTag("Object Parent");
        transform.GetComponent<exportScene>().ExportMapAsFBX(parentOb);
    }

    void Update()
    {
        foreach (var buttonMethodDict in buttons)
        {
            if (Input.GetKeyDown(buttonMethodDict.Key))
            {
                MethodInfo mi = this.GetType().GetMethod(buttonMethodDict.Value, BindingFlags.NonPublic | BindingFlags.Instance);
                mi.Invoke(this, null);
            }
        }

        transform.Translate(Input.GetAxis("Horizontal") * Vector3.right);
        transform.Translate(Input.GetAxis("Vertical") * Vector3.up);
        transform.Translate(Input.GetAxis("Mouse ScrollWheel") * Vector3.forward * scrollSensitivity);

        if (sceneController.GetComponent<cameraController>().getCurrentCamera() == "EditModeCamera")
        {
            spotlight.SetActive(true);
        }
        else
        {
            spotlight.SetActive(false);
        }

        // spotlight follows mouse
        curMousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.y));
        spotlight.transform.position = new Vector3(curMousePos.x, 
            terrain.SampleHeight(new Vector3(curMousePos.x, 0, curMousePos.z)), 
            curMousePos.z);

        // select scene object to move it
        if (Input.GetKeyDown(KeyCode.Mouse0) && curSceneObject == null)
        {
            setCurrentObject();
        }

        // place currently holding object
        else if ((Input.GetKeyDown(KeyCode.Mouse0) | Input.GetKeyDown(KeyCode.F)) && curSceneObject != null)
        {
            curSceneObject.transform.position = new Vector3(curSceneObject.transform.position.x,
                terrain.SampleHeight(curSceneObject.transform.position),
                curSceneObject.transform.position.z);
            curSceneObject = null;
            tempCollision = null;
        }

        if (curSceneObject != null)
        {
            moveObject();
        }
    }

    private void outputCurHit()
    {
        Debug.Log($"curOb {curSceneObject}, temp{tempCollision}");
    }

    private void connectWalls()
    {
        // select two buildings and spawn wall between them
    }

    private int getSelection()
    {
        int selection = prefabList.value;
        return selection;
    }

    private void rotateObject()
    {
        if (curSceneObject != null)
        {
            curSceneObject.transform.Rotate(new Vector3(0, 90, 0));
        }
    }

    private void moveObject()
    {
        mousePosinWorld = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.y));
        curSceneObject.transform.position = new Vector3(mousePosinWorld.x,
            terrain.SampleHeight(curSceneObject.transform.position), mousePosinWorld.z);
    }

    public void callSpawn()
    {
        spawnObject();
    }

    private void spawnObject()
    {
        int selection = getSelection();
        GameObject tempOb = transform.GetComponent<townGeneration>().instantiateObject(objectPrefabs[selection], Vector3.zero, Quaternion.identity, "Details", true);
        curSceneObject = tempOb;
    }

    private void setCurrentObject()
    {
        curSceneObject = tempCollision;
    }

    private void deleteCurrentObject()
    {
        if (curSceneObject != null)
        {
            Destroy(curSceneObject);
            curSceneObject = null;
        }
    }

    public void setCurrentCollisionObject(GameObject ob)
    {
        if (tempCollision == null)
        {
            tempCollision = ob;
        }
        tempCollision = ob;
    }
}
