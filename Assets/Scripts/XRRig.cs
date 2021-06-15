using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRRig : MonoBehaviour
{
    public GameObject cameraGameObject;
    public Vector3 cameraPosInRigSpace;
    public float cameraHeightInRigSpace;
    
    void Update()
    {
        cameraPosInRigSpace = transform.InverseTransformPoint(cameraGameObject.transform.position);
        cameraHeightInRigSpace = cameraPosInRigSpace.y;
    }
}
