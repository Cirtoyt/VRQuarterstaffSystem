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
    [SerializeField] private Material staffMat;
    [SerializeField] private Material staffTransMat;
    [SerializeField] private Material northTipMat;
    [SerializeField] private Material northTipTransMat;
    [SerializeField] private Material southTipMat;
    [SerializeField] private Material southTipTransMat;
    public Collider[] physicsColliders;
    public MeshRenderer[] meshesRenderers;
    [Header("Variables")]
    public float weaponLength;
    [SerializeField] private float materialisationSpeed;
    [SerializeField] private float minParticleAngVec;
    [SerializeField] private float maxParticleAngVec;
    [SerializeField] private float trailParticleAlphaMax;

    private Rigidbody rb;
    private bool isPresent;
    private bool isSolid;
    private bool lastIsSolid;
    private float materialisationPerc;
    private bool isMaterialising;
    private bool isDematerialising;
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
        materialisationPerc = 0;
        isMaterialising = false;
        isDematerialising = false;
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
            if (meshRenderer.name == "Core Mesh" || meshRenderer.name == "90 Degree Handle")
                meshRenderer.material = staffTransMat;
            else if (meshRenderer.name == "North Tip Mesh")
                meshRenderer.material = northTipTransMat;
            else if (meshRenderer.name == "South Tip Mesh")
                meshRenderer.material = southTipTransMat;

            var colour = meshRenderer.material.color;
            Color newColour = new Color(colour.r, colour.g, colour.b, 0);
            meshRenderer.material.color = newColour;
        }
    }

    void Update()
    {
        // MATERIALISATION UPDATES
        if (isMaterialising)
        {
            materialisationPerc += materialisationSpeed * Time.deltaTime;

            foreach (MeshRenderer meshRenderer in meshesRenderers)
            {
                var colour = meshRenderer.material.color;
                Color newColour = new Color(colour.r, colour.g, colour.b, materialisationPerc / 100);
                meshRenderer.material.color = newColour;
            }

            // Once done materialising
            if (materialisationPerc >= 100)
            {
                materialisationPerc = 100;
                isMaterialising = false;
                isSolid = true;
            }
        }

        if (isDematerialising)
        {
            materialisationPerc -= materialisationSpeed * Time.deltaTime;

            foreach (MeshRenderer meshRenderer in meshesRenderers)
            {
                var colour = meshRenderer.material.color;
                Color newColour = new Color(colour.r, colour.g, colour.b, materialisationPerc / 100);
                meshRenderer.material.color = newColour;
            }

            // Once done dematerialising
            if (materialisationPerc <= 0)
            {
                materialisationPerc = 0;
                ResetWeaponLocals();
                northPS.Stop();
                southPS.Stop();

                foreach (Collider collider in physicsColliders)
                {
                    collider.isTrigger = true;
                }

                foreach (MeshRenderer meshRenderer in meshesRenderers)
                {
                    var colour = meshRenderer.material.color;
                    Color newColour = new Color(colour.r, colour.g, colour.b, 0);
                    meshRenderer.material.color = newColour;
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
                    if (meshRenderer.name == "Core Mesh" || meshRenderer.name == "90 Degree Handle")
                        meshRenderer.material = staffMat;
                    else if (meshRenderer.name == "North Tip Mesh")
                        meshRenderer.material = northTipMat;
                    else if (meshRenderer.name == "South Tip Mesh")
                        meshRenderer.material = southTipMat;

                    var colour = meshRenderer.material.color;
                    Color newColour = new Color(colour.r, colour.g, colour.b, 1);
                    meshRenderer.material.color = newColour;
                }

                northPS.Play();
                southPS.Play();
            }
            else
            {
                // Do stuff when the weapon starts to become non-solid e.g. particle effect
            }
        }

        // VISUAL UPDATES
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
    }

    public void BeginMaterialising()
    {
        isPresent = true;
        isMaterialising = true;
        isDematerialising = false;
    }

    public void BeginDematerialising()
    {
        foreach (MeshRenderer meshRenderer in meshesRenderers)
        {
            if (meshRenderer.name == "Core Mesh" || meshRenderer.name == "90 Degree Handle")
                meshRenderer.material = staffTransMat;
            else if (meshRenderer.name == "North Tip Mesh")
                meshRenderer.material = northTipTransMat;
            else if (meshRenderer.name == "South Tip Mesh")
                meshRenderer.material = southTipTransMat;
        }

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnAttachTransform.position, 0.025f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(rightAttachTransform.position, 0.025f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(leftAttachTransform.position, 0.025f);
    }
}
