using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WLPause : MonoBehaviour, IInputClickHandler
{
    [Tooltip("Data Mananger")]
    public GameObject dm;

    public virtual void OnInputClicked(InputClickedEventData eventData)
    {
        HandleUpdate();
        eventData.Use();
    }

    public void HandleUpdate()
    {
        if (dm != null)
        {
            dm.GetComponent<WebLoader>().pause = !dm.GetComponent<WebLoader>().pause;
        }

    }
}
