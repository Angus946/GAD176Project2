using UnityEngine;

public class OverlapDetectScript : MonoBehaviour
{
    // get player rigid body
    [SerializeField]
    protected Rigidbody playerRB;

    // fixed update for more consistent physics detection
    public void FixedUpdate()
    {
        OverlapCollisions();
    }

    // function containing overlap shape code
    public void OverlapCollisions()
    {
        // create a list of colliders that enter the area covered by overlap box
        Collider[] hitColliders = Physics.OverlapBox(transform.position, transform.localScale / 2, Quaternion.identity);
        int i = 0;

        // when the number of colliders is more than "i" run code then increase "i"
        while (i < hitColliders.Length)
        {
            Debug.Log("hit " + hitColliders[i].gameObject.name + i);
            i++; 

            OnOverlap();
        }
    }

    // draw gizmo for visualisation and debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (Application.isPlaying)
        {
            Gizmos.DrawCube(transform.position, transform.localScale);
        }
    }

    // virtual void for child scripts to override code
    protected virtual void OnOverlap()
    {
        
    }

}
