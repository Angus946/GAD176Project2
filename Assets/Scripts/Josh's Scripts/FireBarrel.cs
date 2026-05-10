using UnityEngine;

public class FireBarrel : ExplorebleObjects
{
    private void OnTriggerEnter(Collider collision)
    {
        if (!hasExploded) Explode();
    }
}
