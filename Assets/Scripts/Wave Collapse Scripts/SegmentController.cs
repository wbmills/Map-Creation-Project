using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentController : MonoBehaviour
{
    public MapSegment segmentData;
    public List<GameObject> placeholderObjects;

    private void Start()
    {
        if (segmentData)
        {
            setupSegment();
        }
    }

    private void setupSegment()
    {
        transform.localScale = segmentData.segmentScale;
    }

    public void setSegmentData(MapSegment m)
    {
        print("Segment data set");
        segmentData = m;
        setupSegment();
    }
}
