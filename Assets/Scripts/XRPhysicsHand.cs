using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRPhysicsHand : MonoBehaviour
{
    [Header("Variables")]
    public XRController parentController;
    public Transform attachTransform;
    [Range(0, 1)] [SerializeField] private float slowDownVelocity = 0.75f;
    [Range(0, 1)] [SerializeField] private float slowDownAngularVelocity = 0.75f;
    [Range(0, 100)] [SerializeField] private float maxPositionChange = 75;
    [Range(0, 500)] [SerializeField] private float maxRotationChange = 250;
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
            // Currently do not have hand physics and instant snap physics hand to controller position & rotation:
            //rb.position = parentController.transform.position;
            //rb.rotation = parentController.transform.rotation;

            // Position
            //rb.velocity *= slowDownVelocity;

            //Vector3 posDifference = parentController.transform.position - rb.position;
            //Vector3 newVelocity = posDifference / Time.deltaTime;

            //if (!float.IsNaN(newVelocity.x) && !float.IsInfinity(newVelocity.x))
            //{
            //    float maxChange = maxPositionChange * Time.deltaTime;
            //    rb.velocity = Vector3.MoveTowards(rb.velocity, newVelocity, maxChange);
            //}

            rb.velocity = (parentController.transform.position - transform.position) * positionSpeed;

            // Rotation
            //rb.angularVelocity *= slowDownAngularVelocity;

            //Quaternion rotDifference = parentController.transform.rotation * Quaternion.Inverse(rb.rotation);
            //rotDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

            //if (angleInDegrees > 180)
            //    angleInDegrees -= 360;

            //Vector3 newAngularVelocity = (rotationAxis * angleInDegrees * Mathf.Deg2Rad) / Time.deltaTime;

            //if (!float.IsNaN(newAngularVelocity.x) && !float.IsInfinity(newAngularVelocity.x))
            //{
            //    float maxChange = maxRotationChange * Time.deltaTime;
            //    rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, newAngularVelocity, maxChange);
            //}

            Quaternion rotDifference = parentController.transform.rotation * Quaternion.Inverse(transform.rotation);
            rotDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

            rb.angularVelocity = rotationAxis * angleInDegrees * Mathf.Deg2Rad * rotationSpeed;
        }
    }
}
