using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Controls the filter selection UI and communicates with FaceFilterManager
/// </summary>
public class FilterController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button glassesButton;
    [SerializeField] private Button maskButton;
    [SerializeField] private Button facePaintButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button screenshotButton;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image[] filterButtonImages;
    [SerializeField] private Color selectedColor = new Color(0.3f, 0.7f, 1f);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private GameObject selectionIndicator;
    
    [Header("Status Display")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject faceDetectionIndicator;
    [SerializeField] private Image faceDetectionIcon;
    [SerializeField] private Color detectedColor = Color.green;
    [SerializeField] private Color notDetectedColor = Color.red;
    
    [Header("Filter Menu Animation")]
    [SerializeField] private RectTransform filterMenuPanel;
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private bool animateOnStart = true;
    
    private FaceFilterManager faceFilterManager;
    private ScreenshotManager screenshotManager;
    private FaceFilterManager.FilterType currentSelection = FaceFilterManager.FilterType.None;
    
    void Start()
    {
        // Get managers
        faceFilterManager = FindObjectOfType<FaceFilterManager>();
        screenshotManager = FindObjectOfType<ScreenshotManager>();
        
        if (faceFilterManager == null)
        {
            Debug.LogError("FaceFilterManager not found in scene!");
            return;
        }
        
        // Setup button listeners
        SetupButtons();
        
        // Subscribe to events
        faceFilterManager.OnFaceDetectionChanged += OnFaceDetectionChanged;
        faceFilterManager.OnFilterChanged += OnFilterChanged;
        
        // Initial UI state
        UpdateStatusText("Point camera at your face");
        UpdateSelectionIndicator(FaceFilterManager.FilterType.None);
        
        // Animate menu entrance
        if (animateOnStart && filterMenuPanel != null)
        {
            StartCoroutine(SlideInMenu());
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (faceFilterManager != null)
        {
            faceFilterManager.OnFaceDetectionChanged -= OnFaceDetectionChanged;
            faceFilterManager.OnFilterChanged -= OnFilterChanged;
        }
    }
    
    void SetupButtons()
    {
        if (glassesButton != null)
            glassesButton.onClick.AddListener(() => SelectFilter(FaceFilterManager.FilterType.Glasses));
            
        if (maskButton != null)
            maskButton.onClick.AddListener(() => SelectFilter(FaceFilterManager.FilterType.Mask));
            
        if (facePaintButton != null)
            facePaintButton.onClick.AddListener(() => SelectFilter(FaceFilterManager.FilterType.FacePaint));
            
        if (clearButton != null)
            clearButton.onClick.AddListener(() => SelectFilter(FaceFilterManager.FilterType.None));
            
        if (backButton != null)
            backButton.onClick.AddListener(ReturnToHome);
            
        if (screenshotButton != null)
            screenshotButton.onClick.AddListener(TakeScreenshot);
    }
    
    void SelectFilter(FaceFilterManager.FilterType filterType)
    {
        if (faceFilterManager != null)
        {
            currentSelection = filterType;
            faceFilterManager.ApplyFilter(filterType);
            UpdateSelectionIndicator(filterType);
            
            // Provide haptic feedback
            #if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
            #endif
        }
    }
    
    void UpdateSelectionIndicator(FaceFilterManager.FilterType filterType)
    {
        // Reset all button colors
        ResetButtonColors();
        
        // Highlight selected button
        Image selectedImage = null;
        
        switch (filterType)
        {
            case FaceFilterManager.FilterType.Glasses:
                selectedImage = glassesButton?.GetComponent<Image>();
                break;
            case FaceFilterManager.FilterType.Mask:
                selectedImage = maskButton?.GetComponent<Image>();
                break;
            case FaceFilterManager.FilterType.FacePaint:
                selectedImage = facePaintButton?.GetComponent<Image>();
                break;
            case FaceFilterManager.FilterType.None:
                selectedImage = clearButton?.GetComponent<Image>();
                break;
        }
        
        if (selectedImage != null)
        {
            selectedImage.color = selectedColor;
            
            // Move selection indicator if available
            if (selectionIndicator != null)
            {
                selectionIndicator.transform.SetParent(selectedImage.transform);
                selectionIndicator.transform.localPosition = Vector3.zero;
                selectionIndicator.SetActive(true);
            }
        }
    }
    
    void ResetButtonColors()
    {
        if (glassesButton != null)
            glassesButton.GetComponent<Image>().color = normalColor;
            
        if (maskButton != null)
            maskButton.GetComponent<Image>().color = normalColor;
            
        if (facePaintButton != null)
            facePaintButton.GetComponent<Image>().color = normalColor;
            
        if (clearButton != null)
            clearButton.GetComponent<Image>().color = normalColor;
    }
    
    void OnFaceDetectionChanged(bool detected)
    {
        if (detected)
        {
            UpdateStatusText("Face detected! Select a filter");
            UpdateFaceDetectionIndicator(true);
        }
        else
        {
            UpdateStatusText("No face detected");
            UpdateFaceDetectionIndicator(false);
        }
    }
    
    void OnFilterChanged(FaceFilterManager.FilterType filterType)
    {
        string filterName = filterType.ToString();
        if (filterType == FaceFilterManager.FilterType.None)
        {
            UpdateStatusText("No filter applied");
        }
        else
        {
            UpdateStatusText($"Filter: {filterName}");
        }
    }
    
    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
    
    void UpdateFaceDetectionIndicator(bool detected)
    {
        if (faceDetectionIndicator != null)
        {
            faceDetectionIndicator.SetActive(true);
        }
        
        if (faceDetectionIcon != null)
        {
            faceDetectionIcon.color = detected ? detectedColor : notDetectedColor;
        }
    }
    
    void TakeScreenshot()
    {
        if (screenshotManager != null)
        {
            screenshotManager.CaptureScreenshot();
            UpdateStatusText("Screenshot saved!");
            StartCoroutine(ResetStatusTextAfterDelay(2f));
        }
    }
    
    IEnumerator ResetStatusTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (faceFilterManager != null && faceFilterManager.IsFaceDetected())
        {
            UpdateStatusText("Face detected! Select a filter");
        }
        else
        {
            UpdateStatusText("Point camera at your face");
        }
    }
    
    void ReturnToHome()
    {
        // Load the welcome scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("AppInfoScene");
    }
    
    IEnumerator SlideInMenu()
    {
        if (filterMenuPanel == null)
            yield break;
        
        Vector2 originalPosition = filterMenuPanel.anchoredPosition;
        Vector2 offscreenPosition = originalPosition;
        offscreenPosition.y -= 500f; // Start below screen
        
        filterMenuPanel.anchoredPosition = offscreenPosition;
        
        float elapsedTime = 0f;
        while (elapsedTime < slideInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideInDuration;
            
            // Ease out cubic
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            
            filterMenuPanel.anchoredPosition = Vector2.Lerp(offscreenPosition, originalPosition, progress);
            yield return null;
        }
        
        filterMenuPanel.anchoredPosition = originalPosition;
    }
}