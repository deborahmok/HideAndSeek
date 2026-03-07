using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource heartbeatSource;
    
    [Header("Clips")]
    public AudioClip tickSound;
    public AudioClip timerEndSound;
    public AudioClip caughtSound;
    public AudioClip winSound;
    public AudioClip heartbeatClip;

    void Awake()
    {
        Instance = this;
    }

    public void PlayTickStart()
    {
        // Initial setup
    }

    public void PlayTick()
    {
        if (sfxSource && tickSound)
            sfxSource.PlayOneShot(tickSound);
    }

    public void PlayTimerEnd()
    {
        if (sfxSource && timerEndSound)
            sfxSource.PlayOneShot(timerEndSound);
    }

    public void PlayCaught()
    {
        if (sfxSource && caughtSound)
            sfxSource.PlayOneShot(caughtSound);
        StopHeartbeat();
    }

    public void PlayWin()
    {
        if (sfxSource && winSound)
            sfxSource.PlayOneShot(winSound);
        StopHeartbeat();
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