using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource roamSource;
    public AudioSource buildupFootstepSource;
    public AudioSource chaserWalkSource;
    public AudioSource stabSource;
    public AudioSource heartbeatSource;

    [Header("Roaming / Warning Clips")]
    public AudioClip whistleClip;
    public AudioClip gaspClip;
    public AudioClip doorOpenClip;
    public AudioClip buildupFootstepClip;

    [Header("Chaser In Room Clips")]
    public AudioClip chaserWalkClip;
    public AudioClip stabClip;
    public AudioClip screamClip;

    [Header("Game End Clips")]
    public AudioClip caughtSound;
    public AudioClip winSound;

    [Header("Heartbeat")]
    public AudioClip heartbeatClip;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayWhistle()
    {
        if (roamSource == null || whistleClip == null) return;

        roamSource.clip = whistleClip;
        roamSource.loop = true;
        roamSource.volume = 1f;
        roamSource.pitch = 1f;
        roamSource.Play();
    }

    public void StopWhistle()
    {
        if (roamSource != null)
            roamSource.Stop();
    }

    public void PlayBuildupFootsteps()
    {
        if (buildupFootstepSource == null || buildupFootstepClip == null) return;

        buildupFootstepSource.clip = buildupFootstepClip;
        buildupFootstepSource.loop = true;
        buildupFootstepSource.volume = 0.15f;
        buildupFootstepSource.pitch = 1f;
        buildupFootstepSource.Play();
    }

    public void SetBuildupFootstepIntensity(float normalized)
    {
        if (buildupFootstepSource == null) return;

        normalized = Mathf.Clamp01(normalized);
        buildupFootstepSource.volume = Mathf.Lerp(0.15f, 1f, normalized);
    }

    public void StopBuildupFootsteps()
    {
        if (buildupFootstepSource != null)
            buildupFootstepSource.Stop();
    }

    public void PlayGasp()
    {
        if (sfxSource != null && gaspClip != null)
            sfxSource.PlayOneShot(gaspClip);
    }

    public void PlayDoorOpen()
    {
        if (sfxSource != null && doorOpenClip != null)
            sfxSource.PlayOneShot(doorOpenClip);
    }

    public void PlayChaserWalkLoop()
    {
        if (chaserWalkSource == null || chaserWalkClip == null) return;

        chaserWalkSource.clip = chaserWalkClip;
        chaserWalkSource.loop = true;
        chaserWalkSource.volume = 1f;
        chaserWalkSource.pitch = 1f;
        chaserWalkSource.Play();
    }

    public void StopChaserWalkLoop()
    {
        if (chaserWalkSource != null)
            chaserWalkSource.Stop();
    }

    public void PlayStab()
    {
        if (stabSource != null && stabClip != null)
            stabSource.PlayOneShot(stabClip);
    }

    public void PlaySuccessfulStab()
    {
        if (stabSource != null && stabClip != null)
            stabSource.PlayOneShot(stabClip);

        if (sfxSource != null && screamClip != null)
            sfxSource.PlayOneShot(screamClip);
    }

    public void StopAllChaserAudio()
    {
        StopWhistle();
        StopBuildupFootsteps();
        StopChaserWalkLoop();

        if (stabSource != null)
            stabSource.Stop();
    }

    public void PlayCaught()
    {
        StopAllChaserAudio();
        StopHeartbeat();

        if (sfxSource != null && caughtSound != null)
            sfxSource.PlayOneShot(caughtSound);
    }

    public void PlayWin()
    {
        StopAllChaserAudio();
        StopHeartbeat();

        if (sfxSource != null && winSound != null)
            sfxSource.PlayOneShot(winSound);
    }

    public void SetHeartbeatIntensity(float intensity)
    {
        if (heartbeatSource == null) return;

        if (intensity > 0.1f)
        {
            if (!heartbeatSource.isPlaying && heartbeatClip != null)
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
            heartbeatSource.volume = 0f;
        }
    }

    private void StopHeartbeat()
    {
        if (heartbeatSource != null)
            heartbeatSource.Stop();
    }
}