using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using System.Linq;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Utilities;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity;

[RequireComponent(typeof(Interpolator))]
public class Actions : MonoBehaviour, IInputClickHandler
{
    [Tooltip("Map to affect")]
    public AbstractMap map;
    public targetManager DM;
    public vars Handler;
    public GameObject ButtonPanel;

    [Tooltip("Distance from camera to keep the object while placing it.")]
    public float DefaultGazeDistance = 2.0f;
    [Tooltip("Specify the parent game object to be moved on tap, if the immediate parent is not desired.")]
    public GameObject ParentGameObjectToPlace;
    [Tooltip("Setting this to true will enable the user to move and place the object in the scene without needing to tap on the object. Useful when you want to place an object immediately.")]
    public bool IsBeingPlaced;
    [Tooltip("Setting this to true will allow this behavior to control the DrawMesh property on the spatial mapping.")]
    public bool AllowMeshVisualizationControl = true;
    [Tooltip("Place parent on tap instead of current game object.")]
    public bool PlaceParentOnTap;
    private Interpolator interpolator;
    /// <summary>
    /// The default ignore raycast layer built into unity.
    /// </summary>
    private const int IgnoreRaycastLayer = 2;

    private Dictionary<GameObject, int> layerCache = new Dictionary<GameObject, int>();
    private Vector3 PlacementPosOffset;


    private void findMap()
    {
        map = GameObject.FindGameObjectWithTag("Map").GetComponent<AbstractMap>();
    }
    private void findHandler()
    {
        Handler = GameObject.FindWithTag("ButtonHandler").GetComponent<vars>();
    }
    private void findButtonPanel()
    {
        ButtonPanel = GameObject.FindGameObjectWithTag("ButtonPanel");
    }
    private void findDM()
    {
        DM = GameObject.FindGameObjectWithTag("GameController").GetComponent<targetManager>();
    }
    private void Start()
    {
        if (map == null)
            findMap();
        if (Handler == null)
            findHandler();
        if (ButtonPanel == null)
            findButtonPanel();
        if (DM == null)
            findDM();
        interpolator = EnsureInterpolator();
        if (IsBeingPlaced)
        {
            StartPlacing();
        }
        else // If we are not starting out with actively placing the object, give it a World Anchor
        {
            AttachWorldAnchor();
        }
    }
    
    public void Update()
    {
        if (!IsBeingPlaced) { return; }
        Transform cameraTransform = CameraCache.Main.transform;

        Vector3 placementPosition = GetPlacementPosition(cameraTransform.position, cameraTransform.forward, DefaultGazeDistance);
        // update the placement to match the user's gaze.
        interpolator.SetTargetPosition(placementPosition);

        // Rotate this object to face the user.
        interpolator.SetTargetRotation(Quaternion.Euler(0, cameraTransform.localEulerAngles.y, 0));
    }
    
    public void Menu(bool open)
    {
        if (open)
        {
            ButtonPanel.SetActive(true);
            ButtonPanel.transform.position = Camera.main.transform.position + (Camera.main.transform.forward*1.5f);
            ButtonPanel.transform.rotation = Camera.main.transform.rotation;
        }
        else
        {
            ButtonPanel.SetActive(false);
        }
    }
    public void test()
    {
        
    }
    public void showText(bool show)
    {
        foreach (Transform target in DM.transform)
        {
            target.GetComponent<targetHandler>().showText = show;
        }
    }
    public void updateStatus(bool pause)
    {
        DM.dataSource.pause = pause;
    }
    public void Zoom(bool ZoomIn)
    {
        
        if (map == null)
            findMap();
        if (Handler == null)
        {
            findHandler();
        }
        float zoomAmount = Handler.zoomAmount;
        float currZoom = Handler.CurrZoom;
        if (ZoomIn)
        {
            currZoom = Mathf.Min(20, Mathf.Max(0, currZoom + zoomAmount));
        }
        else
        {
            currZoom = Mathf.Min(20, Mathf.Max(0, currZoom - zoomAmount));
        }
        Handler.CurrZoom = currZoom;
        map.SetZoom(currZoom);
        map.UpdateMap();
    }
    public void Scale(int scaleAmount)
    {
        if (map == null)
            findMap();
        if (Handler == null)
        {
            findHandler();
        }
        var t = new RangeTileProviderOptions();
        t.SetOptions(scaleAmount, scaleAmount, scaleAmount, scaleAmount);
        map.SetExtentOptions(t);
        Handler.GetComponent<vars>().CurrScale = scaleAmount;
    }
    public void Scale(bool Enlarge)
    {
        if (map == null)
            findMap();
        if (Handler == null)
        {
            findHandler();
        }
        int currScale = Handler.CurrScale;
        float scaleAmount = Handler.scaleAmount;
        var t = new RangeTileProviderOptions();
        if (Enlarge)
        {
            currScale = (int)Mathf.Min(100, Mathf.Max(1, currScale + scaleAmount));
        }
        else
        {
            currScale = (int)Mathf.Min(100, Mathf.Max(1, currScale - scaleAmount));
        }
        Handler.GetComponent<vars>().CurrScale = currScale;
        t.SetOptions(currScale, currScale, currScale, currScale);
        map.SetExtentOptions(t);
        map.UpdateMap();
    }
    public void PlaceMap()
    {
        IsBeingPlaced = true;
        HandlePlacement();
    }

