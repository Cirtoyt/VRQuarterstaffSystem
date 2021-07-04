using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRPhysicsHand : MonoBehaviour
{
    [Header("Variables")]
    public XRController parentController;
    public Transform attachTransform;
    [SerializeField] private float positionSpeed = 20;
    [SerializeField] private float rotationSpeed = 100;
    [Header("Statics")]
    public XRHandVisuals handVisuals;

    [HideInInspector] public bool enablePhysics;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 30f;

        enablePhysics = true;

        transform.position = parentController.transform.position;
        transform.rotation = parentController.transform.rotation;
    }

    private void Update()
    {
        if (enablePhysics)
        {
            // Position
            rb.velocity = (parentController.transform.position - transform.position) * positionSpeed;

            // Rotation
            Quaternion rotDifference = parentController.transform.rotation * Quaternion.Inverse(transform.rotation);
            rotDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

            rb.angularVelocity = rotationAxis * angleInDegrees * Mathf.Deg2Rad * rotationSpeed;
        }
    }
}
