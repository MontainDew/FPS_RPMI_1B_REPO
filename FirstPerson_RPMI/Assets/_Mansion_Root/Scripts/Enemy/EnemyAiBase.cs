using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class EnemyAiBase : MonoBehaviour
{
    public enum EnemyState { Patrol, Investigate, Chase, Attack }

    #region Variables
    [Header("AI Configuration")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform target;
    [SerializeField] FP_Controller playerScript;
    [SerializeField] LayerMask obstacleLayer;
    public Animator animator;

    [Header("Patrol Points")]
    [SerializeField] Transform[] patrolPoints;
    int currentPatrolIndex = 0;

    [Header("Jumpscare")]
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
    [SerializeField] float attackRange = 1.8f;
    [SerializeField] float timeToLoseAggro = 3f;
    public bool stuned = false; 
    public bool inChase = false;
    float timeSinceLastSeen;
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
    #endregion

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
        if (stuned)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }
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
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        bool canSeePlayer = false;
        bool canHearPlayer = false;

        // ===== VISION =====
        if (distanceToPlayer <= sightRange)
        {
            Vector3 eyes = transform.position + Vector3.up * 1.5f;
            Vector3 playerPos = target.position + Vector3.up;

            Vector3 dir = (playerPos - eyes).normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            float realDist = Vector3.Distance(eyes, playerPos);

            if (angle < fieldOfViewAngle / 2f)
            {
                if (!Physics.Raycast(eyes, dir, realDist, obstacleLayer))
                {
                    canSeePlayer = true;
                }
            }
        }

        // ===== HEARING =====
        bool playerIsMoving = currentPlayerSpeed > 0.1f;

        if (playerIsMoving)
        {
            float noiseRange = playerScript.isSprinting ? sprintNoiseRange :
                               playerScript.isCrounching ? crouchNoiseRange :
                               walkNoiseRange;

            if (distanceToPlayer <= noiseRange)
            {
                canHearPlayer = true;
            }
        }

        // ===== DECISIONES =====

        // 🔥 PRIORIDAD TOTAL: VISIÓN
        if (canSeePlayer)
        {
            lastKnownPosition = target.position;
            currentState = EnemyState.Chase;
            timeSinceLastSeen = 0f;
            return;
        }

        // 🔥 SI LO ESTABA PERSIGUIENDO Y LO PIERDE
        if (currentState == EnemyState.Chase)
        {
            timeSinceLastSeen += Time.deltaTime;

            if (timeSinceLastSeen >= timeToLoseAggro)
            {
                currentState = EnemyState.Investigate;
            }

            return;
        }

        // 🔥 SI ESCUCHA
        if (canHearPlayer)
        {
            lastKnownPosition = target.position;
            currentState = EnemyState.Investigate;
            return;
        }

        // 🔥 SI NO DETECTA NADA
        if (currentState != EnemyState.Investigate)
        {
            currentState = EnemyState.Patrol;
        }
    }

    // ========================= STATES =========================
    void UpdateState()
    {
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange && currentState == EnemyState.Chase)
        {
            CatchPlayer();
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol: PatrolLogic();
                inChase = false;
                break;
            case EnemyState.Investigate: InvestigateLogic();
                inChase = false;
                break;
            case EnemyState.Chase: ChaseLogic();
                inChase = true;
                break;
        }
    }

    void PatrolLogic()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints.Length == 0) return;

        if (!agent.hasPath || HasReached())
        {
            currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void InvestigateLogic()
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

    void ChaseLogic()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
    }

    bool HasReached()
    {
        if (agent.pathPending) return false;

        return agent.remainingDistance <= agent.stoppingDistance + 0.2f;
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
        if (jumpscareUI != null) jumpscareUI.SetActive(true);
        if (jumpscareVideo != null) jumpscareVideo.Play();

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