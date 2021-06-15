using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform rightAttachTransform;
    public Transform leftAttachTransform;
    public Collider[] physicsColliders;

    private bool isPresent = false;
    private bool isSolid = false;
    private bool lastIsSolid = false;
    private bool isMaterialising = false;
    private bool isDematerialising = false;

    private Vector3 originRightAttachTransLocPos;
    private Quaternion originRightAttachTransLocRot;
    private Vector3 originLeftAttachTransLocPos;
    private Quaternion originLeftAttachTransLocRot;

    private void Start()
    {
        originRightAttachTransLocPos = rightAttachTransform.localPosition;
        originRightAttachTransLocRot = rightAttachTransform.localRotation;
        originLeftAttachTransLocPos = leftAttachTransform.localPosition;
        originLeftAttachTransLocRot = leftAttachTransform.localRotation;

        gameObject.SetActive(false);
    }

    void Update()
    {
        // MATERIALISATION UPDATES
        if (isMaterialising)
        {
            // Once done materialising
            if (true)
            {
                isMaterialising = false;
                isPresent = true;
            }
        }

        if (isDematerialising)
        {
            // Once done dematerialising
            if (true)
            {
                gameObject.SetActive(false);
                transform.SetParent(null);
                ResetWeaponLocals();
                isDematerialising = false;
                isPresent = false;
            }
        }

        // SOLIDITY UPDATES
        if (isSolid != lastIsSolid)
        {
            lastIsSolid = isSolid;

            // If now solid
            if (isSolid)
            {
                // Enable colliders, do a 'pop' visual effect, play a sound effect, etc
                foreach (Collider collider in physicsColliders)
                {
                    collider.enabled = true;
                }
            }
            else
            {
                // Disable colliders, update a hud element to show weapon is no longer fully active (present), etc
                foreach (Collider collider in physicsColliders)
                {
                    collider.enabled = false;
                }
            }
        }
    }

    public void BeginMaterialising()
    {
        gameObject.SetActive(true);
        isPresent = true;
        isMaterialising = true;
        isDematerialising = false;
    }

    public void BeginDematerialising()
    {
        isSolid = false;
        isDematerialising = true;
        isMaterialising = false;
    }

    /// <summary>
    /// Returns true if the weapon is present in any way, be that materialising, dematerialising, or solid.
    /// </summary>
    public bool GetPresenceState()
    {
        return isPresent;
    }

    public bool GetSolidityState()
    {
        return isSolid;
    }

    public bool GetMaterialisingState()
    {
        return isMaterialising;
    }

    public bool GetDematerialisingState()
    {
        return isDematerialising;
    }

    public void ResetWeaponLocals()
    {
        rightAttachTransform.localPosition = originRightAttachTransLocPos;
        rightAttachTransform.localRotation = originRightAttachTransLocRot;
        leftAttachTransform.localPosition = originLeftAttachTransLocPos;
        leftAttachTransform.localRotation = originLeftAttachTransLocRot;
    }
}
