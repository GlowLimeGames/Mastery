using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
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
        DEAD,
        RESPAWNING,
    }

    public CharacterAction action;
    public Animator anim;

    public int stock;
    public bool isInvulnerable;
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
    public bool disarmed;
    public bool shieldBroken;
    public bool movementDisabled;
    public bool rollDisabled;

    public float deathTime;

    // Whether an action has been performed with this button press
    public bool actionThisPress;
    public bool damageThisSwing;

    // Need to access sword/shield in order to change its tag to active/inactive.
    public GameObject swordObject;
    public GameObject shieldObject;
 
    // Use this for initialization
    private void Start()
    {
        action = CharacterAction.IDLE;
        stock = GameController.stockMax;
        HP = GameController.hpMax;
        isInvulnerable = false;

        // Set all event times to a negative, so their relevant conditions don't trigger
        disarmed = false;
        shieldBroken = false;
        movementDisabled = false;
        rollDisabled = false;
    }

    private void Update()
    {
        // Update states based on animations.
        // (I'm hoping there is a better way to do this)
        // (It's mostly handled in the relevant functions now. Need to keep Idle at least)
 
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            action = CharacterAction.IDLE;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Light Attack (Return)"))
        {
            action = CharacterAction.DELAY;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Heavy Attack (Windup)"))
        {
            action = CharacterAction.DELAY;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Heavy Attack (Swing)"))
        {
            action = CharacterAction.HEAVY_ATTACKING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Heavy Attack (Return)"))
        {
            action = CharacterAction.DELAY;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Stun"))
        {
            action = CharacterAction.STUN;
        }
        
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Die"))
        {
            action = CharacterAction.DEAD;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Respawn"))
        {
            action = CharacterAction.RESPAWNING;
        }

        _setSwordState();
        _setShieldState();
    }

    public bool CanAct()
    {
        if (action == CharacterAction.IDLE || action == CharacterAction.MOVING || action == CharacterAction.GUARDING)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Roll()
    {
        action = PlayerController.CharacterAction.ROLLING;
        anim.Play("None", 1);         // Disable the leg animation, which messes up the roll
        if (facingLeft == rollingLeft)
        {
            anim.SetFloat("RollMultiplier", 1.0f);
            anim.Play("Roll");
        } else
        {
            anim.SetFloat("RollMultiplier", -1.0f);
            anim.Play("Roll", 0, 1);  // play from the end
            // It won't transition if it reaches the beginning of the animation.
            // Need to reset the speed and bail ourselves out by switching to Idle when the animation's over.
            StartCoroutine(_ResetRollSpeed());
        }

        //anim.Play("Roll");
        rollDisabled = true;
        StartCoroutine(_ReenableRoll());
    }

    public void TurnAround()
    {
        action = CharacterAction.TURNING;
        anim.Play("Turn Around");
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        // Set the facing variable after a delay
        StartCoroutine(SetFacing());
    }

    private IEnumerator SetFacing()
    {
        yield return new WaitForSeconds(0.35f);
        facingLeft = !facingLeft;
    }

    public void Stun()
    {
        action = CharacterAction.STUN;
        anim.Play("None", 1);
        anim.Play("Stun");
    }

    public void Knockback()
    {
        // Need to knockback player in opposite direction so they rotate away from the collision
        action = CharacterAction.KNOCKBACK;
        anim.Play("Knockback");
        /*
        if (facingLeft)
        {
            anim.Play("KnockbackR");
        }
        else
        {
            anim.Play("Knockback (Left)");
        }
        */
    }

    public void Disarm()
    {
        anim.Play("Disarmed");  // TODO fix the disarmed animation
        disarmed = true;
        disarmText.text = "Disarmed";
        StartCoroutine(_HideSword());
        StartCoroutine(_Rearm());
    }

    public void ShieldBreak()
    {
        anim.Play("Shield Broken");
        shieldBroken = true;
        shieldBreakText.text = "Shield Broken";
        StartCoroutine(_HideShield());
        StartCoroutine(_RepairShield());
    }

    public void DisableMovement()
    {
        movementDisabled = true;
        disableMovementText.text = "Movement Disabled";
        StartCoroutine(_ReenableMovement());
    }

    public void TakeDamage(int dmg)
    {
        HP -= dmg;
        hpText.text = "HP: " + HP.ToString();

        if (HP == 0)
        {
            // TODO: They're "vulnerable." Animate the helmet coming off or something
        }
        else if (HP < 0)
        {
            IsKilled();
        }
    }

    public void TakeNonMortalDamage(int dmg)
    {
        // Damage the player shouldn't die from. Sword clashes, etc
        HP -= dmg;

        if (HP < 0)
        {
            HP = 0;
            // TODO: Set vulnerable, with animations and such
        }
        hpText.text = "HP: " + HP.ToString();
    }

    public void MaxOutHP()
    {
        HP = GameController.hpMax;
        hpText.text = "HP: " + HP.ToString();
    }

    public void IsKilled()
    {
        if (GameController.isStartScreen)
        {
            return;
        }

        anim.Play("Die");
        StartCoroutine(_HideBody());

        action = CharacterAction.DEAD;
        
        stock -= 1;
        stockText.text = "Stock: " + stock.ToString();

        deathTime = Time.time;

        // Disarming states shouldn't carry over between stocks
        disarmed = shieldBroken = movementDisabled = false;
        disarmText.text = shieldBreakText.text = disableMovementText.text = "";

        // Can't check gameOver, since that is set after the death.
        // It's good enough just to check this player's lives
        if (stock > 0)
        {
            StartCoroutine(_Respawn());
        }
        
    }

    public IEnumerator _Respawn()
    {
        // TODO: Find a safe random position to respawn in
        // Needs to be within the walls, and away from the other player
        // safeXMin and safeXMax give bounds for the walls. How to think about this?

        // |-----------X-------------------------|
        // lw          p1                        rw

        // p2 should spawn in the larger portion, either in the middle of p1rw or right against rw

        // TODO: Should also face the other player by default

        yield return new WaitForSeconds(GameController.respawnTime);

        float respawnX = Random.Range(GameController.safeXMin, GameController.safeXMax);
        gameObject.transform.position = new Vector3(respawnX, -1.35f, 0.0f);
        anim.Play("Respawn");
        disarmed = shieldBroken = movementDisabled = false;
        disarmText.text = shieldBreakText.text = disableMovementText.text = "";
        MaxOutHP();
        action = CharacterAction.IDLE;

        isInvulnerable = true;
        StartCoroutine(_BecomeVulnerable());
    }

    private IEnumerator _BecomeVulnerable()
    {
        yield return new WaitForSeconds(GameController.respawnInvulnerabilityTime);
        isInvulnerable = false;
    }

    private IEnumerator _HideSword()
    {
        yield return new WaitForSeconds(1.0f);
        swordObject.SetActive(false);
    }

    private IEnumerator _Rearm()
    {
        yield return new WaitForSeconds(GameController.disarmTime);
        swordObject.SetActive(true);
        disarmed = false;
        disarmText.text = "";
    }

    private IEnumerator _HideShield()
    {
        yield return new WaitForSeconds(0.667f);  // length of char_anim_shieldBreak
        shieldObject.SetActive(false);
    }

    private IEnumerator _RepairShield()
    {
        yield return new WaitForSeconds(GameController.shieldBreakTime);
        shieldObject.SetActive(true);
        shieldBroken = false;
        shieldBreakText.text = "";
    }

    private IEnumerator _HideBody()
    {
        yield return new WaitForSeconds(1.333f);
        gameObject.transform.position += Vector3.right * 100.0f;
    }

    private IEnumerator _ReenableMovement()
    {
        yield return new WaitForSeconds(GameController.disableMovementTime);
        movementDisabled = false;
        disableMovementText.text = "";
    }

    private IEnumerator _ReenableRoll()
    {
        yield return new WaitForSeconds(GameController.disableRollTime);
        rollDisabled = false;
    }

    private IEnumerator _ResetRollSpeed()
    {
        yield return new WaitForSeconds(1.3f);
        anim.SetFloat("RollMultiplier", 1.0f);
        anim.Play("Idle");

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
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Parry") || (anim.GetCurrentAnimatorStateInfo(0).IsName("Guard (On)")))    // TODO: Include Guard (Off)?
        {
            shieldObject.tag = "ShieldActive";
        } else
        {
            shieldObject.tag = "ShieldInactive";
        }
    }

}