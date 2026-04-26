using System.Collections;
using UnityEngine;

public class LaserScript : MonoBehaviour
{
    public AlarmManagerScript alarmManager;

    public float duration = 10f;

    public void Awake()
    {
        alarmManager = Object.FindAnyObjectByType<AlarmManagerScript>();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag + " has entered the trigger zone");
        if (alarmManager != null && other.CompareTag("Player"))
        {
            alarmManager.isAlarmActive = true;
            alarmManager.TriggerAlarm();
            StartCoroutine(AlarmDuration(duration));
        }
    }

    public IEnumerator AlarmDuration(float  duration)
    {
        yield return new WaitForSeconds(duration);
        alarmManager.DeactivateAlarm();
    }

    public void AlarmTest()
    {
        Debug.Log("alarm activated");
    }
}
