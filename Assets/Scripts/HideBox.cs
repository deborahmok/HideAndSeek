using UnityEngine;

public class HideBox : MonoBehaviour
{
    [Header("Colors")]
    public Color normalColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color occupiedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    
    private SpriteRenderer sr;
    public bool IsOccupied { get; private set; }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = normalColor;
    }

    public void OnPlayerEnter()
    {
        IsOccupied = true;
        sr.color = occupiedColor;
    }

    public void OnPlayerExit()
    {
        IsOccupied = false;
        sr.color = normalColor;
    }
}