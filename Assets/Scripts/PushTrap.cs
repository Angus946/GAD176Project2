using UnityEngine;

public class PushTrap : OverlapDetectScript
{
    // visual push force for editor changes
    public float pushForce = 10f;

    // override void for code
    protected override void OnOverlap()
    {
        // debug
        Debug.Log("PushTrap Activated");

        // add force to player to push them
        playerRB.AddForce(Vector3.up * pushForce, ForceMode.Impulse);
        base.OnOverlap();
    }

}
