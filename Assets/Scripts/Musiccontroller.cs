using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages background music playback across all scenes
/// Persists through scene transitions using DontDestroyOnLoad
/// </summary>
public class MusicController : MonoBehaviour
{
    public static MusicController Instance { get; private set; }
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private bool playOnAwake = true;
    
    [Header("UI References")]
    [SerializeField] private Button muteToggleButton;
    [SerializeField] private Image muteIcon;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    
    private AudioSource audioSource;
    private bool isMuted = false;
    
    void Awake()
    {
        // Singleton pattern - ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        ConfigureAudioSource();
    }
    
    void Start()
    {
        // Setup mute toggle button
        if (muteToggleButton != null)
        {
            muteToggleButton.onClick.AddListener(ToggleMute);
        }
        
        // Load saved mute preference
        LoadMutePreference();
        
        // Start playing if enabled
        if (playOnAwake && !isMuted)
        {
            PlayMusic();
        }
        
        UpdateMuteIcon();
    }
    
    void ConfigureAudioSource()
    {
        audioSource.clip = backgroundMusic;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }
    
    public void PlayMusic()
    {
        if (audioSource != null && backgroundMusic != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }
    
    public void ToggleMute()
    {
        isMuted = !isMuted;
        audioSource.mute = isMuted;
        
        // Save preference
        SaveMutePreference();
        
        // Update icon
        UpdateMuteIcon();
        
        // Provide haptic feedback (if available)
        #if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
        #endif
    }
    
    public void SetMute(bool mute)
    {
        isMuted = mute;
        audioSource.mute = isMuted;
        SaveMutePreference();
        UpdateMuteIcon();
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }
    
    void UpdateMuteIcon()
    {
        if (muteIcon != null)
        {
            if (isMuted && soundOffSprite != null)
            {
                muteIcon.sprite = soundOffSprite;
            }
            else if (!isMuted && soundOnSprite != null)
            {
                muteIcon.sprite = soundOnSprite;
            }
        }
    }
    
    void SaveMutePreference()
    {
        PlayerPrefs.SetInt("MusicMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    void LoadMutePreference()
    {
        isMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        audioSource.mute = isMuted;
        
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            volume = PlayerPrefs.GetFloat("MusicVolume");
            audioSource.volume = volume;
        }
    }
    
    public bool IsMuted()
    {
        return isMuted;
    }
    
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
    
    // Call this from the scene that needs to update the button reference
    public void UpdateMuteButton(Button newButton, Image newIcon = null)
    {
        if (muteToggleButton != null)
        {
            muteToggleButton.onClick.RemoveListener(ToggleMute);
        }
        
        muteToggleButton = newButton;
        muteIcon = newIcon;
        
        if (muteToggleButton != null)
        {
            muteToggleButton.onClick.AddListener(ToggleMute);
        }
        
        UpdateMuteIcon();
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}