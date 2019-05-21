// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using Mapbox.Unity.Map;
using System.Linq;

namespace HoloToolkit.Unity.SpatialMapping
{
    [RequireComponent(typeof(Collider))]
    public class MapScale : MonoBehaviour, IInputClickHandler
    {
        [Tooltip("Enlarge (true) or Shrink (false).")]
        public bool Enlarge = false;




        public Actions Handler;

        private void findHandler()
        {
            Handler = GameObject.FindWithTag("ButtonHandler").GetComponent<Actions>();
        }

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
            Handler.Scale(Enlarge);
        }
    }
}
