using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Adds scale and color animations to buttons
/// </summary>
[RequireComponent(typeof(Button))]
public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animationSpeed = 10f;
    
    [Header("Color Animation")]
    [SerializeField] private bool useColorAnimation = true;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 1f);
    [SerializeField] private Color pressColor = new Color(0.8f, 0.8f, 0.9f);
    
    [Header("Bounce Effect")]
    [SerializeField] private bool useBounceEffect = true;
    [SerializeField] private float bounceAmount = 0.2f;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image buttonImage;
    private Color targetColor;
    private bool isPressed = false;
    private bool isHovering = false;
    
    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        buttonImage = GetComponent<Image>();
        
        if (buttonImage != null && useColorAnimation)
        {
            normalColor = buttonImage.color;
            targetColor = normalColor;
        }
    }
    
    void Start()
    {
        // Add entry animation
        if (useBounceEffect)
        {
            StartCoroutine(BounceIn());
        }
    }
    
    void Update()
    {
        // Smooth scale animation
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        
        // Smooth color animation
        if (buttonImage != null && useColorAnimation)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isPressed)
        {
            isHovering = true;
            targetScale = originalScale * hoverScale;
            if (useColorAnimation)
                targetColor = hoverColor;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (!isPressed)
        {
            targetScale = originalScale;
            if (useColorAnimation)
                targetColor = normalColor;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        targetScale = originalScale * pressScale;
        if (useColorAnimation)
            targetColor = pressColor;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        if (isHovering)
        {
            targetScale = originalScale * hoverScale;
            if (useColorAnimation)
                targetColor = hoverColor;
        }
        else
        {
            targetScale = originalScale;
            if (useColorAnimation)
                targetColor = normalColor;
        }
        
        // Add click bounce
        if (useBounceEffect)
        {
            StartCoroutine(ClickBounce());
        }
    }
    
    IEnumerator BounceIn()
    {
        transform.localScale = Vector3.zero;
        float elapsedTime = 0f;
        float duration = 0.5f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // Bounce curve
            float scale = Mathf.Sin(progress * Mathf.PI);
            if (progress < 0.5f)
            {
                scale = Mathf.Lerp(0f, 1f + bounceAmount, progress * 2f);
            }
            else
            {
                scale = Mathf.Lerp(1f + bounceAmount, 1f, (progress - 0.5f) * 2f);
            }
            
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    IEnumerator ClickBounce()
    {
        Vector3 startScale = transform.localScale;
        Vector3 bounceScale = originalScale * (1f + bounceAmount * 0.3f);
        
        float duration = 0.15f;
        float elapsedTime = 0f;
        
        // Bounce up
        while (elapsedTime < duration / 2f)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, bounceScale, (elapsedTime / (duration / 2f)));
            yield return null;
        }
        
        // Bounce down
        elapsedTime = 0f;
        while (elapsedTime < duration / 2f)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(bounceScale, targetScale, (elapsedTime / (duration / 2f)));
            yield return null;
        }
    }
    
    void OnDisable()
    {
        transform.localScale = originalScale;
        if (buttonImage != null && useColorAnimation)
        {
            buttonImage.color = normalColor;
        }
    }
}