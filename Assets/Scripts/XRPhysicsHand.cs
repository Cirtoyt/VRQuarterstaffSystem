using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRPhysicsHand : MonoBehaviour
{
    [Header("Variables")]
    public XRController parentController;
    public Transform grabPointTransform;
    [SerializeField] private float positionSpeed = 20;
    [Range(0.01f,1)][SerializeField] private float rotationSpeedDamper = 100;
    [Header("Statics")]
    public XRHandVisuals handVisuals;

    [HideInInspector] public bool enablePhysics;
    [HideInInspector] public Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 30;

        enablePhysics = true;

        transform.position = parentController.transform.position;
        transform.rotation = parentController.transform.rotation;
    }

    private void FixedUpdate()
    {
        if (enablePhysics)
        {
            // Position
            rb.velocity = (parentController.transform.position - transform.position) * positionSpeed * Time.deltaTime;

            // Rotation
            Quaternion rotDifference = parentController.transform.rotation * Quaternion.Inverse(transform.rotation);
            rotDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

            rb.angularVelocity = (0.9f * rotationSpeedDamper * Mathf.Deg2Rad * angleInDegrees / Time.deltaTime) * rotationAxis.normalized;
        }
    }
}
