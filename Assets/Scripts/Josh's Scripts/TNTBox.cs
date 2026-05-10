using UnityEngine;

public class TNTBox : ExplorebleObjects
{
    public override void Interact()
    {
        base.Interact();
        // Additional behavior specific to TNTBox can be added here

        Invoke("Explode", 2f); // Delay the explosion by 2 seconds for dramatic effect
    }
}
