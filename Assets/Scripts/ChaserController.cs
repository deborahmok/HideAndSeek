using UnityEngine;
using System.Collections;

public class ChaserController : MonoBehaviour
{
    [SerializeField] private float appearInterval = 10f;
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
    [SerializeField] private float heartbeatDistance = 3f;
    [SerializeField] private AudioSource stabAudio;
    [SerializeField] private AudioSource bloodJabAudio;
    [SerializeField] private AudioSource doorAudio;
    [SerializeField] private GameObject shadowObject;
    [SerializeField] private AudioSource screamAudio;
    
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private PlayerController player;

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
        player = FindObjectOfType<PlayerController>();
        HideChaser();
        StartCoroutine(ChaserRoutine());
    }

    private IEnumerator JabAttack(Vector3 dir)
    {
        isAttacking = true;

        Vector3 startPos = transform.position;
        Vector3 jabPos = startPos + dir * jabDistance;

        int jabCount = 3;
        if (GameManager.Instance && GameManager.Instance.chaserWalkAudio)
            GameManager.Instance.chaserWalkAudio.Stop();
        for (int i = 0; i < jabCount; i++)
        {   
            if (stabAudio && stabAudio.clip != null) stabAudio.PlayOneShot(stabAudio.clip);
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
                if (bloodJabAudio) bloodJabAudio.Play();
                if (screamAudio) screamAudio.Play();

                if (heartbeatOverlay != null)
                    heartbeatOverlay.StopAllHeartbeat();

                float screamDelay = 0.8f;
                if (screamAudio != null && screamAudio.clip != null)
                    screamDelay = Mathf.Min(screamAudio.clip.length, 1.0f);

                yield return new WaitForSeconds(screamDelay);

                if (screamAudio && screamAudio.isPlaying)
                    screamAudio.Stop();

                GameManager.Instance.PlayerCaught();
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
        if (GameManager.Instance && GameManager.Instance.chaserWalkAudio && !GameManager.Instance.chaserWalkAudio.isPlaying)
        {
            GameManager.Instance.chaserWalkAudio.volume = 0.8f;
            GameManager.Instance.chaserWalkAudio.Play();
        }
        
        Vector3 dirToExit = (exitPoint.position - transform.position).normalized;
        Vector3 stopBeforeExit = exitPoint.position - dirToExit * 1.0f;

        while (Vector3.Distance(transform.position, stopBeforeExit) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                stopBeforeExit,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (doorAudio)
            doorAudio.Play();

        if (GameManager.Instance && GameManager.Instance.chaserWalkAudio)
        {
            AudioSource walk = GameManager.Instance.chaserWalkAudio;

            float startVolume = walk.volume;
            float fadeDuration = 1.2f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                walk.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            walk.Stop();
            walk.volume = startVolume;
        }

        HideChaser();
        if (GameManager.Instance)
            GameManager.Instance.StartNewRound();
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
        if (shadowObject != null) shadowObject.SetActive(true);
        if (player != null) player.MovementLocked = true;

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
        if (shadowObject != null) shadowObject.SetActive(false);
        if (col != null) col.enabled = false;
        if (player != null) player.MovementLocked = false;
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