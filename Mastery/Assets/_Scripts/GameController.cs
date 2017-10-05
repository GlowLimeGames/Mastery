using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public PlayerController playerOne;
    public PlayerController playerTwo;

    private PlayerController[] _players = new PlayerController[2];

    private static float _moveSpeed = 0.05f;
    private static float _rollSpeed = 0.10f;

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
        _players[0] = playerOne;
        _players[1] = playerTwo;
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
        // Movement
        playerOne.inputHorizontal = Input.GetAxisRaw("P1Horizontal");
        playerOne.inputRollDown = Input.GetButtonDown("P1Fire3");

        // P1Fire1: button 0 (A/bottom button on Xbone/360), left ctrl
        playerOne.inputAttackDown = Input.GetButtonDown("P1Fire1");
        playerOne.inputAttackHeld = Input.GetButton("P1Fire1");
        playerOne.inputAttackUp = Input.GetButtonUp("P1Fire1");

        // P1Fire2: button 1 (B/right button on Xbone/360), left shift
        playerOne.inputDefendDown = Input.GetButtonDown("P1Fire2");
        playerOne.inputDefendHeld = Input.GetButton("P1Fire2");
        playerOne.inputDefendUp = Input.GetButtonUp("P1Fire2");

        // Player Two input reading
        // Movement
        playerTwo.inputHorizontal = Input.GetAxisRaw("P2Horizontal");
        playerTwo.inputRollDown = Input.GetButtonDown("P2Fire3");

        // P2Fire1: button 0, right ctrl
        playerTwo.inputAttackDown = Input.GetButtonDown("P2Fire1");
        playerTwo.inputAttackHeld = Input.GetButton("P2Fire1");
        playerTwo.inputAttackUp = Input.GetButtonUp("P2Fire1");

        // P2Fire2: buton 1, right shift
        playerTwo.inputDefendDown = Input.GetButtonDown("P2Fire2");
        playerTwo.inputDefendHeld = Input.GetButton("P2Fire2");
        playerTwo.inputDefendUp = Input.GetButtonUp("P2Fire2");


        foreach(PlayerController player in _players)
        {
            // Rolling disables pretty much all inputs
            if (player.CanMove())
            {
                if (player.inputHorizontal != 0.0f)
                {
                    if (player.inputHorizontal < 0.0f && !player.facingLeft)
                    {
                        player.TurnAround();
                    }
                    else if (player.inputHorizontal > 0.0f && player.facingLeft)
                    {
                        player.TurnAround();
                    }

                    player.action = PlayerController.CharacterAction.MOVING;
                    player.transform.position += Vector3.right * (_moveSpeed * player.inputHorizontal);
                    player.anim.Play("Walking");
                }

                // Attack inputs
                if (player.inputAttackDown)
                {
                    if (Time.time >= (player.disarmStartTime + _disarmTime))
                    {
                        player.pressStartTime = Time.time;
                        player.actionThisPress = false;
                    }
                    else
                    {
                        // TODO: Play some sort of "whoops, I'm disarmed" animation
                    }

                }
                if (player.inputAttackHeld)
                {
                    if (Time.time >= (player.pressStartTime + _holdTime) && !player.actionThisPress)
                    {
                        player.state = PlayerController.CharacterState.ATTACKING;
                        player.action = PlayerController.CharacterAction.HEAVY_ATTACKING;
                        player.anim.Play("Heavy Attack");

                        player.actionThisPress = true;
                    }
                }
                if (player.inputAttackUp)
                {
                    if (Time.time < (player.pressStartTime + _holdTime))
                    {
                        player.state = PlayerController.CharacterState.ATTACKING;
                        player.action = PlayerController.CharacterAction.LIGHT_ATTACKING;
                        player.anim.Play("Light Attack");

                        player.actionThisPress = true;
                    }
                }


                // Defend inputs
                if (player.inputDefendDown)
                {
                    player.pressStartTime = Time.time;
                    player.actionThisPress = false;
                }
                if (player.inputDefendHeld)
                {
                    if (Time.time >= (player.pressStartTime + _holdTime) && !player.actionThisPress)
                    {
                        player.state = PlayerController.CharacterState.GUARDING;
                        player.action = PlayerController.CharacterAction.GUARDING;
                        player.anim.Play("Guard");
                    }
                }
                if (player.inputDefendUp)
                {
                    if (Time.time < (player.pressStartTime + _holdTime))
                    {
                        player.state = PlayerController.CharacterState.GUARDING;
                        player.action = PlayerController.CharacterAction.PARRYING;
                        player.anim.Play("Parry");
                    }
                }

                if (player.inputRollDown)
                {
                    player.action = PlayerController.CharacterAction.ROLLING;
                    player.anim.Play("Roll");
                    //StartCoroutine(StopRoll(player));
                }
            }
        }

        // UI and game loop stuff:
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
        foreach(PlayerController player in _players)
        {
            if (player.action == PlayerController.CharacterAction.ROLLING)
            {
                if (player.facingLeft)
                {
                    player.transform.position += Vector3.left * _rollSpeed;
                }
                else
                {
                    player.transform.position += Vector3.right * _rollSpeed;
                }
            }
        }
    }

    private void LateUpdate()
    {
        //Do something after Movement / Combat here
        foreach (PlayerController player in _players)
        {
            if (player.HP <= 0)
            {
                player.state = PlayerController.CharacterState.VULNERABLE;
            }
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
                    attackerController.TurnAround();
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
            // UPDATE: We're not using rigidbody physics, so could just use a coroutine or state change, like for the roll

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

// TODO: P2 can kill P1 but not the other way.