using UnityEngine;
using UnityEngine.InputSystem;

public class FP_Controller2 : MonoBehaviour
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

    [Header("Lean")]
    [SerializeField] float leanAngle = 15f;
    [SerializeField] float leanSpeed = 6f;
    [SerializeField] float leanOffset = 0.4f;
    #endregion

    Rigidbody rb;
    Animator anim;

    Vector2 moveInput;
    Vector2 lookInput;

    float lookRotation;

    float currentLean;
    float targetLean;
    Vector3 camInitialPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        camInitialPos = camHolder.transform.localPosition;
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
        Lean();
    }

    void CameraLook()
    {
        // Rotación horizontal del player
        transform.Rotate(Vector3.up * lookInput.x * sensitivity);

        // Rotación vertical de la cámara
        lookRotation += (-lookInput.y * sensitivity);
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
    }

    void Lean()
    {
        // No permitir lean en el aire
        if (!isGrounded)
            targetLean = 0;

        // Interpolación suave
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        // Aplicar rotación combinando vertical + lean
        camHolder.transform.localRotation = Quaternion.Euler(lookRotation, 0f, currentLean);

        // Movimiento lateral suave
        Vector3 desiredPosition = camInitialPos +
                                  camHolder.transform.right *
                                  (currentLean / leanAngle) *
                                  leanOffset;

        camHolder.transform.localPosition = Vector3.Lerp(
            camHolder.transform.localPosition,
            desiredPosition,
            Time.deltaTime * leanSpeed
        );
    }

    void Movement()
    {
        Vector3 currentVelocity = rb.linearVelocity;

        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y);

        targetVelocity *= isCrounching ? crouchSpeed :
                          isSprinting ? sprintSpeed :
                          speed;

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
        if (context.performed)
            Jump();
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
        if (context.performed && !isCrounching)
            isSprinting = true;

        if (context.canceled)
            isSprinting = false;
    }

    public void OnLean(InputAction.CallbackContext context)
    {
        float leanInput = context.ReadValue<float>();
        targetLean = leanInput * leanAngle;
    }

    #endregion
}