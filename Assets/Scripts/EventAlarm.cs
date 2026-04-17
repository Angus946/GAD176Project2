using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EventAlarm : MonoBehaviour
{

   public GameObject alarmManager;


    PlayerInput playerInput;
    public bool alarmTest;
    protected void OnAlarmTest(InputValue value)
    {
        alarmTest = value.isPressed;
        Debug.Log("input Value " + value.isPressed); 

        if (alarmTest && value.isPressed)
        {
            alarmTest = !value.isPressed;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {

        Debug.Log(collision.collider.tag + " entered collider");
       if (collision.collider.CompareTag("Player") == true)
        {
            Debug.Log("player is in alarm zone");
            if (alarmManager != null)
            {
                AlarmManagerScript alarmScript = alarmManager.GetComponent<AlarmManagerScript>();
                alarmScript.ActivateAlarm();
            }
        }
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();

    }

   
}
