using UnityEngine;

public class HideBox : MonoBehaviour
{
    [Header("Colors")]
    public Color normalColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color highlightColor = new Color(0.6f, 0.6f, 0.3f, 1f);
    public Color occupiedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    
    [Header("Scale Feedback")]
    public float highlightScale = 1.1f;
    public float scaleSpeed = 8f;
    
    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool playerNear;
    public bool IsOccupied { get; private set; }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        targetScale = originalScale;
        sr.color = normalColor;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            playerNear = true;
            player.SetNearBox(this);
            if (!IsOccupied)
            {
                sr.color = highlightColor;
                targetScale = originalScale * highlightScale;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            playerNear = false;
            player.ClearNearBox(this);
            if (!IsOccupied)
            {
                sr.color = normalColor;
                targetScale = originalScale;
            }
        }
    }

    public void OnPlayerEnter()
    {
        IsOccupied = true;
        sr.color = occupiedColor;
        targetScale = originalScale;
    }

    public void OnPlayerExit()
    {
        IsOccupied = false;
        sr.color = playerNear ? highlightColor : normalColor;
        targetScale = playerNear ? originalScale * highlightScale : originalScale;
    }
}