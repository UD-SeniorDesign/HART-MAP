using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;

public class targetManager : MonoBehaviour
{
    public GameObject targetPrefab;
    [Range(0,100)]
    public float dropTime = 10; // Number of ticks to keep a target alive after losing contact
    public Color dropColor = Color.red;
    private Vector3 dropC = new Vector3(255, 0, 0);
    private WebLoader dataSource;
    private frame dataFrame;
    private int lastTick = -1;
    private int tick = 0;
    private AbstractMap map;
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

    }
    public Vector3 ColorToVector(Color c)
    {
        Vector3 tmp = new Vector3(0,0,0);
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
        if (lat>=dataSource.latMin && lat<=dataSource.latMax && lng>=dataSource.lngMin && lng<=dataSource.lngMax)
        {
            return true;
        }
        return false;
    }
    // Update is called once per frame
    void Update()
    {
        dropC = ColorToVector(dropColor);

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
                        GameObject radarTarget = GameObject.Find("DataManager/" + target.id);

                        if (radarTarget != null)
                        {
                            
                            Vector3 pos = map.GeoToWorldPosition(new Mapbox.Utils.Vector2d(target.Latitude, target.Longitude));
                            //Debug.Log(map.QueryElevationInMetersAt(new Mapbox.Utils.Vector2d(target.Latitude, target.Longitude)));
                            pos.y += radarTarget.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y + (target.Elevation * map.WorldRelativeScale);
                            radarTarget.transform.position = pos;
                            radarTarget.transform.rotation = Quaternion.Euler(0, target.TrueTrack, 0);
                            radarTarget.GetComponent<targetHandler>().metadata.copy(target);
                            if (radarTarget.GetComponent<targetHandler>().lostContact)
                            {
                                radarTarget.transform.GetChild(0).GetComponent<MeshRenderer>().material.color =radarTarget.GetComponent<targetHandler>().orgColor;
                                
                                radarTarget.GetComponent<targetHandler>().lostContact = false;
                            }
                            radarTarget.GetComponent<targetHandler>().timeSinceUpdate = tick;
                            
                        }
                        else
                        {
                            if (insideMap(target.Latitude, target.Longitude) && target.Elevation>-10)
                            {
                                Vector3 pos = map.GeoToWorldPosition(new Mapbox.Utils.Vector2d(target.Latitude, target.Longitude));
                                //if (target.Type = "")
                                pos.y += (target.Elevation * map.WorldRelativeScale);
                                //radarTarget = Instantiate(targetPrefab, pos, Quaternion.Euler(0, target.TrueTrack, 0), transform);

                                
                                try
                                {
                                    radarTarget = Instantiate(Resources.Load("HART/Prefabs/" +target.Type, typeof(GameObject)), pos, Quaternion.Euler(0, target.TrueTrack, 0), transform) as GameObject;
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

                            }
                            else
                            {
                                //Debug.Log(target.id);
                            }
                        }
                    }
                }                            
                foreach (Transform target in transform)
                {
                    Gradient g = new Gradient();
                    GradientColorKey[] colorKey = new GradientColorKey[2];
                    GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
                    colorKey[0].time = 0.0f;
                    colorKey[1].color = dropColor;
                    colorKey[1].time = 1.0f;
                    alphaKey[1].alpha = 1.0f;
                    alphaKey[1].time = 1.0f;
                    if (target.GetComponent<targetHandler>().timeSinceUpdate != tick && tick - target.GetComponent<targetHandler>().timeSinceUpdate <= dropTime)
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
                        Destroy(target.gameObject);
                    }
                }
                lastTick = tick;
            }

            
        }
    }
}
