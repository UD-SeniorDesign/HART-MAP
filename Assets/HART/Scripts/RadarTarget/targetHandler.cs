using System;
using HoloToolkit.Unity.InputModule;
using Mapbox.Unity.Map;
using UnityEngine;

public class targetHandler : MonoBehaviour, IInputClickHandler
{
    public delegate void mapInit();
    public string lookup = "https://flightaware.com/live/flight/{callsign}";
    public int timeSinceUpdate = 0;
    public bool lostContact = false;
    public float diff = 0f;
    public gpsData metadata = new gpsData();
    public Color orgColor;
    private AbstractMap map;
    public Vector3 orgScale;
    public bool showText = true;
    Action onMapChange;
    private void Start()
    {
        try
        {
            onMapChange = delegate { updateMap(); };
            map = GameObject.FindGameObjectWithTag("Map").GetComponent<AbstractMap>();
            //map.OnInitialized += onMapChange;
            map.OnUpdated += onMapChange;
        }
        catch (System.Exception)
        {

        }
        orgScale = transform.localScale;
        if (metadata.Type.Equals("Aircraft"))
        {
            lookup = lookup.Replace("{callsign}", gameObject.name.Substring(gameObject.name.IndexOf("-") + 1));
            orgScale *= 706f * 2;
        }
        else if (metadata.Type.Equals("Sat"))
        {
            orgScale *= 1000f;
        }
        else if (metadata.Type.Equals("Train"))
        {
            orgScale *= 300f;
        }

        orgColor = transform.GetChild(0).GetComponent<MeshRenderer>().material.color;
        this.Invoke("updateMap",.1f);
    }
    public void Update()
    {
        Transform dataDisp = transform.Find("DataDisplay");
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        if (dataDisp && onScreen)
        {
            if (showText)
            {

                dataDisp.GetComponent<TextMesh>().text = metadata.getTextData();
                dataDisp.LookAt(Camera.main.transform);
                dataDisp.Rotate(new Vector3(0, 180, 0));
            }
            else
            {
                dataDisp.GetComponent<TextMesh>().text = "";
                return;
            }
        }
        else
        {
            return;
        }
    }
    public void removeDelegate()
    {
        map.OnUpdated -= onMapChange;
    }
    public void updateMap()
    {
        if (transform)
            transform.localScale = orgScale * map.WorldRelativeScale;
        if (gameObject.GetComponent<LineRenderer>())
            gameObject.GetComponent<LineRenderer>().startWidth = Mathf.Min(Mathf.Max(transform.lossyScale.magnitude / 3, .04f),.1f);
        if (transform.Find("DataDisplay"))
            transform.Find("DataDisplay").transform.position = transform.position+(new Vector3(0, transform.Find("DataDisplay").GetComponent<TextMesh>().GetComponent<Renderer>().bounds.size.y));
        //Debug.Log("Map updated, changing scale of " + gameObject.name + " to " + transform.localScale);
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        showText = !showText;
    }
}
