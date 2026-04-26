using System.Collections;
using UnityEngine;

namespace Kye.StealthGame.Enemies
{
    /// <summary>
    /// Drone enemy — an aerial unit that hovers at a set altitude and strafes the player.
    /// Overrides movement entirely to operate in 3D space rather than on the ground.
    /// Deals direct damage when in attack range.
    /// Inherits from BaseEnemy.
    /// </summary>
    public class DroneEnemy : BaseEnemy
    {
        // ─────────────────────────────────────────────
        // SERIALIZED FIELDS
        // ─────────────────────────────────────────────

        [Header("Drone — Hover")]
        [SerializeField] private float hoverAltitude       = 5f;    // target Y position above ground
        [SerializeField] private float hoverForce          = 15f;   // upward force to maintain altitude
        [SerializeField] private float altitudeTolerance   = 0.3f;  // acceptable Y deviation

        [Header("Drone — Strafe")]
        [SerializeField] private float strafeRadius        = 6f;    // orbit distance from player during chase
        [SerializeField] private float strafeSpeed         = 3f;    // orbit speed (degrees per second)

        [Header("Drone — Attack")]
        [SerializeField] private float attackRadius        = 8f;    // range to deal damage
        [SerializeField] private float attackDamage        = 15f;   // damage per hit
        [SerializeField] private float attackCooldown      = 2f;    // seconds between attacks

        // ─────────────────────────────────────────────
        // PRIVATE STATE
        // ─────────────────────────────────────────────

        private float   attackTimer     = 0f;
        private float   strafeAngle     = 0f;   // current orbit angle in degrees

        // ─────────────────────────────────────────────
        // UNITY LIFECYCLE  (override)
        // ─────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();

            // Drones have no gravity — they manage their own altitude via forces
            if (rb != null)
            {
                rb.useGravity   = false;
                rb.linearDamping = 2f;
            }
        }

        protected override void Start()
        {
            base.Start();

            // Initialise strafe angle based on current direction from player
            // so the drone doesn't snap to 0 degrees on spawn
            if (playerTransform != null)
            {
                Vector3 toSelf  = transform.position - playerTransform.position;    // subtraction
                strafeAngle     = Mathf.Atan2(toSelf.x, toSelf.z) * Mathf.Rad2Deg; // angle from vector
            }
        }

        protected override void Update()
        {
            base.Update();

            MaintainAltitude();

            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;
        }

        // ─────────────────────────────────────────────
        // VIRTUAL OVERRIDES
        // ─────────────────────────────────────────────

        protected override void OnAlert()
        {
            Debug.Log($"[DroneEnemy] {gameObject.name} has acquired the player — engaging.");
        }

        /// <summary>
        /// Drone attack: fires when the player is within attack range.
        /// Deals direct damage via the PlayerHealth component.
        /// </summary>
        /// <param name="distanceToPlayer">Current distance from this drone to the player.</param>
        protected override void PerformAttackCheck(float distanceToPlayer)
        {
            if (distanceToPlayer > attackRadius) return;
            if (attackTimer > 0f) return;

            DealDamageToPlayer(attackDamage);
            attackTimer = attackCooldown;
        }

        /// <summary>
        /// Overrides base CanSeePlayer to give the drone a full 360° horizontal FOV —
        /// it still raycasts, but skips the angle cone check.
        /// </summary>
        protected override bool CanSeePlayer()
        {
            if (playerTransform == null) return false;

            Vector3 origin      = transform.position;
            Vector3 toPlayer    = playerTransform.position - origin;    // vector subtraction
            float   distance    = toPlayer.magnitude;                   // magnitude

            if (distance > sightRange) return false;

            // No angle cone — drone has 360° awareness
            // Raycast still checks for obstructions
            if (Physics.Raycast(origin, toPlayer.normalized, out RaycastHit hit, distance, sightMask))
            {
                if (hit.transform != playerTransform)
                    return false;
            }

            return true;
        }

        // ─────────────────────────────────────────────
        // DRONE MOVEMENT
        // ─────────────────────────────────────────────

