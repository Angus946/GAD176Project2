using UnityEngine;

public class ExplorebleObjects : MonoBehaviour, Interactble.IInteractable
{
    internal Interactble _interactble;
    [SerializeField] protected float explosionRadius = 5f;
    [SerializeField] protected float explosionForce = 700f;
    [SerializeField] protected GameObject explosionEffect;

    protected bool hasExploded = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _interactble = GetComponent<Interactble>();
    }

    public virtual void Interact()
    {
        if (!hasExploded)
        {
            Explode();
            hasExploded = true;
        }
    }
    protected virtual void Explode()
    {
        hasExploded = true;

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        //if (hit.CompareTag("Player"))
        //{
            // Apply damage to the player
            // PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
               // playerHealth.TakeDamage(explosionDamage);
            // }
        //}

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }
    }

}
