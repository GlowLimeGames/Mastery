using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour {

    public enum CharacterState
    {
        IDLE,
        ATTACKING,
        GUARDING,
        VULNERABLE,
        INVULNERABLE
    }

    public CharacterState state;

    // TODO: Use an event listener for attacks

    public Animator anim;

	// Use this for initialization
	void Start () {
        state = CharacterState.IDLE;
	}
}
