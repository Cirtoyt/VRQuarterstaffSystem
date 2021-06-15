using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandling : MonoBehaviour
{
    [Header("Options")]
    public Weapon weapon;
    public XRController dominantHand;
    public float weaponGripChangeSmoothing;
    [Header("Statics")]
    [SerializeField] private XRController leftController;
    [SerializeField] private XRController rightController;

    private bool mustSetupSingleGrip;
    private bool mustSetupDoubleGrip;
    private XRController lastSingleGrippingHand;
    private XRController lastSingleFreeHand;
    private Vector3 originLeftHandAttachTransLocPos;
    private Quaternion originLeftHandAttachTransLocRot;
    private Vector3 originRightHandAttachTransLocPos;
    private Quaternion originRightHandAttachTransLocRot;
    private Vector3 originWeaponAttachTrans1LocPos;
    private Quaternion originWeaponAttachTrans1LocRot;
    private Vector3 originWeaponAttachTrans2LocPos;
    private Quaternion originWeaponAttachTrans2LocRot;
    private Vector3 originWeaponLocPos;

    private void Start()
    {
        mustSetupSingleGrip = false;
        mustSetupDoubleGrip = false;

        lastSingleGrippingHand = dominantHand;
        lastSingleFreeHand = (dominantHand == rightController) ? rightController : leftController;

        originLeftHandAttachTransLocPos = leftController.attachTransform.localPosition;
        originLeftHandAttachTransLocRot = leftController.attachTransform.localRotation;
        originRightHandAttachTransLocPos = rightController.attachTransform.localPosition;
        originRightHandAttachTransLocRot = rightController.attachTransform.localRotation;

        originWeaponAttachTrans1LocPos = weapon.attachTransform1.localPosition;
        originWeaponAttachTrans1LocRot = weapon.attachTransform1.localRotation;
        originWeaponAttachTrans2LocPos = weapon.attachTransform2.localPosition;
        originWeaponAttachTrans2LocRot = weapon.attachTransform2.localRotation;
    }

    void Update()
    {
        UpdateWeaponSpawnState();

        if (weapon.gameObject.activeInHierarchy && (rightController.isGripActivated || leftController.isGripActivated))
        {
            MoveWeapon();
        }
    }

    private void MoveWeapon()
    {
        // ## ONE-HANDED MODE ##
        if (rightController.isGripActivated && !leftController.isGripActivated)
        {
            ProcessOneHandedMovement(rightController);
        }
        else if (leftController.isGripActivated && !rightController.isGripActivated)
        {
            ProcessOneHandedMovement(leftController);
        }

        // Fix error where non-dominant hand grip is held when dominant hand grip begins holding
        else if (weapon.transform.parent == null && rightController.isGripActivated && leftController.isGripActivated)
        {
            ProcessOneHandedMovement(dominantHand);
            ProcessTwoHandedMovement();
        }

        // ## TWO-HANDED MODE ##
        else if (rightController.isGripActivated && leftController.isGripActivated)
        {
            ProcessTwoHandedMovement();
        }
    }


    private void ProcessOneHandedMovement(XRController grippingHand)
    {
        if (!mustSetupDoubleGrip)
        {
            mustSetupDoubleGrip = true;
        }
        if (mustSetupSingleGrip)
        {
            SetupOneHandedMovement(grippingHand);
        }

        weapon.transform.localPosition = -weapon.attachTransform1.localPosition;
        weapon.transform.localRotation = weapon.attachTransform1.localRotation;
    }

    private void SetupOneHandedMovement(XRController grippingHand)
    {
        weapon.transform.SetParent(grippingHand.attachTransform);
        
        lastSingleGrippingHand = (grippingHand == rightController) ? rightController : leftController;
        lastSingleFreeHand = (grippingHand == rightController) ? leftController : rightController;
        ResetLastHands();
        ResetWeaponTransforms();

        mustSetupSingleGrip = false;
    }


    private void ProcessTwoHandedMovement()
    {
        if (!mustSetupSingleGrip)
        {
            mustSetupSingleGrip = true;
        }

        // ## Rotation ##
        Vector3 handGripDifference = lastSingleFreeHand.attachTransform.position - lastSingleGrippingHand.attachTransform.position;
        Quaternion newRot = Quaternion.identity;

        // Test is second hand is gripping above or below first gripping hand along the shaft
        if (weapon.transform.InverseTransformDirection(handGripDifference).z >= 0)
        {
            newRot = Quaternion.LookRotation(handGripDifference);
        }
        else
        {
            Vector3 inverseHandGripDifference = lastSingleGrippingHand.attachTransform.position - lastSingleFreeHand.attachTransform.position;
            newRot = Quaternion.LookRotation(inverseHandGripDifference);
        }
        
        lastSingleGrippingHand.attachTransform.rotation = newRot;

        // Setup attach positions
        if (mustSetupDoubleGrip)
        {
            SetupTwoHandedMovement();
        }

        // ## Postion ##

        float rightHandStretchDist = rightController.attachTransform.position.z - weapon.attachTransform1.position.z;
        float leftHandStretchDist = leftController.attachTransform.position.z - weapon.attachTransform2.position.z;
        Debug.Log(rightHandStretchDist + "                 " + leftHandStretchDist);

        float totalStrechDist = rightHandStretchDist + leftHandStretchDist;
        // TODO/NOT WORKING
        //weapon.transform.localPosition = new Vector3(originWeaponLocPos.x, originWeaponLocPos.y, originWeaponLocPos.z + (totalStrechDist));
    }

    private void SetupTwoHandedMovement()
    {
        weapon.attachTransform1.position = rightController.attachTransform.position;
        weapon.attachTransform2.position = leftController.attachTransform.position;
        originWeaponLocPos = weapon.transform.localPosition;

        mustSetupDoubleGrip = false;
    }


    private void UpdateWeaponSpawnState()
    {
        if (!weapon.gameObject.activeInHierarchy && (dominantHand.isGripActivated))
        {
            SpawnWeapon(dominantHand);

            mustSetupSingleGrip = true;
            mustSetupDoubleGrip = true;
        }
        else if (weapon.gameObject.activeInHierarchy && !rightController.isGripActivated && !leftController.isGripActivated)
        {
            DespawnWeapon();
        }
    }

    private void SpawnWeapon(XRController controller)
    {
        weapon.transform.position = dominantHand.attachTransform.position + -weapon.attachTransform1.localPosition;
        weapon.transform.rotation = dominantHand.attachTransform.rotation;

        weapon.gameObject.SetActive(true);
    }

    private void DespawnWeapon()
    {
        weapon.gameObject.SetActive(false);

        weapon.transform.SetParent(null);
        ResetLastHands();
    }


    private void ResetLastHands()
    {
        if (lastSingleGrippingHand == rightController)
        {
            lastSingleGrippingHand.attachTransform.localPosition = originRightHandAttachTransLocPos;
            lastSingleGrippingHand.attachTransform.localRotation = originRightHandAttachTransLocRot;
            lastSingleFreeHand.attachTransform.localPosition = originLeftHandAttachTransLocPos;
            lastSingleFreeHand.attachTransform.localRotation = originLeftHandAttachTransLocRot;
        }
        else
        {
            lastSingleGrippingHand.attachTransform.localPosition = originLeftHandAttachTransLocPos;
            lastSingleGrippingHand.attachTransform.localRotation = originLeftHandAttachTransLocRot;
            lastSingleFreeHand.attachTransform.localPosition = originRightHandAttachTransLocPos;
            lastSingleFreeHand.attachTransform.localRotation = originRightHandAttachTransLocRot;
        }
    }

    private void ResetWeaponTransforms()
    {
        weapon.attachTransform1.localPosition = originWeaponAttachTrans1LocPos;
        weapon.attachTransform1.localRotation = originWeaponAttachTrans1LocRot;
        weapon.attachTransform2.localPosition = originWeaponAttachTrans2LocPos;
        weapon.attachTransform2.localRotation = originWeaponAttachTrans2LocRot;
    }
}
