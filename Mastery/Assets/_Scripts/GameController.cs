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

    // Time to distinguish a press from a hold, for light/heavy and parry/guard
    // (Different numbers for those two categories?)
    private static float holdTime = 0.15f;

    // Length of disarm
    private static float disarmTime = 3.0f;

    // Make sure only one action is taken per button press
    private bool playerOne.actionThisPress = false;
    private bool playerTwo.actionThisPress = false;

    //private float p1StartTime;
    //private float p2StartTime;


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
            if (Time.time >= (playerOne.disarmStartTime + disarmTime))
            {
                playerOne.pressStartTime = Time.time;
                playerOne.actionThisPress = false;
            } else
            {
                // TODO: Play some sort of "whoops, I'm disarmed" animation
            }

        }
        if (oneAttackHeld)
        {
            if (Time.time >= (playerOne.pressStartTime + holdTime) && !playerOne.actionThisPress)
            {
                playerOne.state = PlayerController.CharacterState.ATTACKING;
                playerOne.action = PlayerController.CharacterAction.HEAVY_ATTACKING;
                playerOne.anim.Play("Heavy Attack");

                playerOne.actionThisPress = true;
            }
        }
        if (oneAttackUp)
        {
            if (Time.time < (playerOne.pressStartTime + holdTime))
            {
                playerOne.state = PlayerController.CharacterState.ATTACKING;
                playerOne.action = PlayerController.CharacterAction.LIGHT_ATTACKING;
                playerOne.anim.Play("Light Attack");

                playerOne.actionThisPress = true;
            }
        }


        // Defend inputs
        if (oneDefendDown)
        {
            playerOne.pressStartTime = Time.time;
            playerOne.actionThisPress = false;
        }
        if (oneDefendHeld)
        {
            if (Time.time >= (playerOne.pressStartTime + holdTime) && !playerOne.actionThisPress)
            {
                playerOne.state = PlayerController.CharacterState.GUARDING;
                playerOne.action = PlayerController.CharacterAction.GUARDING;
                playerOne.anim.Play("Guard");
            }
        }
        if (oneDefendUp)
        {
            if (Time.time < (playerOne.pressStartTime + holdTime))
            {
                playerOne.state = PlayerController.CharacterState.GUARDING;
                playerOne.action = PlayerController.CharacterAction.PARRYING;
                playerOne.anim.Play("Parry");
            }
        }

        // P2 Attack inputs
        if (twoAttackDown)
        {
            if (Time.time >= (playerTwo.disarmStartTime + disarmTime))
            {
                playerTwo.pressStartTime = Time.time;
                playerTwo.actionThisPress = false;
            } else
            {
                // TODO Disarmed animation
            }

        }
        if (twoAttackHeld)
        {
            if (Time.time >= (playerTwo.pressStartTime + holdTime) && !playerTwo.actionThisPress)
            {
                playerTwo.state = PlayerController.CharacterState.ATTACKING;
                playerTwo.action = PlayerController.CharacterAction.HEAVY_ATTACKING;
                playerTwo.anim.Play("Heavy Attack");

                playerTwo.actionThisPress = true;
            }
        }
        if (twoAttackUp)
        {
            if (Time.time < (playerTwo.pressStartTime + holdTime))
            {
                playerTwo.state = PlayerController.CharacterState.ATTACKING;
                playerTwo.action = PlayerController.CharacterAction.LIGHT_ATTACKING;
                playerTwo.anim.Play("Light Attack");

                playerTwo.actionThisPress = true;
            }
        }

        // Defend inputs
        if (twoDefendDown)
        {
            playerTwo.pressStartTime = Time.time;
            playerTwo.actionThisPress = false;
        }
        if (twoDefendHeld)
        {
            if (Time.time >= (playerTwo.pressStartTime + holdTime) && !playerTwo.actionThisPress)
            {
                playerTwo.state = PlayerController.CharacterState.GUARDING;
                playerTwo.action = PlayerController.CharacterAction.GUARDING;
                playerTwo.anim.Play("Guard");
            }
        }
        if (twoDefendUp)
        {
            if (Time.time < (playerTwo.pressStartTime + holdTime))
            {
                playerTwo.state = PlayerController.CharacterState.GUARDING;
                playerTwo.action = PlayerController.CharacterAction.PARRYING;
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
        if (attackerController.action == PlayerController.CharacterAction.LIGHT_ATTACKING)
        {
            switch (defenderController.action)
            {
                case PlayerController.CharacterAction.PARRYING:
                    // Attacker gets turned around
                    // TODO: Not implemented, will after movement is added
                    break;
                case PlayerController.CharacterAction.GUARDING:
                    // Attacker gets knocked back
                    Knockback(attackerController);
                    break;
                case PlayerController.CharacterAction.LIGHT_ATTACKING:
                    // Both are knocked back
                    // Idle for now, but should be a knockback animation in the future
                    attackerController.anim.Play("Idle");
                    defenderController.anim.Play("Idle");
                    Knockback(attackerController);
                    Knockback(defenderController);
                    break;
                case PlayerController.CharacterAction.HEAVY_ATTACKING:
                    // Disables attacker's attack button for X seconds
                    Disarm(attackerController);
                    break;
                case PlayerController.CharacterAction.IDLE:
                    switch (defenderController.state)
                    {
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
                    break;
            }
        }
        if (attackerController.action == PlayerController.CharacterAction.HEAVY_ATTACKING)
        {
            switch (defenderController.action)
            {
                case PlayerController.CharacterAction.PARRYING:
                    // Disables attacker's attack button for X seconds
                    Disarm(attackerController);
                    break;
                case PlayerController.CharacterAction.GUARDING:
                    // Defender gets knocked back
                    Knockback(defenderController);
                    break;
                case PlayerController.CharacterAction.LIGHT_ATTACKING:
                    // Heavy overpowers light; attack connects with half damage
                    defenderController.HP -= 1;
                    break;
                case PlayerController.CharacterAction.HEAVY_ATTACKING:
                    // Both lose 1HP and are knocked back
                    Knockback(attackerController);
                    Knockback(defenderController);
                    attackerController.HP -= 1;
                    defenderController.HP -= 1;
                    break;
                case PlayerController.CharacterAction.IDLE:
                    switch (defenderController.state)
                    {
                        case PlayerController.CharacterState.VULNERABLE:
                            // Kills defender
                            defender.SetActive(false);
                            break;
                        case PlayerController.CharacterState.IDLE:
                            // Attack connects fully
                            attackerController.anim.Play("Idle");
                            defenderController.HP -= 2;
                            break;
                    }
                    break;
            }
        }
            
    }

    public void Knockback(PlayerController player)
    {
        // Having issues with AddForce, it was working on the legs but not the animated parts.
        // TODO: try putting each player object in an empty object with a rigidbody2d.
        // http://answers.unity3d.com/questions/559976/can-i-addforce-to-a-model-while-using-animator.html

        if (player.facingLeft)
        {
            player.gameObject.transform.position += Vector3.right * 0.2f;
        } else
        {
            player.gameObject.transform.position += Vector3.left * 0.2f;
        }
    }
    

    public void Disarm(PlayerController player)
    {
        player.disarmStartTime = Time.time;
    }

}