        /// <summary>
        /// Maintains the drone's hover altitude by applying an upward force
        /// when below the target height, and damping when above.
        /// Uses physics forces rather than direct position manipulation.
        /// </summary>
        private void MaintainAltitude()
        {
            if (rb == null) return;

            // Find the ground beneath the drone with a downward raycast
            float targetY = transform.position.y;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 50f))
                targetY = hit.point.y + hoverAltitude;  // target = ground Y + desired altitude

            float yDifference = targetY - transform.position.y;    // signed vertical error

            if (Mathf.Abs(yDifference) > altitudeTolerance)
            {
                // Apply upward or downward corrective force proportional to the error
                Vector3 correctionForce = new Vector3(0f, yDifference * hoverForce, 0f);
                rb.AddForce(correctionForce, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Overrides base horizontal movement to orbit the player in a strafe circle
        /// while maintaining altitude. Uses trigonometry to calculate orbit position.
        /// </summary>
        protected new void MoveInDirection(Vector3 direction, float speed)
        {
            // For the drone, standard MoveInDirection is replaced by UpdateChaseMovement.
            // This override intentionally left minimal — altitude is handled separately.
            if (rb == null) return;

            Vector3 targetVelocity  = direction * speed;
            // Preserve Y velocity so altitude correction forces aren't overwritten
            targetVelocity.y        = rb.linearVelocity.y;
            rb.linearVelocity       = targetVelocity;
        }

        /// <summary>
        /// Called each chase frame to strafe around the player at strafeRadius.
        /// Advances the orbit angle over time and computes the target XZ position
        /// using sine/cosine (rotation around a point).
        /// </summary>
        private void UpdateStrafePosition()
        {
            if (playerTransform == null) return;

            // Advance the orbit angle — degrees per second
            strafeAngle += strafeSpeed * Time.deltaTime;
            if (strafeAngle >= 360f) strafeAngle -= 360f;

            float   rad         = strafeAngle * Mathf.Deg2Rad;             // angle in radians
            Vector3 offset      = new Vector3(
                                    Mathf.Sin(rad) * strafeRadius,          // X component
                                    0f,
                                    Mathf.Cos(rad) * strafeRadius);         // Z component

            // Target position = player position + orbit offset (vector addition)
            Vector3 targetXZ    = playerTransform.position + offset;
            Vector3 toTarget    = targetXZ - transform.position;            // vector subtraction
            toTarget.y          = 0f;

            float dist          = toTarget.magnitude;                       // magnitude

            if (dist > 0.5f)
            {
                Vector3 moveDir = toTarget.normalized;                      // normalisation
                // Only drive XZ — altitude handled by MaintainAltitude
                Vector3 velocity        = moveDir * chaseSpeed;
                velocity.y              = rb.linearVelocity.y;
                rb.linearVelocity       = velocity;
            }

            // Always face the player
            RotateToward(playerTransform.position);
        }

        protected override void OnChaseBegin()
        {
            // Reset strafe angle so drone smoothly begins its orbit
            if (playerTransform != null)
            {
                Vector3 toSelf  = transform.position - playerTransform.position;
                strafeAngle     = Mathf.Atan2(toSelf.x, toSelf.z) * Mathf.Rad2Deg;
            }

            Debug.Log($"[DroneEnemy] {gameObject.name} entering strafe orbit.");
        }

        // ─────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────

        /// <summary>
        /// Finds the PlayerHealth component on the player and applies damage.
        /// Null-checked to avoid errors if the component is missing.
        /// </summary>
        /// <param name="damage">Amount of damage to deal.</param>
        private void DealDamageToPlayer(float damage)
        {
            if (playerTransform == null) return;

            PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"[DroneEnemy] {gameObject.name} attacked player for {damage} damage.");
            }
            else
            {
                Debug.LogWarning("[DroneEnemy] Player is missing a PlayerHealth component.");
            }
        }

        // ─────────────────────────────────────────────
        // GIZMOS
        // ─────────────────────────────────────────────

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Strafe orbit radius
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.4f);  // cyan
            Gizmos.DrawWireSphere(transform.position, strafeRadius);

            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);

            // Hover altitude line
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * hoverAltitude);
        }
    }
}
