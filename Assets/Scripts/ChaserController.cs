using UnityEngine;
using System.Collections;

public class ChaserController : MonoBehaviour
{
    [SerializeField] private float appearInterval = 5f;
    [SerializeField] private float visibleDuration = 10f;
    [SerializeField] private float roamDuration = 7f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minX = -7f;
    [SerializeField] private float maxX = 7f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;
    [SerializeField] private float reachDistance = 0.1f;
    [SerializeField] private float jabDistance = 0.5f;
    [SerializeField] private float jabSpeed = 8f;
    [SerializeField] private float attackStopDistance = 0.6f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private HeartbeatOverlayController heartbeatOverlay;
    [SerializeField] private float heartbeatDistance = 3.5f;
    
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private Vector3 targetPosition;
    private bool isVisible = false;
    private bool isAttacking = false;
    private bool hasAttackedThisCycle = false;
    private bool isRoaming = false;
    private float roamTimer = 0f;
    private GameObject lastTargetBox;
    
    private GameObject[] boxes;
    private GameObject targetBox;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        boxes = GameObject.FindGameObjectsWithTag("Box");
        HideChaser();
        StartCoroutine(ChaserRoutine());
    }

    private void Update()
    {
        if (!isVisible || isAttacking)
        {
            if (heartbeatOverlay != null) heartbeatOverlay.StopPulse();
            return;
        }
        
        if (isRoaming)
        {
            roamTimer -= Time.deltaTime;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) <= reachDistance)
            {
                PickNewTarget();
            }

            if (roamTimer <= 0f)
            {
                isRoaming = false;
                ChooseRandomBox();
            }
            UpdateHeartbeat();
            return;
        }

        if (hasAttackedThisCycle || targetBox == null)
        {
            UpdateHeartbeat();
            return;
        }
        
        Vector3 dirToBox = (targetBox.transform.position - transform.position).normalized;
        targetPosition = targetBox.transform.position - dirToBox * attackStopDistance;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) <= reachDistance)
        {
            StartCoroutine(JabAttack(dirToBox));
        }

        UpdateHeartbeat();
    }

    private IEnumerator ChaserRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(appearInterval);

            ShowChaser();

            while (isVisible)
            {
                yield return null;
            }
        }
    }

    private void ShowChaser()
    {
        isVisible = true;
        isAttacking = false;
        hasAttackedThisCycle = false;
        spriteRenderer.enabled = true;
        if (col != null) col.enabled = true;

        PickSpawnPosition();

        isRoaming = true;
        roamTimer = roamDuration;
        PickNewTarget();
    }

    private void HideChaser()
    {
        isVisible = false;
        isAttacking = false;
        spriteRenderer.enabled = false;
        if (col != null) col.enabled = false;
    }

    private void ChooseRandomBox()
    {
        if (boxes == null || boxes.Length == 0) return;

        GameObject[] activeBoxes = System.Array.FindAll(boxes, box => box != null && box.activeInHierarchy);

        if (activeBoxes.Length == 0) return;

        if (activeBoxes.Length == 1)
        {
            targetBox = activeBoxes[0];
            lastTargetBox = targetBox;
            return;
        }

        GameObject chosen;
        do
        {
            int index = Random.Range(0, activeBoxes.Length);
            chosen = activeBoxes[index];
        }
        while (chosen == lastTargetBox);

        targetBox = chosen;
        lastTargetBox = targetBox;
    }

    private void PickSpawnPosition()
    {
        transform.position = spawnPoint.position;
    }

    private IEnumerator JabAttack(Vector3 dir)
    {
        isAttacking = true;

        Vector3 startPos = transform.position;
        Vector3 jabPos = startPos + dir * jabDistance;

        int jabCount = 3;

        for (int i = 0; i < jabCount; i++)
        {
            while (Vector3.Distance(transform.position, jabPos) > 0.02f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    jabPos,
                    jabSpeed * Time.deltaTime
                );
                yield return null;
            }

            while (Vector3.Distance(transform.position, startPos) > 0.02f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    startPos,
                    jabSpeed * Time.deltaTime
                );
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }

        BoxController box = targetBox.GetComponent<BoxController>();

        if (box != null)
        {
            if (box.playerInside)
            {
                Debug.Log("PLAYER CAUGHT");
            }
            else
            {
                targetBox.SetActive(false);
            }
            StartCoroutine(LeaveRoom());
        }

        hasAttackedThisCycle = true;
        isAttacking = false;
    }
    
    private IEnumerator LeaveRoom()
    {
        yield return new WaitForSeconds(2f);

        while (Vector3.Distance(transform.position, exitPoint.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                exitPoint.position,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        HideChaser();
    }
    
    private void PickNewTarget()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        targetPosition = new Vector3(randomX, randomY, transform.position.z);
    }
    
    private void UpdateHeartbeat()
    {
        if (heartbeatOverlay == null || !isVisible)
        {
            return;
        }

        GameObject[] allBoxes = GameObject.FindGameObjectsWithTag("Box");
        BoxController playerBox = null;

        foreach (GameObject boxObj in allBoxes)
        {
            if (!boxObj.activeInHierarchy) continue;

            BoxController bc = boxObj.GetComponent<BoxController>();
            if (bc != null && bc.playerInside)
            {
                playerBox = bc;
                break;
            }
        }

        if (playerBox == null)
        {
            heartbeatOverlay.StopPulse();
            return;
        }

        float dist = Vector3.Distance(transform.position, playerBox.transform.position);

        if (dist <= heartbeatDistance)
        {
            heartbeatOverlay.StartPulse();

            float normalized = 1f - Mathf.Clamp01(dist / heartbeatDistance);
            heartbeatOverlay.SetPulseByDistance(normalized);
        }
        else
        {
            heartbeatOverlay.StopPulse();
        }
    }
}