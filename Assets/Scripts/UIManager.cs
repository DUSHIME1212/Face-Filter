using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages all UI screens, animations, and transitions
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject appInfoPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button getStartedButton;
    [SerializeField] private Button startARButton;
    [SerializeField] private Button backButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float scaleDuration = 0.3f;
    
    [Header("Student Info")]
    [SerializeField] private TextMeshProUGUI studentNameText;
    [SerializeField] private TextMeshProUGUI cohortText;
    [SerializeField] private TextMeshProUGUI specializationText;
    [SerializeField] private TextMeshProUGUI appDescriptionText;
    
    private CanvasGroup currentPanelCanvasGroup;
    private CanvasGroup nextPanelCanvasGroup;
    
    void Start()
    {
        // Initialize UI
        SetupStudentInfo();
        
        // Setup button listeners
        if (getStartedButton != null)
            getStartedButton.onClick.AddListener(ShowAppInfo);
            
        if (startARButton != null)
            startARButton.onClick.AddListener(LoadARScene);
            
        if (backButton != null)
            backButton.onClick.AddListener(ShowWelcome);
        
        // Show welcome panel first
        ShowPanel(welcomePanel);
        
        // Add button animations
        AddButtonAnimations();
    }
    
    void SetupStudentInfo()
    {
        StudentInfo info = StudentInfo.Instance;
        
        if (studentNameText != null)
            studentNameText.text = info.studentName;
            
        if (cohortText != null)
            cohortText.text = "Cohort: " + info.cohort;
            
        if (specializationText != null)
            specializationText.text = "Track: " + info.specialization;
            
        if (appDescriptionText != null)
            appDescriptionText.text = info.appDescription;
    }
    
    void AddButtonAnimations()
    {
        // Add scale animations to all buttons
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button btn in buttons)
        {
            AnimatedButton animBtn = btn.gameObject.GetComponent<AnimatedButton>();
            if (animBtn == null)
            {
                animBtn = btn.gameObject.AddComponent<AnimatedButton>();
            }
        }
    }
    
    void ShowPanel(GameObject panel)
    {
        // Hide all panels first
        if (welcomePanel != null) welcomePanel.SetActive(false);
        if (appInfoPanel != null) appInfoPanel.SetActive(false);
        
        // Show requested panel
        if (panel != null)
        {
            panel.SetActive(true);
            StartCoroutine(FadeInPanel(panel));
        }
    }
    
    void ShowAppInfo()
    {
        StartCoroutine(TransitionPanels(welcomePanel, appInfoPanel));
    }
    
    void ShowWelcome()
    {
        StartCoroutine(TransitionPanels(appInfoPanel, welcomePanel));
    }
    
    void LoadARScene()
    {
        StartCoroutine(LoadSceneWithTransition("ARFaceFilterScene"));
    }
    
    IEnumerator TransitionPanels(GameObject fromPanel, GameObject toPanel)
    {
        // Fade out current panel
        if (fromPanel != null)
        {
            yield return StartCoroutine(FadeOutPanel(fromPanel));
            fromPanel.SetActive(false);
        }
        
        // Show and fade in next panel
        if (toPanel != null)
        {
            toPanel.SetActive(true);
            yield return StartCoroutine(FadeInPanel(toPanel));
        }
    }
    
    IEnumerator FadeInPanel(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();
        
        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    IEnumerator FadeOutPanel(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();
        
        float elapsedTime = 0f;
        canvasGroup.alpha = 1f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }
    
    IEnumerator LoadSceneWithTransition(string sceneName)
    {
        // Create fade overlay
        GameObject fadeOverlay = new GameObject("FadeOverlay");
        Canvas canvas = fadeOverlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        Image fadeImage = fadeOverlay.AddComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.rectTransform.anchorMin = Vector2.zero;
        fadeImage.rectTransform.anchorMax = Vector2.one;
        fadeImage.rectTransform.sizeDelta = Vector2.zero;
        
        CanvasGroup fadeGroup = fadeOverlay.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        
        // Fade to black
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        
        // Load scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}