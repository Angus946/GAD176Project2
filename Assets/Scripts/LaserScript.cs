using System.Collections;
using UnityEngine;

public class LaserScript : MonoBehaviour
{
    // set reference for alarm manager
    public AlarmManagerScript alarmManager;

    // set alarm duration
    public float duration = 10f;

    public void Awake()
    {
        // find alarm manager in scene
        alarmManager = Object.FindAnyObjectByType<AlarmManagerScript>();
    }

    // collision detection
    void OnTriggerEnter(Collider other)
    {
        // debugging
        Debug.Log(other.gameObject.tag + " has entered the trigger zone");
        // did the player enter the trigger?
        if (alarmManager != null && other.CompareTag("Player"))
        {
            // set alarm active bool in alarm manager
            alarmManager.isAlarmActive = true;

            // trigger alarm event in the alarm manager
            alarmManager.TriggerAlarm();

            // run alarm duration timer
            StartCoroutine(AlarmDuration(duration));
        }
    }

    public IEnumerator AlarmDuration(float  duration)
    {
        yield return new WaitForSeconds(duration);
        // call the deactivate alarm function in alarm manager
        alarmManager.DeactivateAlarm();
    }

    public void AlarmTest()
    {
        // debugging
        Debug.Log("alarm activated");
    }
}
