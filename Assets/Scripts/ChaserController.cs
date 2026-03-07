using UnityEngine;
using System.Collections;

public class ChaserController : MonoBehaviour
{
    [SerializeField] private float appearInterval = 10f;
    [SerializeField] private float visibleDuration = 3f;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        HideChaser();
        StartCoroutine(ChaserRoutine());
    }

    private IEnumerator ChaserRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(appearInterval);
            ShowChaser();
            yield return new WaitForSeconds(visibleDuration);
            HideChaser();
        }
    }

    private void ShowChaser()
    {
        spriteRenderer.enabled = true;
        if (col != null) col.enabled = true;
    }

    private void HideChaser()
    {
        spriteRenderer.enabled = false;
        if (col != null) col.enabled = false;
    }
}