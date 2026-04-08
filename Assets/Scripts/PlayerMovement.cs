using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Angus Variables

    [SerializeField]
    private float crouchSpeed = 10f;
    [SerializeField]
    private bool isCrouching = false;   
    private bool isToggleCrouch;
    [SerializeField]
    private float crouchHeight = 1f;
    [SerializeField]
    private float crouchBuffer = 0.4f;

    [SerializeField]
    private CapsuleCollider playerCollider;

    #endregion


    #region Iain McManus's player movement script
    // region segmenting for readability
    #region Declaring Variables
    [Header("Character")]
    [SerializeField]
    private Rigidbody rb;
    private float Height => isCrouching ? crouchHeight: 2f;
    [SerializeField]
    private float Width = .3f;

    [Header("Grounded Check")]
    public LayerMask GroundedLayerMask = ~0;
    [SerializeField]
    private float buffer => isCrouching ? crouchBuffer : .8f;
    [SerializeField]
    public float radiusBuffer = .02f;
    [SerializeField]
    private bool isGrounded = true;
    [SerializeField]
    private float slopeLimit = 60f;

    [Header("Movement")]
    [SerializeField]
    private float defaultSpeed = 15f;
    public float maxSpeed
    {
        get
        {
            if (isGrounded)
            {
                if (!isSprinting)
                {
                    if (!isCrouching)
                    {
                        return defaultSpeed;
                    }
                    else
                    {
                        return crouchSpeed;
                    }
                }
                else
                {
                    return sprintSpeed;
                }
            }
            else
            {
                return airControl ? airControlSpeed : 0f;
            }
        }
    }
    [SerializeField]
    private float sprintSpeed = 20f;
    [SerializeField]
    private bool isSprinting { get; set; }
    [SerializeField]
    private bool isAutoSprint;
    //[SerializeField]
    private bool canSprint;
    

    [Header("Jumping")]
    [SerializeField]
    private bool canJump = true;
    [SerializeField]
    private bool isJumping = false;
    [SerializeField]
    private float jumpVelocity = 20;
    [SerializeField]
    private float jumpTime = 0.1f;
    private float jumpTimeRemaining;

    

    [Header("Falling")]
    [SerializeField]
    private float fallVel = 10f;
    [SerializeField]
    private bool airControl = true;
    [SerializeField]
    private float airControlSpeed = 10f;

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
    private bool crouchInput;
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
        Debug.Log("jump pressed? "+ value.ToString());
    }
    private void OnSprint(InputValue value)
    {
        // get and set value of pressed input into the cache
        sprintInput = value.isPressed;
        Debug.Log("sprint pressed? " + value.ToString());
    }

    private void OnCrouch(InputValue value)
    {
        // get and set value of pressed input into the cache
        crouchInput = value.isPressed;
        Debug.Log("crouch pressed? " + value.ToString());
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

        playerCollider = GetComponent<CapsuleCollider>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        
    }

    private void FixedUpdate()
    {
        RaycastHit groundCheckResult = UpdateIsGrounded();

        UpdateSprinting();

        UpdateMovement(groundCheckResult);

        UpdateCrouching();
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
        if (!isAutoSprint)
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
        else
        {
            if (!sprintInput)
            {
                isSprinting = true;
            }
            else
            {
                isSprinting = false;
            }
        }
    }

    private RaycastHit UpdateIsGrounded()
    {
        Vector3 startPos = rb.position + Vector3.up * Height * 0.5f;
        float groundCheckDistance = (Height * 0.5f) + buffer;

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

        // are we on the ground?
        if (isGrounded)
        {
            // project onto floor surface 
            movementVector = Vector3.ProjectOnPlane(movementVector, groundCheckResult.normal);

            // stop movement on steep slope
            if (movementVector.y > 0 && Vector3.Angle(Vector3.up, groundCheckResult.normal) > slopeLimit)
            {
                //movementVector = Vector3.zero;
            }
        } 
        else if (!isGrounded && ! isJumping)
        {
            movementVector += Vector3.down * fallVel;
        }

        bool frameTriggered = false;
        if (jumpInput)
        {
            if (isGrounded)
            {
                Debug.Log("Trigger Jump Check passed");
                isJumping = true;
                jumpTimeRemaining = jumpTime;
                frameTriggered = true;
            }

            
        }

        if (isJumping)
        {
            // reduce jump time if not jumping this frame
            if (!frameTriggered)
            {
                jumpTimeRemaining -= Time.deltaTime;
                Debug.Log("not the frame jump is triggered");
            }

            // Jump finished
            if (jumpTimeRemaining <= 0)
            {
                isJumping = false;
                Debug.Log("Not jumping anymore");
            }
            else
            {
                movementVector.y += jumpVelocity;
                Debug.Log("Jump Velocity added to movement vector");
            }
        }

        // apply movement vector to rigid body velocity
        rb.linearVelocity = movementVector;

        Debug.Log(jumpTimeRemaining);
        Debug.Log("Jump Triggered this frame? " + frameTriggered);
        Debug.Log("grounded " + isGrounded);
       /* Debug.Log("Sprinting " + isSprinting);
        Debug.Log(movementVector);*/
    }
    #endregion

    #region Angus programming
    private void UpdateCrouching()
    {

        if (!isToggleCrouch)
        {
            if (crouchInput)
            {
                isCrouching = true;
            }
            else
            {
                isCrouching = false;
            }

            if (isCrouching)
            {
                // adjust the collider to allow the player to hide
                playerCollider.height = 1f;
            }
            else
            {
                playerCollider.height = 2f;
            }
        }
        /*else
        {
            if (crouchInput)
            {
                isCrouching = !isCrouching;
                if (isCrouching)
                {
                    isCrouching = false;
                    playerCollider.height = 2f;

                    return;
                }
                else if (crouchInput)
                {
                    isCrouching = true;
                    playerCollider.height = 1f;
                }
                
            }
        }*/
    }

    #endregion
}
