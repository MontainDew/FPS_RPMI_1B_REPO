using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class EnemyAiBase : MonoBehaviour
{
    public enum EnemyState { Patrol, Investigate, Chase, Attack }

    [Header("AI Configuration")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform target;
    [SerializeField] FP_Controller playerScript;
    [SerializeField] LayerMask obstacleLayer;
    public Animator animator;

    [Header("Patrol Points")]
    [SerializeField] Transform[] patrolPoints;
    int currentPatrolIndex = 0;

    [Header("Jumpscare & Game Over")]
    [SerializeField] GameObject jumpscareUI;
    [SerializeField] VideoPlayer jumpscareVideo;
    [SerializeField] float jumpscareDuration = 2.5f;
    [SerializeField] string sceneToLoad = "";

    [Header("State")]
    public EnemyState currentState;
    Vector3 lastKnownPosition;

    [Header("Speeds")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float investigateSpeed = 3f;
    [SerializeField] float chaseSpeed = 6f;

    [Header("Investigate")]
    [SerializeField] float waitTimeAtInvestigation = 4f;
    float investigateTimer;

    [Header("Combat")]
    bool isAttacking;

    [Header("Vision")]
    [SerializeField] float sightRange = 20f;
    [SerializeField] float fieldOfViewAngle = 120f;

    [Header("Hearing")]
    [SerializeField] float sprintNoiseRange = 20f;
    [SerializeField] float walkNoiseRange = 8f;
    [SerializeField] float crouchNoiseRange = 1.5f;

    Vector3 previousPlayerPosition;
    float currentPlayerSpeed;

    void Awake()
    {
        GameObject playerObj = GameObject.Find("Player");

        if (playerObj != null)
        {
            target = playerObj.transform;
            playerScript = playerObj.GetComponent<FP_Controller>();
        }

        agent = GetComponent<NavMeshAgent>();
        currentState = EnemyState.Patrol;

        if (target != null)
            previousPlayerPosition = target.position;

        if (jumpscareUI != null)
            jumpscareUI.SetActive(false);
    }

    void Update()
    {
        if (isAttacking || target == null) return;

        CalculatePlayerSpeed();
        CheckSenses();
        UpdateState();
        ChooseAnimation();
    }

    void CalculatePlayerSpeed()
    {
        if (Time.deltaTime > 0f)
        {
            currentPlayerSpeed = Vector3.Distance(target.position, previousPlayerPosition) / Time.deltaTime;
        }

        previousPlayerPosition = target.position;
    }

    // ========================= SENSES =========================
    void CheckSenses()
    {
        float distance = Vector3.Distance(transform.position, target.position);

        bool canSeePlayer = CheckVision(distance);
        bool canHearPlayer = CheckHearing(distance);

        if (canSeePlayer)
        {
            lastKnownPosition = target.position;
            currentState = EnemyState.Chase;
        }
        else if (canHearPlayer)
        {
            lastKnownPosition = target.position;
            currentState = EnemyState.Investigate;
        }
    }

    bool CheckVision(float distance)
    {
        if (distance > sightRange) return false;

        Vector3 eyes = transform.position + Vector3.up * 1.5f;
        Vector3 playerPos = target.position + Vector3.up;

        Vector3 dir = (playerPos - eyes).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle > fieldOfViewAngle / 2f) return false;

        float realDist = Vector3.Distance(eyes, playerPos);

        return !Physics.Raycast(eyes, dir, realDist, obstacleLayer);
    }

    bool CheckHearing(float distance)
    {
        if (currentPlayerSpeed < 0.1f) return false;

        float noiseRange = playerScript.isSprinting ? sprintNoiseRange :
                           playerScript.isCrounching ? crouchNoiseRange :
                           walkNoiseRange;

        return distance <= noiseRange;
    }

    // ========================= STATES =========================
    void UpdateState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol: Patrol(); break;
            case EnemyState.Investigate: Investigate(); break;
            case EnemyState.Chase: Chase(); break;
        }
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints.Length == 0) return;

        if (!agent.hasPath || HasReached())
        {
            currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void Investigate()
    {
        agent.speed = investigateSpeed;
        agent.SetDestination(lastKnownPosition);

        if (HasReached())
        {
            investigateTimer += Time.deltaTime;

            if (investigateTimer >= waitTimeAtInvestigation)
            {
                investigateTimer = 0;
                currentState = EnemyState.Patrol;
                agent.ResetPath();
            }
        }
        else
        {
            investigateTimer = 0;
        }
    }

    void Chase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
    }

    bool HasReached()
    {
        if (agent.pathPending) return false;

        return agent.remainingDistance <= agent.stoppingDistance + 0.2f;
    }

    // ========================= DETECCIÓN POR COLLIDER =========================
    void OnTriggerEnter(Collider other)
    {
        if (isAttacking) return;

        if (other.CompareTag("Player"))
        {
            CatchPlayer();
        }
    }

    // ========================= JUMPSCARE =========================
    void CatchPlayer()
    {
        isAttacking = true;
        currentState = EnemyState.Attack;

        agent.isStopped = true;

        if (playerScript != null)
            playerScript.enabled = false;

        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);

        StartCoroutine(Jumpscare());
    }

    IEnumerator Jumpscare()
    {
        if (jumpscareUI != null)
            jumpscareUI.SetActive(true);

        if (jumpscareVideo != null)
            jumpscareVideo.Play();

        if (AudioManager.Instance != null)
            AudioManager.Instance.Playsfx(17);

        yield return new WaitForSeconds(jumpscareDuration);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(sceneToLoad);
    }

    // ========================= ANIMACIONES =========================
    void ChooseAnimation()
    {
        if (animator == null) return;

        animator.SetBool("Attack", currentState == EnemyState.Chase || currentState == EnemyState.Attack);
        animator.SetBool("Investigate", currentState == EnemyState.Investigate);
    }
}