using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    public float hideTime = 8f;
    public float urgentTime = 3f;
    
    [Header("References")]
    public SpriteRenderer darkOverlay;
    
    [Header("Overlay Settings")]
    public Color overlayColor = new Color(0, 0, 0, 0.7f);
    public float urgentFlashInterval = 0.3f;
    
    private float timer;
    private float flashTimer;
    private bool overlayVisible;
    private bool timerActive;
    private bool gameStarted;
    
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timer = hideTime;
        timerActive = true;
        gameStarted = true;
        
        if (darkOverlay)
        {
            darkOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
            darkOverlay.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.rKey.wasPressedThisFrame) RestartGame();
        if (kb != null && kb.escapeKey.wasPressedThisFrame) Application.Quit();

        if (!timerActive || IsGameOver) return;

        timer -= Time.deltaTime;
        
        if (timer <= urgentTime)
        {
            UpdateUrgentFlash();
        }
        
        if (timer <= 0)
        {
            TimerEnd();
        }
    }

    void UpdateUrgentFlash()
    {
        flashTimer -= Time.deltaTime;
        
        if (flashTimer <= 0)
        {
            overlayVisible = !overlayVisible;
            float alpha = overlayVisible ? overlayColor.a : 0f;

            if (darkOverlay)
                darkOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, alpha);
            
            flashTimer = urgentFlashInterval;
        }
    }

    void TimerEnd()
    {
        timerActive = false;
        
        if (darkOverlay)
            darkOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
        
        Invoke(nameof(SpawnMonster), 0.3f);
    }

    void SpawnMonster()
    {
    }

    public void PlayerCaught()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Debug.Log("CAUGHT! Game Over");

        // Stop whatever's running
        // AudioManager.Instance?.StopBuildupFootsteps();
        CancelInvoke();

        // Camera shake — strong
        CameraShake.Instance?.Shake(0.8f, 0.45f);

        // Flash white first (shock), then hold red
        StartCoroutine(CaughtScreenRoutine());
    }

    IEnumerator CaughtScreenRoutine()
    {
        if (!darkOverlay) yield break;

        // 1. Instant white flash
        darkOverlay.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(0.08f);

        // 2. Slam to red
        darkOverlay.color = new Color(0.7f, 0f, 0f, 0.85f);
        yield return new WaitForSeconds(0.15f);

        // 3. Slowly fade red darker (game over hold)
        float t = 0f;
        Color startColor = darkOverlay.color;
        Color endColor = new Color(0.4f, 0f, 0f, 0.95f);
        while (t < 1f)
        {
            t += Time.deltaTime / 1.2f;
            darkOverlay.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f); // brief beat before freeze
        Time.timeScale = 0f;
    }

    public void PlayerWin()
    {
        IsGameOver = true;
        Debug.Log("You survived!");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}