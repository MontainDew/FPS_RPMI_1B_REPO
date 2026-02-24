using UnityEngine;
using UnityEngine.InputSystem;

public class FP_Controller : MonoBehaviour
{
    #region General Variables
    [Header ("Movement & Look")]
    [SerializeField] GameObject camHolder;
    [SerializeField] float speed = 5f;
    [SerializeField] float crouchSpeed = 3f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float maxForce = 1f;
    [SerializeField] float sensitivity = 0.1f;

    [Header("Player State Bools")]
    [SerializeField] bool isSprinting;
    [SerializeField] bool isCrounching;
    #endregion
    //Variables de autoreferencia
    Rigidbody rb;

    //Variables de input
    Vector2 moveInput;
    Vector2 lookInput;
    float lookRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Lock y visualizacion del cursor del raton
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
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

    }

    public void OnCrouch(InputAction.CallbackContext context)
    {

    }

    public void OnSprint(InputAction.CallbackContext context)
    {

    }
    #endregion
}
