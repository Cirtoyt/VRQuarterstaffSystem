using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.LegacyInputHelpers;

public class ContinuousMovement : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private XRController controller;
    [SerializeField] private float speed;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] private float additionalHeight;

    private CharacterController characterCtrlr;
    private XRRig rig;

    private float fallingSpeed;

    void Start()
    {
        characterCtrlr = GetComponent<CharacterController>();
        rig = GetComponent<XRRig>();
    }

    void FixedUpdate()
    {
        CapsuleFollowHeadset();

        Move();
        ApplyGravity();
    }

    /// <summary>
    /// Move player in direction of input thumbstick, with forward being the facing direction of the headset.
    /// </summary>
    private void Move()
    {
        Quaternion headYaw = Quaternion.Euler(0, rig.cameraGameObject.transform.eulerAngles.y, 0);

        Vector3 direction = headYaw * new Vector3(controller.thumbstickValue.x, 0, controller.thumbstickValue.y);

        characterCtrlr.Move(direction * Time.fixedDeltaTime * speed);
    }

    /// <summary>
    /// Apply gravity unless grounded.
    /// </summary>
    private void ApplyGravity()
    {
        bool isGrounded = CheckIfGrounded();
        if (isGrounded)
        {
            fallingSpeed = 0;
        }
        else
        {
            fallingSpeed += gravity * Time.fixedDeltaTime;
        }

        characterCtrlr.Move(Vector3.up * fallingSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Resize character controller collider based on headset height & world position.
    /// </summary>
    private void CapsuleFollowHeadset()
    {
        characterCtrlr.height = rig.cameraHeightInRigSpace + additionalHeight;
        characterCtrlr.center = new Vector3(rig.cameraPosInRigSpace.x, characterCtrlr.height / 2 + characterCtrlr.skinWidth, rig.cameraPosInRigSpace.z);
    }

    private bool CheckIfGrounded()
    {
        Vector3 rayStart = transform.TransformPoint(characterCtrlr.center);
        float rayLength = characterCtrlr.center.y + 0.01f;
        bool hasHit = Physics.SphereCast(rayStart, characterCtrlr.radius, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);
        return hasHit;
    }
}
