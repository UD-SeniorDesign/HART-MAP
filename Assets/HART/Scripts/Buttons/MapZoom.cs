// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using Mapbox.Unity.Map;

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
    public class MapZoom : MonoBehaviour, IInputClickHandler
    {
        [Tooltip("Zoom in (true) or Zoom out (false).")]
        public bool ZoomIn = false;

        public Actions Handler;
        
        public virtual void OnInputClicked(InputClickedEventData eventData)
        {
            HandleEvent();
            eventData.Use();
        }

        private void HandleEvent()
        {
            if (Handler == null)
                Handler = GameObject.FindGameObjectWithTag("ButtonHandler").GetComponent<Actions>();
            if (Handler == null)
            {
                Debug.LogError("No Button Handler found");
                throw new System.MissingMemberException();
            }
            Handler.Zoom(ZoomIn);
        }

    }
}
