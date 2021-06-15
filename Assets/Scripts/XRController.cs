using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class XRController : MonoBehaviour
{
    [Header("Statics")]
    public Transform attachTransform;
    [Header("Variables")]
    [Range(0, 1)] [SerializeField] private float gripBeginThreshold = 0.7f;
    [Range(0, 1)] [SerializeField] private float triggerBeginThreshold = 0.7f;
    [Range(0, 1)] [SerializeField] private float thumbstickBeginThreshold = 0.3f;
    [Header("Debugging")]
    public bool isGripActivated;
    [Range(0, 1)] public float gripValue = 0;
    public bool isTriggerActivated;
    [Range(0, 1)] public float triggerValue = 0;
    public Vector2 thumbstickValue;
    [Header("Events")]
    public UnityEvent gripBeginEvent;
    public UnityEvent gripEndEvent;
    public UnityEvent triggerBeginEvent;
    public UnityEvent triggerEndEvent;
    [Header("Input Actions")]
    [SerializeField] private InputActionProperty positionAction;
    [SerializeField] private InputActionProperty rotationAction;
    [SerializeField] private InputActionProperty gripAction;
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty thumbstickAction;

    private List<InputActionProperty> actionList = new List<InputActionProperty>();

    void Awake()
    {
        actionList.Add(positionAction);
        actionList.Add(rotationAction);
        actionList.Add(gripAction);
        actionList.Add(triggerAction);
        actionList.Add(thumbstickAction);

        positionAction.action.performed += OnPosition;
        rotationAction.action.performed += OnRotation;
        gripAction.action.performed += OnGrip;
        triggerAction.action.performed += OnTrigger;
        thumbstickAction.action.performed += OnThumbstick;
    }

    private void OnPosition(InputAction.CallbackContext obj)
    {
        transform.localPosition = obj.ReadValue<Vector3>();
    }

    private void OnRotation(InputAction.CallbackContext obj)
    {
        transform.localRotation = obj.ReadValue<Quaternion>();
    }

    private void OnGrip(InputAction.CallbackContext obj)
    {
        gripValue = obj.ReadValue<float>();
        
        if (gripValue >= gripBeginThreshold && !isGripActivated)
        {
            isGripActivated = true;
            gripBeginEvent.Invoke();
        }
        else if (gripValue < gripBeginThreshold && isGripActivated)
        {
            isGripActivated = false;
            gripEndEvent.Invoke();
        }
    }

    private void OnTrigger(InputAction.CallbackContext obj)
    {
        triggerValue = obj.ReadValue<float>();

        if (triggerValue >= triggerBeginThreshold && !isTriggerActivated)
        {
            isTriggerActivated = true;
            triggerBeginEvent.Invoke();
        }
        else if (triggerValue < triggerBeginThreshold && isTriggerActivated)
        {
            isTriggerActivated = false;
            triggerEndEvent.Invoke();
        }
    }

    private void OnThumbstick(InputAction.CallbackContext obj)
    {
        var rawValue = obj.ReadValue<Vector2>();

        if (rawValue.magnitude >= thumbstickBeginThreshold)
        {
            thumbstickValue = rawValue;
        }
        else
        {
            thumbstickValue = Vector2.zero;
        }
    }

    private void OnEnable()
    {
        foreach(var actionProperty in actionList)
        {
            actionProperty.action.Enable();
        }
    }

    private void OnDisable()
    {
        foreach (var actionProperty in actionList)
        {
            actionProperty.action.Disable();
        }
    }
}
