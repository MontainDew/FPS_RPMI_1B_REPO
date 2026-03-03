using UnityEngine;
using UnityEngine.InputSystem;

public class FP_Controller : MonoBehaviour
{
    #region General Variables
    [Header("Movement & Look")]
    [SerializeField] GameObject camHolder;    // Cámara dentro de un empty para girar la cabeza
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
    [SerializeField] float bobAmplitude = 0.02f;  // altura de subida/bajada
    [SerializeField] float bobFrequency = 6f;     // velocidad del movimiento
    [SerializeField] float bobSmoothing = 5f;
    #endregion

    Rigidbody rb;
    Animator anim;

    // Inputs
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
        // Rotación horizontal solo en el empty que contiene la cámara
        float yaw = lookInput.x * sensitivity;
        float pitch = -lookInput.y * sensitivity;

        // Actualiza pitch acumulado
        lookRotation += pitch;
        lookRotation = Mathf.Clamp(lookRotation, -90f, 90f);

        // Lean suave
        float targetLean = leanInput * leanAngle;
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        // Aplicar rotación horizontal (giro de cabeza)
        transform.Rotate(Vector3.up * yaw);

        // Aplicar pitch y lean a la cámara
        camHolder.transform.localRotation = Quaternion.Euler(lookRotation, 0f, -currentLean);
    }

    void ApplyHeadBob()
    {
        Vector3 targetPos = initialCamPos;

        if (moveInput.magnitude > 0.1f && isGrounded)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float yOffset = Mathf.Sin(bobTimer) * bobAmplitude; // solo subida/bajada
            targetPos.y += yOffset;
        }
        else
        {
            bobTimer = 0f; // reset timer cuando no camina
        }

        // Aplicar lean lateral + head bob vertical
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
        if (isGrounded) rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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