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

    private void Update()
    {
        //Input reading goes here
        if (Input.GetKeyDown("f"))
        {
            playerOne.state = PlayerController.CharacterState.ATTACKING;
            playerOne.anim.Play("Light Attack");
        }

        if (Input.GetKeyDown("g"))
        {
            playerOne.state = PlayerController.CharacterState.ATTACKING;
            playerOne.anim.Play("Heavy Attack");
        }
        if (Input.GetKeyDown("h"))
        {
            playerOne.state = PlayerController.CharacterState.GUARDING;
            playerOne.anim.Play("Guard");
        }

        if (Input.GetKeyDown("j"))
        {
            playerTwo.state = PlayerController.CharacterState.ATTACKING;
            playerTwo.anim.Play("Light Attack");
        }

        if (Input.GetKeyDown("k"))
        {
            playerTwo.state = PlayerController.CharacterState.ATTACKING;
            playerTwo.anim.Play("Heavy Attack");
        }
        if (Input.GetKeyDown("l"))
        {
            playerTwo.state = PlayerController.CharacterState.GUARDING;
            playerTwo.anim.Play("Guard");
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
        //print(attacker + " attacked " + defender);

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
            defender.SetActive(false);
        }
            
    }

}
