using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour {

    public enum CharacterState
    {
        IDLE,
        LIGHT_ATTACKING,
        HEAVY_ATTACKING,
        PARRYING,
        GUARDING,
        VULNERABLE,
        INVULNERABLE
    }

    public int HP;

    public CharacterState state;
    public Animator anim;

	// Use this for initialization
	private void Start () {
        state = CharacterState.IDLE;
	}

    private void Update()
    {
        // Set state to IDLE when other animations are finished
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            state = CharacterState.IDLE;
        }
    }

}
