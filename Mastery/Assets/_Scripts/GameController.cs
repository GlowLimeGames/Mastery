using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    public PlayerController playerOne;
    public PlayerController playerTwo;

    // TODO: Deprecate these two
    public Text playerOneHpText;
    public Text playerTwoHpText;

    public Text VictoryOverlay;

    private PlayerController[] _players = new PlayerController[2];

    public static int stockMax = 3;
    public static int hpMax = 2;

    private static float _moveSpeed = 0.05f;
    private static float _rollSpeed = 0.12f;
    private static float _knockbackSpeed = 0.06f;

    // Time to distinguish a press from a hold, for light/heavy and parry/guard
    // (Different numbers for those two categories?)
    private static float _holdTime = 0.15f;

    // Length of disarm
    private static float _disarmTime = 3.0f;

    private static float _disableMovementTime = 3.0f;

    private static float _shieldBreakTime = 3.0f;

    private static float _respawnTime = 2.0f;

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

        // Subscribe to the events from CollisionBehavior
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
                    // Right stick determines the direction you're facing
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

                    // Left stick determines motion
                    if (player.inputHorizontal != 0.0f && player.action != PlayerController.CharacterAction.TURNING)
                    {
                        PlayerController otherPlayer;
                        if (player == playerOne)
                        {
                            otherPlayer = playerTwo;
                        }
                        else
                        {
                            otherPlayer = playerOne;
                        }

                        float deltaX = player.transform.position.x - otherPlayer.transform.position.x;

                        if (player.inputHorizontal < 0.0f)
                        {
                            // Make sure the other player is not adjacent to this one
                            // (Numbers must be different to avoid trapping the player on contact)
                            if ((deltaX <= -2.4) || (deltaX >= 2.7))
                            {
                                player.action = PlayerController.CharacterAction.MOVING;
                                player.transform.position += Vector3.right * (_moveSpeed * player.inputHorizontal);
                                player.anim.Play("Walking");
                            }

                        } else if (player.inputHorizontal > 0.0f)

                        {
                            if ((deltaX <= -2.7) || (deltaX >= 2.4))
                            {
                                player.action = PlayerController.CharacterAction.MOVING;
                                player.transform.position += Vector3.right * (_moveSpeed * player.inputHorizontal);
                                player.anim.Play("Walking");
                            }
                        }

                    }

                    // Rolling inputs. Disabled when movement disabled
                    if (player.inputRollDown)
                    {
                        // Roll in the direction you're moving.
                        // If not moving, roll in the direction you're facing
                        if (player.inputHorizontal < 0.0f)
                        {
                            player.rollingLeft = true;
                        }
                        else if (player.inputHorizontal == 0.0f)
                        {
                            player.rollingLeft = player.facingLeft;
                        }
                        else
                        {
                            player.rollingLeft = false;
                        }
                        player.action = PlayerController.CharacterAction.ROLLING;
                        player.anim.Play("Roll");
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
                        player.anim.Play("Heavy Attack (Swing)");

                        player.actionThisPress = true;
                    }
                }
                if (player.inputAttackUp)
                {
                    if (Time.time < (player.pressStartTime + _holdTime))
                    {
                        player.state = PlayerController.CharacterState.ATTACKING;
                        player.action = PlayerController.CharacterAction.LIGHT_ATTACKING;
                        player.anim.Play("Light Attack (Swing)");

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

        foreach(PlayerController player in _players)
        {
            if ((player.isDead) && (player.stock > 0))
            {
                if (Time.time >= (player.deathTime + _respawnTime))
                {
                    player.Respawn();
                }
            }

        }

    }

    private void FixedUpdate()
    {
        // Movement / Combat Functionality goes here
        foreach (PlayerController player in _players)
        {
            // Rolling motion
            if (player.action == PlayerController.CharacterAction.ROLLING)
            {
                if (player.rollingLeft)
                {
                    player.transform.position += Vector3.left * _rollSpeed;
                }
                else
                {
                    player.transform.position += Vector3.right * _rollSpeed;
                }
            }

            // Knockback motion
            if (player.state == PlayerController.CharacterState.KNOCKBACK)
            {
                if (player.facingLeft)
                {
                    player.transform.position += Vector3.right * _knockbackSpeed;
                }
                else
                {
                    player.transform.position += Vector3.left * _knockbackSpeed;
                }
            }

            // Fix player positions if one has rolled inside the other
            if (playerOne.action != PlayerController.CharacterAction.ROLLING && playerTwo.action != PlayerController.CharacterAction.ROLLING)
            {
                float deltaX = playerOne.transform.position.x - playerTwo.transform.position.x;
                if (Math.Abs(deltaX) < 2.4)
                {
                    _fixIllegalPlayerPosition();
                }
            }
        }
    }

    private void LateUpdate()
    {
        // Do something after Movement / Combat here
        playerOneHpText.text = "HP: " + playerOne.HP.ToString();
        playerTwoHpText.text = "HP: " + playerTwo.HP.ToString();

        foreach (PlayerController player in _players)
        {
            // TODO: Move this all into PlayerController methods Disarm() etc
            if (player.HP <= 0)
            {
                player.state = PlayerController.CharacterState.VULNERABLE;
            }

            if (Time.time >= (player.disarmStartTime + _disarmTime))
            {
                player.disarmText.text = "";
            }
            if (Time.time >= (player.disableMovementStartTime + _disableMovementTime))
            {
                player.disableMovementText.text = "";
            }
            if (Time.time >= (player.shieldBreakStartTime + _shieldBreakTime))
            {
                player.shieldBreakText.text = "";
            }

            // This can stay
            if (player.stock <= 0)
            {
                PlayerWins(_otherPlayer(player));
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from those events to avoid issues when restarting the match
        CollisionBehavior.AttackResolution -= Attack;
        CollisionBehavior.KickResolution -= Kick;
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
                    attackerController.Knockback();
                    defenderController.Knockback();
                    break;
                case PlayerController.CharacterAction.HEAVY_ATTACKING:
                    // Disables attacker's attack button for X seconds
                    attackerController.Disarm();
                    break;
                case PlayerController.CharacterAction.ROLLING:
                    // Nothing, they're invulnerable
                    break;
                default:                               // IDLE, MOVING, DELAY, TURNING, KICKING(?), STUN(?), ...
                    switch (defenderController.state)
                    {
                        case PlayerController.CharacterState.VULNERABLE:
                            // Kills defender
                            defenderController.IsKilled();
                            break;
                        case PlayerController.CharacterState.IDLE:
                            // Successful hit.
                            attackerController.anim.Play("Light Attack (Return)");

                            defenderController.anim.Play("Stun");
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
                    // Heavy overpowers light; heavy attack connects with half damage
                    attackerController.anim.Play("Heavy Attack (Return)");
                    defenderController.anim.Play("Stun");
                    defenderController.HP -= 1;
                    break;
                case PlayerController.CharacterAction.HEAVY_ATTACKING:
                    // Both lose 1HP and are knocked back
                    attackerController.Knockback();
                    defenderController.Knockback();
                    attackerController.HP -= 1;
                    defenderController.HP -= 1;
                    break;
                case PlayerController.CharacterAction.ROLLING:
                    // Nothing, they're invulnerable
                    break;
                default:                                // IDLE, MOVING, DELAY, TURNING, KICKING(?), STUN(?), ...
                    switch (defenderController.state)
                    {
                        case PlayerController.CharacterState.VULNERABLE:
                            // Kills defender
                            defenderController.IsKilled();
                            break;
                        case PlayerController.CharacterState.IDLE:
                            // Attack connects fully
                            attackerController.anim.Play("Heavy Attack (Return)");
                            defenderController.HP -= 2;
                            defenderController.anim.Play("Stun");
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

    public void PlayerWins(PlayerController player)
    {
        //print(player.ToString() + " wins");
        VictoryOverlay.text = player.name.ToString() + " Wins";
        VictoryOverlay.gameObject.SetActive(true);

    }

    public void RestartMatch()
    {
        SceneManager.LoadScene("Main");
    }

    private void _fixIllegalPlayerPosition()
        // Players can end a roll inside the other player. Push them apart whenever this happens.
    {
        PlayerController leftPlayer;
        PlayerController rightPlayer;

        if (playerOne.transform.position.x <= playerTwo.transform.position.x)
        {
            leftPlayer = playerOne;
            rightPlayer = playerTwo;
        } else
        {
            leftPlayer = playerTwo;
            rightPlayer = playerOne;
        }

        leftPlayer.gameObject.transform.position += Vector3.left * 0.1f;
        rightPlayer.gameObject.transform.position += Vector3.right * 0.1f;

    }

    private PlayerController _otherPlayer(PlayerController thisPlayer)
    {
        if (thisPlayer == playerOne)
        {
            return playerTwo;
        } else
        {
            return playerOne;
        }
    }

}
