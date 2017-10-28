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
        KNOCKBACK,
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

    // Need to access shield in order to change its tag to active/inactive.
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
            state = CharacterState.KNOCKBACK;
            action = CharacterAction.IDLE;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("KnockbackR"))
        {
            state = CharacterState.KNOCKBACK;
            action = CharacterAction.IDLE;
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
        isDead = true;
        
        stock -= 1;
        stockText.text = "Stock: " + stock.ToString();
        gameObject.transform.position += Vector3.right * 100.0f;
        gameObject.SetActive(false);

        deathTime = Time.time;
    }

    public void Respawn()
    {
        // TODO: Find a safe random position to respawn in, for now it's just at 1.0
        isDead = false;
        float respawnX = 1.0f;
        gameObject.transform.position = new Vector3(respawnX, -2.0f, 0.0f);
        // Game object is getting stuck with the rotation it had while dying. TODO fix this
        // It'll probably go away as I add a death animation...
        // gameObject.transform.rotation = new Quaternion(0, 0, 0, 0);
        gameObject.SetActive(true);
        anim.Play("Idle");
        HP = GameController.hpMax;
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