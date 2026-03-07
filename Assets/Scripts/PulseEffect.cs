using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.1f;
    
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }
}