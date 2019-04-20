using Mapbox.Unity.Map;
using UnityEngine;

public class targetHandler : MonoBehaviour
{
    public delegate void mapInit();
    public string lookup = "https://flightaware.com/live/flight/{callsign}";
    public int timeSinceUpdate = 0;
    public bool lostContact = false;
    public float diff = 0f;
    public gpsData metadata = new gpsData();
    public Color orgColor;
    private AbstractMap map;
    private Vector3 orgScale;
    
    private void Start()
    {
        try
        {
            map = GameObject.FindGameObjectWithTag("Map").GetComponent<AbstractMap>();
            map.OnInitialized += delegate { updateMap(); };
            map.OnUpdated += delegate { updateMap(); };
        }
        catch (System.Exception)
        {

        }
        if (metadata.Type.Equals("Aircraft"))
        {
            lookup = lookup.Replace("{callsign}", gameObject.name.Substring(gameObject.name.IndexOf("-") + 1));
        }
        orgScale = transform.localScale;
        if (metadata.Type.Equals("Aircraft")) 
            orgScale *= 20;
        //orgColor.x = transform.GetChild(0).GetComponent<MeshRenderer>().material.color.r;
        //orgColor.y = transform.GetChild(0).GetComponent<MeshRenderer>().material.color.g;
        //orgColor.z = transform.GetChild(0).GetComponent<MeshRenderer>().material.color.b;
        orgColor = transform.GetChild(0).GetComponent<MeshRenderer>().material.color;
        updateMap();
    }
    private void Update()
    {
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
    public void updateMap()
    {
        if (transform)
            transform.localScale = orgScale* map.WorldRelativeScale;

        //Debug.Log("Map updated, changing scale of " + gameObject.name + " to " + transform.localScale);
    }
}
