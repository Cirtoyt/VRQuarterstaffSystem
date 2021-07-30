using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRHandVisual : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private GameObject handModel;
    [SerializeField] private bool hideHandWhilstGrabbing;
    [Header("Statics")]
    [SerializeField] private XRController controller;
    [SerializeField] private XRPhysicsHand physicsHand;
    
    private Animator anim;

    void Start()
    {
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
