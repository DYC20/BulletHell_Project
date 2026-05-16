using System;
using UnityEngine;

public class PauseAnimator : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();   
        
    }

    public void PauseAniamtor()
    {
        animator.speed = 0f;
    }

    public void ResumeAniamtor()
    {
        animator.speed = 1f;
    }
}
