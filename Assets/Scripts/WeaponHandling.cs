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

    [Header("Grabbing Options")]
    public HandTypes dominantHandType;
    [Range(0.01f, 0.2f)] [SerializeField] private float grabRadius;
    [SerializeField] private LayerMask grabbableLayer;
    [Header("Loose Grip Options")]
    [SerializeField] private float looseGripSlipSpeed;
    [Range(0.01f, 1)] [SerializeField] private float looseGripTwoHandedMultiplier;
    [Header("Movement Options")]
    [SerializeField] private float positionSpeed;
    [Range(0.01f, 1)] [SerializeField] private float minPositionSpeedDamper;
    [Range(0.01f, 1)] [SerializeField] private float maxPositionSpeedDamper;
    [Range(0.01f, 1)] [SerializeField] private float minRotationSpeedDamper;
    [Range(0.01f, 1)] [SerializeField] private float maxRotationSpeedDamper;
    [Space]
    [Range(0.01f, 1)] [SerializeField] private float oneHandedSpeedDamperMultiplier;
    [Range(0.01f, 1)] [SerializeField] private float minGripDistanceStrengthMultiplier;
    [Header("Hand Visuals Options")]
    [SerializeField] private float handVisualMinEndCapDepth;
    [SerializeField] private Vector3 handVisualPositionOffset;
    [SerializeField] private Vector3 handVisualEulerRotationOffset;
    [SerializeField] private Vector3 handVisualStaffEndEulerRotationOffset;
    
    [Header("Debugging (Don't change in inspector)")]
    [Range(0.01f, 1)] [SerializeField] private float positionSpeedDamper;
    [Range(0.01f, 1)] [SerializeField] private float rotationSpeedDamper;
    [SerializeField] private Vector3 looseGripGravityForce;
    
    [Header("Statics")]
    public Weapon weapon;
    [SerializeField] private XRController leftController;
    [SerializeField] private XRPhysicsHand leftHand;
    [SerializeField] private XRController rightController;
    [SerializeField] private XRPhysicsHand rightHand;

    private enum GripStates
    {
        EMPTY,
        RIGHTHANDED,
        LEFTHANDED,
        TWOHANDED,
    }

    private HandTypes lastDominantHandType;
    private GripStates gripState;
    private bool mustSetupGrips;
    private bool lastRightGripValue;
    private bool lastLeftGripValue;
    private Rigidbody rb;
    private XRPhysicsHand firstGrippingHand;
    private XRPhysicsHand secondGrippingHand;
    private XRController dominantController;
    private XRPhysicsHand dominantHand;
    private Vector3 weaponTargetPos;
    private Quaternion weaponTargetRot;
    private bool weaponIsFacingThumb;
    private bool secondHandGrippingAboveFirst;
    private Vector3 weaponRotationUp;

    private void Start()
    {
        lastDominantHandType = dominantHandType;
        gripState = GripStates.EMPTY;
        mustSetupGrips = false;
        lastRightGripValue = lastLeftGripValue = false;
        looseGripGravityForce = Vector3.zero;
        rb = weapon.GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 30;

        UpdateDominantHand();

        firstGrippingHand = dominantHand;
        secondGrippingHand = (dominantHand == rightHand) ? leftHand : rightHand;
    }

    private void FixedUpdate()
    {
        // ## SPAWNING WEAPON ##
        UpdateWeaponSpawnState();

        // ## CHECKING GRIP CHANGES ##
        CheckGripChanges();

        // ## GRIP REPOSITIONING ##
        GripRepositioning();

        // ## WEAPON MOVEMENT ##
        if (weapon.GetPresenceState())
        {
            MoveWeapon();
        }
        else
        {
            // Teleport invisible weapon constantly to hand
            weapon.transform.position = dominantHand.grabPointTransform.position;
            weapon.transform.rotation = dominantHand.grabPointTransform.rotation;
        }

        // ## HAND VISUALS ##
        UpdateVisualHands();
    }

    private void UpdateWeaponSpawnState()
    {
        // Check if the dominant hand type has changed
        if (dominantHandType != lastDominantHandType)
        {
            UpdateDominantHand();
        }
        lastDominantHandType = dominantHandType;

        // Check for spawning the weapon in the dominent hand when it grips
        if (!weapon.GetPresenceState() && dominantController.isGripActivated && !lastRightGripValue && !lastLeftGripValue)
        {
            if (dominantHandType == HandTypes.RIGHT)
            {
                weapon.transform.rotation = rightHand.grabPointTransform.rotation;
                weapon.transform.position = rightHand.grabPointTransform.position + (weapon.transform.position - weapon.spawnAttachTransform.position);
                weapon.rightAttachTransform.position = weapon.spawnAttachTransform.position;
            }
            else
            {
                weapon.transform.rotation = leftHand.grabPointTransform.rotation;
                weapon.transform.position = leftHand.grabPointTransform.position + (weapon.transform.position - weapon.spawnAttachTransform.position);
                weapon.leftAttachTransform.position = weapon.spawnAttachTransform.position;
            }

            weapon.BeginMaterialising();
        }
        // Check for despawning the weapon
        else if (weapon.GetPresenceState() && gripState == GripStates.RIGHTHANDED && !rightController.isGripActivated
                 || weapon.GetPresenceState() && gripState == GripStates.LEFTHANDED && !leftController.isGripActivated
                 || weapon.GetPresenceState() && gripState == GripStates.TWOHANDED && !rightController.isGripActivated && !leftController.isGripActivated)
        {
            weapon.BeginDematerialising();
        }
    }

    private void UpdateDominantHand()
    {
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
    }

    private void CheckGripChanges()
    {
        if (lastRightGripValue != rightController.isGripActivated
            || lastLeftGripValue != leftController.isGripActivated)
        {
            // ## ONE-HANDED MODE ##
            if ((rightController.isGripActivated && !leftController.isGripActivated && IsWithinGrabRange(rightHand))
                || (rightController.isGripActivated && leftController.isGripActivated && IsWithinGrabRange(rightHand) && !IsWithinGrabRange(leftHand) && !lastLeftGripValue))
            {
                UpdateGripState(GripStates.RIGHTHANDED);
            }
            else if ((leftController.isGripActivated && !rightController.isGripActivated && IsWithinGrabRange(leftHand))
                     || (leftController.isGripActivated && rightController.isGripActivated && IsWithinGrabRange(leftHand) && !IsWithinGrabRange(rightHand) && !lastRightGripValue))
            {
                UpdateGripState(GripStates.LEFTHANDED);
            }

            // ## TWO-HANDED MODE ##
            else if (rightController.isGripActivated && leftController.isGripActivated
                     && IsWithinGrabRange(rightHand, true) && IsWithinGrabRange(leftHand, true))
            {
                UpdateGripState(GripStates.TWOHANDED);
            }

            // ## EMPTY GRIP MODE ##
            else if (!rightController.isGripActivated && !leftController.isGripActivated
                     || rightController.isGripActivated && !IsWithinGrabRange(rightHand) && !leftController.isGripActivated
                     || leftController.isGripActivated && !IsWithinGrabRange(leftHand) && !rightController.isGripActivated)
            {
                UpdateGripState(GripStates.EMPTY);
                rb.isKinematic = true;
            }

            lastRightGripValue = rightController.isGripActivated;
            lastLeftGripValue = leftController.isGripActivated;
        }
    }

    private bool IsWithinGrabRange(XRPhysicsHand grabbingHand, bool enteringTwoHandedMode = false)
    {
        if (gripState == GripStates.TWOHANDED)
        {
            if (grabbingHand == rightHand)
            {
                return Vector3.Distance(weapon.transform.position, weapon.rightAttachTransform.position) <= weapon.weaponLength / 2;
            }
            else
            {
                return Vector3.Distance(weapon.transform.position, weapon.leftAttachTransform.position) <= weapon.weaponLength / 2;
            }
        }
        else if (gripState == GripStates.RIGHTHANDED && enteringTwoHandedMode)
        {
            if (grabbingHand == rightHand)
            {
                return Vector3.Distance(weapon.transform.position, weapon.rightAttachTransform.position) <= weapon.weaponLength / 2;
            }
            else
            {
                bool overlapping = Physics.CheckSphere(grabbingHand.grabPointTransform.position, grabRadius, grabbableLayer, QueryTriggerInteraction.Collide);
                bool onStaff = Vector3.Distance(weapon.transform.position, grabbingHand.grabPointTransform.position) <= weapon.weaponLength / 2;
                return overlapping && onStaff;
            }
        }
        else if (gripState == GripStates.LEFTHANDED && enteringTwoHandedMode)
        {
            if (grabbingHand == leftHand)
            {
                return Vector3.Distance(weapon.transform.position, weapon.leftAttachTransform.position) <= weapon.weaponLength / 2;
            }
            else
            {
                bool overlapping = Physics.CheckSphere(grabbingHand.grabPointTransform.position, grabRadius, grabbableLayer, QueryTriggerInteraction.Collide);
                bool onStaff = Vector3.Distance(weapon.transform.position, grabbingHand.grabPointTransform.position) <= weapon.weaponLength / 2;
                return overlapping && onStaff;
            }
        }
        else
        {
            bool overlapping = Physics.CheckSphere(grabbingHand.grabPointTransform.position, grabRadius, grabbableLayer, QueryTriggerInteraction.Collide);
            bool onStaff = Vector3.Distance(weapon.transform.position, grabbingHand.grabPointTransform.position) <= weapon.weaponLength / 2;
            return overlapping && onStaff;
        }
    }

    private void UpdateGripState(GripStates newState)
    {
        gripState = newState;

        if (newState != GripStates.EMPTY)
            mustSetupGrips = true;
    }

    private void GripRepositioning()
    {
        if (gripState == GripStates.TWOHANDED)
        {
            if (rightController.isTriggerActivated && !leftController.isTriggerActivated)
            {
                MoveGripPosition(rightHand.grabPointTransform, weapon.rightAttachTransform, false);
                // No loose grip gravity when one hand has a grip still
                looseGripGravityForce = Vector3.zero;
            }
            else if (leftController.isTriggerActivated && !rightController.isTriggerActivated)
            {
                MoveGripPosition(leftHand.grabPointTransform, weapon.leftAttachTransform, false);
                // No loose grip gravity when one hand has a grip still
                looseGripGravityForce = Vector3.zero;
            }
            else if (rightController.isTriggerActivated && leftController.isTriggerActivated)
            {
                SimulateLooseGripGravity();

                Vector3 newRightAttachTransformLocalPos = new Vector3(0, 0, weapon.transform.InverseTransformPoint(rightHand.grabPointTransform.position).z);
                Vector3 newLeftAttachTransformLocalPos = new Vector3(0, 0, weapon.transform.InverseTransformPoint(leftHand.grabPointTransform.position).z);
                float rightDirectionDif = newRightAttachTransformLocalPos.z - weapon.rightAttachTransform.localPosition.z;
                float leftDirectionDif = newLeftAttachTransformLocalPos.z - weapon.leftAttachTransform.localPosition.z;
                bool applyFriction = (rightDirectionDif > 0 && leftDirectionDif > 0) || (rightDirectionDif < 0 && leftDirectionDif < 0);

                MoveGripPosition(rightHand.grabPointTransform, weapon.rightAttachTransform, applyFriction, false);
                MoveGripPosition(leftHand.grabPointTransform, weapon.leftAttachTransform, applyFriction, false);
            }
            else if (looseGripGravityForce != Vector3.zero) // No repositioning, no loose grip gravity, as both hands are gripping
            {
                looseGripGravityForce = Vector3.zero;
            }
        }
        else if (gripState == GripStates.RIGHTHANDED && rightController.isTriggerActivated)
        {
            SimulateLooseGripGravity();
            MoveGripPosition(rightHand.grabPointTransform, weapon.rightAttachTransform, true, true);
        }
        else if (gripState == GripStates.LEFTHANDED && leftController.isTriggerActivated)
        {
            SimulateLooseGripGravity();
            MoveGripPosition(leftHand.grabPointTransform, weapon.leftAttachTransform, true, true);
        }
        else // No repositioning, empty grip, so no loose grip gravity
        {
            looseGripGravityForce = Vector3.zero;
            weaponRotationUp = (rightHand.handVisual.transform.up + leftHand.handVisual.transform.up).normalized;
        }
    }

    private void MoveGripPosition(Transform grabPointTransform, Transform attachTransform, bool applyFriction, bool ignoreOtherHandOverlapping = false)
    {
        bool onStaff = Vector3.Distance(weapon.transform.position, grabPointTransform.position) <= weapon.weaponLength / 2;

        Vector3 newAttachTransformLocalPos = new Vector3(0, 0, weapon.transform.InverseTransformPoint(grabPointTransform.position).z);
        float weaponWorldUpDot = Vector3.Dot(weapon.transform.forward, Vector3.up);
        
        if (!ignoreOtherHandOverlapping)
        {
            bool isntPassingOtherHand = false;
            if (attachTransform == weapon.rightAttachTransform)
            {
                // if hand is above before move and new pos is still above and also away enough from the other hand (to ensure no overlapping hand visuals)
                if (attachTransform.localPosition.z > weapon.leftAttachTransform.localPosition.z
                    && newAttachTransformLocalPos.z >= weapon.leftAttachTransform.localPosition.z + grabRadius)
                {
                    isntPassingOtherHand = true;
                }
                // else if hand is below before move and new pos is still below and also away enough from the other hand (to ensure no overlapping hand visuals)
                else if (attachTransform.localPosition.z < weapon.leftAttachTransform.localPosition.z
                         && newAttachTransformLocalPos.z <= weapon.leftAttachTransform.localPosition.z - grabRadius)
                {
                    isntPassingOtherHand = true;
                }
            }
            else // attachTransform == weapon.leftAttachTransform
            {
                if (attachTransform.localPosition.z > weapon.rightAttachTransform.localPosition.z
                    && newAttachTransformLocalPos.z >= weapon.rightAttachTransform.localPosition.z + grabRadius)
                {
                    isntPassingOtherHand = true;
                }
                else if (attachTransform.localPosition.z < weapon.rightAttachTransform.localPosition.z
                         && newAttachTransformLocalPos.z <= weapon.rightAttachTransform.localPosition.z - grabRadius)
                {
                    isntPassingOtherHand = true;
                }
            }

            if (onStaff && isntPassingOtherHand)
            {
                // Lerp prevents moving the attach transfrom along the pole is the staff is horizontal with world to simulate realistic friction
                if (applyFriction)
                {
                    attachTransform.localPosition = Vector3.Lerp(attachTransform.localPosition, newAttachTransformLocalPos, Mathf.Abs(weaponWorldUpDot));
                }
                else
                {
                    attachTransform.localPosition = newAttachTransformLocalPos;
                }

                UpdateDampers();
            }
        }
        else
        {
            if (onStaff)
            {
                // Lerp prevents moving the attach transfrom along the pole is the staff is horizontal with world to simulate realistic friction
                if (applyFriction)
                {
                    attachTransform.localPosition = Vector3.Lerp(attachTransform.localPosition, newAttachTransformLocalPos, Mathf.Abs(weaponWorldUpDot));
                }
                else
                {
                    attachTransform.localPosition = newAttachTransformLocalPos;
                }

                UpdateDampers();
            }
        }
        
    }

    private void SimulateLooseGripGravity()
    {
        looseGripGravityForce = -weapon.transform.forward * Vector3.Dot(weapon.transform.forward, Vector3.up) * looseGripSlipSpeed * Time.deltaTime;

        // Half speed if both hands are hald gripping
        if (gripState == GripStates.TWOHANDED)
            looseGripGravityForce *= looseGripTwoHandedMultiplier;

        Vector3 newPos = weapon.transform.position + looseGripGravityForce;

        bool rightHandIsWithin = Vector3.Distance(newPos, weapon.rightAttachTransform.position) <= weapon.weaponLength / 2;
        bool leftHandIsWithin = Vector3.Distance(newPos, weapon.leftAttachTransform.position) <= weapon.weaponLength / 2;

        // If hands aren within weapon length bounds, update weapon position
        if (gripState == GripStates.RIGHTHANDED && rightHandIsWithin)
        {
            weapon.transform.position = newPos;
        }
        else if (gripState == GripStates.LEFTHANDED && leftHandIsWithin)
        {
            weapon.transform.position = newPos;
        }
        else if (gripState == GripStates.TWOHANDED && rightHandIsWithin && leftHandIsWithin)
        {
            weapon.transform.position = newPos;
        }
        else // don't set new gravity affected position & zero-out built-up gravity force
        {
            looseGripGravityForce = Vector3.zero;
        }
    }

    private void MoveWeapon()
    {
        // ## ONE-HANDED MODE ##
        if (gripState == GripStates.RIGHTHANDED)
        {
            ProcessOneHandedMovement(rightHand);
        }
        else if (gripState == GripStates.LEFTHANDED)
        {
            ProcessOneHandedMovement(leftHand);
        }

        // ## TWO-HANDED MODE ##
        else if (gripState == GripStates.TWOHANDED)
        {
            ProcessTwoHandedMovement();
        }
    }

    private void SetupOneHandedMovement(XRPhysicsHand grippingHand)
    {
        rb.isKinematic = false;
        firstGrippingHand = grippingHand;
        secondGrippingHand = (grippingHand == rightHand) ? leftHand : rightHand;

        if (grippingHand == rightHand)
        {
            rb.centerOfMass = weapon.transform.InverseTransformPoint(weapon.rightAttachTransform.position);
        }
        else if (grippingHand == leftHand)
        {
            rb.centerOfMass = weapon.transform.InverseTransformPoint(weapon.leftAttachTransform.position);
        }

        UpdateDampers();

        weaponIsFacingThumb = Vector3.Dot(weapon.transform.forward, grippingHand.grabPointTransform.forward) >= 0;
    }

    private void ProcessOneHandedMovement(XRPhysicsHand grippingHand)
    {
        if (mustSetupGrips)
        {
            SetupOneHandedMovement(grippingHand);
            mustSetupGrips = false;
        }

        // Rotation
        if (weaponIsFacingThumb)
        {
            weaponTargetRot = Quaternion.LookRotation(grippingHand.grabPointTransform.forward, grippingHand.grabPointTransform.up);
        }
        else
        {
            weaponTargetRot = Quaternion.LookRotation(-grippingHand.grabPointTransform.forward, grippingHand.grabPointTransform.up);
        }

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
        rb.isKinematic = false;

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

        UpdateDampers();

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

    private void UpdateDampers()
    {
        float closestToCentreDistance;
        float damperStrength;

        if (gripState == GripStates.TWOHANDED)
        {
            // Calculate multiplier based on how far away from the centre of mass the closest hand is
            closestToCentreDistance = Vector3.Distance(weapon.rightAttachTransform.position, weapon.transform.position);
            float leftDist = Vector3.Distance(weapon.leftAttachTransform.position, weapon.transform.position);
            if (leftDist < closestToCentreDistance)
                closestToCentreDistance = leftDist;
            float closestToCentreStrengthMultipler = 1 - (closestToCentreDistance / (weapon.weaponLength / 2));

            float gripDistance = Vector3.Distance(weapon.rightAttachTransform.position, weapon.leftAttachTransform.position);
            float gripOverWeaponLength = gripDistance / weapon.weaponLength;
            float gripStrengthRange = 1 - minGripDistanceStrengthMultiplier;
            float gripStrengthMultipler = (gripStrengthRange * gripOverWeaponLength) + minGripDistanceStrengthMultiplier;

            damperStrength = closestToCentreStrengthMultipler * gripStrengthMultipler;

            // When one hand is on either side of the weapon centre:
            if ((weapon.rightAttachTransform.localPosition.z >= 0 && weapon.leftAttachTransform.localPosition.z <= 0)
                || (weapon.leftAttachTransform.localPosition.z >= 0 && weapon.rightAttachTransform.localPosition.z <= 0))
            {
                // Only grip distance multiplier is applied as you have full control over the centre of mass
                damperStrength = gripStrengthMultipler;
            }
        }
        else // gripState == GripStates.RIGHTHANDED || GripStates.LEFTHANDED
        {
            if (firstGrippingHand == rightHand)
            {
                closestToCentreDistance = Vector3.Distance(weapon.rightAttachTransform.position, weapon.transform.position);
            }
            else // firstGrippingHand == leftHand
            {
                closestToCentreDistance = Vector3.Distance(weapon.leftAttachTransform.position, weapon.transform.position);
            }

            damperStrength = (1 - (closestToCentreDistance / (weapon.weaponLength / 2))) * oneHandedSpeedDamperMultiplier;
        }

        float positionDamperRange = maxPositionSpeedDamper - minPositionSpeedDamper;
        float rotationDamperRange = maxRotationSpeedDamper - minRotationSpeedDamper;

        positionSpeedDamper = (positionDamperRange * damperStrength) + minPositionSpeedDamper;
        rotationSpeedDamper = (rotationDamperRange * damperStrength) + minRotationSpeedDamper;
    }

    private void ProcessWeaponPhysics(Vector3 weaponTrackingPos)
    {
        // Movement
        rb.velocity = (weaponTargetPos - weaponTrackingPos) * positionSpeed * positionSpeedDamper * Time.deltaTime;

        // Rotation
        Quaternion rotDifference = weaponTargetRot * Quaternion.Inverse(weapon.transform.rotation);
        rotDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        // Infinity if already aligned, so return
        if (float.IsInfinity(rotationAxis.x))
            return;

        if (angleInDegrees > 180)
            angleInDegrees -= 360;

        rb.angularVelocity = (0.9f * rotationSpeedDamper * Mathf.Deg2Rad * angleInDegrees / Time.deltaTime) * rotationAxis.normalized;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rightHand.grabPointTransform.position, grabRadius);
        Gizmos.DrawWireSphere(leftHand.grabPointTransform.position, grabRadius);
    }

    private void UpdateVisualHands()
    {
        Vector3 rightPositionOffset = handVisualPositionOffset;
        Vector3 leftPositionOffset = new Vector3(-handVisualPositionOffset.x, handVisualPositionOffset.y, handVisualPositionOffset.z);
        Quaternion rotationOffset = Quaternion.Euler(handVisualEulerRotationOffset.x, handVisualEulerRotationOffset.y, handVisualEulerRotationOffset.z);
        Quaternion rightStickEndRotationOffset = Quaternion.Euler(handVisualStaffEndEulerRotationOffset.x, handVisualStaffEndEulerRotationOffset.y, handVisualStaffEndEulerRotationOffset.z);
        Quaternion leftStickEndRotationOffset = Quaternion.Euler(handVisualStaffEndEulerRotationOffset.x, -handVisualStaffEndEulerRotationOffset.y, handVisualStaffEndEulerRotationOffset.z);

        if (rightController.isGripActivated && (gripState == GripStates.RIGHTHANDED || gripState == GripStates.TWOHANDED))
        {
            // Right Position
            rightHand.handVisual.transform.position = weapon.rightAttachTransform.position;
            rightHand.handVisualModel.localPosition = rightPositionOffset;

            // Right Rotation
            Quaternion newRotation;
            // If on stick end
            if (Vector3.Distance(weapon.transform.position, weapon.rightAttachTransform.position) >= weapon.weaponLength / 2 - handVisualMinEndCapDepth)
            {
                // If at north end or south end
                Vector3 handGripDirection = weapon.rightAttachTransform.position - weapon.transform.position;
                if (weapon.transform.InverseTransformDirection(handGripDirection).z >= 0)
                {
                    newRotation = Quaternion.LookRotation(weapon.transform.forward, rightHand.grabPointTransform.up) * rightStickEndRotationOffset;
                }
                else
                {
                    newRotation = Quaternion.LookRotation(-weapon.transform.forward, rightHand.grabPointTransform.up) * rightStickEndRotationOffset;
                }
            }
            // If thumb is facing north
            else if (Vector3.Dot(rightHand.grabPointTransform.forward, weapon.transform.forward) >= 0)
            {
                newRotation = Quaternion.LookRotation(weapon.transform.forward, rightHand.grabPointTransform.up) * rotationOffset;
            }
            // If thumb is facing south
            else
            {
                newRotation = Quaternion.LookRotation(-weapon.transform.forward, rightHand.grabPointTransform.up) * rotationOffset;
            }

            rightHand.handVisual.transform.rotation = Quaternion.RotateTowards(rightHand.handVisual.transform.rotation, newRotation, float.PositiveInfinity);
        }
        else if (rightHand.handVisual.transform.localPosition != Vector3.zero || rightHand.handVisual.transform.localRotation != Quaternion.identity)
        {
            rightHand.handVisual.transform.localPosition = Vector3.zero;
            rightHand.ResetHandVisualModelLocalPosition();
            rightHand.handVisual.transform.localRotation = Quaternion.identity;
        }

        if (leftController.isGripActivated && (gripState == GripStates.LEFTHANDED || gripState == GripStates.TWOHANDED))
        {
            // Left Position
            leftHand.handVisual.transform.position = weapon.leftAttachTransform.position;
            leftHand.handVisualModel.localPosition = leftPositionOffset;

            // Left Rotation
            Quaternion newRotation;
            // If on stick end
            if (Vector3.Distance(weapon.transform.position, weapon.leftAttachTransform.position) >= weapon.weaponLength / 2 - handVisualMinEndCapDepth)
            {
                // If at north end or south end
                Vector3 handGripDirection = weapon.leftAttachTransform.position - weapon.transform.position;
                if (weapon.transform.InverseTransformDirection(handGripDirection).z >= 0)
                {
                    newRotation = Quaternion.LookRotation(weapon.transform.forward, leftHand.grabPointTransform.up) * leftStickEndRotationOffset;
                }
                else
                {
                    newRotation = Quaternion.LookRotation(-weapon.transform.forward, leftHand.grabPointTransform.up) * leftStickEndRotationOffset;
                }
            }
            // If thumb is facing north
            else if (Vector3.Dot(leftHand.grabPointTransform.forward, weapon.transform.forward) >= 0)
            {
                newRotation = Quaternion.LookRotation(weapon.transform.forward, leftHand.grabPointTransform.up) * rotationOffset;
            }
            // If thumb is facing south
            else
            {
                newRotation = Quaternion.LookRotation(-weapon.transform.forward, leftHand.grabPointTransform.up) * rotationOffset;
            }

            leftHand.handVisual.transform.rotation = Quaternion.RotateTowards(leftHand.handVisual.transform.rotation, newRotation, float.PositiveInfinity);
        }
        else if (leftHand.handVisual.transform.localPosition != Vector3.zero || leftHand.handVisual.transform.localRotation != Quaternion.identity)
        {
            leftHand.handVisual.transform.localPosition = Vector3.zero;
            leftHand.ResetHandVisualModelLocalPosition();
            leftHand.handVisual.transform.localRotation = Quaternion.identity;
        }
    }
}
