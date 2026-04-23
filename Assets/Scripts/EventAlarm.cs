using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EventAlarm : MonoBehaviour
{

   public AlarmManagerScript alarmManager;


    // Debug system for manually activating the alarm from seperate class
   /* PlayerInput playerInput;
    public bool alarmTest;
    protected void OnAlarmTest(InputValue value)
    {
        alarmTest = value.isPressed;
        Debug.Log("input Value " + value.isPressed); 

        if (alarmTest && value.isPressed)
        {
            alarmTest = !value.isPressed;
        }
    }*/

    // this function sets the alarm active bool, and calls the trigger alarm function
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag + " has entered the trigger zone");
        if (alarmManager != null && other.CompareTag("Player"))
        {
            alarmManager.isAlarmActive = true;
            alarmManager.TriggerAlarm();
        }
        

    }

    // this void calls for the deactivate alarm function when the player exits the trigger zone
    public void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.tag + " has exited the trigger zone");
        if (alarmManager != null && other.CompareTag("Player"))
        {
           alarmManager.DeactivateAlarm();
        }
    }

    public void TestEventAlarm()
    {
        Debug.Log("the alarm event has occured");
    }
}
