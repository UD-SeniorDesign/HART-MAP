using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Map;

public class CornerFinder : MonoBehaviour
{


    private List<float> lats = new List<float>();
    private List<float> longs = new List<float>();
    private WebLoader wl;
    private AbstractMap map;
    private RangeTileProviderOptions range;
    // Use this for initialization
    void Start()
    {
        if (wl == null)
        {
            try
            {
                foreach (var webl in GameObject.FindGameObjectWithTag("GameController").GetComponents<WebLoader>())
                {
                    if (webl.isActiveAndEnabled)
                    {
                        wl = webl;
                        break;
                    }
                }

            }
            catch (System.Exception)
            {

                Debug.Log("Did you forget to start the webloader?");
            }


        }
        if (map == null)
        {
            try
            {
                map = GameObject.FindGameObjectWithTag("Map").GetComponent<AbstractMap>();
                map.OnInitialized += delegate { newMap(); };
                map.OnUpdated += delegate { updateMap(); };
            }
            catch (System.Exception)
            {

            }
        }
    }
    private float distance(float lat1, float lng1, float lat2, float lng2)
    {
        //Debug.Log(lat1 + " " + lng1 + " " + lat2 + " " + lng2);
        float R = 6371000.0f;
        float l1 = deg2rad(lat1);
        float l2 = deg2rad(lat2);

        float latDiff = deg2rad(lat2 - lat1);
        float lngDiff = deg2rad(lng2 - lng1);
        float a = Mathf.Sin(latDiff / 2.0f) * Mathf.Sin(latDiff / 2.0f) +
            Mathf.Cos(l1) * Mathf.Cos(deg2rad(l2)) *
            Mathf.Sin(lngDiff / 2.0f) * Mathf.Sin(lngDiff / 2.0f);
        float c = 2.0f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1.0f - a));
        return R * c;
    }


    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    //::  This function converts decimal degrees to radians             :::
    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    private float deg2rad(float deg)
    {
        return (deg * Mathf.PI / 180.0f);
    }
    public void newMap()
    {
        if (map != null)
        {
            lats.Clear();
            longs.Clear();
            foreach (Transform tile in map.transform)
            {
                if (tile.GetComponent<UnityTile>())
                {
                    Mapbox.Utils.Vector2d latLong = tileToLatLong(tile.GetComponent<UnityTile>().UnwrappedTileId.Z, tile.GetComponent<UnityTile>().UnwrappedTileId.X, tile.GetComponent<UnityTile>().UnwrappedTileId.Y);
                    lats.Add((float)latLong.x);
                    longs.Add((float)latLong.y);
                }
            }
            lats.Sort();
            longs.Sort();
            if (lats.Count > 0 && longs.Count > 0)
            {
                //Debug.Log("Lat min:" + lats[0] + ", Lat max:" + lats[lats.Count - 1]);
                //Debug.Log("Long min:" + longs[0] + ", Long max:" + longs[longs.Count - 1]);
                if (wl != null)
                {
                    wl.latMax = lats[lats.Count - 1];
                    wl.latMin = lats[0];
                    wl.lngMax = longs[longs.Count - 1];
                    wl.lngMin = longs[0];
                    float tmp = distance((float)map.CenterLatitudeLongitude.x, (float)map.CenterLatitudeLongitude.y, wl.latMax, wl.lngMin);
                    wl.satRadius = (90 - Mathf.Atan(400000.0f / tmp) * Mathf.Rad2Deg);
                    //Debug.Log(tmp + " " + wl.satRadius + " " + (90 - Mathf.Atan(400000.0f / tmp) * Mathf.Rad2Deg));
                }
                else
                {
                    Debug.Log("No webloader");
                }
            }
        }
    }
    private Mapbox.Utils.Vector2d tileToLatLong(int zoom, int xTile, int yTile)
    {
        float n = Mathf.Pow(2, zoom);
        float lon_deg = xTile / n * 360.0f - 180.0f;
        float lat_deg = (float)System.Math.Atan(System.Math.Sinh(System.Math.PI * (1.0f - (2.0f * yTile / n))));
        lat_deg = lat_deg * (180.0f / Mathf.PI);
        return new Mapbox.Utils.Vector2d(lat_deg, lon_deg);
    }
    public void updateMap()
    {
        if (map != null)
        {
            lats.Clear();
            longs.Clear();
            foreach (Transform tile in map.transform)
            {
                if (tile.GetComponent<UnityTile>())
                {
                    Mapbox.Utils.Vector2d latLong = tileToLatLong(tile.GetComponent<UnityTile>().UnwrappedTileId.Z, tile.GetComponent<UnityTile>().UnwrappedTileId.X, tile.GetComponent<UnityTile>().UnwrappedTileId.Y);
                    lats.Add((float)latLong.x);
                    longs.Add((float)latLong.y);
                }
            }
            lats.Sort();
            longs.Sort();
            if (lats.Count > 0 && longs.Count > 0)
            {
                //Debug.Log("Lat min:" + lats[0] + ", Lat max:" + lats[lats.Count - 1]);
                //Debug.Log("Long min:" + longs[0] + ", Long max:" + longs[longs.Count - 1]);
                if (wl != null)
                {
                    wl.latMax = lats[lats.Count - 1];
                    wl.latMin = lats[0];
                    wl.lngMax = longs[longs.Count - 1];
                    wl.lngMin = longs[0];

                    float tmp = distance((float)map.CenterLatitudeLongitude.x, (float)map.CenterLatitudeLongitude.y, wl.latMax, wl.lngMin);
                    wl.satRadius = (90 - Mathf.Atan(400000.0f / tmp) * Mathf.Rad2Deg);
                    //Debug.Log(tmp + " " + wl.satRadius + " " + (90 - Mathf.Atan(400000.0f / tmp) * Mathf.Rad2Deg));
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (wl == null)
        {
            try
            {
                wl = GameObject.FindGameObjectWithTag("GameController").GetComponent<WebLoader>();
            }
            catch (System.Exception)
            {

                Debug.LogError("Did you forget to start the webloader?");
            }


        }
        if (map == null)
        {
            try
            {
                map = GameObject.FindGameObjectWithTag("Map").GetComponent<AbstractMap>();
                map.OnInitialized += delegate { updateMap(); };
                map.OnUpdated += delegate { updateMap(); };
                updateMap();
            }
            catch (System.Exception)
            {

            }
        }
    }
}
