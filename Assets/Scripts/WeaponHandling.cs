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
    [Range(0.01f, 1)][SerializeField] private float positionSpeedDamper;
    [SerializeField] private float rotationSpeed;
    [Range(0.01f, 1)] [SerializeField] private float rotationSpeedDamper;
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
    private XRPhysicsHand firstGrippingHand;
    private XRPhysicsHand secondGrippingHand;
    private XRController dominantController;
    private XRPhysicsHand dominantHand;
    private Vector3 weaponTargetPos;
    private Quaternion weaponTargetRot;
    private bool secondHandGrippingAboveFirst;

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

        firstGrippingHand = dominantHand;
        secondGrippingHand = (dominantHand == rightHand) ? leftHand : rightHand;
    }

    private void FixedUpdate()
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
            rightHand.rb.isKinematic = false;
            leftHand.rb.isKinematic = false;
            rb.isKinematic = true;
        }
    }

    private void Update()
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
        firstGrippingHand = grippingHand;
        firstGrippingHand.enablePhysics = false;
        firstGrippingHand.rb.isKinematic = true;
        secondGrippingHand = (grippingHand == rightHand) ? leftHand : rightHand;
        secondGrippingHand.enablePhysics = true;
        secondGrippingHand.rb.isKinematic = false;

        Vector3 newAttachTransformLocalPos = new Vector3(0, 0, weapon.transform.InverseTransformPoint(grippingHand.grabPointTransform.position).z);
        if (grippingHand == rightHand)
        {
            weapon.rightAttachTransform.localPosition = newAttachTransformLocalPos;
            rb.centerOfMass = weapon.transform.InverseTransformPoint(weapon.rightAttachTransform.position);
        }
        else if (grippingHand == leftHand)
        {
            weapon.leftAttachTransform.localPosition = newAttachTransformLocalPos;
            rb.centerOfMass = weapon.transform.InverseTransformPoint(weapon.leftAttachTransform.position);
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
        // Hand instantaneously tracks controller without any physics, but must handle visuals seperately
        grippingHand.transform.position = grippingHand.parentController.transform.position;
        grippingHand.transform.rotation = grippingHand.parentController.transform.rotation;

        // ## Weapon ##
        // Rotation
        //weaponTargetRot = grippingHand.grabPointTransform.rotation;
        weaponTargetRot = Quaternion.LookRotation(grippingHand.grabPointTransform.forward, firstGrippingHand.grabPointTransform.up);

        // Position
        if (grippingHand == rightHand)
        {
            weaponTargetPos = rightHand.grabPointTransform.position;
            ProcessWeaponPhysics(weapon.rightAttachTransform.position);
        }
        else if (grippingHand == leftHand)
        {
            weaponTargetPos = leftHand.grabPointTransform.position;
            ProcessWeaponPhysics(weapon.leftAttachTransform.position);
        }
    }


    private void SetupTwoHandedMovement()
    {
        Debug.Log("Two Hand Setup");
        rb.isKinematic = false;
        rightHand.enablePhysics = false;
        rightHand.rb.isKinematic = true;
        leftHand.enablePhysics = false;
        leftHand.rb.isKinematic = true;

        //ResetGrabPointTransformLocals();

        // Set attach transforms on the weapon
        Vector3 newAttachTransformLocalPos = new Vector3(0, 0, weapon.transform.InverseTransformPoint(secondGrippingHand.grabPointTransform.position).z);
        if (secondGrippingHand == rightHand)
        {
            weapon.rightAttachTransform.localPosition = newAttachTransformLocalPos;
        }
        else if (secondGrippingHand == leftHand)
        {
            weapon.leftAttachTransform.localPosition = newAttachTransformLocalPos;
        }

        // Test if second hand is gripping above or below first gripping hand along the shaft
        Vector3 handGripDirection = secondGrippingHand.grabPointTransform.position - firstGrippingHand.grabPointTransform.position;
        if (weapon.transform.InverseTransformDirection(handGripDirection).z >= 0)
        {
            secondHandGrippingAboveFirst = true;
        }
        else
        {
            secondHandGrippingAboveFirst = false;
        }
    }

    private void ProcessTwoHandedMovement()
    {
        if (mustSetupGrips)
        {
            SetupTwoHandedMovement();
            mustSetupGrips = false;
        }

        // ## Hands ##
        rightHand.transform.position = rightController.transform.position;
        rightHand.transform.rotation = rightController.transform.rotation;
        leftHand.transform.position = leftController.transform.position;
        leftHand.transform.rotation = leftController.transform.rotation;

        // ## Weapon ##
        Vector3 handGripDirection = secondGrippingHand.grabPointTransform.position - firstGrippingHand.grabPointTransform.position;

        // Rotation
        if (secondHandGrippingAboveFirst)
        {
            weaponTargetRot = Quaternion.LookRotation(handGripDirection, firstGrippingHand.grabPointTransform.up);
        }
        else
        {
            weaponTargetRot = Quaternion.LookRotation(-handGripDirection, firstGrippingHand.grabPointTransform.up);
        }

        // Postion
        weaponTargetPos = firstGrippingHand.grabPointTransform.position + (handGripDirection / 2);

        Vector3 weaponGripDirection = weapon.leftAttachTransform.position - weapon.rightAttachTransform.position;
        Vector3 weaponGripMidPoint = weapon.rightAttachTransform.position + (weaponGripDirection / 2);
        rb.centerOfMass = weapon.transform.InverseTransformPoint(weaponGripMidPoint);

        ProcessWeaponPhysics(weaponGripMidPoint);
    }

    private void ProcessWeaponPhysics(Vector3 weaponTrackingPos)
    {
        // Movement
        rb.velocity = (weaponTargetPos - weaponTrackingPos) * positionSpeed * positionSpeedDamper * Time.deltaTime;

        // Rotation
        Quaternion rotDifference = weaponTargetRot * Quaternion.Inverse(weapon.transform.rotation);
        rotDifference.ToAngleAxis( out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180)
            angleInDegrees -= 360;

        rb.maxAngularVelocity = maxAngularVelocity;
        rb.angularVelocity = rotationAxis * angleInDegrees * Mathf.Deg2Rad * rotationSpeed * rotationSpeedDamper * Time.deltaTime;
    }


    private void UpdateWeaponSpawnState()
    {
        if (!weapon.GetPresenceState() && (dominantController.isGripActivated))
        {
            if (dominantHandType == HandTypes.RIGHT)
            {
                weapon.transform.rotation = rightHand.grabPointTransform.rotation;
                weapon.transform.position = rightHand.grabPointTransform.position + (weapon.transform.position - weapon.spawnAttachTransform.position);
            }
            else
            {
                weapon.transform.rotation = leftHand.grabPointTransform.rotation;
                weapon.transform.position = leftHand.grabPointTransform.position + (weapon.transform.position - weapon.spawnAttachTransform.position);
            }

            weapon.BeginMaterialising();
        }
        // Add extra else if for when you grip again before it finishes dematerialising, not teleporting the weapon back to the start spawn position
        else if (weapon.GetPresenceState() && !rightController.isGripActivated && !leftController.isGripActivated)
        {
            weapon.BeginDematerialising();
        }
    }
}
