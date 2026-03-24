using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // Necesario para reiniciar o cambiar de escena
using UnityEngine.Video; // Necesario para reproducir el vídeo del jumpscare
using System.Collections; // Necesario para las corrutinas (esperar tiempo)

public class EnemyAiBase : MonoBehaviour
{
    public enum EnemyState { Patrol, Investigate, Chase, Attack }

    #region General Variables
    [Header("AI Configuration")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform target;
    [SerializeField] FP_Controller playerScript;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask obstacleLayer; // Capa para las paredes

    [Header("Jumpscare & Game Over")]
    [SerializeField] GameObject jumpscareUI; // El Canvas o Panel que contiene tu vídeo/imagen
    [SerializeField] VideoPlayer jumpscareVideo; // Tu reproductor de vídeo
    [SerializeField] float jumpscareDuration = 2.5f; // Cuánto dura el vídeo antes de cargar escena
    [SerializeField] string sceneToLoad = ""; // Si lo dejas vacío, recarga la escena actual

    [Header("Current State")]
    public EnemyState currentState;
    Vector3 lastKnownPosition;

    [Header("Patroling Stats")]
    [SerializeField] float walkPointRange = 15f;
    [SerializeField] float patrolSpeed = 2f;
    Vector3 walkPoint;
    bool walkPointSet;

    [Header("Investigating Stats")]
    [SerializeField] float investigateSpeed = 3f;
    [SerializeField] float waitTimeAtInvestigation = 4f;
    float investigateTimer;

    [Header("Chasing & Attacking Stats")]
    [SerializeField] float chaseSpeed = 6f;
    [SerializeField] float attackRange = 1.8f;
    [SerializeField] float timeToLoseAggro = 3f;
    float timeSinceLastSeen;
    bool isAttacking;

    [Header("Senses: Vision")]
    [SerializeField] float sightRange = 20f;
    [SerializeField] float fieldOfViewAngle = 120f;

    [Header("Senses: Hearing")]
    [SerializeField] float sprintNoiseRange = 20f;
    [SerializeField] float walkNoiseRange = 8f;
    [SerializeField] float crouchNoiseRange = 1.5f;

    [Header("Stuck Detection")]
    [SerializeField] float stuckCheckTime = 2f;
    [SerializeField] float stuckThreshold = 0.1f;
    [SerializeField] float maxStuckDuration = 3f;
    float stuckTimer;
    float lastCheckTime;
    Vector3 lastPosition;

    // --- VARIABLES PARA VELOCIDAD REAL ---
    Vector3 previousPlayerPosition;
    float currentPlayerSpeed;
    #endregion

    private void Awake()
    {
        GameObject playerObj = GameObject.Find("Player");
        target = playerObj.transform;
        playerScript = playerObj.GetComponent<FP_Controller>();
        agent = GetComponent<NavMeshAgent>();

        lastPosition = transform.position;
        lastCheckTime = Time.time;
        currentState = EnemyState.Patrol;
        previousPlayerPosition = target.position;

        // Asegurarnos de que el UI del jumpscare esté apagado al empezar a jugar
        if (jumpscareUI != null) jumpscareUI.SetActive(false);
    }

    void Update()
    {
        if (isAttacking) return; // Si ya te atrapó, deja de pensar

        // Calcula la velocidad real del jugador para el sonido
        if (Time.deltaTime > 0f)
        {
            currentPlayerSpeed = Vector3.Distance(target.position, previousPlayerPosition) / Time.deltaTime;
        }
        previousPlayerPosition = target.position;

        CheckSenses();
        UpdateState();
        CheckIfStuck();
    }

    // ==========================================
    // SISTEMA DE SENTIDOS (VISTA Y OÍDO)
    // ==========================================
    void CheckSenses()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        bool canSeePlayer = false;
        bool canHearPlayer = false;

        // --- VISIÓN ---
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

        // --- OÍDO ---
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

        // --- TOMA DE DECISIONES ---
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
            CatchPlayer(); // TE ATRAPÓ
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol: PatrolLogic(); break;
            case EnemyState.Investigate: InvestigateLogic(); break;
            case EnemyState.Chase: ChaseLogic(); break;
        }
    }

    void PatrolLogic()
    {
        agent.speed = patrolSpeed;

        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        if (walkPointSet && HasReachedDestination())
        {
            walkPointSet = false;
        }
    }

    void InvestigateLogic()
    {
        agent.speed = investigateSpeed;

        if (Vector3.Distance(agent.destination, lastKnownPosition) > 1f)
        {
            agent.SetDestination(lastKnownPosition);
        }

        if (HasReachedDestination())
        {
            investigateTimer += Time.deltaTime;
            if (investigateTimer >= waitTimeAtInvestigation)
            {
                investigateTimer = 0;
                currentState = EnemyState.Patrol;
                walkPointSet = false;
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

        if (Vector3.Distance(agent.destination, lastKnownPosition) > 0.5f)
        {
            agent.SetDestination(lastKnownPosition);
        }

        if (HasReachedDestination())
        {
            currentState = EnemyState.Investigate;
        }
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

    void SearchWalkPoint()
    {
        int attempts = 0;
        const int maxAttempts = 5;

        while (!walkPointSet && attempts < maxAttempts)
        {
            attempts++;
            Vector3 randomPoint = transform.position + new Vector3(Random.Range(-walkPointRange, walkPointRange), 0, Random.Range(-walkPointRange, walkPointRange));

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                walkPoint = hit.position;
                if (Physics.Raycast(walkPoint + Vector3.up * 2, -Vector3.up, 3f, groundLayer))
                {
                    walkPointSet = true;
                    agent.SetDestination(walkPoint);
                }
            }
        }
    }

    // ==========================================
    // LÓGICA DE JUMPSCARE Y GAME OVER
    // ==========================================
    void CatchPlayer()
    {
        isAttacking = true;
        currentState = EnemyState.Attack;
        agent.isStopped = true;

        // 1. Bloqueamos al jugador para que no pueda moverse ni mover la cámara
        playerScript.enabled = false;

        // 2. Hacemos que el enemigo mire fijamente al jugador
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        // 3. Iniciamos la secuencia de Jumpscare (Video + Recarga de escena)
        StartCoroutine(JumpscareSequence());
    }

    IEnumerator JumpscareSequence()
    {
        // Encendemos el panel del vídeo/animación
        if (jumpscareUI != null) jumpscareUI.SetActive(true);

        // Si hay un vídeo asignado, lo reproducimos
        if (jumpscareVideo != null) jumpscareVideo.Play();

        // Esperamos el tiempo que dure el vídeo (jumpscareDuration)
        yield return new WaitForSeconds(jumpscareDuration);

        // Liberamos el cursor por si vas a un menú principal
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Si dejaste la variable vacía en el inspector, recarga la escena actual
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else // Si escribiste un nombre (ej: "Menu"), carga esa escena
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // ==========================================
    // SISTEMA ANTI-ATASCOS Y GIZMOS
    // ==========================================
    void CheckIfStuck()
    {
        if (Time.time - lastCheckTime > stuckCheckTime)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < stuckThreshold && agent.hasPath && !agent.pathPending)
            {
                stuckTimer += stuckCheckTime;
            }
            else
            {
                stuckTimer = 0;
            }

            if (stuckTimer >= maxStuckDuration)
            {
                walkPointSet = false;
                agent.ResetPath();
                stuckTimer = 0;
            }

            lastPosition = transform.position;
            lastCheckTime = Time.time;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        // Visión Visual
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, leftBoundary * sightRange);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, rightBoundary * sightRange);

        // Rangos de Ruido (Esferas visuales para que puedas calibrar el tamaño)
        Gizmos.color = new Color(1, 0, 0, 0.1f); // Rojo: Esprintar
        Gizmos.DrawWireSphere(transform.position, sprintNoiseRange);

        Gizmos.color = new Color(0, 0, 1, 0.1f); // Azul: Caminar
        Gizmos.DrawWireSphere(transform.position, walkNoiseRange);

        Gizmos.color = new Color(0, 1, 0, 0.1f); // Verde: Agachado
        Gizmos.DrawWireSphere(transform.position, crouchNoiseRange);
    }
}