using UnityEngine;

/// <summary>
/// Stores student information that is displayed on the App Info screen
/// This is a ScriptableObject that can be configured in the Unity Inspector
/// </summary>
[CreateAssetMenu(fileName = "StudentInfo", menuName = "Face Filter/Student Info", order = 1)]
public class StudentInfo : ScriptableObject
{
    [Header("Student Details")]
    [Tooltip("Your full name")]
    public string studentName = "DUSHIME Don Aime Hosanna";
    
    [Tooltip("Your cohort (e.g., Cohort 2024)")]
    public string cohort = "Cohort 2024";
    
    [Tooltip("Your specialization track (e.g., AR/VR Development)")]
    public string specialization = "AR/VR Development";
    
    [Header("App Information")]
    [TextArea(3, 6)]
    [Tooltip("Description of your face filter app")]
    public string appDescription = "An immersive AR face filter application featuring multiple filters, " +
                                   "smooth animations, and interactive features. Built with Unity and AR Foundation " +
                                   "for both iOS and Android platforms.";
    
    [Header("Technical Details")]
    public string unityVersion = "2022.3 LTS";
    public string arFramework = "AR Foundation 5.x";
    
    [Header("Features")]
    public string[] features = new string[]
    {
        "Multiple face filters (Glasses, Mask, Face Paint)",
        "Real-time face tracking",
        "Screenshot functionality",
        "Animated UI transitions",
        "Background music with toggle",
        "Smooth filter switching"
    };
    
    // Singleton instance for easy access
    private static StudentInfo instance;
    public static StudentInfo Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<StudentInfo>("StudentInfo");
                if (instance == null)
                {
                    // Create a default instance if not found
                    instance = CreateInstance<StudentInfo>();
                    Debug.LogWarning("StudentInfo not found in Resources. Using default instance.");
                }
            }
            return instance;
        }
    }
    
    /// <summary>
    /// Get features as formatted string
    /// </summary>
    public string GetFeaturesString()
    {
        string result = "Features:\n";
        foreach (string feature in features)
        {
            result += "• " + feature + "\n";
        }
        return result;
    }
    
    /// <summary>
    /// Get complete app info as formatted string
    /// </summary>
    public string GetCompleteInfo()
    {
        return $"Name: {studentName}\n" +
               $"Cohort: {cohort}\n" +
               $"Track: {specialization}\n\n" +
               $"About:\n{appDescription}\n\n" +
               GetFeaturesString();
    }
}