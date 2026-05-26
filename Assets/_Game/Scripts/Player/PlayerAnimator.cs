
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    // Animation state name constants
    public const string IDLE = "Idle";
    public const string RUN = "Run";
    public const string JUMP = "Jump";
    public const string FALL = "Fall";
    public const string DOUBLE_JUMP = "DoubleJump";
    public const string HIT = "Hit";
    public const string DEATH = "Death";

    private Animator animator;
    private string currentAnim;

    
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogError("[PlayerAnimator] No Animator found on " + gameObject.name
                + " or its children.");
    }

    //Play animation by name. Skips if already playing.
    public void Play(string animName)
    {
        if (animator == null) return;
        if (currentAnim == animName) return;

        currentAnim = animName;
        animator.Play(animName);
    }

    //shorthands
    public void PlayIdle() => Play(IDLE);
    public void PlayRun() => Play(RUN);
    public void PlayJump() => Play(JUMP);
    public void PlayFall() => Play(FALL);
    public void PlayDoubleJump() => Play(DOUBLE_JUMP);
    public void PlayHit() => Play(HIT);
    public void PlayDeath() => Play(DEATH);

    //Force reset — use when respawning to avoid stale state
    public void ResetToIdle()
    {
        currentAnim = null;
        Play(IDLE);
    }

    public bool HasAnimator => animator != null;
}