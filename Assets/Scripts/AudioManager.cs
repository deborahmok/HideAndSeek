using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource tickSource;
    public AudioSource timerEndSource;
    public AudioSource heartbeatSource;
    
    [Header("Timer Clips")]
    public AudioClip timerStartSound;
    public AudioClip tickSound;
    public AudioClip timerEndSound;
    
    [Header("Game End Clips")]
    public AudioClip caughtSound;
    public AudioClip winSound;
    
    [Header("Heartbeat")]
    public AudioClip heartbeatClip;

    private bool timerEndStarted;

    void Awake()
    {
        Instance = this;
    }

    public float PlayTimerStartAndGetLength()
    {
        if (sfxSource && timerStartSound)
        {
            sfxSource.PlayOneShot(timerStartSound);
            return timerStartSound.length;
        }
        return 0f;
    }

    public void PlayTickLoop()
    {
        if (tickSource && tickSound)
        {
            tickSource.clip = tickSound;
            tickSource.loop = true;
            tickSource.Play();
        }
    }

    public void EnsureTimerEndPlaying()
    {
        if (timerEndStarted) return;
        timerEndStarted = true;
        
        // Stop tick, start end sound
        if (tickSource) tickSource.Stop();
        
        if (timerEndSource && timerEndSound)
        {
            timerEndSource.clip = timerEndSound;
            timerEndSource.loop = false;
            timerEndSource.Play();
        }
    }

    public void StopAllTimerSounds()
    {
        if (tickSource) tickSource.Stop();
        if (timerEndSource) timerEndSource.Stop();
    }

    public void PlayCaught()
    {
        StopAllTimerSounds();
        StopHeartbeat();
        if (sfxSource && caughtSound)
            sfxSource.PlayOneShot(caughtSound);
    }

    public void PlayWin()
    {
        StopAllTimerSounds();
        StopHeartbeat();
        if (sfxSource && winSound)
            sfxSource.PlayOneShot(winSound);
    }

    public void SetHeartbeatIntensity(float intensity)
    {
        if (heartbeatSource == null) return;
        
        if (intensity > 0.1f)
        {
            if (!heartbeatSource.isPlaying && heartbeatClip)
            {
                heartbeatSource.clip = heartbeatClip;
                heartbeatSource.loop = true;
                heartbeatSource.Play();
            }
            heartbeatSource.volume = intensity;
            heartbeatSource.pitch = 0.8f + intensity * 0.6f;
        }
        else
        {
            heartbeatSource.volume = 0;
        }
    }

    void StopHeartbeat()
    {
        if (heartbeatSource) heartbeatSource.Stop();
    }
}