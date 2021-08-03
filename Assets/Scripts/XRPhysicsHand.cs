using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRPhysicsHand : MonoBehaviour
{
    [Header("Variables")]
    public XRController parentController;
    public Transform grabPointTransform;
    [SerializeField] private float positionSpeed = 2000;
    [Range(0.01f,1)][SerializeField] private float rotationSpeedDamper;
    [Header("Statics")]
    public XRHandVisual handVisual;
    public Transform handVisualModel;

    [HideInInspector] public bool enablePhysics;
    [HideInInspector] public Rigidbody rb;

    private Vector3 originHandVisualModelLocalPosition;

    private void Start()
    {
        enablePhysics = true;
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 30;
        originHandVisualModelLocalPosition = handVisualModel.localPosition;

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

            if (float.IsInfinity(rotationAxis.x))
                return;

            if (angleInDegrees > 180)
                angleInDegrees -= 360;

            rb.angularVelocity = (0.9f * rotationSpeedDamper * Mathf.Deg2Rad * angleInDegrees / Time.deltaTime) * rotationAxis.normalized;
        }
    }

    public void ResetHandVisualModelLocalPosition()
    {
        handVisualModel.localPosition = originHandVisualModelLocalPosition;
    }
}
