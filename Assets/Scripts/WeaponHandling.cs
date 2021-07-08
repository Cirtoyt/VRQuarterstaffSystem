using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandling : MonoBehaviour
{
    public enum HandTypes
    {
        RIGHT,
        LEFT,
    }

    [Header("Options")]
    public Weapon weapon;
    public HandTypes dominantHandType;
    [Header("Trying to find the right values")]
    [SerializeField] private float positionSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxAngularVelocity;
    [Header("Statics")]
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
    private Rigidbody rb;
    private XRPhysicsHand lastSingleGrippingHand;
    private XRPhysicsHand lastSingleFreeHand;
    private XRController dominantController;
    private XRPhysicsHand dominantHand;
    private Vector3 originLeftHandAttachTransLocPos;
    private Quaternion originLeftHandAttachTransLocRot;
    private Vector3 originRightHandAttachTransLocPos;
    private Quaternion originRightHandAttachTransLocRot;
    private Vector3 weaponTargetPos;
    private Quaternion weaponTargetRot;

    private void Start()
    {
        gripState = GripStates.EMPTY;
        mustSetupGrips = false;
        rb = weapon.GetComponent<Rigidbody>();

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

        originLeftHandAttachTransLocPos = leftHand.grabPointTransform.localPosition;
        originLeftHandAttachTransLocRot = leftHand.grabPointTransform.localRotation;
        originRightHandAttachTransLocPos = rightHand.grabPointTransform.localPosition;
        originRightHandAttachTransLocRot = rightHand.grabPointTransform.localRotation;
    }

    private void Update()
    {
        // ## SPAWNING ##
        UpdateWeaponSpawnState();

        // ## WEAPON MOVEMENT ##
        if (weapon.GetPresenceState())
        {
            MoveWeapon();
        }

        // ## ENTERING EMPTY GRIP MODE ##
        if (!rightController.isGripActivated && !leftController.isGripActivated
            && gripState != GripStates.EMPTY)
        {
            gripState = GripStates.EMPTY;
            rightHand.enablePhysics = true;
            leftHand.enablePhysics = true;
            rb.isKinematic = true;
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


    private void SetupOneHandedMovement(XRPhysicsHand grippingHand)
    {
        Debug.Log("One Hand Setup");
        rb.isKinematic = false;

        lastSingleGrippingHand = grippingHand;
        lastSingleGrippingHand.enablePhysics = false;
        lastSingleFreeHand = (grippingHand == rightHand) ? leftHand : rightHand;
        lastSingleFreeHand.enablePhysics = true;

        ResetGrabPointTransformLocals();

        // Set initial attach transform position & move weapon pivot point to it without moving the weapon
        Vector3 newAttachTransformLocalPos = new Vector3(0, 0, weapon.transform.InverseTransformPoint(grippingHand.grabPointTransform.position).z);
        
        if (grippingHand == rightHand)
        {
            weapon.rightAttachTransform.localPosition = newAttachTransformLocalPos;
            rb.centerOfMass = weapon.transform.InverseTransformPoint(weapon.rightAttachTransform.position);
        }
        else if (grippingHand == leftHand)
        {
            weapon.leftAttachTransform.localPosition = newAttachTransformLocalPos;
            rb.position = weapon.leftAttachTransform.position;
        }

        bool weaponIsFacingThumb = (Vector3.Dot(weapon.transform.forward, grippingHand.grabPointTransform.forward) >= 0);
        Debug.Log(Vector3.Dot(weapon.transform.forward, grippingHand.grabPointTransform.forward));

        //weapon.transform.SetParent(grippingHand.grabPointTransform);

        //weapon.transform.localRotation = Quaternion.identity; // Maybe this should be set based on previous upper/lower grip forward facing direction

        //Debug.Log(grippingHand.attachTransform.InverseTransformDirection(weapon.transform.forward).z);
        //Debug.Log(Vector3.Dot(weapon.transform.forward, grippingHand.grabPointTransform.forward));
        if (weaponIsFacingThumb)
        //if (grippingHand.grabPointTransform.InverseTransformDirection(weapon.transform.forward).z >= 0)
        {
            //weaponTargetRot = Quaternion.identity;
        }
        else
        {
            //weaponTargetRot = Quaternion.identity;
            //weapon.transform.localRotation.SetLookRotation(-grippingHand.grabPointTransform.forward);
        }

        if (grippingHand == rightHand)
        {
            //weaponTargetPos = rightHand.grabPointTransform.position + (weapon.transform.position - weapon.rightAttachTransform.position);
        }
        else
        {
            //weaponTargetPos = leftHand.grabPointTransform.position + (weapon.transform.position - weapon.leftAttachTransform.position);
        }
    }

    private void ProcessOneHandedMovement(XRPhysicsHand grippingHand)
    {
        if (mustSetupGrips)
        {
            SetupOneHandedMovement(grippingHand);
            mustSetupGrips = false;
        }

        // ## Hands ##
        // Hand instantaneously tracks controller without any physics
        grippingHand.transform.position = grippingHand.parentController.transform.position;
        grippingHand.transform.rotation = grippingHand.parentController.transform.rotation;

        // ## Weapon ##
        // Position
        weaponTargetPos = rightHand.grabPointTransform.position;
        rb.velocity = (weaponTargetPos - weapon.rightAttachTransform.position) * positionSpeed * Time.deltaTime;

        // Rotation
        weaponTargetRot = grippingHand.grabPointTransform.rotation; //Quaternion.LookRotation(grippingHand.grabPointTransform.forward, grippingHand.grabPointTransform.up);

        Quaternion rotDifference = weaponTargetRot * Quaternion.Inverse(weapon.transform.rotation);
        rotDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        rb.maxAngularVelocity = maxAngularVelocity;
        rb.angularVelocity = rotationAxis * angleInDegrees * Mathf.Deg2Rad * rotationSpeed * Time.deltaTime;
    }


    private void SetupTwoHandedMovement()
    {
        Debug.Log("Two Hand Setup");
        rb.isKinematic = false;

        //ResetHandLocals();
        weapon.transform.SetParent(lastSingleGrippingHand.grabPointTransform);
        weapon.transform.localPosition = (lastSingleGrippingHand == rightHand) ? -weapon.rightAttachTransform.localPosition : -weapon.leftAttachTransform.localPosition;
        rightHand.enablePhysics = false;
        leftHand.enablePhysics = false;
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

        Vector3 handGripDirection = (lastSingleFreeHand.grabPointTransform.position - lastSingleGrippingHand.grabPointTransform.position).normalized;
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
        
        lastSingleGrippingHand.grabPointTransform.rotation = newRot;

        // ## Postion ##

        if (mustSetupGrips)
        {
            // Setup attach position markers after rotation has been applied
            weapon.rightAttachTransform.position = rightHand.grabPointTransform.position;
            weapon.leftAttachTransform.position = leftHand.grabPointTransform.position;
            mustSetupGrips = false;
        }

        float stretchDist = Vector3.Distance(rightHand.grabPointTransform.position, leftHand.grabPointTransform.position);
        float baseStretchDist = Vector3.Distance(weapon.rightAttachTransform.position, weapon.leftAttachTransform.position);
        float stretchCorrection = ((stretchDist - baseStretchDist) / 2);
        Vector3 newPos = Vector3.zero;

        if (lastSingleGrippingHand == rightHand)
        {
            if (weapon.transform.InverseTransformDirection(handGripDirection).z >= 0)
            {
                newPos = new Vector3(weapon.transform.localPosition.x, weapon.transform.localPosition.y, -weapon.rightAttachTransform.localPosition.z + stretchCorrection);
            }
            else
            {
                newPos = new Vector3(weapon.transform.localPosition.x, weapon.transform.localPosition.y, -weapon.rightAttachTransform.localPosition.z - stretchCorrection);
            }
        }
        else
        {
            if (weapon.transform.InverseTransformDirection(handGripDirection).z >= 0)
            {
                newPos = new Vector3(weapon.transform.localPosition.x, weapon.transform.localPosition.y, -weapon.leftAttachTransform.localPosition.z + stretchCorrection);
            }
            else
            {
                newPos = new Vector3(weapon.transform.localPosition.x, weapon.transform.localPosition.y, -weapon.leftAttachTransform.localPosition.z - stretchCorrection);
            }
        }

        weapon.transform.localPosition = newPos;
    }


    private void UpdateWeaponSpawnState()
    {
        if (!weapon.GetPresenceState() && (dominantController.isGripActivated))
        {
            ResetGrabPointTransformLocals();
            if (dominantHandType == HandTypes.RIGHT)
            {
                weapon.transform.rotation = rightHand.grabPointTransform.rotation;
                weapon.transform.position = rightHand.grabPointTransform.position + (weapon.transform.position - weapon.spawnAttachTransform.position);
            }
            else
            {
                //weaponPivotPoint.transform.position = leftHand.grabPointTransform.position + (weapon.transform.position - weapon.spawnAttachTransform.position);
            }

            weapon.BeginMaterialising();
        }
        // Add extra else if for when you grip again before it finishes dematerialising, not teleporting the weapon back to the start spawn position
        else if (weapon.GetPresenceState() && !rightController.isGripActivated && !leftController.isGripActivated)
        {
            weapon.BeginDematerialising();
        }
    }

    private void ResetGrabPointTransformLocals()
    {
        rightHand.grabPointTransform.localPosition = originRightHandAttachTransLocPos;
        rightHand.grabPointTransform.localRotation = originRightHandAttachTransLocRot;
        leftHand.grabPointTransform.localPosition = originLeftHandAttachTransLocPos;
        leftHand.grabPointTransform.localRotation = originLeftHandAttachTransLocRot;
    }
}
