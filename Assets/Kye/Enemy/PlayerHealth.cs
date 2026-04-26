using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public void TakeDamage(float damage)
    {
        Debug.Log($"[PlayerHealth] Took {damage} damage.");
    }
}
