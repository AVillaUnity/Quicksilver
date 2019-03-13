using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlowUpBehavior : StateMachineBehaviour
{
    public GameObject particleBlowUp;
    public float slowDown = .3f;
    public float slowDownSpeed = .5f;
    public float musicFadeInSpeed = 1f;
    public bool slowDownTime = false;

    private AudioSource audioSource;
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Instantiate(particleBlowUp, animator.transform.position, Quaternion.identity);
        animator.gameObject.GetComponent<MeshRenderer>().enabled = false;
        audioSource = animator.GetComponent<AudioSource>();

        if (slowDownTime)
        {
            audioSource.Play();
        }
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(!slowDownTime) { return; }
        if(Time.timeScale > slowDown)
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale - Time.unscaledDeltaTime * slowDownSpeed, 0f, 1f);
            Time.fixedDeltaTime = Time.timeScale * 0.01f;
        }

        if(audioSource.volume < 1)
        {
            audioSource.volume += (Time.unscaledDeltaTime * musicFadeInSpeed);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
