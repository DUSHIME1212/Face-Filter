using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// Manages screenshot capture functionality with visual feedback
/// Saves screenshots to device gallery
/// </summary>
public class ScreenshotManager : MonoBehaviour
{
    [Header("Screenshot Settings")]
    [SerializeField] private int superSize = 1; // Multiplier for resolution
    [SerializeField] private string screenshotPrefix = "FaceFilter_";
    
    [Header("Visual Feedback")]
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private AudioClip shutterSound;
    
    [Header("Save Location")]
    [SerializeField] private bool saveToGallery = true;
    [SerializeField] private bool saveToDocuments = false;
    
    private AudioSource audioSource;
    private bool isCapturing = false;
    
    void Awake()
    {
        // Setup audio source for shutter sound
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = shutterSound;
        
        // Setup flash image if not assigned
        if (flashImage == null)
        {
            flashImage = CreateFlashImage();
        }
        
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(false);
        }
    }
    
    Image CreateFlashImage()
    {
        // Create a canvas for the flash effect
        GameObject flashObj = new GameObject("ScreenshotFlash");
        flashObj.transform.SetParent(transform);
        
        Canvas canvas = flashObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        Image img = flashObj.AddComponent<Image>();
        img.color = Color.white;
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.one;
        img.rectTransform.sizeDelta = Vector2.zero;
        
        return img;
    }
    
    /// <summary>
    /// Capture a screenshot
    /// </summary>
    public void CaptureScreenshot()
    {
        if (!isCapturing)
        {
            StartCoroutine(CaptureScreenshotCoroutine());
        }
    }
    
    IEnumerator CaptureScreenshotCoroutine()
    {
        isCapturing = true;
        
        // Play shutter sound
        if (shutterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }
        
        // Show flash effect
        if (flashImage != null)
        {
            StartCoroutine(FlashEffect());
        }
        
        // Wait for end of frame to capture
        yield return new WaitForEndOfFrame();
        
        // Generate filename with timestamp
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = screenshotPrefix + timestamp + ".png";
        
        // Capture the screenshot
        Texture2D screenshot = CaptureScreen();
        
        if (screenshot != null)
        {
            // Save screenshot
            yield return StartCoroutine(SaveScreenshot(screenshot, filename));
            
            // Clean up
            Destroy(screenshot);
        }
        
        isCapturing = false;
    }
    
    Texture2D CaptureScreen()
    {
        // Create texture to store screenshot
        int width = Screen.width * superSize;
        int height = Screen.height * superSize;
        
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        
        // Read pixels from screen
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();
        
        return screenshot;
    }
    
    IEnumerator SaveScreenshot(Texture2D screenshot, string filename)
    {
        byte[] bytes = screenshot.EncodeToPNG();
        
        #if UNITY_ANDROID || UNITY_IOS
        if (saveToGallery)
        {
            // Save to gallery using NativeGallery plugin or native functionality
            yield return StartCoroutine(SaveToGallery(bytes, filename));
        }
        #else
        // Save to Application.persistentDataPath for other platforms
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Screenshot saved to: {path}");
        #endif
        
        if (saveToDocuments)
        {
            // Also save to documents folder
            string docPath = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllBytes(docPath, bytes);
            Debug.Log($"Screenshot also saved to: {docPath}");
        }
        
        yield return null;
    }
    
    IEnumerator SaveToGallery(byte[] bytes, string filename)
    {
        #if UNITY_ANDROID
        // Save to Android gallery
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, bytes);
        
        // Add to Android gallery
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
        
        AndroidJavaClass mediaStore = new AndroidJavaClass("android.provider.MediaStore$Images$Media");
        mediaStore.CallStatic<string>("insertImage", contentResolver, path, filename, "Face Filter Screenshot");
        
        Debug.Log($"Screenshot saved to Android Gallery: {filename}");
        
        #elif UNITY_IOS
        // Save to iOS photo library
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, bytes);
        
        // This would require NativeGallery plugin or similar
        // For now, just save to persistent data path
        Debug.Log($"Screenshot saved to: {path}");
        Debug.Log("To save to iOS Photos, add NativeGallery plugin");
        
        #endif
        
        yield return null;
    }
    
    IEnumerator FlashEffect()
    {
        if (flashImage == null)
            yield break;
        
        flashImage.gameObject.SetActive(true);
        
        // Fade in quickly
        float elapsedTime = 0f;
        Color startColor = new Color(1f, 1f, 1f, 0f);
        Color peakColor = new Color(1f, 1f, 1f, 0.8f);
        
        while (elapsedTime < flashDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (flashDuration / 2f);
            flashImage.color = Color.Lerp(startColor, peakColor, progress);
            yield return null;
        }
        
        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < flashDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (flashDuration / 2f);
            flashImage.color = Color.Lerp(peakColor, startColor, progress);
            yield return null;
        }
        
        flashImage.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Get the path where screenshots are saved
    /// </summary>
    public string GetScreenshotPath()
    {
        return Application.persistentDataPath;
    }
    
    /// <summary>
    /// Check if currently capturing
    /// </summary>
    public bool IsCapturing()
    {
        return isCapturing;
    }
}