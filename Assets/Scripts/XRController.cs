using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class XRController : MonoBehaviour
{
    [Header("Actions")]
    [SerializeField] private InputActionProperty positionAction;
    [SerializeField] private InputActionProperty rotationAction;
    [SerializeField] private InputActionProperty grabAction;
    [SerializeField] private InputActionProperty triggerAction;
    [Header("Variables")]
    public GameObject attachTransform;
    [Range(0, 1)] [SerializeField] private float grabUpperClamp = 0.7f;
    //[Range(0, 1)] [SerializeField] private float grabLowerClamp = 0.125f;
    [Range(0, 1)] [SerializeField] private float triggerUpperClamp = 0.7f;
    //[Range(0, 1)] [SerializeField] private float triggerLowerClamp = 0.125f;
    [Header("Events")]
    public UnityEvent grabBeginEvent;
    public UnityEvent grabEndEvent;
    public UnityEvent triggerBeginEvent;
    public UnityEvent triggerEndEvent;

    private List<InputActionProperty> actionList = new List<InputActionProperty>();
    private bool isGrabActivated = false;
    private bool isTriggerActivated = false;

    void Start()
    {
        actionList.Add(positionAction);
        actionList.Add(rotationAction);
        actionList.Add(grabAction);
        actionList.Add(triggerAction);

        EnableActions();

        positionAction.action.performed += OnPosition;
        rotationAction.action.performed += OnRotation;
        grabAction.action.performed += OnGrip;
        triggerAction.action.performed += OnTrigger;
    }

    private void OnPosition(InputAction.CallbackContext obj)
    {
        transform.position = obj.ReadValue<Vector3>();
    }

    private void OnRotation(InputAction.CallbackContext obj)
    {
        transform.rotation = obj.ReadValue<Quaternion>();
    }

    private void OnGrip(InputAction.CallbackContext obj)
    {
        var gripValue = obj.ReadValue<float>();
        
        if (gripValue >= grabUpperClamp && !isGrabActivated)
        {
            isGrabActivated = true;
            Debug.Log(gameObject.name + " Grip detected");
            grabBeginEvent.Invoke();
        }
        else if (gripValue < grabUpperClamp && isGrabActivated)
        {
            isGrabActivated = false;
            Debug.Log(gameObject.name + " Grip let go");
            grabEndEvent.Invoke();
        }
    }

    private void OnTrigger(InputAction.CallbackContext obj)
    {
        var triggerValue = obj.ReadValue<float>();

        if (triggerValue >= triggerUpperClamp && !isTriggerActivated)
        {
            isTriggerActivated = true;
            Debug.Log(gameObject.name + " Trigger detected");
            triggerBeginEvent.Invoke();
        }
        else if (triggerValue < triggerUpperClamp && isTriggerActivated)
        {
            isTriggerActivated = false;
            Debug.Log(gameObject.name + " Trigger let go");
            triggerEndEvent.Invoke();
        }
    }

    void Update()
    {
        
    }

    private void EnableActions()
    {
        OnEnable();
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
