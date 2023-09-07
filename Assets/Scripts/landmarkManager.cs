using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class landmarkManager : MonoBehaviour
{
    private Dictionary<GameObject, Vector3> landmarkPositionsDefault;
    private Dictionary<GameObject, Vector3> landmarkPositionsRandom;
    private Dictionary<string, Dictionary<GameObject, Vector3>> modes;
    private float[] rangeX = new float[] { 0, 100f };
    private float[] rangeZ = new float[] { 114.8f, 373.9f };

    public GameObject[] landmarkObjects;
    public string curMode;
    
    void Start()
    {
        curMode = "default";
        landmarkObjects = GameObject.FindGameObjectsWithTag("Landmark");
        landmarkPositionsDefault = new Dictionary<GameObject, Vector3>();
        landmarkPositionsRandom = new Dictionary<GameObject, Vector3>();
        modes = new Dictionary<string, Dictionary<GameObject, Vector3>>();

        foreach (GameObject landmark in landmarkObjects)
        {
            landmarkPositionsDefault.Add(landmark, landmark.gameObject.transform.position);

            Vector3 randPosTemp = new Vector3(Random.Range(rangeX[0], rangeX[1]), landmark.transform.position.y, Random.Range(rangeZ[0], rangeZ[1]));
            landmarkPositionsRandom.Add(landmark, randPosTemp);
        }

        modes.Add("default", landmarkPositionsDefault);
        modes.Add("random", landmarkPositionsRandom);

        setLandmarkPositions();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            setLandmarkPositions();
        }
    }

    public void setLandmarkPositions()
    {

        foreach (GameObject landmark in landmarkObjects)
        {
            if (curMode == "random")
            {
                landmarkPositionsRandom[landmark] = new Vector3(Random.Range(rangeX[0], rangeX[1]), landmark.transform.position.y, 
                    Random.Range(rangeZ[0], rangeZ[1]));
            }

            landmark.transform.position = modes[curMode][landmark];
        }
    }
}
