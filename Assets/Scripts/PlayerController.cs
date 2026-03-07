using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    
    [Header("Visual")]
    public Color normalColor = Color.cyan;
    public Color hiddenColor = new Color(0.2f, 0.8f, 0.8f, 0.5f);
    
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private TrailRenderer trail;
    
    public bool IsHiding { get; private set; }
    public HideBox CurrentBox { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        
        HandleMovement();
        HandleHideInput();
    }

    void HandleMovement()
    {
        if (IsHiding) return;
        
        Vector2 dir = Vector2.zero;
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) dir.y += 1;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) dir.y -= 1;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) dir.x -= 1;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x += 1;
        
        dir = dir.normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
    }

    void HandleHideInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        
        if (kb.spaceKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
        {
            if (IsHiding)
                ExitHide();
            else if (CurrentBox != null)
                EnterHide();
        }
    }

    void EnterHide()
    {
        IsHiding = true;
        sr.color = hiddenColor;
        if (trail) trail.enabled = false;
        transform.position = CurrentBox.transform.position;
        CurrentBox.OnPlayerEnter();
    }

    void ExitHide()
    {
        IsHiding = false;
        sr.color = normalColor;
        if (trail) trail.enabled = true;
        CurrentBox?.OnPlayerExit();
    }

    public void SetNearBox(HideBox box)
    {
        CurrentBox = box;
    }

    public void ClearNearBox(HideBox box)
    {
        if (CurrentBox == box) CurrentBox = null;
    }
}