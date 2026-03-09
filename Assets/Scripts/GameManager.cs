using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    public float hideTime = 10f;
    public float urgentTime = 3f;
    
    [Header("References")]
    // public ChaserController monster;
    // public Transform monsterSpawnPoint;
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

        AudioManager.Instance?.PlayWhistle();
        
        if (darkOverlay)
        {
            darkOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
            darkOverlay.gameObject.SetActive(true);
        }
    }

    // void BeginTimer()
    // {
    //     gameStarted = true;
    //     timerActive = true;
    //     AudioManager.Instance?.PlayTickLoop();
    // }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.rKey.wasPressedThisFrame) RestartGame();
        if (kb != null && kb.escapeKey.wasPressedThisFrame) Application.Quit();

        if (!timerActive || IsGameOver) return;

        if (timer <= 3f && gameStarted)
        {
            gameStarted = false;

            AudioManager.Instance?.StopWhistle();
            AudioManager.Instance?.PlayGasp();
            AudioManager.Instance?.PlayBuildupFootsteps();
        }
        
        // Urgent phase: overlay flash + end sound
        if (timer <= urgentTime)
        {
            UpdateUrgentFlash();
            // AudioManager.Instance?.EnsureTimerEndPlaying();
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

        AudioManager.Instance?.StopBuildupFootsteps();
        AudioManager.Instance?.PlayDoorOpen();
        
        if (darkOverlay)
            darkOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
        
        // AudioManager.Instance?.StopAllTimerSounds();
        
        Invoke(nameof(SpawnMonster), 0.3f);
    }

    void SpawnMonster()
    {
        // if (monster != null && monsterSpawnPoint != null)
        // {
        //     monster.Activate(monsterSpawnPoint.position);
        // }
    }

    public void PlayerCaught()
    {
        if (IsGameOver) return;
        IsGameOver = true;

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

        // At the end of CaughtScreenRoutine, after the fade loop:
        yield return new WaitForSeconds(0.3f); // brief beat before freeze
        Time.timeScale = 0f;
    }

    public void PlayerWin()
    {
        IsGameOver = true;
        // AudioManager.Instance?.PlayWin();
        Debug.Log("You survived!");
        // Add win logic
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}