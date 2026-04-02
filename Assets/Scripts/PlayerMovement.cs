using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Iain McManus's player movement script
    // region segmenting for readability
    #region Declaring Variables
    [Header("Character")]
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float Height = 2f;
    [SerializeField]
    private float Width = .5f;

    [Header("Grounded Check")]
    public LayerMask GroundedLayerMask = ~0;
    private float buffer = .5f;
    private float radiusBuffer = .05f;

    [Header("Movement")]
    public float maxSpeed => isSprinting ? sprintSpeed : defaultSpeed;
    [SerializeField]
    private float defaultSpeed = 100f;
    [SerializeField]
    private float sprintSpeed = 130f;
    [SerializeField]
    private bool isSprinting { get; set; }
    [SerializeField]
    private bool isSprintToggle;
    [SerializeField]
    private bool isSprintPossible;
    [SerializeField]
    private float crouchSpeed = 70f;
    [SerializeField]
    private bool isCrouching;

    [SerializeField]
    private bool isGrounded = true;

    [SerializeField]
    private Transform cameraTransform;

    [SerializeField]
    PlayerInput PlayerInput;
    [SerializeField]
    private InputAction moveAction;
    #endregion

    #region Camera Adjustments
    [Header("camera Adjustments")]
    [SerializeField]
    private bool invertCameraY;
    [SerializeField]
    private float horizontalCameraSensitivity = 5f;
    [SerializeField]
    private float verticalCameraSensitivity = 5f;


    [SerializeField]
    private float minCameraPitch = -75;
    [SerializeField]
    private float maxCameraPitch = 75f;
    [SerializeField]
    private float currentCameraPitch = 0f;
    #endregion

    #region InputAction Handling
    // variables to cache the event flags
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool primaryInput;
    private bool secondaryInput;

    // void returns an input value for the "Move" input
    private void OnMove(InputValue value)
    {
        // get and set the vector2 value into the cache
        moveInput = value.Get<Vector2>();
        Debug.Log($"Move {value}");
    }

    // void returns an input value for the "look" input
    private void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
        Debug.Log($"Move {value}");
    }

    // void returns an input value for the "jump" input
    private void OnJump(InputValue value)
    {
        // get and set value of pressed input into the cache
        jumpInput = value.isPressed;
    }
    private void OnSprint(InputValue value)
    {
        // get and set value of pressed input into the cache
        sprintInput = value.isPressed;
        Debug.Log(value.ToString());
    }

    // void returns an input value for the "Primary Action" input
    private void OnPrimary (InputValue value)
    {
        primaryInput = value.isPressed;
    }

    // void returns an input value for the "Secondary Action" input
    private void OnSecondary(InputValue value)
    {
        secondaryInput = value.isPressed;
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        PlayerInput = GetComponent<PlayerInput>();
        moveAction = PlayerInput.actions.FindAction("Move");
    }

    private void OnEnable()
    {
        
    }

    private void FixedUpdate()
    {
        RaycastHit groundCheckResult = UpdateIsGrounded();

        UpdateSprinting();

        UpdateMovement(groundCheckResult);
    }

    
    private void LateUpdate()
    {
        CameraUpdate();
    }
     private void CameraUpdate()
    {
        float cameraYawDelta = lookInput.x * horizontalCameraSensitivity * Time.deltaTime;

        transform.localRotation = transform.localRotation * Quaternion.Euler(0f, cameraYawDelta, 0f);
    }
    private void UpdateSprinting()
    {
        if (sprintInput)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    private RaycastHit UpdateIsGrounded()
    {
        Vector3 startPos = rb.position + Vector3.up * Height * 0.5f;
        float groundCheckDistance = (Height *0.5f) + buffer;

        // perform sphere cast
        RaycastHit hitResult;
        if (Physics.SphereCast(startPos, Width + radiusBuffer, Vector3.down, out hitResult, groundCheckDistance, GroundedLayerMask, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
            return hitResult;
        }
        else
        {
            isGrounded = false;
        }
        
        return hitResult;
    }
    private void UpdateMovement(RaycastHit groundCheckResult)
    {
        // calculate movement input
        Vector3 movementVector = (transform.forward * moveInput.y + transform.right * moveInput.x) * maxSpeed * Time.deltaTime;
        movementVector *= maxSpeed;

        if (isGrounded)
        {
            // project onto floor surface 
            movementVector = Vector3.ProjectOnPlane(movementVector, groundCheckResult.normal);
        }
        rb.linearVelocity = movementVector;

        Debug.Log(isGrounded);
        Debug.Log("Sprinting " + isSprinting);
        Debug.Log(movementVector);
    }
    #endregion
}
