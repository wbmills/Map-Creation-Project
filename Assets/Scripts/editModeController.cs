using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class editModeController : MonoBehaviour
{
    private Canvas c;
    public Camera cam;
    public GameObject spotlight;
    private Vector3 mousePosinWorld;
    private Terrain terrain;
    public Button spawnButton;
    public GameObject curSceneObject;
    public Dropdown prefabList;
    private GameObject sceneController;
    public GameObject tempCollision;
    public Terrain t;

    private loadTown ltScript;
    private playerMovement pmScript;
    private townGeneration tgScript;

    private GameObject[] objectPrefabs;
    public Vector3 curMousePos;
    void Start()
    {
        terrain = GameObject.FindAnyObjectByType<Terrain>();
        curSceneObject = null;
        sceneController = GameObject.Find("SceneManager");
        pmScript = sceneController.GetComponent<playerMovement>();
        tgScript = sceneController.GetComponent<townGeneration>();
        ltScript = sceneController.GetComponent<loadTown>();

        spawnButton.onClick.AddListener(spawnObject);
        generateButtons();
    }

    void Update()
    {
        if (pmScript.getCurrentPlayer().name == "Edit Mode")
        {
            spotlight.SetActive(true);
        }
        else
        {
            spotlight.SetActive(false);
        }

        // spotlight follows mouse
        curMousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.y));
        spotlight.transform.position = new Vector3(curMousePos.x, 5f, curMousePos.z);

        // select scene object to move it
        if (Input.GetKeyDown(KeyCode.Mouse0) && curSceneObject == null)
        {
            setCurrentObject();
        }

        // place currently holding object
        else if (Input.GetKeyDown(KeyCode.Mouse0) && curSceneObject != null)
        {
            curSceneObject.transform.position = new Vector3(curSceneObject.transform.position.x,
                t.SampleHeight(curSceneObject.transform.position),
                curSceneObject.transform.position.z);
            curSceneObject = null;
            tempCollision = null;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            print($"curOb {curSceneObject}, temp{tempCollision}");
        }

        if (curSceneObject != null)
        {
            moveObject();
        }

        if (Input.GetKeyDown(KeyCode.R) && curSceneObject != null)
        {
            rotateObject();
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && curSceneObject != null)
        {
            deleteCurrentObject();
        }
    }


    private void connectWalls()
    {
        // select two buildings and spawn wall between them
    }
    private void generateButtons()
    {
        objectPrefabs = tgScript.objectPrefabs;
        List<string> l = new List<string>();
        foreach (GameObject obj in objectPrefabs)
        {
            l.Add(obj.name);
        }
        prefabList.AddOptions(l);
    }

    private int getSelection()
    {
        int selection = prefabList.value;
        return selection;
    }

    private void rotateObject()
    {
        curSceneObject.transform.Rotate(new Vector3(0, 90, 0));
    }

    private void moveObject()
    {
        mousePosinWorld = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.y));
        curSceneObject.transform.position = new Vector3(mousePosinWorld.x, .003f, mousePosinWorld.z);
    }
    private void spawnObject()
    {
        int selection = getSelection();
        GameObject tempOb = Instantiate(objectPrefabs[selection]).gameObject;
        tempOb.tag = "Details";
        curSceneObject = tempOb;
    }

    private void setCurrentObject()
    {
        curSceneObject = tempCollision;
    }

    private void deleteCurrentObject()
    {
        Destroy(curSceneObject);
        curSceneObject = null;
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
