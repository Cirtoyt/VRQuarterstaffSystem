using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugVisualHandsHandler : MonoBehaviour
{   
    [Header("Statics")]
    public Weapon weapon;
    [SerializeField] private XRController leftController;
    [SerializeField] private XRPhysicsHand leftHand;
    [SerializeField] private XRController rightController;
    [SerializeField] private XRPhysicsHand rightHand;
    [SerializeField] private GameObject leftControllerGhostVisual;
    [SerializeField] private GameObject leftPhysicsHandGhostVisual;
    [SerializeField] private GameObject rightControllerGhostVisual;
    [SerializeField] private GameObject rightPhysicsHandGhostVisual;

    private enum DebugVisualStates
    {
        NONE,
        CONTROLLERS,
        PHYSICSHANDS,
    }

    private DebugVisualStates debugVisualState;
    private bool lastIsSecondButtonPressed;

    void Start()
    {
        debugVisualState = DebugVisualStates.NONE;
    }

    void Update()
    {
        if (rightController.isSecondaryButtonPressed && rightController.isSecondaryButtonPressed != lastIsSecondButtonPressed)
        {
            switch (debugVisualState)
            {
                case DebugVisualStates.NONE:
                    debugVisualState = DebugVisualStates.CONTROLLERS;
                    break;
                case DebugVisualStates.CONTROLLERS:
                    debugVisualState = DebugVisualStates.PHYSICSHANDS;
                    break;
                case DebugVisualStates.PHYSICSHANDS:
                    debugVisualState = DebugVisualStates.NONE;
                    break;
            }
        }

        if (debugVisualState == DebugVisualStates.CONTROLLERS)
        {
            leftControllerGhostVisual.SetActive(true);
            rightControllerGhostVisual.SetActive(true);
        }
        else
        {
            leftControllerGhostVisual.SetActive(false);
            rightControllerGhostVisual.SetActive(false);
        }

        if (debugVisualState == DebugVisualStates.PHYSICSHANDS)
        {
            leftPhysicsHandGhostVisual.SetActive(true);
            rightPhysicsHandGhostVisual.SetActive(true);
        }
        else
        {
            leftPhysicsHandGhostVisual.SetActive(false);
            rightPhysicsHandGhostVisual.SetActive(false);
        }

        lastIsSecondButtonPressed = rightController.isSecondaryButtonPressed;
    }
}
