using UnityEngine;

public class AlarmManagerScript : MonoBehaviour
{
    
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
    
    public void ActivateAlarm()
    {
        isAlarmActive = true;
    }

    public void TriggerAlarm()
    {
        if (isAlarmActive)
        {
            isAlarmTriggered = true;
            AddListener();
            Debug.Log("Alarm triggered");
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

    }

    public void RemoveListener()
    {

    }
}
