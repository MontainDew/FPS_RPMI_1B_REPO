using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class EnemyAiBase : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Animator animator;

    [Header("Jumpscare Video")]
    public GameObject jumpscareObject; // Objeto UI que contiene el RawImage + VideoPlayer
    public VideoPlayer jumpscareVideo;
    public AudioSource jumpscareAudio;
    public float jumpscareDelay = 0.2f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    int currentPoint;

    [Header("Vision")]
    public float viewDistance = 15f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Speeds")]
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;

    [Header("Attack")]
    public float attackDistance = 1.5f;

    [Header("Stun")]
    public float stunDuration = 5f;
    public bool stuned = false;

    public bool inChase = false;

    bool isAttacking = false;
    Coroutine stunCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Apagar jumpscare al inicio
        if (jumpscareObject != null)
            jumpscareObject.SetActive(false);

        GoToNextPoint();
    }

    void Update()
    {
        if (stuned)
        {
            agent.isStopped = true;
            UpdateAnimations();
            return;
        }
        else
        {
            agent.isStopped = false;
        }

        if (isAttacking) return;

        if (CanSeePlayer())
        {
            inChase = true;
        }

        if (inChase)
            Chase();
        else
            Patrol();

        CheckAttack();
        UpdateAnimations();
    }

    // ================= PATROL =================
    void Patrol()
    {
        agent.speed = patrolSpeed;

        if (!agent.hasPath || agent.remainingDistance < 0.5f)
            GoToNextPoint();
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        currentPoint = Random.Range(0, patrolPoints.Length);
        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    // ================= CHASE =================
    void Chase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    // ================= VISION =================
    bool CanSeePlayer()
    {
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > viewDistance) return false;

        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle / 2f) return false;

        if (Physics.Raycast(transform.position + Vector3.up, dir, dist, obstacleMask))
            return false;

        return true;
    }

    // ================= ATTACK =================
    void CheckAttack()
    {
        if (player == null || isAttacking) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackDistance)
        {
            StartCoroutine(Jumpscare());
        }
    }

    IEnumerator Jumpscare()
    {
        isAttacking = true;
        agent.isStopped = true;

        // Mirar al jugador
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);

        // Animación
        if (animator != null)
            animator.SetTrigger("Attack");

        // Pequeño delay para impacto
        yield return new WaitForSeconds(jumpscareDelay);

        // Activar objeto del video
        if (jumpscareObject != null)
            jumpscareObject.SetActive(true);

        // Reproducir video
        if (jumpscareVideo != null)
        {
            jumpscareVideo.Stop();
            jumpscareVideo.Play();
        }

        // Audio extra (opcional)
        if (jumpscareAudio != null)
            jumpscareAudio.Play();

        // Bloquear jugador
        if (player != null)
        {
            FP_Controller controller = player.GetComponent<FP_Controller>();
            if (controller != null)
                controller.enabled = false;
        }

        // Esperar a que termine el video
        float duration = (float)jumpscareVideo.length;

        if (duration <= 0f) duration = 3f; // fallback

        yield return new WaitForSeconds(duration);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ================= STUN =================
    public void ApplyStun()
    {
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine());
    }

    IEnumerator StunRoutine()
    {
        stuned = true;

        inChase = false;
        agent.ResetPath();

        if (animator != null)
            animator.SetTrigger("Stun");

        yield return new WaitForSeconds(stunDuration);

        stuned = false;

        GoToNextPoint();
    }

    // ================= ANIMACIONES =================
    void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetBool("isWalking", !inChase && !stuned);
        animator.SetBool("isRunning", inChase && !stuned);
        animator.SetBool("isStunned", stuned);
    }
}