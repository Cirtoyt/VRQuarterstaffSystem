using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRPhysicsHand : MonoBehaviour
{
    [Header("Variables")]
    public XRController parentController;
    public Transform attachTransform;
    [Header("Statics")]
    public XRHandVisuals handVisuals;

    [HideInInspector] public bool enablePhysics;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        enablePhysics = true;
    }

    private void Update()
    {
        if (enablePhysics)
        {
            // Turn hand collider on? Should collider be off when disabled inputs?
            // Have hand react to surroundings (colliders)

            //rb.MovePosition(parentController.transform.position);
            //rb.MoveRotation(parentController.transform.rotation);

            // Currently do not have hand physics and instant snap physics hand to controller position & rotation:
            transform.position = parentController.transform.position;
            transform.rotation = parentController.transform.rotation;
        }
    }
}
