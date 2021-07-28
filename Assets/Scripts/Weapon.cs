using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Statics")]
    public Transform rightAttachTransform;
    public Transform leftAttachTransform;
    public Transform spawnAttachTransform;
    public ParticleSystem northPS;
    public ParticleSystem southPS;
    public Collider[] physicsColliders;
    public MeshRenderer[] meshesRenderers;
    [Header("Variables")]
    public float weaponLength;
    [SerializeField] private float minParticleAngVec;
    [SerializeField] private float maxParticleAngVec;
    [SerializeField] private float trailParticleAlphaMax;

    private Rigidbody rb;
    private bool isPresent = false;
    private bool isSolid = false;
    private bool lastIsSolid = false;
    private bool isMaterialising = false;
    private bool isDematerialising = false;
    private bool isColliding = false;
    private Vector3 originRightAttachTransLocPos;
    private Quaternion originRightAttachTransLocRot;
    private Vector3 originLeftAttachTransLocPos;
    private Quaternion originLeftAttachTransLocRot;

    private void Start()
    {
        northPS.Stop();
        southPS.Stop();

        rb = GetComponent<Rigidbody>();
        isPresent = false;
        isSolid = false;
        lastIsSolid = false;
        isMaterialising = false;
        isDematerialising = false;
        isColliding = false;
        originRightAttachTransLocPos = rightAttachTransform.localPosition;
        originRightAttachTransLocRot = rightAttachTransform.localRotation;
        originLeftAttachTransLocPos = leftAttachTransform.localPosition;
        originLeftAttachTransLocRot = leftAttachTransform.localRotation;

        foreach (Collider collider in physicsColliders)
        {
            collider.isTrigger = true;
        }
        foreach (MeshRenderer meshRenderer in meshesRenderers)
        {
            meshRenderer.enabled = false;
        }
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
                isSolid = true;
            }
        }

        if (isDematerialising)
        {
            // Once done dematerialising
            if (true)
            {
                ResetWeaponLocals();
                northPS.Stop();
                southPS.Stop();

                foreach (Collider collider in physicsColliders)
                {
                    collider.isTrigger = true;
                }
                foreach (MeshRenderer meshRenderer in meshesRenderers)
                {
                    meshRenderer.enabled = false;
                }

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
                    collider.isTrigger = false;
                }
                foreach (MeshRenderer meshRenderer in meshesRenderers)
                {
                    meshRenderer.enabled = true;
                }

                northPS.Play();
                southPS.Play();
            }
            else
            {
                // Do stuff when the weapon starts to become non-solid e.g. particle effect
            }
        }
    }

    private void FixedUpdate()
    {
        float alpha = 0;
        if (rb.angularVelocity.magnitude >= minParticleAngVec && rb.angularVelocity.magnitude <= maxParticleAngVec)
        {
            float range = maxParticleAngVec - minParticleAngVec;
            float correctedStartValue = rb.angularVelocity.magnitude - minParticleAngVec;
            alpha = correctedStartValue / range;
        }
        else if (rb.angularVelocity.magnitude > maxParticleAngVec)
        {
            alpha = 1;
        }

        Gradient northGradient = northPS.colorOverLifetime.color.gradient;
        northGradient.SetKeys(northGradient.colorKeys, new GradientAlphaKey[] { new GradientAlphaKey(alpha * trailParticleAlphaMax, 0), new GradientAlphaKey(0, 1) });
        var northColourOverLifetime = northPS.colorOverLifetime;
        northColourOverLifetime.color = northGradient;

        Gradient southGradient = southPS.colorOverLifetime.color.gradient;
        southGradient.SetKeys(southGradient.colorKeys, new GradientAlphaKey[] { new GradientAlphaKey(alpha * trailParticleAlphaMax, 0), new GradientAlphaKey(0, 1) });
        var southColourOverLifetime = southPS.colorOverLifetime;
        southColourOverLifetime.color = southGradient;
        //grad.SetKeys(new GradientColorKey[] {new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.red, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
    }

    public void BeginMaterialising()
    {
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
        //transform.localPosition = Vector3.zero;
        rightAttachTransform.localPosition = originRightAttachTransLocPos;
        rightAttachTransform.localRotation = originRightAttachTransLocRot;
        leftAttachTransform.localPosition = originLeftAttachTransLocPos;
        leftAttachTransform.localRotation = originLeftAttachTransLocRot;
    }

    public bool GetIsColliding()
    {
        return isColliding;
    }

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }
}
