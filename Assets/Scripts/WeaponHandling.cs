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

        if (dominantController)
        {
            lastSingleGrippingHand = dominantHand;
            lastSingleFreeHand = (dominantHand == rightHand) ? leftHand : rightHand;
        }
        else
        {
            lastSingleGrippingHand = rightHand;
            lastSingleFreeHand = leftHand;
        }

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
            rightHand.enablePhysics = true;
            leftHand.enablePhysics = true;
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
        // (no rotation with weightings yet)
        lastSingleGrippingHand.transform.position = (lastSingleGrippingHand == rightHand) ? rightController.transform.position : leftController.transform.position;

        lastSingleGrippingHand.transform.rotation = (lastSingleGrippingHand == rightHand) ? rightController.transform.rotation : leftController.transform.rotation;
        //weapon.transform.localRotation = rightController.transform.rotation;
    }

    private void SetupOneHandedMovement(XRPhysicsHand grippingHand)
    {
        lastSingleGrippingHand = grippingHand;
        lastSingleGrippingHand.enablePhysics = false;
        lastSingleFreeHand = (grippingHand == rightHand) ? leftHand : rightHand;
        lastSingleFreeHand.enablePhysics = true;
        weapon.transform.SetParent(grippingHand.attachTransform);
        ResetHandLocals();
        weapon.ResetWeaponLocals();
        weapon.transform.localPosition = -weapon.rightAttachTransform.localPosition;
        weapon.transform.localRotation = Quaternion.identity;
    }


    private void ProcessTwoHandedMovement()
    {
        if (mustSetupGrips)
        {
            SetupTwoHandedMovement();
        }

        // ## Updating hands ##

        rightHand.transform.position = rightController.transform.position;
        //rightHand.transform.rotation = rightController.transform.rotation;
        leftHand.transform.position = leftController.transform.position;
        //rightHand.transform.rotation = rightController.transform.rotation;

        // ## Rotation ##

        Vector3 handGripDirection = (lastSingleFreeHand.attachTransform.position - lastSingleGrippingHand.attachTransform.position).normalized;
        Quaternion newRot = Quaternion.identity;

        // Test if second hand is gripping above or below first gripping hand along the shaft
        if (weapon.transform.InverseTransformDirection(handGripDirection).z >= 0)
        {
            newRot = Quaternion.LookRotation(handGripDirection);
        }
        else
        {
            newRot = Quaternion.LookRotation(-handGripDirection);
        }
        
        lastSingleGrippingHand.attachTransform.rotation = newRot;

        // ## Postion ##

        if (mustSetupGrips)
        {
            // Setup attach position markers after rotation has been applied
            weapon.rightAttachTransform.position = rightHand.attachTransform.position;
            weapon.leftAttachTransform.position = leftHand.attachTransform.position;
            mustSetupGrips = false;
        }

        float stretchDist = Vector3.Distance(leftHand.attachTransform.position, rightHand.attachTransform.position);
        float baseStretchDist = Vector3.Distance(weapon.rightAttachTransform.position, weapon.leftAttachTransform.position);
        float stretchCorrection = ((baseStretchDist - stretchDist) / 2);
        Vector3 newPos = Vector3.zero;

        if (lastSingleGrippingHand == rightHand)
        {
            if (weapon.transform.InverseTransformDirection(handGripDirection).z >= 0)
            {
                newPos = new Vector3(weapon.transform.localPosition.x, weapon.transform.localPosition.y, -weapon.rightAttachTransform.localPosition.z - stretchCorrection);
            }
            else
            {
                newPos = new Vector3(weapon.transform.localPosition.x, weapon.transform.localPosition.y, -weapon.rightAttachTransform.localPosition.z + stretchCorrection);
            }
        }
        else
        {
            if (weapon.transform.InverseTransformDirection(handGripDirection).z >= 0)
            {
                newPos = new Vector3(weapon.transform.localPosition.x, weapon.transform.localPosition.y, -weapon.leftAttachTransform.localPosition.z - stretchCorrection);
            }
            else
            {
                newPos = new Vector3(weapon.transform.localPosition.x, weapon.transform.localPosition.y, -weapon.leftAttachTransform.localPosition.z + stretchCorrection);
            }
        }

        weapon.transform.localPosition = newPos;
    }

    private void SetupTwoHandedMovement()
    {
        weapon.transform.SetParent(lastSingleGrippingHand.attachTransform);
        ResetHandLocals();
        weapon.ResetWeaponLocals();
        weapon.transform.localPosition = -weapon.rightAttachTransform.localPosition;
        weapon.transform.localRotation = weapon.rightAttachTransform.localRotation;
        rightHand.enablePhysics = false;
        leftHand.enablePhysics = false;
    }


    private void UpdateWeaponSpawnState()
    {
        if (!weapon.GetPresenceState() && (dominantController.isGripActivated))
        {
            // Move to hand
            ResetHandLocals();
            if (dominantHandType == HandTypes.RIGHT)
            {
                weapon.transform.position = dominantHand.attachTransform.position + -weapon.rightAttachTransform.localPosition;
            }
            else
            {
                weapon.transform.position = dominantHand.attachTransform.position + -weapon.leftAttachTransform.localPosition;
            }
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
