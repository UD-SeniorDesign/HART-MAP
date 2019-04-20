using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapStarter : MonoBehaviour {
    private AbstractMap map;
    public WebLoader wl;
    // Use this for initialization
    void Start () {
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
            map.Initialize(wl.latLong, (int)wl.zoom);
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
