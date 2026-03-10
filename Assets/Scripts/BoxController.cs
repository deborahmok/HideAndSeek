using UnityEngine;
using System.Collections;

public class BoxController : MonoBehaviour
{
    public bool playerInside = false;
    private Vector3 originalScale;
    [SerializeField] private float jiggleAmount = 0.12f;
    [SerializeField] private float jiggleDuration = 0.25f;
    private SpriteRenderer sr;
    private Vector3 originalPosition;
    [SerializeField] private float bumpAmount = 0.05f;
    [SerializeField] private float bumpDuration = 0.12f;
    [SerializeField] private AudioSource switchBoxAudio;
    
    private void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (switchBoxAudio) switchBoxAudio.Play();

            StartCoroutine(Jiggle());
            StartCoroutine(Bump());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
    
    private IEnumerator Jiggle()
    {
        float elapsed = 0f;

        while (elapsed < jiggleDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Sin((elapsed / jiggleDuration) * Mathf.PI);
            float scaleOffset = t * jiggleAmount;

            transform.localScale = originalScale + new Vector3(scaleOffset, -scaleOffset, 0);

            yield return null;
        }

        transform.localScale = originalScale;
    }
    
    private IEnumerator Bump()
    {
        float elapsed = 0f;

        while (elapsed < bumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bumpDuration;

            float offset = Mathf.Sin(t * Mathf.PI) * bumpAmount;
            transform.position = originalPosition - new Vector3(0, offset, 0);

            yield return null;
        }

        transform.position = originalPosition;
    }
}