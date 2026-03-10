using UnityEngine;
using System.Collections;

public class ExitController : MonoBehaviour
{
    public static ExitController Instance { get; private set; }

    [SerializeField] private GameObject exitPrefab;
    [SerializeField] private Vector2[] possiblePositions;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMinScale = 0.95f;
    [SerializeField] private float pulseMaxScale = 1.05f;

    private GameObject spawnedExit;
    private SpriteRenderer sr;
    private Vector3 baseScale;
    private bool isPulsing = false;
    public bool hasSpawned = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isPulsing || spawnedExit == null) return;
        // float s = Mathf.Lerp(pulseMinScale, pulseMaxScale,
        //     (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        // spawnedExit.transform.localScale = baseScale * s;
    }
    public bool HasSpawned => hasSpawned;

    public void SpawnExit()
    {
        if (hasSpawned) return;
        hasSpawned = true;

        Debug.Log("SpawnExit called");

        Vector2 pos = possiblePositions.Length > 0
            ? possiblePositions[Random.Range(0, possiblePositions.Length)]
            : Vector2.zero;

        spawnedExit = Instantiate(exitPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
        sr = spawnedExit.GetComponent<SpriteRenderer>();
        baseScale = spawnedExit.transform.localScale;

        StartCoroutine(SpawnFlash());
    }

    IEnumerator SpawnFlash()
    {
        for (int i = 0; i < 4; i++)
        {
            spawnedExit.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            spawnedExit.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
        isPulsing = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasSpawned) return;
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        other.gameObject.SetActive(false);

        GameManager.Instance?.PlayerWin();
    }
}