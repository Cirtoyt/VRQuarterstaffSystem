using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandPresence : MonoBehaviour
{
    [SerializeField] private GameObject handModel;
    [Header("Variables")]
    [SerializeField] private bool hideHandWhilstGrabbing;

    private Animator anim;

    void Start()
    {
        anim = handModel.GetComponent<Animator>();
    }

    void Update()
    {
        if (handModel)
        {
            //anim.SetFloat("Grip", gripPullAction.action.ReadValue<float>());
            //anim.SetFloat("Trigger", triggerPullAction.action.ReadValue<float>());
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
