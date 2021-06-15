using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRHandVisuals : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private GameObject handModel;
    [SerializeField] private bool hideHandWhilstGrabbing;
    [Header("Statics")]
    [SerializeField] private XRController controller;
    [SerializeField] private XRPhysicsHand physicsHand;

    [HideInInspector] public bool trackPhysicsHand;
    private Animator anim;

    void Start()
    {
        trackPhysicsHand = true;
        anim = handModel.GetComponent<Animator>();
    }

    void Update()
    {
        // ## ANIMATIONS ##
        if (handModel)
        {
            anim.SetFloat("Grip", controller.gripValue);
            anim.SetFloat("Trigger", controller.triggerValue);
        }
        // ## POSITIONING ##
        if (trackPhysicsHand)
        {
            transform.position = physicsHand.transform.position;
            transform.rotation = physicsHand.transform.rotation;
        }
    }

    public void HideHand()
    {
        if (hideHandWhilstGrabbing)
            handModel.SetActive(false);
    }

    public void ShowHand()
    {
        if (hideHandWhilstGrabbing)
            handModel.SetActive(true);
    }
}
