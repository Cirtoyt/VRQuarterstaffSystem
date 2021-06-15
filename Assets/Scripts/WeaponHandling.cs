using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandling : MonoBehaviour
{
    public Transform weapon;
    public XRController dominantHand;
    [SerializeField] private XRController leftController;
    [SerializeField] private XRController rightController;
    [SerializeField] private Transform attachTransform1;
    [SerializeField] private Transform attachTransform2;

    private XRController nonDominantHand;
    private XRController lastSingleGrippingHand;
    private XRController lastSingleFreeHand;
    private bool canEnterSingleGrip;
    private Vector3 lastSingleGrippingHandAttachTransformLocalPos;
    private Quaternion lastSingleGrippingHandAttachTransformLocalRot;

    private void Start()
    {
        nonDominantHand = rightController == dominantHand ? leftController : rightController;

        lastSingleGrippingHand = dominantHand;
        lastSingleFreeHand = nonDominantHand;

        lastSingleGrippingHandAttachTransformLocalPos = lastSingleGrippingHand.attachTransform.localPosition;
        lastSingleGrippingHandAttachTransformLocalRot = lastSingleGrippingHand.attachTransform.localRotation;
    }

    void Update()
    {
        UpdateWeaponSpawnState();

        if (rightController.isGripActivated || leftController.isGripActivated)
        {
            MoveWeapon();
        }
    }

    private void MoveWeapon()
    {
        if (rightController.isGripActivated && leftController.isGripActivated)
        {
            if (!canEnterSingleGrip)
                canEnterSingleGrip = true;

            // ### Do two-handed code here ###

            Vector3 handGripDifference = lastSingleFreeHand.attachTransform.position - lastSingleGrippingHand.attachTransform.position;
            Quaternion newRot = Quaternion.identity;

            if (weapon.InverseTransformDirection(handGripDifference).z >= 0)
            {
                // Second hand that is now gripping on is ^ above ^ first gripping hand along the shaft
                newRot = Quaternion.LookRotation(handGripDifference);
            }
            else
            {
                // Second hand that is now gripping on is v below v first gripping hand along the shaft
                Vector3 inverseHandGripDifference = lastSingleGrippingHand.attachTransform.position - lastSingleFreeHand.attachTransform.position;
                newRot = Quaternion.LookRotation(inverseHandGripDifference);
            }

            //weapon.localPosition = newPos;
            lastSingleGrippingHand.attachTransform.rotation = newRot;

            Debug.Log("Both controllers are gripping right now");
        }
        else if (rightController.isGripActivated && canEnterSingleGrip)
        {
            if (weapon.parent != rightController.attachTransform)
            {
                weapon.SetParent(rightController.attachTransform);
                ResetLastSingleGrippingHand();
                weapon.localPosition = -attachTransform1.localPosition;
                weapon.localRotation = attachTransform1.localRotation;
                lastSingleGrippingHand = rightController;
                lastSingleFreeHand = leftController;
                canEnterSingleGrip = false;
            }
        }
        else if (leftController.isGripActivated && canEnterSingleGrip)
        {
            if (weapon.parent != leftController.attachTransform)
            {
                weapon.SetParent(leftController.attachTransform);
                ResetLastSingleGrippingHand();
                weapon.localPosition = -attachTransform1.localPosition;
                weapon.localRotation = attachTransform1.localRotation;
                lastSingleGrippingHand = leftController;
                lastSingleFreeHand = rightController;
                canEnterSingleGrip = false;
            }
        }
    }

    private void ResetLastSingleGrippingHand()
    {
        lastSingleGrippingHand.attachTransform.localPosition = lastSingleGrippingHandAttachTransformLocalPos;
        lastSingleGrippingHand.attachTransform.localRotation = lastSingleGrippingHandAttachTransformLocalRot;
    }

    private void UpdateWeaponSpawnState()
    {
        if (!weapon.gameObject.activeInHierarchy && (dominantHand.isGripActivated))
        {
            SpawnWeapon(dominantHand);
            canEnterSingleGrip = true;
        }
        else if (weapon.gameObject.activeInHierarchy && !rightController.isGripActivated && !leftController.isGripActivated)
        {
            ResetLastSingleGrippingHand();
            weapon.SetParent(null);
            DespawnWeapon();
        }
    }

    private void SpawnWeapon(XRController controller)
    {
        weapon.gameObject.SetActive(true);
        weapon.SetParent(dominantHand.attachTransform);
        weapon.localPosition = -attachTransform1.localPosition;
        weapon.localRotation = attachTransform1.localRotation;
    }

    private void DespawnWeapon()
    {
        weapon.gameObject.SetActive(false);
    }
}
