using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class EnemyAiBase : MonoBehaviour
{
    public enum EnemyState { Patrol, Investigate, Chase, Attack }

    #region General Variables
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

    [Header("Current State")]
    public EnemyState currentState;
    Vector3 lastKnownPosition;

    [Header("Patroling Stats")]
    [SerializeField] float patrolSpeed = 2f;

    [Header("Investigating Stats")]
    [SerializeField] float investigateSpeed = 3f;
    [SerializeField] float waitTimeAtInvestigation = 4f;
    float investigateTimer;

    [Header("Chasing & Attacking Stats")]
    [SerializeField] float chaseSpeed = 6f;
    [SerializeField] float attackRange = 1.8f;
    [SerializeField] float timeToLoseAggro = 3f;
    public bool inChase;
    float timeSinceLastSeen;
    bool isAttacking;

    [Header("Senses: Vision")]
    [SerializeField] float sightRange = 20f;
    [SerializeField] float fieldOfViewAngle = 120f;

    [Header("Senses: Hearing")]
    [SerializeField] float sprintNoiseRange = 20f;
    [SerializeField] float walkNoiseRange = 8f;
    [SerializeField] float crouchNoiseRange = 1.5f;

    // Velocidad real del jugador
    Vector3 previousPlayerPosition;
    float currentPlayerSpeed;
    #endregion

    private void Awake()
    {
        GameObject playerObj = GameObject.Find("Player");
        target = playerObj.transform;
        playerScript = playerObj.GetComponent<FP_Controller>();
        agent = GetComponent<NavMeshAgent>();

        currentState = EnemyState.Patrol;
        previousPlayerPosition = target.position;

        if (jumpscareUI != null) jumpscareUI.SetActive(false);
    }

    void Update()
    {
        if (isAttacking) return;

        // Calcular velocidad del jugador
        if (Time.deltaTime > 0f)
        {
            currentPlayerSpeed = Vector3.Distance(target.position, previousPlayerPosition) / Time.deltaTime;
        }
        previousPlayerPosition = target.position;

        CheckSenses();
        UpdateState();
        ChooseAnimation();
    }

    // ==========================================
    // SISTEMA DE SENTIDOS
    // ==========================================
    void CheckSenses()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        bool canSeePlayer = false;
        bool canHearPlayer = false;

        // VISIÓN
        if (distanceToPlayer <= sightRange)
        {
            Vector3 enemyEyes = transform.position + Vector3.up * 1.5f;
            Vector3 playerChest = target.position + Vector3.up * 1.0f;

            Vector3 directionToPlayer = (playerChest - enemyEyes).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            float trueDistance = Vector3.Distance(enemyEyes, playerChest);

            if (angleToPlayer < fieldOfViewAngle / 2f)
            {
                if (!Physics.Raycast(enemyEyes, directionToPlayer, trueDistance, obstacleLayer))
                {
                    canSeePlayer = true;
                }
            }
        }

        // OÍDO
        bool playerIsMoving = currentPlayerSpeed > 0.1f;

        if (playerIsMoving)
        {
            float currentNoiseRange = playerScript.isSprinting ? sprintNoiseRange :
                                      playerScript.isCrounching ? crouchNoiseRange : walkNoiseRange;

            if (distanceToPlayer <= currentNoiseRange)
            {
                canHearPlayer = true;
            }
        }

        // DECISIONES
        if (canSeePlayer)
        {
            lastKnownPosition = target.position;
            currentState = EnemyState.Chase;
            timeSinceLastSeen = 0f;
        }
        else
        {
            if (currentState == EnemyState.Chase)
            {
                timeSinceLastSeen += Time.deltaTime;
                if (timeSinceLastSeen >= timeToLoseAggro)
                {
                    currentState = EnemyState.Investigate;
                }
            }
            else if (canHearPlayer && currentState != EnemyState.Chase)
            {
                lastKnownPosition = target.position;
                currentState = EnemyState.Investigate;
            }
        }
    }

    // ==========================================
    // MÁQUINA DE ESTADOS
    // ==========================================
    void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (distanceToPlayer <= attackRange && currentState == EnemyState.Chase)
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

    // ==========================================
    // PATRULLA (WAYPOINTS)
    // ==========================================
   void PatrolLogic()
{
    agent.speed = patrolSpeed;

    if (patrolPoints.Length == 0) return;

    if (!agent.hasPath || HasReachedDestination())
    {
        currentPatrolIndex = Random.Range(0, patrolPoints.Length);
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }
}

    // ==========================================
    // INVESTIGACIÓN
    // ==========================================
    void InvestigateLogic()
    {
        agent.speed = investigateSpeed;

        agent.SetDestination(lastKnownPosition);

        if (HasReachedDestination())
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

    // ==========================================
    // PERSECUCIÓN
    // ==========================================
    void ChaseLogic()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
    }

    bool HasReachedDestination()
    {
        if (agent.pathPending) return false;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            return true;
        }

        return false;
    }

    // ==========================================
    // JUMPSCARE
    // ==========================================
    void CatchPlayer()
    {
        isAttacking = true;
        currentState = EnemyState.Attack;
        agent.isStopped = true;

        playerScript.enabled = false;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        StartCoroutine(JumpscareSequence());
    }

    IEnumerator JumpscareSequence()
    {
        if (jumpscareUI != null) jumpscareUI.SetActive(true);

        if (jumpscareVideo != null) jumpscareVideo.Play();

        AudioManager.Instance.Playsfx(17);

        yield return new WaitForSeconds(jumpscareDuration);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
    void ChooseAnimation()
    {
        if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
        {
            animator.SetBool("Attack", true);
        }
        else
        {
            animator.SetBool("Investigate", true);
            animator.SetBool("Attack", false);
        }
    }
}