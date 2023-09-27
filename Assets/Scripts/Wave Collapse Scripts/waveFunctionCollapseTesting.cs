using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waveFunctionCollapseTesting : MonoBehaviour
{
    public List<MapSegment> segments;
    public List<GameObject> segmentGameObjects;
    public GameObject emptySegment;

    // Start is called before the first frame update
    void Start()
    {
        segments = new List<MapSegment>();
        loadSegments();
        GenerateMap(5, 5);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void setGameObjectMapSegments(GameObject[,] arr)
    {
        foreach(GameObject x in arr)
        {

        }
    }

    // so far, this generates a representation of the map, but does not implement it. 
    // still not tested (I'm too afraid to try). 
    private void GenerateMap(int width, int height)
    {
        // will generate map from avalible segment prefabs
        int area = width * height;
        List<Vector2> options = new List<Vector2>() {
            {new Vector2(0, 1) },
            {new Vector2(0, -1) },
            {new Vector2(1, 0) },
            {new Vector2(-1, 0) },
        };
        // a 2D map of empty segments
        MapSegment[,] mapSegments = new MapSegment[width, height];
        GameObject[,] objectSegments = new GameObject[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                objectSegments[x, y] = Instantiate(emptySegment, new Vector3(x * 10, 0, y * 10), Quaternion.identity);
                objectSegments[x, y].AddComponent<SegmentController>();
            }
        }

        int loopCap = 1000;
        int iterations = 0;
        int tileNum = 0;

        // start at random point in empty array
        int row = Random.Range(0, width);
        int col = Random.Range(0, height);
        MapSegment curSegment = segments[Random.Range(0, segments.Count)];
        mapSegments[row, col] = curSegment;

        // need to make sure all tiles are filled with something
        while (iterations < loopCap && tileNum < area)
        {
            int tempRow = row;
            int tempCol = col;

            // check each adjacent segment to current segment
            foreach(Vector2 option in options)
            {
                tempCol = col + (int)option.y;
                tempRow = row + (int)option.x;
                bool foundTile = false;
                int attempts = 0;
                Vector3 closestWall = new Vector3(option.x, 0, option.y);

                // while a segment has not been found and is within cutoff point, loop. 
                while (!foundTile && attempts < segments.Count + 10)
                {
                    MapSegment possibleSegment = segments[Random.Range(0, segments.Count)];
                    // if segment is within bounds of array and segment wall value is equal to curSegment wall value
                    if (possibleSegment.connectionValues(closestWall) == curSegment.connectionValues(-closestWall))
                    {
                        mapSegments[tempRow, tempCol] = possibleSegment;
                        objectSegments[tempRow, tempCol].GetComponent<SegmentController>().setSegmentData(possibleSegment);
                        foundTile = true;
                    }
                }

                // if no segment fits, then place an empty segment (or at the moment, just default to first in list)
                if (!foundTile)
                {
                    mapSegments[tempRow, tempCol] = segments[0];
                    objectSegments[tempRow, tempCol].GetComponent<SegmentController>().setSegmentData(segments[0]);
                }
            }

            // iterate through all columns and rows of int[,] mapSegments array 
            col += 1;
            if (col == width)
            {
                col = 0;
                row += 1;
            }

            iterations++;
        }
    }

    private string loadSegments()
    {
        MapSegment[] allSegments = Resources.LoadAll<MapSegment>("Segments");
        foreach(MapSegment segment in allSegments)
        {
            addToSegmentList(segment);
        }

        return $"Loaded {allSegments.Length} segments";
    }

    private void CreateSegment()
    {
        MapSegment segmentObject = new MapSegment();
        segmentObject.InstantiateSegment("new");

        addToSegmentList(segmentObject);
    }

    private void addToSegmentList(MapSegment segment)
    {
        segments.Add(segment);
    }

    private void CreateMapSegmentsFromFBX()
    {
        // will create set of squares, learning rules of which squares can sit by each other from this map
    }
}
