using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private string currentAnim;

    // Animation clip names must match Animator state names exactly
    private static readonly string IDLE = "Idle";
    private static readonly string RUN = "Run";
    private static readonly string JUMP = "Jump";
    private static readonly string FALL = "Fall";
    private static readonly string DOUBLE_JUMP = "DoubleJump";
    private static readonly string HIT = "Hit";
    private static readonly string DEATH = "Death";

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        //Debug.Log(animator != null ? "Animator found" : "Animator NOT found");
    }

    public void Play(string animName)
    {
        if (currentAnim == animName) return;
        currentAnim = animName;
        //Debug.Log("Playing animation: " + animName);
        animator.Play(animName);
    }

    
    public void PlayIdle() => Play(IDLE);
    public void PlayRun() => Play(RUN);
    public void PlayJump() => Play(JUMP);
    public void PlayFall() => Play(FALL);
    public void PlayDoubleJump() => Play(DOUBLE_JUMP);
    public void PlayHit() => Play(HIT);
    public void PlayDeath() => Play(DEATH);
}