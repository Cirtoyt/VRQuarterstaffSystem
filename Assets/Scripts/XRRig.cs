using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRRig : MonoBehaviour
{
    public Transform cameraTrans;
    public Vector3 cameraPosInRigSpace;
    public float cameraHeightInRigSpace;
    
    void Update()
    {
        cameraPosInRigSpace = transform.InverseTransformPoint(cameraTrans.transform.position);
        cameraHeightInRigSpace = cameraPosInRigSpace.y;
    }
}
