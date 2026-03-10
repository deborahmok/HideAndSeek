using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Debug")]
    public int BoxesToExit = 3; 

    [Header("Timer")]
    public float hideTime = 8f;
    public float urgentTime = 3f;
    
    [Header("References")]
    public SpriteRenderer darkOverlay;
    public AudioSource whistleAudio;
    public AudioSource gaspAudio;
    public AudioSource footstepBuildupAudio;
    public AudioSource doorAudio;
    public AudioSource chaserWalkAudio;
    [SerializeField] private AudioSource reliefAudio;
    [SerializeField] private AudioSource gameOverAudio;
    [SerializeField] private AudioSource winAudio;
    
    [Header("Overlay Settings")]
    public Color overlayColor = new Color(0, 0, 0, 0.7f);
    public float urgentFlashInterval = 0.3f;
    
    private float timer;
    private float flashTimer;
    private bool overlayVisible;
    private bool timerActive;
    private bool gameStarted;
    private bool urgentAudioPlayed;
    private bool doorPlayed;
    
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
        if (whistleAudio) whistleAudio.Play();
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
        if (timer <= 2f && !doorPlayed)
        {
            doorPlayed = true;

            if (footstepBuildupAudio && footstepBuildupAudio.isPlaying)
                footstepBuildupAudio.Stop();

            if (doorAudio)
                doorAudio.Play();
        }
        
        if (timer <= urgentTime)
        {
            // UpdateUrgentFlash();
            if (footstepBuildupAudio)
            {
                float progress = 1f - Mathf.Clamp01(timer / urgentTime);
                footstepBuildupAudio.volume = Mathf.Lerp(0.2f, 1f, progress);
            }
            if (!urgentAudioPlayed)
            {
                urgentAudioPlayed = true;

                if (whistleAudio && whistleAudio.isPlaying)
                    whistleAudio.Stop();

                if (footstepBuildupAudio)
                    footstepBuildupAudio.Play();

                Invoke(nameof(PlayGasp), 1f);
            }
        }
        
        if (timer <= 0)
        {
            TimerEnd();
        }
    }
    
    void PlayGasp()
    {
        if (gaspAudio)
            gaspAudio.Play();
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

        if (footstepBuildupAudio && footstepBuildupAudio.isPlaying)
            footstepBuildupAudio.Stop();
        
        if (darkOverlay)
            darkOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
        
        Invoke(nameof(SpawnMonster), 0.3f);
    }

    void SpawnMonster()
    {
        if (chaserWalkAudio)
            chaserWalkAudio.Play();
    }

    public void PlayerCaught()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        timerActive = false;
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.MovementLocked = true;
        
        Debug.Log("CAUGHT! Game Over");

        if (whistleAudio && whistleAudio.isPlaying)
            whistleAudio.Stop();

        if (footstepBuildupAudio && footstepBuildupAudio.isPlaying)
            footstepBuildupAudio.Stop();

        if (chaserWalkAudio && chaserWalkAudio.isPlaying)
            chaserWalkAudio.Stop();

        if (reliefAudio && reliefAudio.isPlaying)
            reliefAudio.Stop();

        CancelInvoke();

        CameraShake.Instance?.Shake(0.8f, 0.45f);

        if (gameOverAudio)
        {
            gameOverAudio.Play();
            Invoke(nameof(RestartGame), gameOverAudio.clip.length);
        }
        else
        {
            Invoke(nameof(RestartGame), 3f);
        }

        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        if (gameOverAudio)
            gameOverAudio.Play();

        yield return StartCoroutine(CaughtScreenRoutine());

        float waitTime = gameOverAudio ? gameOverAudio.clip.length : 2f;
        yield return new WaitForSecondsRealtime(waitTime);

        RestartGame();
    }
    
    IEnumerator CaughtScreenRoutine()
    {
        if (!darkOverlay) yield break;

        darkOverlay.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSecondsRealtime(0.08f);

        darkOverlay.color = new Color(0.7f, 0f, 0f, 0.85f);
        yield return new WaitForSecondsRealtime(0.15f);

        float t = 0f;
        Color startColor = darkOverlay.color;
        Color endColor = new Color(0.4f, 0f, 0f, 0.95f);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 1.2f;
            darkOverlay.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.3f);
        Time.timeScale = 0f;
    }

    public void PlayerWin()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        timerActive = false;

        Debug.Log("You survived!");

        if (whistleAudio && whistleAudio.isPlaying)
            whistleAudio.Stop();

        if (footstepBuildupAudio && footstepBuildupAudio.isPlaying)
            footstepBuildupAudio.Stop();

        if (chaserWalkAudio && chaserWalkAudio.isPlaying)
            chaserWalkAudio.Stop();

        if (reliefAudio && reliefAudio.isPlaying)
            reliefAudio.Stop();

        CancelInvoke();

        ChaserController chaser = FindFirstObjectByType<ChaserController>();
        if (chaser != null)
        {
            chaser.StopAllCoroutines();
            chaser.gameObject.SetActive(false);
        }

        if (winAudio)
        {
            winAudio.Play();
            Invoke(nameof(RestartGame), winAudio.clip.length);
        }
        else
        {
            Invoke(nameof(RestartGame), 2f);
        }
    }
    
    public void NotifyBoxDestroyed()
    {
        int count = 0;
        foreach (var go in GameObject.FindGameObjectsWithTag("Box"))
            if (go.activeInHierarchy) count++;
        
        count = GameObject.FindGameObjectsWithTag("Box").Length;
    
        Debug.Log($"Active boxes remaining: {count}");

        if (count <= BoxesToExit)
            ExitController.Instance?.SpawnExit();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void StartNewRound()
    {
        timer = hideTime;
        flashTimer = 0f;
        overlayVisible = false;
        timerActive = true;
        urgentAudioPlayed = false;
        doorPlayed = false;
        
        if (darkOverlay)
            darkOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);

        Invoke(nameof(PlayReliefThenWhistle), 0.8f);
    }
    
    void PlayReliefThenWhistle()
    {
        if (reliefAudio)
        {
            reliefAudio.Play();
            Invoke(nameof(StartWhistle), reliefAudio.clip.length);
        }
        else
        {
            Invoke(nameof(StartWhistle), 1f);
        }
    }
    
    void StartWhistle()
    {
        if (whistleAudio)
            whistleAudio.Play();
    }
}