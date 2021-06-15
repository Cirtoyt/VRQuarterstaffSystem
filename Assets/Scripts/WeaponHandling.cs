using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandling : MonoBehaviour
{
    public enum HandTypes
    {
        RIGHT,
        LEFT,
        NONE,
    }

    [Header("Options")]
    public Weapon weapon;
    public HandTypes dominantHandType;
    [Header("Statics")]
    [SerializeField] private Transform cameraOffset;
    [SerializeField] private XRController leftController;
    [SerializeField] private XRPhysicsHand leftHand;
    [SerializeField] private XRController rightController;
    [SerializeField] private XRPhysicsHand rightHand;

    private enum GripStates
    {
        EMPTY,
        ONEHANDED,
        TWOHANDED,
    }

    private GripStates gripState;
    private bool mustSetupGrips;
    private XRPhysicsHand lastSingleGrippingHand;
    private XRPhysicsHand lastSingleFreeHand;
    private XRController dominantController;
    private XRPhysicsHand dominantHand;
    private Vector3 originLeftHandAttachTransLocPos;
    private Quaternion originLeftHandAttachTransLocRot;
    private Vector3 originRightHandAttachTransLocPos;
    private Quaternion originRightHandAttachTransLocRot;

    private void Start()
    {
        gripState = GripStates.EMPTY;
        mustSetupGrips = false;

        switch (dominantHandType)
        {
            case HandTypes.RIGHT:
                dominantController = rightController;
                dominantHand = rightHand;
                break;
            case HandTypes.LEFT:
                dominantController = leftController;
                dominantHand = leftHand;
                break;
        }

        lastSingleGrippingHand = dominantHand;
        lastSingleFreeHand = (dominantHand == rightHand) ? leftHand : rightHand;

        originLeftHandAttachTransLocPos = leftHand.attachTransform.localPosition;
        originLeftHandAttachTransLocRot = leftHand.attachTransform.localRotation;
        originRightHandAttachTransLocPos = rightHand.attachTransform.localPosition;
        originRightHandAttachTransLocRot = rightHand.attachTransform.localRotation;
    }

    void Update()
    {
        // ## SPAWNING ##
        UpdateWeaponSpawnState();

        // ## WEAPON MOVEMENT ##
        if (weapon.GetPresenceState())
        {
            MoveWeapon();
        }

        // ## ENTERING EMPTY MODE ##
        if (!rightController.isGripActivated && !leftController.isGripActivated
            && gripState != GripStates.EMPTY)
        {
            gripState = GripStates.EMPTY;
            leftHand.handVisuals.trackPhysicsHand = true;
            rightHand.handVisuals.trackPhysicsHand = true;
            leftHand.handVisuals.transform.SetParent(cameraOffset);
            rightHand.handVisuals.transform.SetParent(cameraOffset);
        }
    }

    private void FixedUpdate()
    {
        //Doing movement in fixed update is just less fluid due to movement being setting position and rotation values to hands which update in regular Update in real time
    }

    private void MoveWeapon()
    {
        // ## ONE-HANDED MODE ##
        if (rightController.isGripActivated && !leftController.isGripActivated)
        {
            UpdateGripState(GripStates.ONEHANDED);
            ProcessOneHandedMovement(rightHand);
        }
        else if (leftController.isGripActivated && !rightController.isGripActivated)
        {
            UpdateGripState(GripStates.ONEHANDED);
            ProcessOneHandedMovement(leftHand);
        }

        // ## TWO-HANDED MODE ##
        else if (rightController.isGripActivated && leftController.isGripActivated)
        {
            UpdateGripState(GripStates.TWOHANDED);
            ProcessTwoHandedMovement();
        }
    }

    private void UpdateGripState(GripStates newState)
    {
        if (gripState != newState)
        {
            gripState = newState;
            mustSetupGrips = true;
        }
    }


    private void ProcessOneHandedMovement(XRPhysicsHand grippingHand)
    {
        if (mustSetupGrips)
        {
            SetupOneHandedMovement(grippingHand);
            mustSetupGrips = false;
        }

        // Instantaneously set rotation & position same as hand's
        weapon.transform.localPosition = -weapon.attachTransform1.localPosition;
        weapon.transform.localRotation = weapon.attachTransform1.localRotation;
    }

    private void SetupOneHandedMovement(XRPhysicsHand grippingHand)
    {
        lastSingleGrippingHand = grippingHand;
        lastSingleFreeHand = (grippingHand == rightHand) ? leftHand : rightHand;
        weapon.transform.SetParent(grippingHand.attachTransform);
        ResetHandLocals();
        weapon.ResetWeaponLocals();
        lastSingleGrippingHand.handVisuals.trackPhysicsHand = false;
        lastSingleFreeHand.handVisuals.trackPhysicsHand = true;
        lastSingleGrippingHand.handVisuals.transform.position = (lastSingleGrippingHand == rightHand) ? rightHand.transform.position : leftHand.transform.position;
        lastSingleGrippingHand.handVisuals.transform.SetParent((lastSingleGrippingHand == rightHand) ? weapon.attachTransform1 : weapon.attachTransform2);
        lastSingleFreeHand.handVisuals.transform.SetParent(cameraOffset);
    }


    private void ProcessTwoHandedMovement()
    {
        if (mustSetupGrips)
        {
            SetupTwoHandedMovement();
        }

        // ## Rotation ##
        Vector3 handGripDifference = lastSingleFreeHand.attachTransform.position - lastSingleGrippingHand.attachTransform.position;
        Quaternion newRot = Quaternion.identity;

        // Test if second hand is gripping above or below first gripping hand along the shaft
        if (weapon.transform.InverseTransformDirection(handGripDifference).z >= 0)
        {
            newRot = Quaternion.LookRotation(handGripDifference.normalized);
        }
        else
        {
            newRot = Quaternion.LookRotation(-handGripDifference.normalized);
        }
        
        lastSingleGrippingHand.attachTransform.rotation = newRot;

        // ## Postion ##
        // Setup attach positions after rotation has been applied
        if (mustSetupGrips)
        {
            weapon.attachTransform1.position = rightHand.attachTransform.position;
            weapon.attachTransform2.position = leftHand.attachTransform.position;
            rightHand.handVisuals.trackPhysicsHand = false;
            leftHand.handVisuals.trackPhysicsHand = false;
            rightHand.handVisuals.transform.SetParent(weapon.attachTransform1);
            leftHand.handVisuals.transform.SetParent(weapon.attachTransform2);
            mustSetupGrips = false;
        }

        float rightHandStretchDist = rightHand.attachTransform.position.z - weapon.attachTransform1.position.z;
        float leftHandStretchDist = leftHand.attachTransform.position.z - weapon.attachTransform2.position.z;
        //Debug.Log(rightHandStretchDist + "                 " + leftHandStretchDist);

        float totalStrechDist = rightHandStretchDist + leftHandStretchDist;
        // TODO/NOT WORKING
        //weapon.transform.localPosition = new Vector3(originWeaponLocPos.x, originWeaponLocPos.y, originWeaponLocPos.z + (totalStrechDist));
    }

    private void SetupTwoHandedMovement()
    {
        weapon.transform.SetParent(lastSingleGrippingHand.attachTransform);
        ResetHandLocals();
        weapon.ResetWeaponLocals();
        weapon.transform.localPosition = -weapon.attachTransform1.localPosition;
        weapon.transform.localRotation = weapon.attachTransform1.localRotation;
    }


    private void UpdateWeaponSpawnState()
    {
        if (!weapon.GetPresenceState() && (dominantController.isGripActivated))
        {
            // Move to hand
            ResetHandLocals();
            weapon.transform.position = dominantHand.attachTransform.position + -weapon.attachTransform1.localPosition;
            weapon.transform.rotation = dominantHand.attachTransform.rotation;

            weapon.BeginMaterialising();
        }
        else if (weapon.GetPresenceState() && !rightController.isGripActivated && !leftController.isGripActivated)
        {
            weapon.BeginDematerialising();
        }
    }

    private void ResetHandLocals()
    {
        rightHand.attachTransform.localPosition = originRightHandAttachTransLocPos;
        rightHand.attachTransform.localRotation = originRightHandAttachTransLocRot;
        leftHand.attachTransform.localPosition = originLeftHandAttachTransLocPos;
        leftHand.attachTransform.localRotation = originLeftHandAttachTransLocRot;
    }
}
