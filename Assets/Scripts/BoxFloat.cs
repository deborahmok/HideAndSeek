using UnityEngine;

public class BoxFloat : MonoBehaviour
{
    public float floatAmount = 0.08f;
    public float floatSpeed = 2f;

    private Vector3 startPos;
    private float phaseOffset;

    void Start()
    {
        startPos = transform.position;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed + phaseOffset) * floatAmount;
        transform.position = startPos + new Vector3(0, offset, 0);
    }
}