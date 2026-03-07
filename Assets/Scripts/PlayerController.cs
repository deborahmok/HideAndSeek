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
    
    private HideBox[] allBoxes;
    private int currentBoxIndex;
    
    public bool IsHiding { get; private set; }
    public HideBox CurrentBox { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
    }

    void Start()
    {
        // Cache all boxes sorted by position (left-right, top-bottom for grid navigation)
        allBoxes = FindObjectsOfType<HideBox>();
        SortBoxesByGrid();
    }

    void SortBoxesByGrid()
    {
        // Sort: top-to-bottom, then left-to-right (for 3x3 grid navigation)
        System.Array.Sort(allBoxes, (a, b) =>
        {
            int rowCompare = -a.transform.position.y.CompareTo(b.transform.position.y); // top first
            if (rowCompare != 0) return rowCompare;
            return a.transform.position.x.CompareTo(b.transform.position.x); // left first
        });
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        
        if (IsHiding)
            HandleBoxSelection();
        else
            HandleMovement();
    }

    void HandleMovement()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        
        Vector2 dir = Vector2.zero;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) dir.y += 1;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) dir.y -= 1;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) dir.x -= 1;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x += 1;
        
        dir = dir.normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
    }

    void HandleBoxSelection()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        int col = currentBoxIndex % 3;
        int row = currentBoxIndex / 3;
        int newIndex = currentBoxIndex;

        // Grid navigation: WASD moves selection
        if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
        {
            if (row > 0) newIndex = currentBoxIndex - 3;
        }
        else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
        {
            if (row < 2) newIndex = currentBoxIndex + 3;
        }
        else if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
        {
            if (col > 0) newIndex = currentBoxIndex - 1;
        }
        else if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
        {
            if (col < 2) newIndex = currentBoxIndex + 1;
        }

        if (newIndex != currentBoxIndex)
        {
            SwitchToBox(newIndex);
        }
    }

    void SwitchToBox(int newIndex)
    {
        // Exit current box
        CurrentBox?.OnPlayerExit();
        
        // Enter new box
        currentBoxIndex = newIndex;
        CurrentBox = allBoxes[currentBoxIndex];
        transform.position = CurrentBox.transform.position;
        CurrentBox.OnPlayerEnter();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsHiding) return;
        
        if (other.TryGetComponent<HideBox>(out var box))
        {
            EnterHide(box);
        }
    }

    void EnterHide(HideBox box)
    {
        IsHiding = true;
        CurrentBox = box;
        currentBoxIndex = System.Array.IndexOf(allBoxes, box);
        
        sr.color = hiddenColor;
        if (trail) trail.enabled = false;
        
        transform.position = box.transform.position;
        box.OnPlayerEnter();
    }

    // Keep these for external access if needed
    public void SetNearBox(HideBox box) { }
    public void ClearNearBox(HideBox box) { }
}