using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public bool gameOver;

    public PlayerController playerOne;
    public PlayerController playerTwo;

    public GameObject wallLeft;
    public GameObject wallRight;

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

    // Distance between players
    private float _playerDeltaX;

    //Remaining time left in the game - use this to transform the closing walls.
    private float _timeRemaining = 20.0f;
    //The time when the walls will begin closing in on the arena. 
    //_timeToClose is the percentage of time that the walls will begin moving. 
    //Change this value to change the time the walls will begin moving.
    private float _timeToClose = 0.75f;
    private float _beginClosing;

    // Where the walls should stop closing in
    private float _wallStopX = 4.0f;

    private float _wallSpeed = 0.001f;

    public static float safeXMin = -7.0f;
    public static float safeXMax = 7.0f;

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
        gameOver = false;
        _players[0] = playerOne;
        _players[1] = playerTwo;
        //flag that will trigger the walls to move
        _beginClosing = _timeRemaining * _timeToClose;
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

        if (gameOver)
        {
            return;
        }

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
                        PlayerController otherPlayer = _otherPlayer(player);

                        // TODO: Remove these magic numbers!
                        float personalDeltaX = player.transform.position.x - otherPlayer.transform.position.x;
                        //float leftWallDeltaX = player.transform.position.x - wallLeft.transform.position.x;

                        // Moving left
                        if (player.inputHorizontal < 0.0f)
                        {
                            // Make sure they're not against the left wall
                            if (player.leftWallDeltaX > 1.75)
                            {
                                // Make sure the other player is not left-adjacent to this one
                                // (Numbers must be different to avoid trapping the player on contact)
                                if ((personalDeltaX <= -2.4) || (personalDeltaX >= 2.7))
                                {
                                    player.action = PlayerController.CharacterAction.MOVING;
                                    player.transform.position += Vector3.right * (_moveSpeed * player.inputHorizontal);
                                    player.anim.Play("Walking", 1);  // walking is in Leg Layer, layer 1
                                }
                            }
                        }
                        // Moving right
                        else if (player.inputHorizontal > 0.0f)
                        {
                            if (player.rightWallDeltaX < -1.75)
                            {
                                if ((personalDeltaX <= -2.7) || (personalDeltaX >= 2.4))
                                {
                                    player.action = PlayerController.CharacterAction.MOVING;
                                    player.transform.position += Vector3.right * (_moveSpeed * player.inputHorizontal);
                                    player.anim.Play("Walking", 1);
                                }
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

                        if ((player.rollingLeft && (player.leftWallDeltaX > 1.75)) || (!player.rollingLeft && (player.rightWallDeltaX < -1.75)))
                        {
                            player.action = PlayerController.CharacterAction.ROLLING;
                            player.anim.Play("Roll");
                        }

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
                        player.action = PlayerController.CharacterAction.HEAVY_ATTACKING;
                        player.anim.Play("Heavy Attack (Swing)");

                        player.actionThisPress = true;
                    }
                }
                if (player.inputAttackUp)
                {
                    if (Time.time < (player.pressStartTime + _holdTime))
                    {
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
                            player.action = PlayerController.CharacterAction.GUARDING;
                            player.anim.Play("Guard (On)");
                        }
                    }
                    if (player.inputDefendUp)
                    {
                        if (Time.time < (player.pressStartTime + _holdTime))
                        {
                            player.action = PlayerController.CharacterAction.PARRYING;
                            player.anim.Play("Parry");
                        }
                        else
                        {
                            player.action = PlayerController.CharacterAction.GUARDING;
                            player.anim.Play("Guard (Off)");
                        }
                    }
                }
            }
        }

        // UI and game loop stuff:
        // continually decreasing time for game timer.
        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining < 0)
        {
            currentScenario = gameScenarios.timeUp;
        }


        if ((_timeRemaining <= _beginClosing) && (wallRight.transform.position.x > _wallStopX) && (!gameOver))
        {
            wallLeft.transform.position += Vector3.right * _wallSpeed;
            safeXMin += _wallSpeed;
            wallRight.transform.position += Vector3.left * _wallSpeed;
            safeXMax -= _wallSpeed;
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
                    if (player.leftWallDeltaX > 1.75)
                    {
                        player.transform.position += Vector3.left * _rollSpeed;
                    }
                }
                else
                {
                    if (player.rightWallDeltaX < -1.75)
                    {
                        player.transform.position += Vector3.right * _rollSpeed;
                    }
                }
            }

            // Knockback motion
            if (player.action == PlayerController.CharacterAction.KNOCKBACK)
            {
                if (player.facingLeft)
                {
                    if (player.rightWallDeltaX < -1.75)
                    {
                        player.transform.position += Vector3.right * _knockbackSpeed;
                    }
                }
                else
                {
                    if (player.leftWallDeltaX > 1.75)
                    {
                        player.transform.position += Vector3.left * _knockbackSpeed;
                    }
                }
            }
        }
        // Fix player positions if one has rolled inside the other
        if (playerOne.action != PlayerController.CharacterAction.ROLLING && playerTwo.action != PlayerController.CharacterAction.ROLLING)
        {

            if (Math.Abs(_playerDeltaX) < 2.4)
            {
                _unstickPlayers();
            }
        }

    }

    private void LateUpdate()
    {
        // Do something after Movement / Combat here

        _playerDeltaX = playerOne.transform.position.x - playerTwo.transform.position.x;

        foreach (PlayerController player in _players)
        {
            player.leftWallDeltaX = player.transform.position.x - wallLeft.transform.position.x;
            player.rightWallDeltaX = player.transform.position.x - wallRight.transform.position.x;

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

            // Walls "push" adjacent players at the same speed they're moving
            if (_timeRemaining <= _beginClosing)
            {
                if (Math.Abs(player.leftWallDeltaX) < 1.75)
                {
                    player.transform.position += Vector3.right * _wallSpeed;
                }

                if (Math.Abs(player.rightWallDeltaX) < 1.75)
                {
                    player.transform.position += Vector3.left * _wallSpeed;
                }
            }

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

        // If attacker hits defender from behind, treat defender as idle, ignoring frontal attacks/defends
        if (attackerController.facingLeft == defenderController.facingLeft)
        {
            defenderController.anim.Play("Idle");
            defenderController.action = PlayerController.CharacterAction.IDLE;
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
                    // Successful hit.
                    attackerController.anim.Play("Light Attack (Return)");

                    defenderController.anim.Play("Stun");
                    defenderController.TakeDamage(1);
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
                    attackerController.anim.Play("Heavy Attack (Return)");
                    defenderController.Knockback();
                    break;
                case PlayerController.CharacterAction.LIGHT_ATTACKING:
                    // Heavy overpowers light; heavy attack connects with half damage
                    attackerController.anim.Play("Heavy Attack (Return)");
                    defenderController.anim.Play("Stun");
                    defenderController.TakeNonMortalDamage(1);
                    break;
                case PlayerController.CharacterAction.HEAVY_ATTACKING:
                    // Both lose 1HP and are knocked back
                    attackerController.Knockback();
                    defenderController.Knockback();
                    attackerController.TakeNonMortalDamage(1);
                    defenderController.TakeNonMortalDamage(1);
                    break;
                case PlayerController.CharacterAction.ROLLING:
                    // Nothing, they're invulnerable
                    break;
                default:                                // IDLE, MOVING, DELAY, TURNING, KICKING(?), STUN(?), ...
                    // Attack connects fully
                    attackerController.anim.Play("Heavy Attack (Return)");
                    defenderController.TakeDamage(2);
                    defenderController.anim.Play("Stun");
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
                    attackerController.MaxOutHP();
                    break;
                case PlayerController.CharacterAction.PARRYING:
                    // (Same as above? Not explicit in spec)
                    defenderController.ShieldBreak();
                    attackerController.MaxOutHP();
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
        gameOver = true;
        player.anim.Play("Victory");
        if (player == playerOne)
        {
            VictoryOverlay.text = "Player One Wins";
        } else
        {
            VictoryOverlay.text = "Player Two Wins";
        }
        
        VictoryOverlay.gameObject.SetActive(true);

    }

    public void RestartMatch()
    {
        SceneManager.LoadScene("Main");
    }

    private void _unstickPlayers()
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

        // If one player is against a moving wall, push the other player only
        if (Math.Abs(leftPlayer.leftWallDeltaX) < 1.75)
        {
            rightPlayer.gameObject.transform.position += Vector3.right * 0.1f;
        }
        else if (Math.Abs(rightPlayer.rightWallDeltaX) < 1.75)
        {
            leftPlayer.gameObject.transform.position += Vector3.left * 0.1f;
        }
        // Otherwise, push each apart at the same rate
        else
        {
            leftPlayer.gameObject.transform.position += Vector3.left* 0.1f;
            rightPlayer.gameObject.transform.position += Vector3.right* 0.1f;
        }
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