    public virtual void OnInputClicked(InputClickedEventData eventData)
    {
        // On each tap gesture, toggle whether the user is in placing mode.
        IsBeingPlaced = !IsBeingPlaced;
        HandlePlacement();
        eventData.Use();
    }

    private void HandlePlacement()
    {
        if (IsBeingPlaced)
        {
            StartPlacing();
        }
        else
        {
            StopPlacing();
        }
    }
    private void StartPlacing()
    {
        var layerCacheTarget = PlaceParentOnTap ? ParentGameObjectToPlace : gameObject;
        layerCacheTarget.SetLayerRecursively(IgnoreRaycastLayer, out layerCache);
        InputManager.Instance.PushModalInputHandler(gameObject);

        ToggleSpatialMesh();
        RemoveWorldAnchor();
    }

    private void StopPlacing()
    {
        var layerCacheTarget = PlaceParentOnTap ? ParentGameObjectToPlace : gameObject;
        layerCacheTarget.ApplyLayerCacheRecursively(layerCache);
        InputManager.Instance.PopModalInputHandler();

        ToggleSpatialMesh();
        AttachWorldAnchor();
    }
    private void AttachWorldAnchor()
    {
        if (WorldAnchorManager.Instance != null)
        {
            // Add world anchor when object placement is done.
            WorldAnchorManager.Instance.AttachAnchor(PlaceParentOnTap ? ParentGameObjectToPlace : gameObject);
        }
    }

    private void RemoveWorldAnchor()
    {
        if (WorldAnchorManager.Instance != null)
        {
            //Removes existing world anchor if any exist.
            WorldAnchorManager.Instance.RemoveAnchor(PlaceParentOnTap ? ParentGameObjectToPlace : gameObject);
        }
    }

    /// <summary>
    /// If the user is in placing mode, display the spatial mapping mesh.
    /// </summary>
    private void ToggleSpatialMesh()
    {
        if (SpatialMappingManager.Instance != null && AllowMeshVisualizationControl)
        {
            SpatialMappingManager.Instance.DrawVisualMeshes = IsBeingPlaced;
        }
    }

    /// <summary>
    /// If we're using the spatial mapping, check to see if we got a hit, else use the gaze position.
    /// </summary>
    /// <returns>Placement position in front of the user</returns>
    private static Vector3 GetPlacementPosition(Vector3 headPosition, Vector3 gazeDirection, float defaultGazeDistance)
    {
        RaycastHit hitInfo;
        if (SpatialMappingRaycast(headPosition, gazeDirection, out hitInfo))
        {
            return hitInfo.point;
        }
        return GetGazePlacementPosition(headPosition, gazeDirection, defaultGazeDistance);
    }

    /// <summary>
    /// Does a raycast on the spatial mapping layer to try to find a hit.
    /// </summary>
    /// <param name="origin">Origin of the raycast</param>
    /// <param name="direction">Direction of the raycast</param>
    /// <param name="spatialMapHit">Result of the raycast when a hit occurred</param>
    /// <returns>Whether it found a hit or not</returns>
    private static bool SpatialMappingRaycast(Vector3 origin, Vector3 direction, out RaycastHit spatialMapHit)
    {
        if (SpatialMappingManager.Instance != null)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(origin, direction, out hitInfo, 30f, SpatialMappingManager.Instance.LayerMask))
            {
                spatialMapHit = hitInfo;
                return true;
            }
        }
        spatialMapHit = new RaycastHit();
        return false;
    }

    /// <summary>
    /// Get placement position either from GazeManager hit or in front of the user as backup
    /// </summary>
    /// <param name="headPosition">Position of the users head</param>
    /// <param name="gazeDirection">Gaze direction of the user</param>
    /// <param name="defaultGazeDistance">Default placement distance in front of the user</param>
    /// <returns>Placement position in front of the user</returns>
    private static Vector3 GetGazePlacementPosition(Vector3 headPosition, Vector3 gazeDirection, float defaultGazeDistance)
    {
        if (GazeManager.Instance.HitObject != null)
        {
            return GazeManager.Instance.HitPosition;
        }
        return headPosition + gazeDirection * defaultGazeDistance;
    }

    /// <summary>
    /// Returns the predefined GameObject or the immediate parent when it exists
    /// </summary>
    /// <returns></returns>
    private GameObject GetParentToPlace()
    {
        if (ParentGameObjectToPlace)
        {
            return ParentGameObjectToPlace;
        }

        return gameObject.transform.parent ? gameObject.transform.parent.gameObject : null;
    }
    /// <summary>
    /// Ensures an interpolator on either the parent or on the GameObject itself and returns it.
    /// </summary>
    private Interpolator EnsureInterpolator()
    {
        var interpolatorHolder = PlaceParentOnTap ? ParentGameObjectToPlace : gameObject;
        return interpolatorHolder.EnsureComponent<Interpolator>();
    }
}
