using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPresence : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private GameObject handModel;
    [SerializeField] private bool hideHandWhilstGrabbing;

    private Animator anim;
    private XRController controller;

    void Start()
    {
        anim = handModel.GetComponent<Animator>();
        controller = GetComponent<XRController>();
    }

    void Update()
    {
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
