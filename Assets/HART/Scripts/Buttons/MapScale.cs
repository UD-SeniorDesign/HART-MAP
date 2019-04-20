// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using Mapbox.Unity.Map;
using System.Linq;

namespace HoloToolkit.Unity.SpatialMapping
{
    /// <summary>
    /// The TapToPlace class is a basic way to enable users to move objects 
    /// and place them on real world surfaces.
    /// Put this script on the object you want to be able to move. 
    /// Users will be able to tap objects, gaze elsewhere, and perform the tap gesture again to place.
    /// This script is used in conjunction with GazeManager, WorldAnchorManager, and SpatialMappingManager.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MapScale : MonoBehaviour, IInputClickHandler
    {
        [Tooltip("Enlarge (true) or Shrink (false).")]
        public bool Enlarge = false;

        [Tooltip("Map to affect")]
        public AbstractMap map;

        [Tooltip("Amount to zoom by")]
        [Range(0, 10)]
        public int scaleAmount = 1;


        public GameObject Handler;

        private void findHandler()
        {
            Handler = GameObject.FindWithTag("ButtonHandler");
        }

        public virtual void OnInputClicked(InputClickedEventData eventData)
        {
            HandleUpdate();
            eventData.Use();
        }

        public void HandleUpdate()
        {
            if (Handler == null)
            {
                findHandler();
            }
            int currScale = Handler.GetComponent<vars>().CurrScale;
            var t = new RangeTileProviderOptions();
            if (Enlarge)
            {
                currScale = Mathf.Min(100, Mathf.Max(0, currScale + scaleAmount));
            }
            else
            {
                currScale = Mathf.Min(100, Mathf.Max(0, currScale - scaleAmount));
            }
            Handler.GetComponent<vars>().CurrScale = currScale;
            t.SetOptions(currScale, currScale, currScale, currScale);
            map.SetExtentOptions(t);
            map.UpdateMap();
        }
    }
}
