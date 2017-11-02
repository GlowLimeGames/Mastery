using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{

    public enum CharacterState
    {
        IDLE,
        ATTACKING,
        // KNOCKBACK,
        GUARDING,
        VULNERABLE,
        INVULNERABLE
    }

    // Since you can be both vulnerable and attacking, there's another enum for player actions.
    public enum CharacterAction
    {
        IDLE,
        TURNING,
        LIGHT_ATTACKING,
        HEAVY_ATTACKING,
        PARRYING,
        GUARDING,
        KICKING,
        MOVING,
        ROLLING,
        KNOCKBACK,
        DELAY,      // bringing weapon back after swinging it. Need a better name
        STUN,
    }

    public CharacterState state;
    public CharacterAction action;
    public Animator anim;

    public int stock;
    public bool isDead;
    public int HP;
    public bool facingLeft;
    public bool rollingLeft;

    public float leftWallDeltaX;
    public float rightWallDeltaX;

    // Debug texts
    public Text stockText;
    public Text hpText;
    public Text disarmText;
    public Text shieldBreakText;
    public Text disableMovementText;

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

    // The time the player was disarmed, shieldbroken, or had movement disabled
    public float disarmStartTime;
    public float shieldBreakStartTime;
    public float disableMovementStartTime;

    public float deathTime;

    // Whether an action has been performed with this button press
    public bool actionThisPress;

    // Need to access sword/shield in order to change its tag to active/inactive.
    public GameObject swordObject;
    public GameObject shieldObject;
 
    // Use this for initialization
    private void Start()
    {
        state = CharacterState.IDLE;
        stock = GameController.stockMax;
        HP = GameController.hpMax;
        isDead = false;

        // Set all event times to a negative, so their relevant conditions don't trigger
        disarmStartTime = -10.0f;
        shieldBreakStartTime = -10.0f;
        disableMovementStartTime = -10.0f;
        deathTime = -10.0f;
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

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Turn Around"))
        {
            state = CharacterState.IDLE;
            action = CharacterAction.TURNING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("KnockbackL"))
        {
            //print("State is knockback" + Time.time);
            state = CharacterState.IDLE;
            action = CharacterAction.KNOCKBACK;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("KnockbackR"))
        {
            state = CharacterState.IDLE;
            action = CharacterAction.KNOCKBACK;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Moving"))
        {
            state = CharacterState.IDLE;
            action = CharacterAction.MOVING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Roll"))
        {
            state = CharacterState.INVULNERABLE;
            action = CharacterAction.ROLLING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Light Attack (Swing)"))
        {
            state = CharacterState.ATTACKING;
            action = CharacterAction.LIGHT_ATTACKING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Light Attack (Return)"))
        {
            state = CharacterState.IDLE;
            action = CharacterAction.DELAY;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Heavy Attack (Swing)"))
        {
            state = CharacterState.ATTACKING;
            action = CharacterAction.HEAVY_ATTACKING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Heavy Attack (Return)"))
        {
            state = CharacterState.IDLE;
            action = CharacterAction.DELAY;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Kick"))
        {
            state = CharacterState.ATTACKING;
            action = CharacterAction.KICKING;
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

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Stun"))
        {
            state = CharacterState.IDLE;
            action = CharacterAction.STUN;
        }

        _setSwordState();
        _setShieldState();
    }

    public bool CanAct()
    {
        if (action == CharacterAction.IDLE || action == CharacterAction.MOVING || action == CharacterAction.TURNING)
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
        anim.Play("Turn Around");
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

    public void Knockback()
    {
        // Need to knockback player in opposite direction so they rotate away from the collision
        if (facingLeft)
        {
            anim.Play("KnockbackR");
        }
        else
        {
            anim.Play("KnockbackL");
        }
    }

    public void Disarm()
    {
        // TODO: This really needs an animation
        disarmStartTime = Time.time;
        disarmText.text = "Disarmed";
    }

    public void ShieldBreak()
    {
        shieldBreakStartTime = Time.time;
        shieldBreakText.text = "Shield Broken";
    }

    public void DisableMovement()
    {
        disableMovementStartTime = Time.time;
        disableMovementText.text = "Movement Disabled";
    }

    public void IsKilled()
    {
        anim.Play("Die");    // doesn't display, since it doesn't wait to move the character out of the way
        isDead = true;
        
        stock -= 1;
        stockText.text = "Stock: " + stock.ToString();
        gameObject.transform.position += Vector3.right * 100.0f;

        deathTime = Time.time;
    }

    public void Respawn()
    {
        // TODO: Find a safe random position to respawn in
        // Needs to be within the walls, and away from the other player
        // safeXMin and safeXMax give bounds for the walls. How to think about this?
        
        // |-----------X-------------------------|
        // lw          p1                        rw
        
        // p2 should spawn in the larger portion, either in the middle of p1rw or right against rw

        // TODO: also set them invulnerable for a time?
        isDead = false;
        float respawnX = Random.Range(GameController.safeXMin, GameController.safeXMax);
        // float respawnX = 1.0f;
        gameObject.transform.position = new Vector3(respawnX, -2.0f, 0.0f);
        gameObject.SetActive(true);
        anim.Play("Idle");
        HP = GameController.hpMax;
        state = CharacterState.IDLE;
    }

    private void _setSwordState()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Light Attack (Swing)") || (anim.GetCurrentAnimatorStateInfo(0).IsName("Heavy Attack (Swing)")))
        {
            swordObject.tag = "SwordActive";
        }
        else
        {
            swordObject.tag = "SwordInactive";
        }
    }

    private void _setShieldState()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Parry") || (anim.GetCurrentAnimatorStateInfo(0).IsName("Guard")))
        {
            shieldObject.tag = "ShieldActive";
        } else
        {
            shieldObject.tag = "ShieldInactive";
        }
    }

}