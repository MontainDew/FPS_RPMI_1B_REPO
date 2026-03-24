using UnityEngine;
using UnityEngine.InputSystem;

public class FP_Controller : MonoBehaviour
{
    #region General Variables
    [Header("Movement & Look")]
    [SerializeField] GameObject camHolder;
    [SerializeField] float speed = 3f; // Más lento para terror
    [SerializeField] float crouchSpeed = 1.5f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float maxForce = 1f;
    [SerializeField] float sensitivity = 0.1f;

    [Header("Jump and GroundCheck")]
    [SerializeField] bool isGrounded;
    [SerializeField] float jumpForce = 4f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] LayerMask groundLayer;

    [Header("Player State Bools")]
    public bool isSprinting;
    public bool isCrounching; // Hecho público para que el enemigo lo lea

    [Header("Horror Mechanics")]
    [SerializeField] Light flashlight; // Linterna
    [SerializeField] float standingHeight = 0.6f; // Altura normal de la cámara
    [SerializeField] float crouchingHeight = -0.2f; // Altura de la cámara al agacharse
    [SerializeField] float crouchTransitionSpeed = 5f;

    [Header("Lean Settings")]
    [SerializeField] float leanAngle = 10f;
    [SerializeField] float leanSpeed = 4f;
    [SerializeField] float leanOffset = 0.1f;

    [Header("Head Bob Settings")]
    [SerializeField] float bobAmplitude = 0.02f;
    [SerializeField] float bobFrequency = 6f;
    [SerializeField] float bobSmoothing = 5f;

    [Header("Footsteps - Sounds")]
    [SerializeField] AudioSource footstepSource;
    [SerializeField] AudioClip[] woodFootsteps;
    [SerializeField] float walkStepRate = 0.6f;
    [SerializeField] float sprintStepRate = 0.4f;
    [SerializeField] float crouchStepRate = 0.9f;
    [SerializeField] float walkVolume = 0.5f;
    [SerializeField] float sprintVolume = 0.8f;
    [SerializeField] float crouchVolume = 0.15f; // Más silencioso al agacharse
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
    float targetCamHeight;

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
        targetCamHeight = standingHeight;
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
        ApplyHeadBobAndCrouch();
    }

    void CameraLook()
    {
        float yaw = lookInput.x * sensitivity;
        float pitch = -lookInput.y * sensitivity;

        lookRotation += pitch;
        lookRotation = Mathf.Clamp(lookRotation, -85f, 85f); // Restringido para que no se rompa el cuello

        float targetLean = leanInput * leanAngle;
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        transform.Rotate(Vector3.up * yaw);
        camHolder.transform.localRotation = Quaternion.Euler(lookRotation, 0f, -currentLean);
    }

    void ApplyHeadBobAndCrouch()
    {
        // Transición suave al agacharse
        targetCamHeight = isCrounching ? crouchingHeight : standingHeight;
        initialCamPos.y = Mathf.Lerp(initialCamPos.y, targetCamHeight, Time.deltaTime * crouchTransitionSpeed);

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
        camHolder.transform.localPosition = Vector3.Lerp(camHolder.transform.localPosition, targetPos, Time.deltaTime * bobSmoothing);
    }

    void Movement()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y);

        targetVelocity *= isCrounching ? crouchSpeed : isSprinting ? sprintSpeed : speed;
        targetVelocity = transform.TransformDirection(targetVelocity);

        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void Jump()
    {
        if (isGrounded && !isCrounching) // No saltar si está agachado
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void HandleFootsteps()
    {
        if (!isGrounded || woodFootsteps.Length == 0) return;

        bool hasMovementInput = moveInput.magnitude > 0.1f;

        if (hasMovementInput)
        {
            float stepRate = isSprinting ? sprintStepRate : isCrounching ? crouchStepRate : walkStepRate;
            float volume = isSprinting ? sprintVolume : isCrounching ? crouchVolume : walkVolume;

            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                int index = Random.Range(0, woodFootsteps.Length);
                footstepSource.clip = woodFootsteps[index];
                footstepSource.volume = volume + Random.Range(-0.05f, 0.05f);
                footstepSource.pitch = Random.Range(0.9f, 1.1f);
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
    public void OnMove(InputAction.CallbackContext context) { moveInput = context.ReadValue<Vector2>(); }
    public void OnLook(InputAction.CallbackContext context) { lookInput = context.ReadValue<Vector2>(); }
    public void OnJump(InputAction.CallbackContext context) { if (context.performed) Jump(); }
    public void OnLean(InputAction.CallbackContext context) { leanInput = context.ReadValue<float>(); }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrounching = !isCrounching;
            if (anim != null) anim.SetBool("isCrouching", isCrounching);
            if (isCrounching) isSprinting = false; // Cancelar sprint al agacharse
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed && !isCrounching) isSprinting = true;
        if (context.canceled) isSprinting = false;
    }

    // Nuevo input para la linterna (necesitas mapearlo en el Input System, por ejemplo, tecla 'F')
    public void OnFlashlight(InputAction.CallbackContext context)
    {
        if (context.performed && flashlight != null)
        {
            flashlight.enabled = !flashlight.enabled;
        }
    }
    #endregion
}