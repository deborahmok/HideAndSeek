using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    public float hideTime = 8f;
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
        timerActive = false;
        gameStarted = false;
        
        if (darkOverlay)
        {
            darkOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
            darkOverlay.gameObject.SetActive(true);
        }
        
        // Play start sound, then begin timer when it ends
        float startSoundLength = AudioManager.Instance?.PlayTimerStartAndGetLength() ?? 0f;
        Invoke(nameof(BeginTimer), startSoundLength);
    }

    void BeginTimer()
    {
        gameStarted = true;
        timerActive = true;
        AudioManager.Instance?.PlayTickLoop();
    }

    void Update()
    {
        if (!timerActive || IsGameOver) return;

        timer -= Time.deltaTime;
        
        // Urgent phase: overlay flash + end sound
        if (timer <= urgentTime)
        {
            UpdateUrgentFlash();
            AudioManager.Instance?.EnsureTimerEndPlaying();
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
        
        AudioManager.Instance?.StopAllTimerSounds();
        
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
        IsGameOver = true;
        AudioManager.Instance?.PlayCaught();
        Debug.Log("CAUGHT! Game Over");
        // Add game over logic
    }

    public void PlayerWin()
    {
        IsGameOver = true;
        AudioManager.Instance?.PlayWin();
        Debug.Log("You survived!");
        // Add win logic
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}