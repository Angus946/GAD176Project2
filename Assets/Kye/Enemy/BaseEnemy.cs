using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kye.StealthGame.Enemies
{
    /// <summary>
    /// Abstract base class for all enemy types in the stealth game.
    /// Provides shared state machine logic, physics movement, and detection systems.
    /// </summary>
    public abstract class BaseEnemy : MonoBehaviour
    {
        //All possible states in the enemy's AI state machine.</summary>
        public enum EnemyState
        {
            Patrol,
            Alert,
            Chase,
            Search
        }

        // ─────────────────────────────────────────────
        // SERIALIZED FIELDS  (encapsulation via SerializeField)
        // ─────────────────────────────────────────────

        [Header("Detection — Line of Sight")]
        [SerializeField] protected float sightRange       = 15f;
        [SerializeField] protected float sightAngle       = 60f;   // half-angle in degrees
        [SerializeField] protected LayerMask sightMask;            // layers that block LOS

        [Header("Movement")]
        [SerializeField] protected float patrolSpeed      = 2f;
        [SerializeField] protected float chaseSpeed       = 5f;
        [SerializeField] protected float rotationSpeed    = 5f;    // degrees per second

        [Header("Patrol")]
        [SerializeField] protected Transform[] patrolPoints;
        [SerializeField] protected float waypointTolerance = 0.5f;

        [Header("Search")]
        [SerializeField] protected float searchDuration   = 5f;    // seconds before returning to patrol
        [SerializeField] protected float alertCooldown    = 2f;    // seconds in Alert before chasing

        // ─────────────────────────────────────────────
        // PRIVATE / PROTECTED STATE  (encapsulation)
        // ─────────────────────────────────────────────

        protected EnemyState  currentState     = EnemyState.Patrol;
        protected Rigidbody   rb;
        protected Transform   playerTransform;

        private   int         patrolIndex      = 0;
        private   float       searchTimer      = 0f;
        private   float       alertTimer       = 0f;
        private   Vector3     lastKnownPosition;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();

            if (rb == null)
                Debug.LogError($"[BaseEnemy] {gameObject.name} is missing a Rigidbody component.");

            // Freeze rotation so physics doesn't tip the enemy over
            if (rb != null)
                rb.freezeRotation = true;
        }

        protected virtual void Start()
        {
            // Locate the player by tag — null-checked before any use
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
                Debug.LogWarning("[BaseEnemy] No GameObject tagged 'Player' found in scene.");

            // Validate patrol points
            if (patrolPoints == null || patrolPoints.Length == 0)
                Debug.LogWarning($"[BaseEnemy] {gameObject.name} has no patrol points assigned.");

            EnterState(EnemyState.Patrol);
        }

        protected virtual void Update()
        {
            RunStateMachine();
        }
        

        /// <summary>Returns the enemy's current AI state (read-only externally).</summary>
        public EnemyState CurrentState => currentState;

        /// <summary>Drives per-frame logic for the active state.</summary>
        private void RunStateMachine()
        {
            switch (currentState)
            {
                case EnemyState.Patrol: UpdatePatrol(); break;
                case EnemyState.Alert:  UpdateAlert();  break;
                case EnemyState.Chase:  UpdateChase();  break;
                case EnemyState.Search: UpdateSearch(); break;
            }
        }

        /// <summary>Transitions to a new state and runs entry logic.</summary>
        protected void EnterState(EnemyState newState)
        {
            currentState = newState;

            switch (newState)
            {
                case EnemyState.Patrol:
                    break;

                case EnemyState.Alert:
                    alertTimer = 0f;
                    OnAlert();
                    break;

                case EnemyState.Chase:
                    OnChaseBegin();
                    break;

                case EnemyState.Search:
                    searchTimer       = searchDuration;
                    lastKnownPosition = playerTransform != null
                                        ? playerTransform.position
                                        : transform.position;
                    break;
            }
        }
        

        private void UpdatePatrol()
        {
            MoveAlongPatrolRoute();

            if (CanSeePlayer())
                EnterState(EnemyState.Alert);
        }

        private void UpdateAlert()
        {
            // Stop moving, look toward player
            ApplyVelocity(Vector3.zero);

            if (playerTransform != null)
                RotateToward(playerTransform.position);

            alertTimer += Time.deltaTime;

            if (alertTimer >= alertCooldown)
            {
                if (CanSeePlayer())
                    EnterState(EnemyState.Chase);
                else
                    EnterState(EnemyState.Search);
            }
        }

        private void UpdateChase()
        {
            if (playerTransform == null)
            {
                EnterState(EnemyState.Search);
                return;
            }

            // Vector maths: direction and magnitude to player
            Vector3 toPlayer      = playerTransform.position - transform.position; // vector subtraction
            float   distToPlayer  = toPlayer.magnitude;                            // magnitude
            Vector3 moveDirection = toPlayer.normalized;                           // normalisation

            MoveInDirection(moveDirection, chaseSpeed);
            RotateToward(playerTransform.position);

            // Update last known position while chasing
            lastKnownPosition = playerTransform.position;

            PerformAttackCheck(distToPlayer);   // abstract derived class decides attack range/logic

            if (!CanSeePlayer())
                EnterState(EnemyState.Search);
        }

        private void UpdateSearch()
        {
            searchTimer -= Time.deltaTime;

            // Walk toward last known position
            Vector3 toTarget     = lastKnownPosition - transform.position;  // vector subtraction
            float   distToTarget = toTarget.magnitude;                      // magnitude

            if (distToTarget > waypointTolerance)
            {
                MoveInDirection(toTarget.normalized, patrolSpeed);
                RotateToward(lastKnownPosition);
            }
            else
            {
                ApplyVelocity(Vector3.zero);
            }

            // Spot the player again?
            if (CanSeePlayer())
            {
                EnterState(EnemyState.Chase);
                return;
            }

            // Give up and return to patrol
            if (searchTimer <= 0f)
                EnterState(EnemyState.Patrol);
        }
        

        /// <summary>
        /// Returns true if the enemy has unobstructed line-of-sight to the player.
        /// Uses a raycast and a dot-product angle check.
        /// </summary>
        protected virtual bool CanSeePlayer()
        {
            if (playerTransform == null) return false;

            Vector3 origin    = transform.position;
            Vector3 toPlayer  = playerTransform.position - origin;              // vector subtraction
            float   distance  = toPlayer.magnitude;                             // magnitude

            if (distance > sightRange) return false;

            // Angle check using dot product 
            // dot = cos(angle) for unit vectors; compare against cos(sightAngle)
            float   dot             = Vector3.Dot(transform.forward, toPlayer.normalized);
            float   cosHalfAngle    = Mathf.Cos(sightAngle * Mathf.Deg2Rad);   // angle conversion

            if (dot < cosHalfAngle) return false;

            // Raycast to confirm no wall between enemy and player
            if (Physics.Raycast(origin, toPlayer.normalized, out RaycastHit hit, distance, sightMask))
            {
                // Something is blocking the ray — check if it's the player
                if (hit.transform != playerTransform)
                    return false;
            }

            return true;
        }

        /// <summary>Moves the enemy in a given world-space direction at a given speed using Rigidbody velocity.</summary>
        protected void MoveInDirection(Vector3 direction, float speed)
        {
            if (rb == null) return;

            // Preserve existing Y velocity (gravity) — only override XZ
            Vector3 targetVelocity = direction * speed;
            targetVelocity.y       = rb.linearVelocity.y;

            ApplyVelocity(targetVelocity);
        }

        /// <summary>Sets Rigidbody velocity directly, preserving gravity on Y.</summary>
        protected void ApplyVelocity(Vector3 velocity)
        {
            if (rb == null) return;
            rb.linearVelocity = velocity;
        }

        /// <summary>
        /// Applies a physics impulse force to the enemy (e.g. knockback).
        /// Force vector direction and magnitude determine the result.
        /// </summary>
        public void ApplyKnockback(Vector3 forceVector)
        {
            if (rb == null) return;
            rb.AddForce(forceVector, ForceMode.Impulse);
        }

        /// <summary>Smoothly rotates the enemy to face a world-space target position.</summary>
        protected void RotateToward(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;    // vector subtraction
            direction.y       = 0f;

            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation        = Quaternion.Slerp(
                                            transform.rotation,
                                            targetRotation,
                                            rotationSpeed * Time.deltaTime);
        }
        

        private void MoveAlongPatrolRoute()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            Transform target     = patrolPoints[patrolIndex];
            if (target == null)
            {
                Debug.LogWarning($"[BaseEnemy] Patrol point at index {patrolIndex} is null.");
                AdvancePatrolIndex();
                return;
            }

            Vector3 toWaypoint   = target.position - transform.position;    // vector subtraction
            float   dist         = toWaypoint.magnitude;                    // magnitude

            MoveInDirection(toWaypoint.normalized, patrolSpeed);
            RotateToward(target.position);

            if (dist <= waypointTolerance)
                AdvancePatrolIndex();
        }

        private void AdvancePatrolIndex()
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
        
        /// <summary>
        /// Called when the enemy enters the Alert state.
        /// Override in derived classes to add type-specific alert behaviour
        /// (e.g. a guard calls for backup, a drone flashes a light).
        /// </summary>
        protected virtual void OnAlert() { }

        /// <summary>
        /// Called when the enemy begins chasing the player.
        /// Override in derived classes for chase-start behaviour.
        /// </summary>
        protected virtual void OnChaseBegin() { }

        /// <summary>
        /// Abstract: derived classes MUST implement attack logic.
        /// Called every chase frame with the current distance to the player.
        /// </summary>
        /// <param name="distanceToPlayer">Current distance from enemy to player.</param>
        protected abstract void PerformAttackCheck(float distanceToPlayer);

 

        /// <summary>Forces the enemy into a specific state (e.g. from a GameManager event).</summary>
        public void ForceState(EnemyState state) => EnterState(state);

        /// <summary>Returns the squared distance to the player without a sqrt — cheaper for comparisons.</summary>
        public float GetSqrDistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;

            Vector3 diff = playerTransform.position - transform.position;   // vector subtraction
            return diff.sqrMagnitude;
        }
        

        protected virtual void OnDrawGizmosSelected()
        {
            // Sight range sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);

            // Forward direction ray
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * sightRange);
        }
    }
}
