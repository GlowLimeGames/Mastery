using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public PlayerController playerOne;
    public PlayerController playerTwo;

    private void Start()
    {
        CollisionBehavior.AttackResolution += Attack; // Subscribe to the AttackResolution event
    }

    private float startTime;

    // Time to distinguish a press from a hold, for light/heavy and parry/guard
    // (Different numbers for those two categories?)
    private float holdTime = 0.15f;

    // Make sure only one action is taken per button press
    private bool actionThisPress = false;

    private void Update()
    {
        //Input reading goes here
        bool oneAttackDown = Input.GetKeyDown("f");
        bool oneAttackHeld = Input.GetKey("f");
        bool oneAttackUp = Input.GetKeyUp("f");

        bool oneDefendDown = Input.GetKeyDown("g");
        bool oneDefendHeld = Input.GetKey("g");
        bool oneDefendUp = Input.GetKeyUp("g");

        // Attack inputs
        if (oneAttackDown)
        {
            startTime = Time.time;
            actionThisPress = false;
        }
        if (oneAttackHeld)
        {
            if (Time.time >= (startTime + holdTime) && !actionThisPress)
            {
                playerOne.state = PlayerController.CharacterState.HEAVY_ATTACKING;
                playerOne.anim.Play("Heavy Attack");

                actionThisPress = true;
            }
        }
        if (oneAttackUp)
        {
            if (Time.time < (startTime + holdTime))
            {
                playerOne.state = PlayerController.CharacterState.LIGHT_ATTACKING;
                playerOne.anim.Play("Light Attack");

                actionThisPress = true;
            }
        }

        // Defend inputs
        if (oneDefendDown)
        {
            startTime = Time.time;
            actionThisPress = false;
        }
        if (oneDefendHeld)
        {
            if (Time.time >= (startTime + holdTime) && !actionThisPress)
            {
                playerOne.state = PlayerController.CharacterState.GUARDING;
                playerOne.anim.Play("Guard");
            }
        }
        if (oneDefendUp)
        {
            if (Time.time < (startTime + holdTime))
            {
                playerOne.state = PlayerController.CharacterState.PARRYING;
                playerOne.anim.Play("Parry");
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


        if (defenderController.state == PlayerController.CharacterState.IDLE)
        {
            defenderController.HP -= 1;
        }
            
    }

}
