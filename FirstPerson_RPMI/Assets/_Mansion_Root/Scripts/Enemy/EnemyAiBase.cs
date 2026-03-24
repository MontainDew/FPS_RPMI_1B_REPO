using UnityEngine;
using UnityEngine.AI;

public class EnemyAiBase : MonoBehaviour
{
    #region General Variables
    [Header("AI Configuration")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform target;
    [SerializeField] FP_Controller playerScript; // Referencia para saber si el jugador está agachado
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask obstacleLayer; // Para comprobar paredes

    [Header("Patroling Stats")]
    [SerializeField] float walkPointRange = 10f;
    [SerializeField] float patrolSpeed = 2.5f;
    Vector3 walkPoint;
    bool walkPointSet;

    [Header("Chasing & Attacking Stats")]
    [SerializeField] float chaseSpeed = 5f;
    [SerializeField] float attackRange = 2f;
    bool isAttacking;

    [Header("Senses (Sight & Hearing)")]
    [SerializeField] float normalSightRange = 15f;
    [SerializeField] float fieldOfViewAngle = 110f; // Cono de visión
    [SerializeField] float hearingRange = 8f; // Radio para oírte correr
    [SerializeField] bool targetDetected;

    [Header("Stuck Detection")]
    [SerializeField] float stuckCheckTime = 2f;
    [SerializeField] float stuckThreshold = 0.1f;
    [SerializeField] float maxStuckDuration = 3f;
    float stuckTimer;
    float lastCheckTime;
    Vector3 lastPosition;
    #endregion

    private void Awake()
    {
        GameObject playerObj = GameObject.Find("Player");
        target = playerObj.transform;
        playerScript = playerObj.GetComponent<FP_Controller>();
        agent = GetComponent<NavMeshAgent>();

        lastPosition = transform.position;
        lastCheckTime = Time.time;
    }

    void Update()
    {
        if (isAttacking) return; // Si te atrapó, deja de actualizar la IA normal

        DetectPlayer();
        CheckIfStuck();
    }

    void DetectPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        targetDetected = false;

        // Si el jugador corre, hace ruido y lo detectamos de espaldas por el oído
        if (playerScript.isSprinting && distanceToPlayer <= hearingRange)
        {
            targetDetected = true;
        }
        else
        {
            // Ajustamos la visión si el jugador está agachado (más difícil de ver)
            float currentSightRange = playerScript.isCrounching ? normalSightRange * 0.5f : normalSightRange;

            if (distanceToPlayer <= currentSightRange)
            {
                Vector3 directionToPlayer = (target.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

                // Comprobar si está dentro del cono de visión
                if (angleToPlayer < fieldOfViewAngle / 2f)
                {
                    // Trazar un rayo para asegurar que no hay paredes entre enemigo y jugador
                    if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstacleLayer))
                    {
                        targetDetected = true;
                    }
                }
            }
        }

        // Determinar Estado
        if (targetDetected && distanceToPlayer > attackRange) ChaseTarget();
        else if (targetDetected && distanceToPlayer <= attackRange) CatchPlayer(); // Jumpscare
        else Patroling();
    }

    void Patroling()
    {
        agent.speed = patrolSpeed;

        if (!walkPointSet)
        {
            SearchWalkPoint();
        }
        else agent.SetDestination(walkPoint);

        if ((transform.position - walkPoint).sqrMagnitude < 2f)
        {
            walkPointSet = false;
        }
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
                }
            }
        }
    }

    void ChaseTarget()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
    }

    void CatchPlayer()
    {
        isAttacking = true;
        agent.isStopped = true;

        // Hacer que mire al jugador fijamente
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        // AQUÍ PONDRÍAS TU LÓGICA DE JUMPSCARE O GAME OVER
        Debug.Log("¡TE ATRAPÓ! Jumpscare / Game Over.");
    }

    void CheckIfStuck()
    {
        if (Time.time - lastCheckTime > stuckCheckTime)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < stuckThreshold && agent.hasPath)
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

        // Rango de ataque (Rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Rango de Oído (Azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        // Cono de Visión (Amarillo)
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftBoundary * normalSightRange);
        Gizmos.DrawRay(transform.position, rightBoundary * normalSightRange);
    }
}