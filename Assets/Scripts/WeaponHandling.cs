using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandling : MonoBehaviour
{
    public Transform weapon;
    [SerializeField] private XRController leftController;
    [SerializeField] private XRController rightController;
    [SerializeField] private Transform attachTransform1;
    [SerializeField] private Transform attachTransform2;

    private bool canEnterSingleGrip;

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
            Debug.Log("Both controllers are gripping right now");
            canEnterSingleGrip = true;
        }
        else if (rightController.isGripActivated && canEnterSingleGrip)
        {
            weapon.SetParent(rightController.attachTransform);
            weapon.localPosition = -attachTransform1.localPosition;
            weapon.localRotation = attachTransform1.localRotation;
            canEnterSingleGrip = false;
        }
        else if (leftController.isGripActivated && canEnterSingleGrip)
        {
            weapon.SetParent(leftController.attachTransform);
            weapon.localPosition = -attachTransform1.localPosition;
            weapon.localRotation = attachTransform1.localRotation;
            canEnterSingleGrip = false;
        }
    }

    private void UpdateWeaponSpawnState()
    {
        if (!weapon.gameObject.activeInHierarchy && (rightController.isGripActivated || leftController.isGripActivated))
        {
            if (rightController.isGripActivated)
            {
                SpawnWeapon(rightController);
            }
            else
            {
                SpawnWeapon(leftController);
            }
            canEnterSingleGrip = true;
        }
        else if (weapon.gameObject.activeInHierarchy && !rightController.isGripActivated && !leftController.isGripActivated)
        {
            weapon.SetParent(null);
            DespawnWeapon();
        }
    }

    private void SpawnWeapon(XRController controller)
    {
        weapon.gameObject.SetActive(true);
    }

    private void DespawnWeapon()
    {
        weapon.gameObject.SetActive(false);
    }
}
