using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartbeatOverlayController : MonoBehaviour
{
    [SerializeField] private float farBeatInterval = 1.2f;
    [SerializeField] private float nearBeatInterval = 0.45f;

    [SerializeField] private float farMaxAlpha = 0.12f;
    [SerializeField] private float nearMaxAlpha = 0.32f;

    [SerializeField] private float riseDuration = 0.18f;
    [SerializeField] private float fallDuration = 0.35f;

    [SerializeField] private Transform player;
    [SerializeField] private Transform chaser;
    
    [SerializeField] private AudioSource heartbeatAudio;
    
    private Image overlayImage;
    private bool isPulsing = false;
    private float currentBeatInterval;
    private float currentMaxAlpha;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        overlayImage = GetComponent<Image>();
        SetAlpha(0f);
    }

    void Update()
    {
        if (chaser == null || player == null) return;

        if (!chaser.gameObject.activeInHierarchy)
        {
            SetAlpha(0f);

            if (heartbeatAudio && heartbeatAudio.isPlaying)
                heartbeatAudio.Stop();

            return;
        }

        float dist = Vector3.Distance(player.position, chaser.position);

        float maxDist = 5f;

        if (dist > maxDist)
        {
            SetAlpha(0f);
            return;
        }

        float t = Mathf.Clamp01(1f - dist / maxDist);
        if (heartbeatAudio)
        {
            if (!heartbeatAudio.isPlaying)
                heartbeatAudio.Play();

            heartbeatAudio.volume = Mathf.Lerp(0.05f, 0.7f, t);
            heartbeatAudio.pitch = Mathf.Lerp(0.8f, 1.5f, t);
        }
        
        float pulseSpeed = Mathf.Lerp(0.7f, 2.2f, t);
        heartbeatAudio.pitch = Mathf.Lerp(0.9f, 1.2f, t);
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        float alpha = Mathf.Lerp(0.04f, 0.35f, pulse * t);

        SetAlpha(alpha);
    }
    
    public void StartPulse()
    {
        if (isPulsing) return;

        isPulsing = true;

        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
        }

        pulseRoutine = StartCoroutine(PulseLoop());
    }

    public void StopPulse()
    {
        isPulsing = false;

        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        SetAlpha(0f);
    }

    public void SetPulseByDistance(float normalizedCloseness)
    {
        normalizedCloseness = Mathf.Clamp01(normalizedCloseness);

        currentBeatInterval = Mathf.Lerp(farBeatInterval, nearBeatInterval, normalizedCloseness);
        currentMaxAlpha = Mathf.Lerp(farMaxAlpha, nearMaxAlpha, normalizedCloseness);
    }

    private IEnumerator PulseLoop()
    {
        while (isPulsing)
        {
            yield return StartCoroutine(FadeAlpha(0f, currentMaxAlpha, riseDuration));
            yield return StartCoroutine(FadeAlpha(currentMaxAlpha, 0f, fallDuration));
            yield return new WaitForSeconds(currentBeatInterval);
        }
    }

    private IEnumerator FadeAlpha(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(endAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (overlayImage == null) return;

        Color c = overlayImage.color;
        c.a = alpha;
        overlayImage.color = c;
    }
    
    public void StopAllHeartbeat()
    {
        SetAlpha(0f);

        if (heartbeatAudio && heartbeatAudio.isPlaying)
            heartbeatAudio.Stop();
    }
}