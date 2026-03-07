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
    
    private GameObject[] allBoxes;
    private int currentBoxIndex;
    
    public bool IsHiding { get; private set; }
    public GameObject CurrentBox { get; private set; }
    public bool MovementLocked { get; set; } // Chaser controls this

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
    }

    void Start()
    {
        allBoxes = GameObject.FindGameObjectsWithTag("Box");
        SortBoxesByGrid();
    }

    void SortBoxesByGrid()
    {
        System.Array.Sort(allBoxes, (a, b) =>
        {
            int rowCompare = -a.transform.position.y.CompareTo(b.transform.position.y);
            if (rowCompare != 0) return rowCompare;
            return a.transform.position.x.CompareTo(b.transform.position.x);
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
        if (MovementLocked) return;

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
        if (MovementLocked) return;
        
        // Current box destroyed? Find nearest valid one
        if (CurrentBox == null || !CurrentBox.activeInHierarchy)
        {
            int nearestIndex = FindNearestActiveBox();
            if (nearestIndex >= 0)
            {
                SwitchToBox(nearestIndex);
            }
            else
            {
                // No boxes left
                IsHiding = false;
                sr.color = normalColor;
                if (trail) trail.enabled = true;
            }
            return;
        }

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        int col = currentBoxIndex % 3;
        int row = currentBoxIndex / 3;
        int newIndex = -1;

        if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
        {
            newIndex = FindActiveBoxInDirection(row, col, -1, 0);
        }
        else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
        {
            newIndex = FindActiveBoxInDirection(row, col, 1, 0);
        }
        else if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
        {
            newIndex = FindActiveBoxInDirection(row, col, 0, -1);
        }
        else if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
        {
            newIndex = FindActiveBoxInDirection(row, col, 0, 1);
        }

        if (newIndex >= 0 && newIndex != currentBoxIndex)
        {
            SwitchToBox(newIndex);
        }
    }

    int FindActiveBoxInDirection(int startRow, int startCol, int rowDir, int colDir)
    {
        int r = startRow + rowDir;
        int c = startCol + colDir;

        while (r >= 0 && r < 3 && c >= 0 && c < 3)
        {
            int index = r * 3 + c;
            if (index < allBoxes.Length && allBoxes[index] != null && allBoxes[index].activeInHierarchy)
            {
                return index;
            }
            r += rowDir;
            c += colDir;
        }

        return -1;
    }

    int FindNearestActiveBox()
    {
        int bestIndex = -1;
        float bestDist = float.MaxValue;

        for (int i = 0; i < allBoxes.Length; i++)
        {
            if (allBoxes[i] != null && allBoxes[i].activeInHierarchy)
            {
                float dist = Vector3.Distance(transform.position, allBoxes[i].transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = i;
                }
            }
        }

        return bestIndex;
    }

    void SwitchToBox(int newIndex)
    {
        // Exit current box
        if (CurrentBox != null && CurrentBox.activeInHierarchy)
        {
            var hideBox = CurrentBox.GetComponent<HideBox>();
            if (hideBox != null) hideBox.OnPlayerExit();
            
            var boxCtrl = CurrentBox.GetComponent<BoxController>();
            if (boxCtrl != null) boxCtrl.playerInside = false;
        }
        
        // Enter new box
        currentBoxIndex = newIndex;
        CurrentBox = allBoxes[currentBoxIndex];
        transform.position = CurrentBox.transform.position;
        
        var newHideBox = CurrentBox.GetComponent<HideBox>();
        if (newHideBox != null) newHideBox.OnPlayerEnter();
        
        var newBoxCtrl = CurrentBox.GetComponent<BoxController>();
        if (newBoxCtrl != null) newBoxCtrl.playerInside = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsHiding) return;
        
        if (other.CompareTag("Box") && other.gameObject.activeInHierarchy)
        {
            EnterHide(other.gameObject);
        }
    }

    void EnterHide(GameObject box)
    {
        IsHiding = true;
        CurrentBox = box;
        currentBoxIndex = System.Array.IndexOf(allBoxes, box);
        
        sr.color = hiddenColor;
        if (trail) trail.enabled = false;
        
        transform.position = box.transform.position;
        
        var hideBox = box.GetComponent<HideBox>();
        if (hideBox != null) hideBox.OnPlayerEnter();
        
        var boxCtrl = box.GetComponent<BoxController>();
        if (boxCtrl != null) boxCtrl.playerInside = true;
    }
}