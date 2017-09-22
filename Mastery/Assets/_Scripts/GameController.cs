using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public PlayerController playerOne;
    public PlayerController playerTwo;

    private void Update()
    {
        //Input reading goes here
        if (Input.GetButtonDown("Fire1"))
        {
            playerOne.state = PlayerController.CharacterState.ATTACKING;
            playerOne.anim.Play("Light Attack");
        }

        if (Input.GetButtonDown("Fire2"))
        {
            playerTwo.state = PlayerController.CharacterState.ATTACKING;
            playerTwo.anim.Play("Light Attack");
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

}
