using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Mapbox.Unity.Map;

public class WebLoader : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public uint port = 9900;
    public string url = "http://{ip}:{port}/data?";
    public bool demoLoop = false;
    public bool commercialFlight = false;
    public bool satellite = false;
    public float satRadius = 20.0f;
    public float latMin = 40.647120f;
    public float latMax = 40.948716f;
    public float lngMin = -74.181003f;
    public float lngMax = -73.619899f;
    public Mapbox.Utils.Vector2d latLong = new Mapbox.Utils.Vector2d(40.778424, -73.966572);
    public float zoom = 16.0f;
    public bool debug = false;
    private AbstractMap map;

    public frame dataFrame;
    //private string demoLoopURL = "demoLoop=1&";
    //private string commericalFlightURL = "commercialFlights=1&lngMin={0}&lngMax={1}&latMin={2}&latMax={3}&";
    //private string satelliteURL = "satellite=1&centerLat={0}&centerLng={1}&satRadius={2}&";
    public float delay = 1f;
    // http://72.78.75.212:9900/data?demoLoop=1&commercialFlights=1&lngMin=-76.623080&lngMax=-73.828576&latMin=38.938079&latMax=40.632118&satellite=1&centerLat=39.9425&centerLng=-75.4799&satRadius=70

    private float lastUpdate = 0f;
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
    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    //::  This function converts radians to decimal degrees             :::
    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    public frame DataFrame
    {
        get
        {
            return dataFrame;
        }

        set
        {
            dataFrame = value;
        }
    }

    void Start()
    {
        lastUpdate = Time.time;
        try
        {
            
            map = GameObject.FindGameObjectWithTag("map").GetComponent<AbstractMap>();
            map.OnInitialized += delegate { updateMap(); };
            map.OnUpdated += delegate { updateMap(); };
        }
        catch (System.Exception)
        {
        }

    }

    public void updateMap()
    {
        latLong = map.CenterLatitudeLongitude;
        zoom = map.Zoom;
        //Debug.Log(distance((float)latLong.x, (float)latLong.y, latMax, lngMin));
        //Debug.Log(Mathf.Atan(40000000.0f/ distance((float)latLong.x, (float)latLong.y, latMax, lngMin)));
        //Debug.Log(Mapbox.Utils.Vector2d.Distance(new Vector2d(latMax, lngMax), latLong) / map.WorldRelativeScale);
        //satRadius = Mathf.Atan(40000000000.0f / distance((float)latLong.x, (float)latLong.y, latMax, lngMin));
    }
    void Update()
    {
        if (map == null)
        {
            try
            {
                map = GameObject.FindGameObjectWithTag("Map").GetComponent<AbstractMap>();
                map.OnUpdated += delegate { updateMap(); };
            }
            catch (System.Exception)
            {
            }
        }

        if (Time.time - lastUpdate > delay && map != null)
        {
            StartCoroutine(GetData());
            lastUpdate = Time.time;
        }


    }
    private IEnumerator GetData()
    {

        string request = url;
        request = request.Replace("{ip}", ip).Replace("{port}", port.ToString());
        //WWW www1 = new WWW(request);
        
        UnityWebRequest www = UnityWebRequest.Get(request);
        if (demoLoop)
        {
            www.SetRequestHeader("demoLoop", "1");
            //request += demoLoopURL;
        }
        if (commercialFlight)
        {
            
            www.SetRequestHeader("commercialFlights", "1");
            www.SetRequestHeader("lngMin", lngMin.ToString());
            www.SetRequestHeader("lngMax", lngMax.ToString());
            www.SetRequestHeader("latMin", latMin.ToString());
            www.SetRequestHeader("latMax", latMax.ToString());
            //request += string.Format(commericalFlightURL, lngMin, lngMax, latMin, latMax);
        }
        if (satellite)
        {


            www.SetRequestHeader("satellite", "1");
            www.SetRequestHeader("centerLat", latLong.x.ToString());
            www.SetRequestHeader("centerLng", latLong.y.ToString());
            www.SetRequestHeader("satRadius", satRadius.ToString());

            //request += string.Format(satelliteURL, latLong.x, latLong.y, satRadius);
        }
        //request = request.TrimEnd('&');
        /*if (debug)
            Debug.Log(request);
        */
        
        www.chunkedTransfer = false;
#if UNITY_EDITOR
        // otherwise requests don't work in Edit mode, eg geocoding
        // also lot of EditMode tests fail otherwise
#pragma warning disable 0618
        www.Send();
#pragma warning restore 0618
        while (!www.isDone) { yield return null; }
#else
#pragma warning disable 0618
			yield return www.Send();
#pragma warning restore 0618
#endif

        if (www.isNetworkError && !string.IsNullOrEmpty(www.error))
        {
            Debug.Log(request + " " + www.responseCode + "\n" + www.error);
            Debug.Log(www);
        }
        else
        {
            if (debug)
                Debug.Log(www.downloadHandler.text);
            dataFrame = JsonUtility.FromJson<frame>(www.downloadHandler.text);
            if (debug)
                Debug.Log(dataFrame);
        }
        www.Dispose();
    }

}
