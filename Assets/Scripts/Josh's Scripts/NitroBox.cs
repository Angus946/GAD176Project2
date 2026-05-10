using UnityEngine;

public class NitroBox : ExplorebleObjects
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded) Explode();
    }


}
