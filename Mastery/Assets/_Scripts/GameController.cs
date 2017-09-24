using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public PlayerController playerOne;
    public PlayerController playerTwo;

    private void Start()
    {
        playerOne.facingLeft = false;
        playerTwo.facingLeft = true;
        CollisionBehavior.AttackResolution += Attack; // Subscribe to the AttackResolution event
    }

    private float p1StartTime;
    private float p2StartTime;

    private float p1DisarmStartTime;
    private float p2DisarmStartTime;

    // Time to distinguish a press from a hold, for light/heavy and parry/guard
    // (Different numbers for those two categories?)
    private float holdTime = 0.15f;

    // Length of disarm
    private float disarmTime = 1.0f;

    // Make sure only one action is taken per button press
    private bool p1ActionThisPress = false;
    private bool p2ActionThisPress = false;


    private void Update()
    {
        //Input reading goes here

        // Player One input reading
        bool oneAttackDown = Input.GetKeyDown("f");
        bool oneAttackHeld = Input.GetKey("f");
        bool oneAttackUp = Input.GetKeyUp("f");

        bool oneDefendDown = Input.GetKeyDown("g");
        bool oneDefendHeld = Input.GetKey("g");
        bool oneDefendUp = Input.GetKeyUp("g");

        // Player Two input reading
        bool twoAttackDown = Input.GetKeyDown("h");
        bool twoAttackHeld = Input.GetKey("h");
        bool twoAttackUp = Input.GetKeyUp("h");

        bool twoDefendDown = Input.GetKeyDown("j");
        bool twoDefendHeld = Input.GetKey("j");
        bool twoDefendUp = Input.GetKeyUp("j");

        // Attack inputs
        if (oneAttackDown)
        {
            p1StartTime = Time.time;
            p1ActionThisPress = false;
        }
        if (oneAttackHeld)
        {
            if (Time.time >= (p1StartTime + holdTime) && !p1ActionThisPress)
            {
                playerOne.state = PlayerController.CharacterState.HEAVY_ATTACKING;
                playerOne.anim.Play("Heavy Attack");

                p1ActionThisPress = true;
            }
        }
        if (oneAttackUp)
        {
            if (Time.time < (p1StartTime + holdTime))
            {
                playerOne.state = PlayerController.CharacterState.LIGHT_ATTACKING;
                print("State switched to LIGHT_ATTACKING");
                playerOne.anim.Play("Light Attack");

                p1ActionThisPress = true;
            }
        }

        // Defend inputs
        if (oneDefendDown)
        {
            p1StartTime = Time.time;
            p1ActionThisPress = false;
        }
        if (oneDefendHeld)
        {
            if (Time.time >= (p1StartTime + holdTime) && !p1ActionThisPress)
            {
                playerOne.state = PlayerController.CharacterState.GUARDING;
                playerOne.anim.Play("Guard");
            }
        }
        if (oneDefendUp)
        {
            if (Time.time < (p1StartTime + holdTime))
            {
                playerOne.state = PlayerController.CharacterState.PARRYING;
                playerOne.anim.Play("Parry");
            }
        }

        // P2 Attack inputs
        if (twoAttackDown)
        {
            p2StartTime = Time.time;
            p2ActionThisPress = false;
        }
        if (twoAttackHeld)
        {
            if (Time.time >= (p2StartTime + holdTime) && !p2ActionThisPress)
            {
                playerTwo.state = PlayerController.CharacterState.HEAVY_ATTACKING;
                playerTwo.anim.Play("Heavy Attack");

                p2ActionThisPress = true;
            }
        }
        if (twoAttackUp)
        {
            if (Time.time < (p2StartTime + holdTime))
            {
                playerTwo.state = PlayerController.CharacterState.LIGHT_ATTACKING;
                playerTwo.anim.Play("Light Attack");

                p2ActionThisPress = true;
            }
        }

        // Defend inputs
        if (twoDefendDown)
        {
            p2StartTime = Time.time;
            p2ActionThisPress = false;
        }
        if (oneDefendHeld)
        {
            if (Time.time >= (p2StartTime + holdTime) && !p2ActionThisPress)
            {
                playerTwo.state = PlayerController.CharacterState.GUARDING;
                playerTwo.anim.Play("Guard");
            }
        }
        if (twoDefendUp)
        {
            if (Time.time < (p2StartTime + holdTime))
            {
                playerTwo.state = PlayerController.CharacterState.PARRYING;
                playerTwo.anim.Play("Parry");
            }
        }

    }

    private void FixedUpdate()
    {
        //Movement / Combat Functionality goes here
    }

    private void LateUpdate()
    {
        //Do something after Movement / Combat here

        if (playerOne.HP <= 0)
        {
            playerOne.state = PlayerController.CharacterState.VULNERABLE;
        }

        if (playerTwo.HP <= 0)
        {
            playerTwo.state = PlayerController.CharacterState.VULNERABLE;
        }
    }

    public void Attack(GameObject attacker, GameObject defender)
    {

        PlayerController attackerController;
        PlayerController defenderController;
        
        if (attacker == playerOne.gameObject)
        {
            attackerController = playerOne;
            defenderController = playerTwo;
        } else
        {
            attackerController = playerTwo;
            defenderController = playerOne;
        }

        print(attackerController +  " (" + attackerController.state + ") attacks " + defenderController + " (" + defenderController.state + ")");
        if (attackerController.state == PlayerController.CharacterState.LIGHT_ATTACKING)
        {
            switch (defenderController.state)
            {
                case PlayerController.CharacterState.PARRYING:
                    // Attacker gets turned around
                    break;
                case PlayerController.CharacterState.GUARDING:
                    // Attacker gets knocked back

                    break;
                case PlayerController.CharacterState.LIGHT_ATTACKING:
                    // Both are knocked back
                    // Idle for now, maybe we need a knockback state?
                    attackerController.anim.Play("Idle");
                    defenderController.anim.Play("Idle");
                    //Knockback(attackerController);
                    //Knockback(defenderController);
                    break;
                case PlayerController.CharacterState.HEAVY_ATTACKING:
                    // Disables attacker's attack button for X seconds
                    break;
                case PlayerController.CharacterState.INVULNERABLE:
                    // Do nothing presumably?
                    break;
                case PlayerController.CharacterState.VULNERABLE:
                    // Kills defender
                    defender.SetActive(false);
                    break;
                case PlayerController.CharacterState.IDLE:
                    // Successful hit.
                    // To avoid duplicate hits, setting animation to idle.
                    // TODO should play a successful attack animation, maybe just reverse the animation?
                    attackerController.anim.Play("Idle");
           
                    defenderController.HP -= 1;
                    break;
            }
        }
        if (attackerController.state == PlayerController.CharacterState.HEAVY_ATTACKING)
        {
            switch (defenderController.state)
            {
                case PlayerController.CharacterState.PARRYING:
                    // Disables attacker's attack button for X seconds
                    break;
                case PlayerController.CharacterState.GUARDING:
                    // Defender gets knocked back
                    break;
                case PlayerController.CharacterState.LIGHT_ATTACKING:
                    defenderController.HP -= 1;
                    break;
                case PlayerController.CharacterState.HEAVY_ATTACKING:
                    // Disables attacker's attack button for X seconds
                    break;
                case PlayerController.CharacterState.INVULNERABLE:
                    // Do nothing presumably?
                    break;
                case PlayerController.CharacterState.VULNERABLE:
                    // Kills defender
                    defender.SetActive(false);
                    break;
                case PlayerController.CharacterState.IDLE:
                    attackerController.anim.Play("Idle");
                    defenderController.HP -= 2;
                    break;

            }
        }
            
    }

    // Knockback not working yet, seems to have no effect
    /*public void Knockback(PlayerController player)
    {
        Vector2 force;
        print(player.gameObject.name);
        Rigidbody2D rb = player.gameObject.GetComponentInChildren<Rigidbody2D>();
        if (player.facingLeft)
        {
            force = new Vector2(-100.0f, 2.0f);
        } else
        {
            force = new Vector2(100.0f, 2.0f);
        }
        rb.AddForce(force, ForceMode2D.Force);
    }
    */

}
