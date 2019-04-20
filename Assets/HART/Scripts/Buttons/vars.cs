using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vars : MonoBehaviour {
    private int currScale = 1;
    private float currZoom;

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

    private void OnEnable()
    {
        // Get the map and inital zoom
        map = GameObject.FindGameObjectWithTag("Map").GetComponent<AbstractMap>();
        if (map != null)
        {
            currZoom = map.Zoom;
        }
    }
    
}
