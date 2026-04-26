using UnityEngine;
using UnityEngine.InputSystem;

namespace Kye.StealthGame.Player
{

    public class ScoutDrone : MonoBehaviour
    {

        [Header("Movement")]
        [SerializeField] private float accelerationRate    = 10f;  // units per second squared
        [SerializeField] private float maxSpeed            = 6f;   // top speed in units per second
        [SerializeField] private float frictionDecel       = 8f;   // deceleration when no input

        [Header("Turning")]
        [SerializeField] private float turnSpeed           = 100f; // degrees per second

        [Header("Camera")]
        [SerializeField] private Transform droneCamera;            // first person camera on drone

        private Rigidbody   rb;
        private Vector3     currentVelocity = Vector3.zero;    // tracked manually for acceleration
        private bool        isControllable  = false;


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            if (rb == null)
            {
                Debug.LogError("[ScoutDrone] Missing Rigidbody component.");
                return;
            }

            // Start with no Y freeze so the throw arc works naturally under gravity
            rb.constraints = RigidbodyConstraints.FreezeRotationX
                           | RigidbodyConstraints.FreezeRotationZ;

            rb.linearDamping  = 0f; // we handle friction manually for wheeled feel
        }

        private void Update()
        {
            if (!isControllable) return;
            HandleTurning();
        }

        private void FixedUpdate()
        {
            if (!isControllable) return;
            HandleAccelerationMovement();
        }
        
        /// <summary>
        /// Moves the drone using acceleration rather than direct velocity assignment.
        /// Input direction is relative to the drone camera's facing direction.
        /// Vector addition combines the current velocity with the acceleration delta each frame.
        /// </summary>
        private void HandleAccelerationMovement()
        {
            if (rb == null) return;

            // Read WASD input via new Input System
            float inputForward  = 0f;
            float inputRight    = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) inputForward += 1f;
                if (Keyboard.current.sKey.isPressed) inputForward -= 1f;
                if (Keyboard.current.dKey.isPressed) inputRight   += 1f;
                if (Keyboard.current.aKey.isPressed) inputRight   -= 1f;
            }

            // Build input direction relative to camera facing (XZ plane only)
            Vector3 camForward  = droneCamera != null ? droneCamera.forward : transform.forward;
            Vector3 camRight    = droneCamera != null ? droneCamera.right   : transform.right;

            camForward.y = 0f;
            camRight.y   = 0f;
            camForward   = camForward.normalized;   // normalisation
            camRight     = camRight.normalized;     // normalisation

            // Desired move direction — vector addition of forward and right components
            Vector3 inputDirection = (camForward * inputForward) + (camRight * inputRight); // vector addition

            bool hasInput = inputDirection.sqrMagnitude > 0.01f;

            if (hasInput)
            {
                inputDirection = inputDirection.normalized;

                // Acceleration: velocity += direction * accelerationRate * deltaTime
                // This is vector addition — each frame we add an acceleration delta to current velocity
                Vector3 accelerationDelta = inputDirection * accelerationRate * Time.fixedDeltaTime;
                currentVelocity           = currentVelocity + accelerationDelta;              // vector addition

                // Clamp to max speed (magnitude check)
                if (currentVelocity.magnitude > maxSpeed)
                    currentVelocity = currentVelocity.normalized * maxSpeed;
            }
            else
            {
                // No input — apply friction deceleration toward zero
                // Subtract a friction delta each frame — wheeled feel
                float frictionDelta = frictionDecel * Time.fixedDeltaTime;
                float currentSpeed  = currentVelocity.magnitude;

                if (currentSpeed <= frictionDelta)
                    currentVelocity = Vector3.zero;
                else
                    currentVelocity = currentVelocity - (currentVelocity.normalized * frictionDelta); // vector subtraction

                Debug.Log($"[ScoutDrone] Decelerating — speed: {currentVelocity.magnitude:F2}");
            }

            // Apply final velocity to rigidbody
            Vector3 finalVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
            rb.linearVelocity     = finalVelocity;
        }

        /// <summary>
        /// Rotates the drone left/right based on mouse X input.
        /// Only active while the drone is being controlled.
        /// </summary>
        private void HandleTurning()
        {
            if (Mouse.current == null) return;
            float mouseX     = Mouse.current.delta.x.ReadValue();
            float turnAmount = mouseX * turnSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, turnAmount);
        }
        

        /// <summary>
        /// Enables or disables player control of the drone.
        /// Called by PlayerDroneDeployer when possession switches.
        /// </summary>
        public void SetControllable(bool controllable)
        {
            isControllable = controllable;

            if (controllable)
            {
                // Now grounded and driving — freeze Y so it stays flat
                rb.constraints = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ
                               | RigidbodyConstraints.FreezePositionY;
            }
            else
            {
                // During throw arc — allow Y movement so gravity works
                rb.constraints = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;
                currentVelocity = Vector3.zero;
                if (rb != null) rb.linearVelocity = Vector3.zero;
            }

            Debug.Log($"[ScoutDrone] Controllable set to: {controllable}");
        }

        /// <summary>Returns whether the drone is currently under player control.</summary>
        public bool IsControllable => isControllable;
    }
}