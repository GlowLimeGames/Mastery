using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{

    public enum CharacterState
    {
        IDLE,
        ATTACKING,
        GUARDING,
        VULNERABLE,
        INVULNERABLE
    }

    // Since you can be both vulnerable and attacking, there's another enum for player actions.
    public enum CharacterAction
    {
        IDLE,
        LIGHT_ATTACKING,
        HEAVY_ATTACKING,
        PARRYING,
        GUARDING,
        KICKING,
        MOVING,
        ROLLING
    }

    public CharacterState state;
    public CharacterAction action;
    public Animator anim;

    public int HP;
    public bool facingLeft;

    // Input holders
    // TODO: Would be nice to put this in an object or something
    public float inputHorizontal;
    public float inputRightHorizontal;  // horizontal axis of right stick
    public bool inputRollDown;
    public bool inputKickDown;
    public bool inputAttackDown;
    public bool inputAttackHeld;
    public bool inputAttackUp;
    public bool inputDefendDown;
    public bool inputDefendHeld;
    public bool inputDefendUp;

    // The time the player pressed the button last
    public float pressStartTime;

    // The time the player was disarmed
    public float disarmStartTime;

    public float shieldBreakStartTime;

    public float disableMovementStartTime;

    // Whether an action has been performed with this button press
    public bool actionThisPress;

    // Use this for initialization
    private void Start()
    {
        state = CharacterState.IDLE;
        HP = 2;
        disarmStartTime = -10.0f;
        shieldBreakStartTime = -10.0f;
        disableMovementStartTime = -10.0f;
    }

    private void Update()
    {
        // Update states based on animations.
        // (I'm hoping there is a better way to do this)
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            state = CharacterState.IDLE;
            action = CharacterAction.IDLE;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Moving"))
        {
            state = CharacterState.IDLE;
            action = CharacterAction.MOVING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Roll"))
        {
            state = CharacterState.IDLE;
            action = CharacterAction.ROLLING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Light Attack"))
        {
            state = CharacterState.ATTACKING;
            action = CharacterAction.LIGHT_ATTACKING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Heavy Attack"))
        {
            state = CharacterState.ATTACKING;
            action = CharacterAction.HEAVY_ATTACKING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Parry"))
        {
            state = CharacterState.GUARDING;
            action = CharacterAction.PARRYING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Guard"))
        {
            state = CharacterState.GUARDING;
            action = CharacterAction.GUARDING;
        }
    }

    public bool CanAct()
    {
        if (action == CharacterAction.IDLE || action == CharacterAction.MOVING)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TurnAround()
    {
        // TODO: Add a delay and animation to this, since it's used as a penalty for something too
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        if (facingLeft)
        {
            facingLeft = false;
        }
        else
        {
            facingLeft = true;
        }
    }

}