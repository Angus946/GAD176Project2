using UnityEngine;

public class AlarmManagerScript : MonoBehaviour
{
    #region Script References
    // example script reference
    //public EventAlarm EA;
    public LaserScript LaserScript;
    #endregion

    // set alarm bools
    public bool isAlarmActive;
    public bool isAlarmTriggered;

    // set delegate event
    public delegate void AlarmDelegate();
    public AlarmDelegate onAlarmTriggered;

    private static AlarmManagerScript instance;

    // make the manager a singleton
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
    
    // Trigger alarm function
    public void TriggerAlarm()
    {
        if (isAlarmActive)
        {
            // set alarm triggered bool
            isAlarmTriggered = true;

            // call the add listener script
            AddListener();

            // debugging
            Debug.Log("Alarm triggered");
            onAlarmTriggered?.Invoke();
        }
    }
    // deactivate alarm function
    public void DeactivateAlarm()
    {
        isAlarmActive = false;
        isAlarmTriggered = false;
        RemoveListener();
    }

    // add listener function
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

    // remove listener function
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
