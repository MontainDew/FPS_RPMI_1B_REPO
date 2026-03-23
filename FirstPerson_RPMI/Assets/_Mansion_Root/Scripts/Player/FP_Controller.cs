using UnityEngine;
using UnityEngine.InputSystem;

public class FP_Controller : MonoBehaviour
{
    #region General Variables
    [Header("Movement & Look")]
    [SerializeField] GameObject camHolder;
    [SerializeField] float speed = 5f;
    [SerializeField] float crouchSpeed = 3f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float maxForce = 1f;
    [SerializeField] float sensitivity = 0.1f;

    [Header("Jump and GroundCheck")]
    [SerializeField] bool isGrounded;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] LayerMask groundLayer;

    [Header("Player State Bools")]
    [SerializeField] bool isSprinting;
    [SerializeField] bool isCrounching;

    [Header("Lean Settings")]
    [SerializeField] float leanAngle = 10f;
    [SerializeField] float leanSpeed = 4f;
    [SerializeField] float leanOffset = 0.1f;

    [Header("Head Bob Y Offset Settings")]
    [SerializeField] float bobAmplitude = 0.02f;
    [SerializeField] float bobFrequency = 6f;
    [SerializeField] float bobSmoothing = 5f;

    [Header("Footsteps - Wood Interior")]
    [SerializeField] AudioSource footstepSource;
    [SerializeField] AudioClip[] woodFootsteps;

    [SerializeField] float walkStepRate = 0.55f;
    [SerializeField] float sprintStepRate = 0.32f;
    [SerializeField] float crouchStepRate = 0.85f;

    [SerializeField] float walkVolume = 0.6f;
    [SerializeField] float sprintVolume = 0.95f;
    [SerializeField] float crouchVolume = 0.25f;

    [SerializeField] float fadeOutSpeed = 3f;

    float stepTimer;
    bool isFootstepPlaying;
    #endregion

    Rigidbody rb;
    Animator anim;

    Vector2 moveInput;
    Vector2 lookInput;
    float lookRotation;

    float leanInput;
    float currentLean;
    Vector3 initialCamPos;
    float bobTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        initialCamPos = camHolder.transform.localPosition;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        HandleFootsteps();
        HandleFootstepFadeOut();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void LateUpdate()
    {
        CameraLook();
        ApplyHeadBob();
    }

    void CameraLook()
    {
        float yaw = lookInput.x * sensitivity;
        float pitch = -lookInput.y * sensitivity;

        lookRotation += pitch;
        lookRotation = Mathf.Clamp(lookRotation, -90f, 90f);

        float targetLean = leanInput * leanAngle;
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        transform.Rotate(Vector3.up * yaw);
        camHolder.transform.localRotation = Quaternion.Euler(lookRotation, 0f, -currentLean);
    }

    void ApplyHeadBob()
    {
        Vector3 targetPos = initialCamPos;

        if (moveInput.magnitude > 0.1f && isGrounded)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float yOffset = Mathf.Sin(bobTimer) * bobAmplitude;
            targetPos.y += yOffset;
        }
        else
        {
            bobTimer = 0f;
        }

        targetPos.x += leanInput * leanOffset;
        camHolder.transform.localPosition =
            Vector3.Lerp(camHolder.transform.localPosition, targetPos, Time.deltaTime * bobSmoothing);
    }

    void Movement()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y);
        targetVelocity *= isCrounching ? crouchSpeed :
                          isSprinting ? sprintSpeed : speed;

        targetVelocity = transform.TransformDirection(targetVelocity);

        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void Jump()
    {
        if (isGrounded)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // =========================
    // FOOTSTEPS SYSTEM CON FADE Y AGACHADO FUNCIONAL
    // =========================

    void HandleFootsteps()
    {
        if (!isGrounded || woodFootsteps.Length == 0) return;

        bool hasMovementInput = moveInput.magnitude > 0.1f;

        if (hasMovementInput)
        {
            float stepRate = walkStepRate;
            float volume = walkVolume;

            if (isSprinting)
            {
                stepRate = sprintStepRate;
                volume = sprintVolume;
                footstepSource.pitch = Random.Range(0.92f, 0.97f);
            }
            else if (isCrounching)
            {
                stepRate = crouchStepRate;
                volume = crouchVolume;
                footstepSource.pitch = Random.Range(0.98f, 1.02f);
            }
            else
            {
                footstepSource.pitch = Random.Range(0.96f, 1.04f);
            }

            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                int index = Random.Range(0, woodFootsteps.Length);
                footstepSource.clip = woodFootsteps[index];
                footstepSource.volume = volume + Random.Range(-0.05f, 0.05f);
                footstepSource.Play();
                isFootstepPlaying = true;

                stepTimer = stepRate + Random.Range(-0.05f, 0.05f);
            }
        }
        else
        {
            stepTimer = 0f;
            isFootstepPlaying = false;
        }
    }

    void HandleFootstepFadeOut()
    {
        if (!isFootstepPlaying && footstepSource.isPlaying)
        {
            footstepSource.volume -= Time.deltaTime * fadeOutSpeed;
            if (footstepSource.volume <= 0f)
                footstepSource.Stop();
        }
    }

    #region Input Methods
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) Jump();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrounching = !isCrounching;
            anim.SetBool("isCrouching", isCrounching);
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed && !isCrounching) isSprinting = true;
        if (context.canceled) isSprinting = false;
    }

    public void OnLean(InputAction.CallbackContext context)
    {
        leanInput = context.ReadValue<float>();
    }
    #endregion
}