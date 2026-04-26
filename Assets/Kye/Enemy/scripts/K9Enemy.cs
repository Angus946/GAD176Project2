using System.Collections;
using UnityEngine;

namespace Kye.StealthGame.Enemies
{
    /// <summary>
    /// K9 enemy — a fast attack dog with a wide field of view and a pounce attack.
    /// Deals direct damage to the player on contact.
    /// Inherits from BaseEnemy.
    /// </summary>
    public class K9Enemy : BaseEnemy
    {
        // ─────────────────────────────────────────────
        // SERIALIZED FIELDS
        // ─────────────────────────────────────────────

        [Header("K9 — Attack")]
        [SerializeField] private float attackRadius     = 1.5f;     // bite range
        [SerializeField] private float attackDamage     = 25f;      // damage per bite
        [SerializeField] private float attackCooldown   = 1.5f;     // seconds between bites

        [Header("K9 — Pounce")]
        [SerializeField] private float pounceRange      = 5f;       // distance to trigger pounce
        [SerializeField] private float pounceForce      = 8f;       // impulse magnitude
        [SerializeField] private float pounceCooldown   = 4f;       // seconds between pounces

        // ─────────────────────────────────────────────
        // PRIVATE STATE
        // ─────────────────────────────────────────────

        private float attackTimer   = 0f;
        private float pounceTimer   = 0f;
        private bool  isPouncing    = false;

        // ─────────────────────────────────────────────
        // UNITY LIFECYCLE  (override)
        // ─────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();

            // K9s are lighter and more agile — less drag so they accelerate quickly
            if (rb != null)
                rb.linearDamping = 1f;
        }

        protected override void Update()
        {
            base.Update();

            if (attackTimer > 0f) attackTimer -= Time.deltaTime;
            if (pounceTimer > 0f) pounceTimer -= Time.deltaTime;
        }

        // ─────────────────────────────────────────────
        // VIRTUAL OVERRIDES
        // ─────────────────────────────────────────────

        /// <summary>
        /// K9s growl and lunge immediately when they spot the player.
        /// Skip the alert pause — go straight to chase.
        /// </summary>
        protected override void OnAlert()
        {
            Debug.Log($"[K9Enemy] {gameObject.name} growls and immediately gives chase.");
            // Force straight to chase — K9s don't hesitate
            EnterState(EnemyState.Chase);
        }

        protected override void OnChaseBegin()
        {
            Debug.Log($"[K9Enemy] {gameObject.name} is sprinting toward the player.");
        }

        /// <summary>
        /// K9 attack logic: pounce if far enough away, bite if in melee range.
        /// Applies damage to the player's health component directly.
        /// </summary>
        /// <param name="distanceToPlayer">Current distance from this K9 to the player.</param>
        protected override void PerformAttackCheck(float distanceToPlayer)
        {
            // ── Pounce ──────────────────────────────
            // If in pounce range but not yet in bite range, launch a physics pounce
            if (distanceToPlayer <= pounceRange && distanceToPlayer > attackRadius && pounceTimer <= 0f && !isPouncing)
            {
                StartCoroutine(PerformPounce());
                return;
            }

            // ── Bite ────────────────────────────────
            if (distanceToPlayer <= attackRadius && attackTimer <= 0f)
            {
                DealDamageToPlayer(attackDamage);
                attackTimer = attackCooldown;
            }
        }

        // ─────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────

        /// <summary>
        /// Launches the K9 toward the player using an impulse force (physics-based pounce).
        /// Temporarily prevents the base Update from overriding velocity mid-pounce.
        /// </summary>
        private IEnumerator PerformPounce()
        {
            if (playerTransform == null) yield break;

            isPouncing  = true;
            pounceTimer = pounceCooldown;

            // Vector maths: direction to player, normalised, scaled by pounce force
            Vector3 toPlayer       = playerTransform.position - transform.position;    // subtraction
            Vector3 pounceDirection = toPlayer.normalized;                              // normalisation
            Vector3 pounceVelocity  = pounceDirection * pounceForce;                   // scaling

            // Add upward arc to the pounce
            pounceVelocity.y += 3f;

            if (rb != null)
                rb.AddForce(pounceVelocity, ForceMode.Impulse);

            Debug.Log($"[K9Enemy] {gameObject.name} pounces! Force: {pounceVelocity.magnitude:F1}");

            // Wait for pounce to play out before allowing normal movement again
            yield return new WaitForSeconds(0.6f);

            isPouncing = false;
        }

        /// <summary>
        /// Finds the PlayerHealth component on the player and applies damage.
        /// Null-checked to avoid errors if the component is missing.
        /// </summary>
        /// <param name="damage">Amount of damage to deal.</param>
        private void DealDamageToPlayer(float damage)
        {
            if (playerTransform == null) return;

            // Attempt to retrieve a health component — null-safe
            PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"[K9Enemy] {gameObject.name} bit the player for {damage} damage.");
            }
            else
            {
                Debug.LogWarning("[K9Enemy] Player is missing a PlayerHealth component.");
            }
        }

        // ─────────────────────────────────────────────
        // GIZMOS
        // ─────────────────────────────────────────────

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Pounce range
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.5f);  // orange
            Gizmos.DrawWireSphere(transform.position, pounceRange);

            // Bite range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }
    }
}
