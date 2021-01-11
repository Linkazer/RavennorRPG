using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseGameObj : MonoBehaviour
{
    public SpriteRenderer feedbackSprite;
    public Animator animator;

    public float PlayAnimation(string animName)
    {
        animator.Play(animName);
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

    public void SetFeedbackColor(Color newColor)
    {
        if (newColor.r != Color.white.r || newColor.b != Color.white.b || newColor.g != Color.white.g)
        {
            newColor.a = 0.5f;
        }
        else
        {
            newColor.a = 0;
        }
        feedbackSprite.color = newColor;
    }
}
