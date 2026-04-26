using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kye.StealthGame.Enemies
{
    /// <summary>
    /// Guard enemy — a standard foot soldier that patrols on foot.
    /// On alert, calls nearby Guards and K9s for backup before chasing.
    /// Does not deal direct damage; instead alerts all enemies in radius.
    /// Inherits from BaseEnemy.
    /// </summary>
    public class GuardEnemy : BaseEnemy
    {
        // ─────────────────────────────────────────────
        // SERIALIZED FIELDS
        // ─────────────────────────────────────────────

        [Header("Guard — Alert")]
        [SerializeField] private float alertCallRadius  = 20f;  // radius to notify nearby allies
        [SerializeField] private LayerMask enemyMask;           // layer containing other enemies

        [Header("Guard — Attack")]
        [SerializeField] private float attackRadius     = 2f;   // melee range to trigger alert
        [SerializeField] private float attackCooldown   = 3f;   // seconds between alert pulses

        // ─────────────────────────────────────────────
        // PRIVATE STATE
        // ─────────────────────────────────────────────

        private float attackTimer = 0f;

        // ─────────────────────────────────────────────
        // UNITY LIFECYCLE  (override)
        // ─────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            // Guards are heavier and don't slide around as much
            if (rb != null)
                rb.linearDamping = 4f;
        }

        protected override void Update()
        {
            base.Update();

            // Cooldown ticks regardless of state
            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;
        }

        // ─────────────────────────────────────────────
        // VIRTUAL OVERRIDES
        // ─────────────────────────────────────────────

        /// <summary>
        /// When a Guard spots the player, it immediately broadcasts an alert
        /// to all nearby Guards and K9s before transitioning to Chase.
        /// </summary>
        protected override void OnAlert()
        {
            Debug.Log($"[GuardEnemy] {gameObject.name} spotted the player — calling for backup.");
            BroadcastAlertToNearbyEnemies();
        }

        /// <summary>
        /// At the start of a chase the guard shouts again so any latecomers respond.
        /// </summary>
        protected override void OnChaseBegin()
        {
            Debug.Log($"[GuardEnemy] {gameObject.name} is chasing — broadcasting chase alert.");
            BroadcastAlertToNearbyEnemies();
        }

        /// <summary>
        /// Guard attack: if the player is within melee range, pulse another alert
        /// to nearby allies and nudge them into Chase state.
        /// </summary>
        /// <param name="distanceToPlayer">Current distance from this guard to the player.</param>
        protected override void PerformAttackCheck(float distanceToPlayer)
        {
            if (distanceToPlayer > attackRadius) return;
            if (attackTimer > 0f) return;

            Debug.Log($"[GuardEnemy] {gameObject.name} is in attack range — alerting nearby allies.");
            BroadcastAlertToNearbyEnemies();
            attackTimer = attackCooldown;
        }

        // ─────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────

        /// <summary>
        /// Uses an OverlapSphere to find all enemy GameObjects within alertCallRadius,
        /// then forces each GuardEnemy and K9Enemy into Chase state.
        /// </summary>
        private void BroadcastAlertToNearbyEnemies()
        {
            // Vector maths: uses world position as sphere centre (magnitude of radius)
            Collider[] hits = Physics.OverlapSphere(transform.position, alertCallRadius, enemyMask);

            foreach (Collider col in hits)
            {
                if (col == null) continue;

                // Don't alert yourself
                if (col.gameObject == gameObject) continue;

                // Alert Guards
                GuardEnemy guard = col.GetComponent<GuardEnemy>();
                if (guard != null && guard.CurrentState == EnemyState.Patrol)
                {
                    guard.ForceState(EnemyState.Chase);
                    Debug.Log($"[GuardEnemy] Alerted nearby Guard: {guard.gameObject.name}");
                    continue;
                }

                // Alert K9s
                K9Enemy k9 = col.GetComponent<K9Enemy>();
                if (k9 != null && k9.CurrentState == EnemyState.Patrol)
                {
                    k9.ForceState(EnemyState.Chase);
                    Debug.Log($"[GuardEnemy] Alerted nearby K9: {k9.gameObject.name}");
                }
            }
        }

        // ─────────────────────────────────────────────
        // GIZMOS
        // ─────────────────────────────────────────────

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Alert call radius
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);  // orange
            Gizmos.DrawWireSphere(transform.position, alertCallRadius);

            // Melee attack radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }
    }
}
