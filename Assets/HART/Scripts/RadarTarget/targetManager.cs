using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using System;

public class targetManager : MonoBehaviour
{
    public GameObject targetPrefab;
    [Range(0, 100)]
    public float dropTime = 10; // Number of ticks to keep a target alive after losing contact
    public Color dropColor = Color.red;
    public WebLoader dataSource;
    private frame dataFrame;
    private int lastTick = -1;
    private int tick = 0;
    private AbstractMap map;
    private Gradient g;
    private GradientColorKey[] colorKey = new GradientColorKey[2];
    private GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            map = GameObject.FindGameObjectWithTag("Map").GetComponent<AbstractMap>();
            foreach (WebLoader wl in gameObject.GetComponents<WebLoader>())
            {
                if (wl.enabled)
                {
                    dataSource = wl;
                    break;
                }
            }

        }
        catch (System.Exception)
        {

        }
        g = new Gradient();

        colorKey[0].time = 0.0f;
        colorKey[1].color = dropColor;
        colorKey[1].time = 1.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;
    }
    public Vector3 ColorToVector(Color c)
    {
        Vector3 tmp = new Vector3(0, 0, 0);
        tmp.x = c.r;
        tmp.y = c.g;
        tmp.z = c.b;
        return tmp;
    }
    public Color VectorToColor(Vector3 a)
    {
        Color tmp = Color.black;
        tmp.r = a.x;
        tmp.g = a.y;
        tmp.b = a.z;
        return tmp;
    }
    public bool insideMap(float lat, float lng)
    {
        if (lat >= dataSource.latMin && lat <= dataSource.latMax && lng >= dataSource.lngMin && lng <= dataSource.lngMax)
        {
            return true;
        }
        return false;
    }
    // Update is called once per frame
    void Update()
    {


        if (map == null)
        {
            try
            {
                map = GameObject.FindGameObjectWithTag("Map").GetComponent<AbstractMap>();

            }
            catch (System.Exception)
            {

            }
        }
        if (dataSource != null)
            dataFrame = dataSource.DataFrame;

        if (dataFrame != null)
        {

            tick = dataFrame.tick;
            if (tick != lastTick)
            {
                if (dataFrame.targets.Length > 0)
                {
                    foreach (gpsData target in dataFrame.targets)
                    {
                        if (target.Type != "Train")
                        {
                            GameObject radarTarget = GameObject.Find("DataManager/" + target.id);
                            if (radarTarget != null)
                            {
                                Vector3 pos = map.GeoToWorldPosition(new Mapbox.Utils.Vector2d(target.Latitude, target.Longitude));
                                //Debug.Log(map.QueryElevationInMetersAt(new Mapbox.Utils.Vector2d(target.Latitude, target.Longitude)));
                                if (target.Type == "Sat")
                                {
                                    pos.y += Mathf.Min((target.Elevation * map.WorldRelativeScale * 1000), 10000f * map.WorldRelativeScale);
                                }
                                else
                                {
                                    pos.y += radarTarget.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y + (target.Elevation * map.WorldRelativeScale);
                                }
                                radarTarget.transform.position = pos;
                                radarTarget.transform.rotation = Quaternion.Euler(0, target.TrueTrack, 0);
                                radarTarget.GetComponent<targetHandler>().metadata.copy(target);
                                if (radarTarget.GetComponent<targetHandler>().lostContact)
                                {
                                    radarTarget.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = radarTarget.GetComponent<targetHandler>().orgColor;

                                    radarTarget.GetComponent<targetHandler>().lostContact = false;
                                }
                                radarTarget.GetComponent<targetHandler>().timeSinceUpdate = tick;
                                if (target.Type == "Aircraft")
                                {
                                    //pos = radarTarget.transform.GetChild(0).Find("node_id33 12").GetComponent<Renderer>().bounds.center;
                                    pos = radarTarget.transform.GetChild(0).Find("node_id33 12").GetComponent<Renderer>().bounds.center;
                                }

                                radarTarget.GetComponent<LineRenderer>().SetPosition(0, new Vector3(pos.x, pos.y - radarTarget.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y * 2, pos.z));
                                radarTarget.GetComponent<LineRenderer>().SetPosition(1, new Vector3(pos.x, Mathf.Max(-10, pos.y - (radarTarget.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y + (target.Elevation * map.WorldRelativeScale))), pos.z));
                            }
                            else
                            {
                                if (insideMap(target.Latitude, target.Longitude) && target.Elevation > -10 && target.Type != "Train")
                                {
                                    Vector3 pos = map.GeoToWorldPosition(new Mapbox.Utils.Vector2d(target.Latitude, target.Longitude));
                                    if (target.Type == "Sat")
                                    {
                                        pos.y += Mathf.Min((target.Elevation * map.WorldRelativeScale * 1000), 10000f * map.WorldRelativeScale);
                                    }
                                    else
                                    {
                                        pos.y += (target.Elevation * map.WorldRelativeScale);
                                    }
                                    try
                                    {
                                        radarTarget = Instantiate(Resources.Load("HART/Prefabs/" + target.Type, typeof(GameObject)), pos, Quaternion.Euler(0, target.TrueTrack, 0), transform) as GameObject;
                                    }
                                    catch (System.Exception)
                                    {
                                        radarTarget = Instantiate(targetPrefab, pos, Quaternion.Euler(0, target.TrueTrack, 0), transform);
                                    }


                                    radarTarget.GetComponent<targetHandler>().metadata.copy(target);
                                    radarTarget.name = target.id;
                                    radarTarget.tag = "target";
                                    radarTarget.layer = 9;
                                    radarTarget.GetComponent<targetHandler>().timeSinceUpdate = tick;
                                    if (target.Type == "Aircraft")
                                    {
                                        //pos = radarTarget.transform.GetChild(0).Find("node_id33 12").GetComponent<Renderer>().bounds.center;
                                        pos = radarTarget.transform.GetChild(0).Find("node_id33 12").GetComponent<Renderer>().bounds.center;
                                    }
                                    if (radarTarget.GetComponent<LineRenderer>())
                                    {
                                        radarTarget.GetComponent<LineRenderer>().positionCount = 2;
                                        radarTarget.GetComponent<LineRenderer>().SetPosition(0, new Vector3(pos.x, pos.y - radarTarget.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y * 2, pos.z));
                                        radarTarget.GetComponent<LineRenderer>().SetPosition(1, new Vector3(pos.x, Mathf.Max(-10, pos.y - (radarTarget.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y + (target.Elevation * map.WorldRelativeScale))), pos.z));

                                        radarTarget.GetComponent<LineRenderer>().startWidth = .04f;
                                        radarTarget.GetComponent<LineRenderer>().endWidth = 0;
                                    }
                                    try
                                    {
                                        Transform dataDisp = radarTarget.transform.Find("DataDisplay");
                                        if (dataDisp.GetComponent<TextMesh>() != null)
                                        {
                                            gpsData tmp = radarTarget.GetComponent<targetHandler>().metadata;
                                            dataDisp.GetComponent<TextMesh>().text = tmp.getTextData();
                                            dataDisp.transform.position += new Vector3(0, dataDisp.GetComponent<TextMesh>().GetComponent<Renderer>().bounds.size.y);
                                            dataDisp.LookAt(Camera.main.transform);
                                            dataDisp.Rotate(new Vector3(0, 180, 0));
                                        }
                                    }
                                    catch (Exception)
                                    {

                                    }

                                }
                            }
                        }
                        else if (target.Type == "Train")
                        {
                            foreach (station sta in target.stops)
                            {
                                GameObject radarTarget = GameObject.Find("DataManager/" + sta.stationId);
                                if (radarTarget != null)
                                {
                                    radarTarget.GetComponent<targetHandler>().metadata.stops[0] = sta;
                                    radarTarget.GetComponent<targetHandler>().metadata.Latitude = sta.Latitude;
                                    radarTarget.GetComponent<targetHandler>().metadata.Longitude = sta.Longitude;
                                    radarTarget.GetComponent<targetHandler>().timeSinceUpdate = tick;
                                }
                                else
                                {
                                    if (insideMap(sta.Latitude, sta.Longitude))
                                    {
                                        Vector3 pos = map.GeoToWorldPosition(new Mapbox.Utils.Vector2d(sta.Latitude, sta.Longitude));
                                        try
                                        {
                                            radarTarget = Instantiate(Resources.Load("HART/Prefabs/" + target.Type, typeof(GameObject)), pos, Quaternion.Euler(0, target.TrueTrack, 0), transform) as GameObject;
                                        }
                                        catch (System.Exception)
                                        {
                                            Debug.LogError("Failed to create train station, something is really broken");
                                        }

                                        radarTarget.name = sta.stationId;
                                        radarTarget.tag = "target";
                                        radarTarget.layer = 9;
                                        radarTarget.GetComponent<targetHandler>().showText = false;
                                        radarTarget.GetComponent<targetHandler>().metadata.copy(target);
                                        radarTarget.GetComponent<targetHandler>().metadata.stops = new station[1] {sta};
                                        radarTarget.GetComponent<targetHandler>().metadata.id = sta.stationId+":"+sta.stationName;
                                        radarTarget.GetComponent<targetHandler>().metadata.Latitude = sta.Latitude;
                                        radarTarget.GetComponent<targetHandler>().metadata.Longitude = sta.Longitude;
                                        radarTarget.GetComponent<targetHandler>().timeSinceUpdate = tick;
                                    }
                                }
                            }

                        }
                    }
                }

                foreach (Transform target in transform)
                {

                    if (target.GetComponent<targetHandler>().timeSinceUpdate != tick && tick - target.GetComponent<targetHandler>().timeSinceUpdate >= dropTime)
                    {
                        colorKey[0].color = target.GetComponent<targetHandler>().orgColor;
                        g.SetKeys(colorKey, alphaKey);
                        //Debug.Log("Lost contact with: " + target.name + " for " + (tick - target.GetComponent<targetHandler>().timeSinceUpdate) + " ticks");
                        target.GetComponent<targetHandler>().lostContact = true;
                        target.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = g.Evaluate((tick - target.GetComponent<targetHandler>().timeSinceUpdate) / dropTime);
                        target.transform.GetComponent<targetHandler>().diff = ((float)(tick - target.GetComponent<targetHandler>().timeSinceUpdate)) / dropTime;
                    }
                    if (tick - target.GetComponent<targetHandler>().timeSinceUpdate > dropTime)
                    {
                        //Debug.Log(target.name + " is gone");
                        target.GetComponent<targetHandler>().removeDelegate();
                        Destroy(target.gameObject);
                    }
                }
                lastTick = tick;
            }


        }
    }
}
