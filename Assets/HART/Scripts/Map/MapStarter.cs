using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapStarter : MonoBehaviour {
    private AbstractMap map;
    public WebLoader wl;
    public vars ButtonHandler;
    // Use this for initialization
    void Start () {
        if (ButtonHandler == null)
        {
            ButtonHandler = GameObject.FindGameObjectWithTag("ButtonHandler").GetComponent<vars>();
        }
        if (ButtonHandler == null)
        {
            Debug.LogError("No Button handler found");
        }
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

                Debug.LogError("Did you forget to start the webloader?");
            }


        }
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
    }
    void Update()
    {
        if (map != null)
        {
            ButtonHandler.GetComponent<Actions>().Scale(wl.scale);
            map.Initialize(wl.latLong, (int)wl.zoom);
            ButtonHandler.CurrZoom = wl.zoom;

            ButtonHandler.CurrScale = wl.scale;
            GameObject.FindGameObjectWithTag("GameController").GetComponent<CornerFinder>().newMap();
            GameObject.Destroy(gameObject);
        }
        else
        {
            try
            {
                map = GameObject.FindGameObjectWithTag("Map").GetComponent<AbstractMap>();
            }
            catch (System.Exception)
            {

            }
        }
    }
}
