using UnityEngine;

public class TheDoor : MonoBehaviour, Interactble.IInteractable
{
    internal Interactble _interactble;

    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 2f;
    private bool _isOpen = false;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _interactble = GetComponent<Interactble>();
        _closedRotation = transform.rotation;
        _openRotation = _closedRotation * Quaternion.Euler(0, openAngle, 0);
    }
    public void Interact()
    {
        _isOpen = !_isOpen;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isOpen)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _openRotation, Time.deltaTime * smoothSpeed);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _closedRotation, Time.deltaTime * smoothSpeed);
        }   
    }
}
