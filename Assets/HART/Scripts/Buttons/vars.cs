using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vars : MonoBehaviour {
    private int currScale = 1;
    private float currZoom;
    [Tooltip("Amount to zoom by")]
    public float zoomAmount = 1;
    [Tooltip("Amount to zoom by")]
    [Range(0, 10)]
    public int scaleAmount = 1;
    private AbstractMap map;
    public int CurrScale
    {
        get
        {
            return currScale;
        }

        set
        {
            currScale = value;
        }
    }

    public float CurrZoom
    {
        get
        {
            return currZoom;
        }

        set
        {
            currZoom = value;   
        }
    }    
}
