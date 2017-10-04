using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public PlayerController playerOne;
    public PlayerController playerTwo;

    // Time to distinguish a press from a hold, for light/heavy and parry/guard
    // (Different numbers for those two categories?)
    private static float _holdTime = 0.15f;

    // Length of disarm
    private static float _disarmTime = 3.0f;

    //Remaining time left in the game - use this to transform the closing walls.
    private float timeRemaining = 300.0f;
    //The time when the walls will begin closing in on the arena. 
    //timeToClose is the percentage of time that the walls will begin moving. 
    //Change this value to change the time the walls will begin moving.
    private float timeToClose = 0.75f;
    private float beginClosing;

    /// <summary>
    ///notOver: current game is still in progress
    ///p1Win: Player 1 is the winner
    ///p2Win: Player 2 is the winner
    ///timeUp: the time runs out
    /// </summary>
    //use this enum to handle endgame
    private enum gameScenarios
    {
        //notover is 0001
        notOver = 0x01,
        // p1Win is 0010.
        p1Win = 0x02,
        // p2Win is 0100.
        p2Win = 0x04,
        // timeUp is 1000.
        timeUp = 0x08,
    }
    //current status of the game, use as flag to end the game
    private gameScenarios currentScenario;

    private void Start()
    {
        //flag that will trigger the walls to move
        beginClosing = timeRemaining * timeToClose;
        //game begins in progress
        currentScenario = gameScenarios.notOver;

        playerOne.facingLeft = false;
        playerTwo.facingLeft = true;

        // Subscribe to the AttackResolution event
        CollisionBehavior.AttackResolution += Attack;
    }

    private void Update()
    {
        //Input reading goes here

        // Player One input reading
        // P1Fire1: button 0 (A/bottom button on Xbone/360), left ctrl
        bool oneAttackDown = Input.GetButtonDown("P1Fire1");
        bool oneAttackHeld = Input.GetButton("P1Fire1");
        bool oneAttackUp = Input.GetButtonUp("P1Fire1");

        // P1Fire2: button 1 (B/right button on Xbone/360), left shift
        bool oneDefendDown = Input.GetButtonDown("P1Fire2");
        bool oneDefendHeld = Input.GetButton("P1Fire2");
        bool oneDefendUp = Input.GetButtonUp("P1Fire2");

        // Player Two input reading
        // P2Fire1: button 0, right ctrl
        bool twoAttackDown = Input.GetButtonDown("P2Fire1");
        bool twoAttackHeld = Input.GetButton("P2Fire1");
        bool twoAttackUp = Input.GetButtonUp("P2Fire1");

        // P2Fire2: buton 1, right shift
        bool twoDefendDown = Input.GetButtonDown("P2Fire2");
        bool twoDefendHeld = Input.GetButton("P2Fire2");
        bool twoDefendUp = Input.GetButtonUp("P2Fire2");

        // TODO: Prevent attacking in the middle of a defend, and vice versa.

        // Attack inputs
        if (oneAttackDown)
        {
            if (Time.time >= (playerOne.disarmStartTime + _disarmTime))
            {
                playerOne.pressStartTime = Time.time;
                playerOne.actionThisPress = false;
            }
            else
            {
                // TODO: Play some sort of "whoops, I'm disarmed" animation
            }

        }
        if (oneAttackHeld)
        {
            if (Time.time >= (playerOne.pressStartTime + _holdTime) && !playerOne.actionThisPress)
            {
                playerOne.state = PlayerController.CharacterState.ATTACKING;
                playerOne.action = PlayerController.CharacterAction.HEAVY_ATTACKING;
                playerOne.anim.Play("Heavy Attack");

                playerOne.actionThisPress = true;
            }
        }
        if (oneAttackUp)
        {
            if (Time.time < (playerOne.pressStartTime + _holdTime))
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
            if (Time.time >= (playerOne.pressStartTime + _holdTime) && !playerOne.actionThisPress)
            {
                playerOne.state = PlayerController.CharacterState.GUARDING;
                playerOne.action = PlayerController.CharacterAction.GUARDING;
                playerOne.anim.Play("Guard");
            }
        }
        if (oneDefendUp)
        {
            if (Time.time < (playerOne.pressStartTime + _holdTime))
            {
                playerOne.state = PlayerController.CharacterState.GUARDING;
                playerOne.action = PlayerController.CharacterAction.PARRYING;
                playerOne.anim.Play("Parry");
            }
        }

        // P2 Attack inputs
        if (twoAttackDown)
        {
            if (Time.time >= (playerTwo.disarmStartTime + _disarmTime))
            {
                playerTwo.pressStartTime = Time.time;
                playerTwo.actionThisPress = false;
            }
            else
            {
                // TODO Disarmed animation
            }

        }
        if (twoAttackHeld)
        {
            if (Time.time >= (playerTwo.pressStartTime + _holdTime) && !playerTwo.actionThisPress)
            {
                playerTwo.state = PlayerController.CharacterState.ATTACKING;
                playerTwo.action = PlayerController.CharacterAction.HEAVY_ATTACKING;
                playerTwo.anim.Play("Heavy Attack");

                playerTwo.actionThisPress = true;
            }
        }
        if (twoAttackUp)
        {
            if (Time.time < (playerTwo.pressStartTime + _holdTime))
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
            if (Time.time >= (playerTwo.pressStartTime + _holdTime) && !playerTwo.actionThisPress)
            {
                playerTwo.state = PlayerController.CharacterState.GUARDING;
                playerTwo.action = PlayerController.CharacterAction.GUARDING;
                playerTwo.anim.Play("Guard");
            }
        }
        if (twoDefendUp)
        {
            if (Time.time < (playerTwo.pressStartTime + _holdTime))
            {
                playerTwo.state = PlayerController.CharacterState.GUARDING;
                playerTwo.action = PlayerController.CharacterAction.PARRYING;
                playerTwo.anim.Play("Parry");
            }
        }


        //continually decreasing time for game timer.
        timeRemaining -= Time.deltaTime;
        if (timeRemaining < 0) {
            currentScenario = gameScenarios.timeUp;
        }


        if (timeRemaining <= beginClosing)
        {
            //TODO: Wall-closing logic
        }

        //endgame logic
        if (currentScenario == gameScenarios.p1Win)
        {
            //TODO: player 1 wins scenario
        }
        if (currentScenario == gameScenarios.p2Win)
        {
            //TODO: player 2 wins scenario
        }
        if (currentScenario == gameScenarios.timeUp)
        {
            //TODO: time runs out scenario
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

        // print(attackerController +  " (" + attackerController.state + ") attacks " + defenderController + " (" + defenderController.state + ")");
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

        // For now, they are just snapping backwards a bit
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
