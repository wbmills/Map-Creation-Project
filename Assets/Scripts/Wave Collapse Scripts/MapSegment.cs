using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Map Segment", order = 1)]
public class MapSegment : ScriptableObject
{
    // name of segment
    public string segmentName;

    // a dictionary of 6 connections, one for each side of the 'cube' segment
    public Dictionary<Vector3, float> connections;

    // theme of segment, determining type of objects within it (e.g. village, city, etc)
    // - will be used when players import their own prefabs
    public string segmentTheme;

    //objects to be randomised based on theme in segment, e.g. 'tree' or 'building'
    public Dictionary<string, GameObject> objectsInSegment;

    // size of segment (e.g. 50 x 50 x 50)
    public Vector3 segmentBounds;

    // scale of segment
    public Vector3 segmentScale;

    public void InstantiateSegment(string name, Dictionary<Vector3, float> connections = null, 
        string theme="default", Dictionary<string, GameObject> objectsInSegmentTemp = null, 
        float segmentBoundsValue = 1, float segmentScaleValue = 1)
    {
        initConnectionValues();
        segmentNameAccess(name);
        setConnectionValues(connections);
        segmentThemeAccess(theme);
        objectsInSegment = objectsInSegmentTemp;
        segmentBounds = new Vector3(segmentBoundsValue, segmentBoundsValue, segmentBoundsValue);
        segmentScaleAccess(segmentScaleValue);
    }

    private void initConnectionValues()
    {
        // set connection values to default in order:
        // left, right, up, down, front, back
        connections = new Dictionary<Vector3, float>() {
            {new Vector3(-1, 0, 0), 0 },
            {new Vector3(1, 0, 0), 0 },
            {new Vector3(0, 1, 0), 0 },
            {new Vector3(1, -1, 0), 0 },
            {new Vector3(0, 0, 1), 0 },
            {new Vector3(0, 0, -1), 0 }
        };
    }

    public float segmentScaleAccess(float newScale = -1)
    {
        if (newScale != -1)
        {
            segmentScale = new Vector3(newScale, newScale, newScale);
        }

        return segmentScale.x;
    }

    public string segmentNameAccess(string newName = null)
    {
        if (newName != null)
        {
            segmentName = newName;
        }

        return segmentName;
    }

    public string segmentThemeAccess(string newTheme = null)
    {
        if (newTheme != null)
        {
            segmentTheme = newTheme;
        }

        return segmentTheme;
    }

    private Dictionary<Vector3, float> setConnectionValues(Dictionary<Vector3, float> values = null)
    {
        if (values == null)
        {
            return connections;
        }

        foreach (Vector3 side in values.Keys)
        {
            if (connections.ContainsKey(side))
            {
                connections[side] = values[side];
            }
            else
            {
                Debug.Log($"Unknown Side {side} in connections dictionary");
            }
        }

        return connections;
    }

    public float connectionValues(Vector3 pos, float val = 0)
    {
        if (connections[pos] != 0 && val == 0)
        {
            return connections[pos];
        }
        else
        {
            connections[pos] = val;
            return connections[pos];
        }
    }
}

