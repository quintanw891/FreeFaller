using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    private Animator animator;
    public bool startOpen;          // Should the door start opened or closed
    public bool startOscillating;   // Should the door be oscillating at start
    public bool isPausingWhenOscillating;   // When oscillating, should the door pause breifly when fully open and fully closed

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        animator.SetBool("startOpen", startOpen ? true : false);
        animator.SetBool("isOscillating", startOscillating ? true : false);
        animator.SetBool("isPausingWhenOscillating", isPausingWhenOscillating ? true : false);
    }

    public void Open()
    {
        //animator.ResetTrigger("openTrigger");
        animator.ResetTrigger("closeTrigger");
        animator.SetBool("isOscillating", false);
        animator.SetTrigger("openTrigger");
    }

    public void Close()
    {
        animator.ResetTrigger("openTrigger");
        //animator.ResetTrigger("closeTrigger");
        animator.SetBool("isOscillating", false);
        animator.SetTrigger("closeTrigger");
    }

    public void Oscillate()
    {
        animator.ResetTrigger("openTrigger");
        animator.ResetTrigger("closeTrigger");
        animator.SetBool("isOscillating", true);
    }
}
