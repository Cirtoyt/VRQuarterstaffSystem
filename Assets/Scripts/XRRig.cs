using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRRig : MonoBehaviour
{
    public Transform cameraTrans;
    public Vector3 cameraPosInRigSpace;
    public float cameraHeightInRigSpace;
    static XRInputSubsystem XRInputSubsystem;

    private void Start()
    {
        List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances<XRInputSubsystem>(subsystems);
        for (int i = 0; i < subsystems.Count; i++)
        {
            subsystems[i].TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
        }
    }

    void Update()
    {
        cameraPosInRigSpace = transform.InverseTransformPoint(cameraTrans.transform.position);
        cameraHeightInRigSpace = cameraPosInRigSpace.y;
    }
}
