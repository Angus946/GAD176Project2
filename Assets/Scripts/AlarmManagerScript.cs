using UnityEngine;

public class AlarmManagerScript : MonoBehaviour
{
    #region Script References
    // example script reference
    //public EventAlarm EA;
    public LaserScript LaserScript;
    #endregion

    public bool isAlarmActive;
    public bool isAlarmTriggered;

    public delegate void AlarmDelegate();
    public AlarmDelegate onAlarmTriggered;

    private static AlarmManagerScript instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static AlarmManagerScript Instance
    {
        get { return instance; }
    }
    

    public void TriggerAlarm()
    {
        if (isAlarmActive)
        {
            isAlarmTriggered = true;
            AddListener();
            Debug.Log("Alarm triggered");
            onAlarmTriggered?.Invoke();
        }
    }
    public void DeactivateAlarm()
    {
        isAlarmActive = false;
        isAlarmTriggered = false;
        RemoveListener();
    }

    public void AddListener()
    {
        // example of adding listener
        /*if (EA != null)
        {
            onAlarmTriggered -= EA.TestEventAlarm;
            onAlarmTriggered += EA.TestEventAlarm; (only this line is NECESSARY, but the other code is to avoid breaking things)
        }
        else
        {
            Debug.Log("Script is Null");
        }*/

        if (LaserScript  != null)
        {
            onAlarmTriggered -= LaserScript.AlarmTest;
            onAlarmTriggered += LaserScript.AlarmTest;
        }
    }

    public void RemoveListener()
    {
        // example of removing listener (with redundency)
        /*if (EA != null)
        {
            onAlarmTriggered -= EA.TestEventAlarm; (only this line is NECESSARY, but the other code is to avoid breaking things)
        }
        else
        {
            Debug.Log("Script is Null");
        }*/

        if (LaserScript != null)
        {
            onAlarmTriggered -= LaserScript.AlarmTest;
        }
    }
}
