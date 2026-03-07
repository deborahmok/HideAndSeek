using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    public float hideTime = 5f;
    
    [Header("References")]
    // public Monster monster;
    public Transform monsterSpawnPoint;
    public GameObject darkOverlay;
    
    [Header("Overlay Flash")]
    public float flashInterval = 1f;
    public float flashSpeedUp = 0.1f;
    public float minFlashInterval = 0.2f;
    
    private float timer;
    private float flashTimer;
    private float currentFlashInterval;
    private bool timerActive = true;
    private bool overlayVisible;
    
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timer = hideTime;
        currentFlashInterval = flashInterval;
        if (darkOverlay) darkOverlay.SetActive(false);
        AudioManager.Instance?.PlayTickStart();
    }

    void Update()
    {
        if (!timerActive || IsGameOver) return;

        timer -= Time.deltaTime;
        UpdateOverlayFlash();
        
        if (timer <= 0)
        {
            TimerEnd();
        }
    }

    void UpdateOverlayFlash()
    {
        flashTimer -= Time.deltaTime;
        
        // Speed up as time runs out
        float urgency = 1f - (timer / hideTime);
        currentFlashInterval = Mathf.Lerp(flashInterval, minFlashInterval, urgency);
        
        if (flashTimer <= 0)
        {
            overlayVisible = !overlayVisible;
            if (darkOverlay) darkOverlay.SetActive(overlayVisible);
            flashTimer = currentFlashInterval;
            
            if (overlayVisible) AudioManager.Instance?.PlayTick();
        }
    }

    void TimerEnd()
    {
        timerActive = false;
        if (darkOverlay) darkOverlay.SetActive(false);
        AudioManager.Instance?.PlayTimerEnd();
        
        // Spawn monster
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