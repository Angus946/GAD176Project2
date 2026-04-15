using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EventAlarm : MonoBehaviour
{
    public bool isalarmEnabled = false;

    public delegate void AlarmDelegate();
    public AlarmDelegate onEventTriggered;

    PlayerInput playerInput;
    public bool alarmTest;
    public InputAction AlarmTest;
    protected void OnAlarmTest(InputValue value)
    {
        Debug.Log("input Value " + value.isPressed); 
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }


    public void Update()
    {
        Debug.Log("alarm test is " + alarmTest);
       if (alarmTest)
        {
            AddListener();
        }
       else if (!alarmTest)
        {
            RemoveListener();
        }
    }

    void AddListener()
    {
        onEventTriggered -= alarmActive;
        onEventTriggered += alarmActive;

        onEventTriggered?.Invoke();
    }
    private void RemoveListener()
    {
        onEventTriggered -= alarmActive;
    }

    public void alarmActive()
    {
        Debug.Log("alarm active function is triggered");
    }
}
