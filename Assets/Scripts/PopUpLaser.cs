using Unity.Mathematics;
using UnityEngine;

public class PopUpLaser : OverlapDetectScript
{
    // get references for the laser
    public GameObject movingLaser;
    public Transform playerTransform;

    protected override void OnOverlap()
    {
        // debugging
        Debug.Log("Pop up laser activated");

        // rotate laser to look at the player could be achieved by setting rotation to player transform - movinglaser transform
        movingLaser.transform.LookAt(playerTransform);

        // activating laser after making it look at the player
        movingLaser.SetActive(true);
        base.OnOverlap();
    }
}
