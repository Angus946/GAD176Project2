using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kye.StealthGame.Player
{
    /// <summary>
    /// Attached to the Player. Handles throwing the scout drone with a physics arc,
    /// switching camera possession to the drone, and recalling it back to the player.
    /// </summary>
    public class PlayerDroneDeployer : MonoBehaviour
    {
     

        [Header("Drone Setup")]
        [SerializeField] private GameObject dronePrefab;           // assign your drone primitive in inspector
        [SerializeField] private Transform  throwOrigin;           // where the drone spawns (e.g. player hand)
        [SerializeField] private float      throwForce      = 8f;  // forward throw strength
        [SerializeField] private float      throwArcForce   = 5f;  // upward arc force

        [Header("Range")]
        [SerializeField] private float      maxDroneRange   = 30f; // overlap sphere radius
        [SerializeField] private float      rangeCheckRate  = 0.5f; // seconds between range checks

        [Header("Cameras")]
        [SerializeField] private Camera     playerCamera;           // main player camera
        [SerializeField] private Camera     droneCamera;            // camera on the drone prefab (auto-found)

        [Header("Input")]
        // Deploy = Left Mouse Button, Recall = Keyboard 2
        

        private GameObject      activeDrone     = null;
        private ScoutDrone      droneController = null;
        private bool            isInDroneMode   = false;
        private Coroutine       rangeCoroutine  = null;
        

        private void Start()
        {
            if (playerCamera == null)
                Debug.LogError("[PlayerDroneDeployer] No player camera assigned.");

            if (dronePrefab == null)
                Debug.LogError("[PlayerDroneDeployer] No drone prefab assigned.");

            // Ensure drone camera starts disabled
            if (droneCamera != null)
                droneCamera.gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleDeployInput();
            HandleRecallInput();
        }
        

        /// <summary>Listens for the deploy key. Only throws if no drone is currently active.</summary>
        private void HandleDeployInput()
        {
            if (activeDrone != null) return;
            if (Mouse.current == null) return;
            if (Mouse.current.leftButton.wasPressedThisFrame)
                ThrowDrone();
        }

        /// <summary>Listens for the recall key ('2'). Recalls the drone if one is active.</summary>
        private void HandleRecallInput()
        {
            if (activeDrone == null) return;
            if (Keyboard.current == null) return;
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
                RecallDrone();
        }


        /// <summary>
        /// Spawns the drone at the throw origin and launches it with a physics arc.
        /// Vector addition combines forward throw force and upward arc force.
        /// </summary>
        private void ThrowDrone()
        {
            if (dronePrefab == null) return;

            Vector3 spawnPosition = throwOrigin != null ? throwOrigin.position : transform.position;

            activeDrone      = Instantiate(dronePrefab, spawnPosition, transform.rotation);
            droneController  = activeDrone.GetComponent<ScoutDrone>();

            if (droneController == null)
            {
                Debug.LogError("[PlayerDroneDeployer] Drone prefab is missing a ScoutDrone component.");
                Destroy(activeDrone);
                activeDrone = null;
                return;
            }

            // Disable drone control until it lands
            droneController.SetControllable(false);

            // Physics arc launch — vector addition of forward + upward forces
            Rigidbody droneRb = activeDrone.GetComponent<Rigidbody>();
            if (droneRb != null)
            {
                Vector3 forwardForce = transform.forward * throwForce;  // forward component
                Vector3 arcForce     = Vector3.up * throwArcForce;      // upward component
                Vector3 launchForce  = forwardForce + arcForce;         // vector addition

                droneRb.AddForce(launchForce, ForceMode.Impulse);
                Debug.Log($"[PlayerDroneDeployer] Drone thrown with force: {launchForce}");
            }

            // Find the drone camera on the instantiated prefab
            droneCamera = activeDrone.GetComponentInChildren<Camera>();
            if (droneCamera == null)
                Debug.LogWarning("[PlayerDroneDeployer] No camera found on drone prefab.");

            // Wait for drone to land then switch to drone view
            StartCoroutine(WaitForLandingThenPossess());
        }

        /// <summary>
        /// Waits until the drone's Rigidbody has settled before switching camera possession.
        /// Polls velocity magnitude to detect landing.
        /// </summary>
        private IEnumerator WaitForLandingThenPossess()
        {
            if (activeDrone == null) yield break;

            Rigidbody droneRb = activeDrone.GetComponent<Rigidbody>();

            // Wait a minimum time for the arc to play out
            yield return new WaitForSeconds(0.4f);

            // Then wait until drone is nearly still
            while (droneRb != null && droneRb.linearVelocity.magnitude > 0.5f)
                yield return new WaitForSeconds(0.1f);

            PossessDrone();
        }

  
        /// <summary>
        /// Switches camera control to the drone and enables drone movement.
        /// Disables the player camera and enables the drone camera.
        /// </summary>
        private void PossessDrone()
        {
            if (activeDrone == null) return;

            // Re-find drone camera in case it wasn't caught on instantiation
            if (droneCamera == null)
                droneCamera = activeDrone.GetComponentInChildren<Camera>(true);

            if (droneCamera == null)
            {
                Debug.LogError("[PlayerDroneDeployer] Cannot possess drone — no Camera found on drone prefab. Add a Camera component to a child of the drone.");
                return;
            }

            isInDroneMode = true;

            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
                Debug.Log("[PlayerDroneDeployer] Player camera disabled.");
            }
            else
            {
                Debug.LogError("[PlayerDroneDeployer] Player camera reference is null.");
            }

            droneCamera.gameObject.SetActive(true);
            Debug.Log($"[PlayerDroneDeployer] Drone camera enabled: {droneCamera.gameObject.name}");

            if (droneController != null)
                droneController.SetControllable(true);

            if (rangeCoroutine != null) StopCoroutine(rangeCoroutine);
            rangeCoroutine = StartCoroutine(MonitorDroneRange());

            Debug.Log("[PlayerDroneDeployer] Now inhabiting drone.");
        }

        /// <summary>
        /// Periodically checks if the player is within the drone's max range
        /// using an OverlapSphere centred on the drone.
        /// If the player falls outside the sphere, the drone is recalled automatically.
        /// </summary>
        private IEnumerator MonitorDroneRange()
        {
            while (isInDroneMode && activeDrone != null)
            {
                yield return new WaitForSeconds(rangeCheckRate);

                // OverlapSphere centred on drone checks for the player collider
                Collider[] hits = Physics.OverlapSphere(activeDrone.transform.position, maxDroneRange);

                bool playerInRange = false;
                foreach (Collider col in hits)
                {
                    if (col == null) continue;
                    if (col.gameObject == gameObject)
                    {
                        playerInRange = true;
                        break;
                    }
                }

                if (!playerInRange)
                {
                    Debug.Log("[PlayerDroneDeployer] Drone out of range — recalling automatically.");
                    RecallDrone();
                }
            }
        }
        

        /// <summary>
        /// Returns camera control to the player and destroys the active drone.
        /// </summary>
        public void RecallDrone()
        {
            if (rangeCoroutine != null)
            {
                StopCoroutine(rangeCoroutine);
                rangeCoroutine = null;
            }

            isInDroneMode = false;

            // Swap cameras back
            if (playerCamera != null) playerCamera.gameObject.SetActive(true);
            if (droneCamera  != null) droneCamera.gameObject.SetActive(false);

            // Destroy drone
            if (activeDrone != null)
            {
                Destroy(activeDrone);
                activeDrone     = null;
                droneController = null;
                droneCamera     = null;
            }

            Debug.Log("[PlayerDroneDeployer] Returned to player.");
        }

     

        private void OnDrawGizmosSelected()
        {
            // Show max drone range sphere around player in editor
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, maxDroneRange);
        }
    }
}