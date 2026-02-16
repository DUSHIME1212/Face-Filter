using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using System.Collections.Generic;

/// <summary>
/// Main manager for AR face tracking and filter application
/// Works with AR Foundation to detect faces and apply filters
/// </summary>
[RequireComponent(typeof(ARFaceManager))]
public class FaceFilterManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARFaceManager arFaceManager;
    [SerializeField] private ARSession arSession;
    [SerializeField] private XROrigin xrOrigin;
    
    [Header("Face Filter Prefabs")]
    [SerializeField] private GameObject glassesFilterPrefab;
    [SerializeField] private GameObject maskFilterPrefab;
    [SerializeField] private GameObject facePaintFilterPrefab;
    
    [Header("Current Filter")]
    [SerializeField] private FilterType currentFilterType = FilterType.None;
    
    [Header("Settings")]
    [SerializeField] private bool autoDetectFace = true;
    [SerializeField] private float filterScale = 1.0f;
    
    // Active filter tracking
    private Dictionary<FilterType, GameObject> filterPrefabs;
    private GameObject currentFilterInstance;
    private ARFace trackedFace;
    private bool isFaceDetected = false;
    
    // Events
    public delegate void FaceDetectionEvent(bool detected);
    public event FaceDetectionEvent OnFaceDetectionChanged;
    
    public delegate void FilterChangedEvent(FilterType filterType);
    public event FilterChangedEvent OnFilterChanged;
    
    public enum FilterType
    {
        None,
        Glasses,
        Mask,
        FacePaint
    }
    
    void Awake()
    {
        // Get AR components if not assigned
        if (arFaceManager == null)
            arFaceManager = GetComponent<ARFaceManager>();
            
        if (arSession == null)
            arSession = FindObjectOfType<ARSession>();
            
        if (xrOrigin == null)
            xrOrigin = FindObjectOfType<XROrigin>();
        
        // Initialize filter dictionary
        filterPrefabs = new Dictionary<FilterType, GameObject>
        {
            { FilterType.Glasses, glassesFilterPrefab },
            { FilterType.Mask, maskFilterPrefab },
            { FilterType.FacePaint, facePaintFilterPrefab }
        };
    }
    
    void OnEnable()
    {
        // Subscribe to AR face events
        if (arFaceManager != null)
        {
            arFaceManager.facesChanged += OnFacesChanged;
        }
    }
    
    void OnDisable()
    {
        // Unsubscribe from events
        if (arFaceManager != null)
        {
            arFaceManager.facesChanged -= OnFacesChanged;
        }
    }
    
    void Start()
    {
        // Ensure AR session is running
        if (arSession != null && !arSession.enabled)
        {
            arSession.enabled = true;
        }
    }
    
    void OnFacesChanged(ARFacesChangedEventArgs args)
    {
        // Handle new faces detected
        if (args.added.Count > 0)
        {
            trackedFace = args.added[0];
            isFaceDetected = true;
            OnFaceDetected();
        }
        
        // Handle face updates
        if (args.updated.Count > 0 && trackedFace != null)
        {
            UpdateFilterPosition();
        }
        
        // Handle face removal
        if (args.removed.Count > 0)
        {
            if (trackedFace != null && args.removed.Contains(trackedFace))
            {
                isFaceDetected = false;
                OnFaceLost();
            }
        }
    }
    
    void OnFaceDetected()
    {
        Debug.Log("Face detected!");
        OnFaceDetectionChanged?.Invoke(true);
        
        // Apply current filter if one is selected
        if (currentFilterType != FilterType.None)
        {
            ApplyCurrentFilter();
        }
    }
    
    void OnFaceLost()
    {
        Debug.Log("Face lost!");
        OnFaceDetectionChanged?.Invoke(false);
    }
    
    void UpdateFilterPosition()
    {
        if (currentFilterInstance != null && trackedFace != null)
        {
            // Update filter position and rotation to match face
            currentFilterInstance.transform.position = trackedFace.transform.position;
            currentFilterInstance.transform.rotation = trackedFace.transform.rotation;
        }
    }
    
    /// <summary>
    /// Apply a specific filter type
    /// </summary>
    public void ApplyFilter(FilterType filterType)
    {
        if (filterType == currentFilterType)
            return;
        
        // Remove current filter
        RemoveCurrentFilter();
        
        // Apply new filter
        currentFilterType = filterType;
        
        if (filterType != FilterType.None && isFaceDetected)
        {
            ApplyCurrentFilter();
        }
        
        OnFilterChanged?.Invoke(filterType);
    }
    
    void ApplyCurrentFilter()
    {
        if (trackedFace == null || currentFilterType == FilterType.None)
            return;
        
        // Get the prefab for the current filter type
        if (filterPrefabs.ContainsKey(currentFilterType) && filterPrefabs[currentFilterType] != null)
        {
            // Instantiate the filter
            currentFilterInstance = Instantiate(
                filterPrefabs[currentFilterType],
                trackedFace.transform.position,
                trackedFace.transform.rotation
            );
            
            // Parent to the face so it follows automatically
            currentFilterInstance.transform.SetParent(trackedFace.transform);
            currentFilterInstance.transform.localScale = Vector3.one * filterScale;
            
            Debug.Log($"Applied filter: {currentFilterType}");
        }
        else
        {
            Debug.LogWarning($"Filter prefab not found for type: {currentFilterType}");
        }
    }
    
    void RemoveCurrentFilter()
    {
        if (currentFilterInstance != null)
        {
            Destroy(currentFilterInstance);
            currentFilterInstance = null;
        }
    }
    
    /// <summary>
    /// Remove all filters
    /// </summary>
    public void ClearFilters()
    {
        RemoveCurrentFilter();
        currentFilterType = FilterType.None;
        OnFilterChanged?.Invoke(FilterType.None);
    }
    
    /// <summary>
    /// Check if a face is currently being tracked
    /// </summary>
    public bool IsFaceDetected()
    {
        return isFaceDetected;
    }
    
    /// <summary>
    /// Get the current filter type
    /// </summary>
    public FilterType GetCurrentFilter()
    {
        return currentFilterType;
    }
    
    /// <summary>
    /// Get the tracked face (if any)
    /// </summary>
    public ARFace GetTrackedFace()
    {
        return trackedFace;
    }
    
    /// <summary>
    /// Set the filter scale
    /// </summary>
    public void SetFilterScale(float scale)
    {
        filterScale = scale;
        if (currentFilterInstance != null)
        {
            currentFilterInstance.transform.localScale = Vector3.one * filterScale;
        }
    }
}