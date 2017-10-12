using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public PlayerController playerOne;
    public PlayerController playerTwo;

    public Text playerOneHpText;
    public Text playerTwoHpText;

    private PlayerController[] _players = new PlayerController[2];

    public static int hpMax = 2;

    private static float _moveSpeed = 0.05f;
    private static float _rollSpeed = 0.10f;

    // Time to distinguish a press from a hold, for light/heavy and parry/guard
    // (Different numbers for those two categories?)
    private static float _holdTime = 0.15f;

    // Length of disarm
    private static float _disarmTime = 3.0f;

    private static float _disableMovementTime = 3.0f;

    private static float _shieldBreakTime = 3.0f;

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
        CollisionBehavior.KickResolution += Kick;
    }

    private void Update()
    {
        //Input reading goes here

        // Player One input reading
        // Movement and facing
        playerOne.inputHorizontal = Input.GetAxisRaw("P1Horizontal");
        playerOne.inputRightHorizontal = Input.GetAxisRaw("P1RightHorizontal");

        // P1Fire1: button 0 (A/bottom button on Xbone/360), left ctrl
        playerOne.inputAttackDown = Input.GetButtonDown("P1Fire1");
        playerOne.inputAttackHeld = Input.GetButton("P1Fire1");
        playerOne.inputAttackUp = Input.GetButtonUp("P1Fire1");

        // P1Fire2: button 1 (B/right button on Xbone/360), left alt
        playerOne.inputDefendDown = Input.GetButtonDown("P1Fire2");
        playerOne.inputDefendHeld = Input.GetButton("P1Fire2");
        playerOne.inputDefendUp = Input.GetButtonUp("P1Fire2");

        // P1Fire3: button 2 (X/left button on Xbone/360), left shift
        playerOne.inputRollDown = Input.GetButtonDown("P1Fire3");

        // P1Fire4: button 3 (Y/top button on Xbone/360), no key yet
        playerOne.inputKickDown = Input.GetButtonDown("P1Fire4");

        // Player Two input reading
        // Movement and facing
        playerTwo.inputHorizontal = Input.GetAxisRaw("P2Horizontal");
        playerTwo.inputRightHorizontal = Input.GetAxisRaw("P2RightHorizontal");

        // P2Fire1: button 0, right ctrl
        playerTwo.inputAttackDown = Input.GetButtonDown("P2Fire1");
        playerTwo.inputAttackHeld = Input.GetButton("P2Fire1");
        playerTwo.inputAttackUp = Input.GetButtonUp("P2Fire1");

        // P2Fire2: buton 1, right alt
        playerTwo.inputDefendDown = Input.GetButtonDown("P2Fire2");
        playerTwo.inputDefendHeld = Input.GetButton("P2Fire2");
        playerTwo.inputDefendUp = Input.GetButtonUp("P2Fire2");

        // P2Fire3: button 2, right shift
        playerTwo.inputRollDown = Input.GetButtonDown("P2Fire3");

        // P2Fire4: button 3 (Y/top button on Xbone/360), no key yet
        playerTwo.inputKickDown = Input.GetButtonDown("P2Fire4");


        foreach (PlayerController player in _players)
        {
            // Rolling disables pretty much all inputs
            if (player.CanAct())
            {
                if (Time.time >= (player.disableMovementStartTime + _disableMovementTime))
                {
                    // Right stick takes priority over left stick for determining orientation.
                    if (player.inputRightHorizontal != 0.0f)
                    {
                        if (player.inputRightHorizontal < 0.0f && !player.facingLeft)
                        {
                            player.TurnAround();
                        }
                        else if (player.inputRightHorizontal > 0.0f && player.facingLeft)
                        {
                            player.TurnAround();
                        }
                    }
                    else
                    {
                        if (player.inputHorizontal != 0.0f)
                        {
                            // Not overridden by right stick, so turn around if not facing direction of motion
                            if (player.inputHorizontal < 0.0f && !player.facingLeft)
                            {
                                player.TurnAround();
                            }
                            else if (player.inputHorizontal > 0.0f && player.facingLeft)
                            {
                                player.TurnAround();
                            }
                        }
                    }

                    // Motion happens regardless of right stick status
                    if (player.inputHorizontal != 0.0f && player.action != PlayerController.CharacterAction.TURNING)
                    {
                        player.action = PlayerController.CharacterAction.MOVING;
                        player.transform.position += Vector3.right * (_moveSpeed * player.inputHorizontal);
                        player.anim.Play("Walking");
                    }
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

                // Kick input
                if (player.inputKickDown)
                {
                    player.action = PlayerController.CharacterAction.KICKING;
                    player.anim.Play("Kick");
                }


                // Defend inputs
                if (Time.time >= (player.shieldBreakStartTime + _shieldBreakTime))
                {
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
        }

        // UI and game loop stuff:
        //continually decreasing time for game timer.
        timeRemaining -= Time.deltaTime;
        if (timeRemaining < 0)
        {
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
        foreach (PlayerController player in _players)
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
            // TODO: Something similar for knockback
        }
    }

    private void LateUpdate()
    {
        //Do something after Movement / Combat here
        playerOneHpText.text = "P1 HP: " + playerOne.HP.ToString();
        playerTwoHpText.text = "P2 HP: " + playerTwo.HP.ToString();

        foreach (PlayerController player in _players)
        {
            if (player.HP <= 0)
            {
                player.state = PlayerController.CharacterState.VULNERABLE;
            }
        }
    }

    // TODO: This and Kick probably belong in PlayerController.
    public void Attack(GameObject attacker, GameObject defender)
    {
        PlayerController attackerController;
        PlayerController defenderController;

        if (attacker == playerOne.gameObject)
        {
            attackerController = playerOne;
            defenderController = playerTwo;
        }
        else
        {
            attackerController = playerTwo;
            defenderController = playerOne;
        }

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
                    attackerController.Knockback();
                    break;
                case PlayerController.CharacterAction.LIGHT_ATTACKING:
                    // Both are knocked back
                    // Idle for now, but should be a knockback animation in the future
                    attackerController.anim.Play("Idle");
                    defenderController.anim.Play("Idle");
                    attackerController.Knockback();
                    defenderController.Knockback();
                    break;
                case PlayerController.CharacterAction.HEAVY_ATTACKING:
                    // Disables attacker's attack button for X seconds
                    attackerController.Disarm();
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
                    attackerController.Disarm();
                    break;
                case PlayerController.CharacterAction.GUARDING:
                    // Defender gets knocked back
                    defenderController.Knockback();
                    break;
                case PlayerController.CharacterAction.LIGHT_ATTACKING:
                    // Heavy overpowers light; attack connects with half damage
                    defenderController.HP -= 1;
                    break;
                case PlayerController.CharacterAction.HEAVY_ATTACKING:
                    // Both lose 1HP and are knocked back
                    attackerController.Knockback();
                    defenderController.Knockback();
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

    public void Kick(GameObject attacker, GameObject defender)
    {
        PlayerController attackerController;
        PlayerController defenderController;

        if (attacker == playerOne.gameObject)
        {
            attackerController = playerOne;
            defenderController = playerTwo;
        }
        else
        {
            attackerController = playerTwo;
            defenderController = playerOne;
        }

        if (attackerController.action == PlayerController.CharacterAction.KICKING)
        {
            switch (defenderController.action)
            {
                case PlayerController.CharacterAction.GUARDING:
                    // Breaks defender's shield, restores attacker's health
                    defenderController.ShieldBreak();
                    attackerController.HP = hpMax;
                    break;
                case PlayerController.CharacterAction.PARRYING:
                    // (Same as above? Not explicit in spec)
                    defenderController.ShieldBreak();
                    attackerController.HP = hpMax;
                    break;
                default:
                    // If no shield up, then disable attacker's movement
                    attackerController.DisableMovement();
                    break;
            }
        }
    }

}
