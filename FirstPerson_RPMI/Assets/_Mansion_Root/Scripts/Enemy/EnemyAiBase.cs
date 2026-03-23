using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyAiBase : MonoBehaviour
{
    #region General Variables
    [Header("AI Configuration")]
    [SerializeField] NavMeshAgent agent; //Ref al cerebro
    [SerializeField] Transform target; //Ref a el que persigue
    [SerializeField] LayerMask targetLayer;
    [SerializeField] LayerMask groundLayer;

    [Header("Patroling Stats")]
    [SerializeField] float walkPointRange = 8f; //Radio de puntos navegables
    Vector3 walkPoint;
    bool walkPointSet;

    [Header("Attacking Stats")]
    [SerializeField] float timeBetweenAttacks = 1f;
    [SerializeField] GameObject projectile;
    [SerializeField] Transform shootPoint;
    [SerializeField] float shootSpeedY = 0;//Altura en Y
    [SerializeField] float shootSpeedZ = 10f; //Altura en Z
    bool alreadyAttacked;

    [Header("States & Detection Areas")]
    [SerializeField] float sightRange = 8f; //Radio de deteccion
    [SerializeField] float attacktRange =2; //Radio de ataque
    [SerializeField] bool targetInSightRange;
    [SerializeField] bool targetInAttackRange;

    [Header("Stuck Detection")]
    [SerializeField] float stuckCheckTime = 2f;
    [SerializeField] float stuckThreshold = 0.1f; //margen de detencion e stuck
    [SerializeField] float maxStuckDuration = 3f;
    float stuckTimer;
    float lastCheckTime; //Tiempo de chequeo antes de estar stuck
    Vector3 lastPosition;//Posicion del ltimo walkpoint perseguido

    #endregion
    private void Awake()
    {
        target = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;
        lastCheckTime = Time.time;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        EnemyStateUpdater();
        CheckIfStuck();
    }

    void EnemyStateUpdater()
    {   // como cambia de estado
        //Esfera de detección fisica
        Collider[] hits = Physics.OverlapSphere(transform.position, sightRange, targetLayer);
        targetInSightRange = hits.Length > 0;
        if (targetInSightRange)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            targetInAttackRange = distance <= attacktRange;
        }
        //camnbio de estados
        if (!targetInSightRange && !targetInAttackRange) Patroling();
        else if (targetInSightRange && !targetInAttackRange) ChaseTarget();
        else if (targetInSightRange && targetInAttackRange) AttackTarget();
    }
    void Patroling()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();  
        }
        else agent.SetDestination(walkPoint);
        if ((transform.position - walkPoint).sqrMagnitude < 1f)
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
            //Mira si el punto esta en un lugar con NavMesh
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                walkPoint = hit.position;
                if (Physics.Raycast(walkPoint, -transform.up, 2f, groundLayer))
                {
                    walkPointSet = true;
                }
            }
        }
    }

    void ChaseTarget()
    {
        agent.SetDestination(target.position);
    }

    void AttackTarget()
    {
        agent.SetDestination(transform.position);
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, agent.angularSpeed * Time.deltaTime);
        }
        if (!alreadyAttacked)
        {
            Rigidbody rb = Instantiate(projectile, shootPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * shootSpeedZ, ForceMode.Impulse);
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    
    void ResetAttack()
    {
        alreadyAttacked = false;
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attacktRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
