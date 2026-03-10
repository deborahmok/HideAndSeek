using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        other.gameObject.SetActive(false);
        GameManager.Instance?.PlayerWin();
    }
}