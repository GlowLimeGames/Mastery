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
    public bool facingLeft;

	// Use this for initialization
	private void Start () {
        state = CharacterState.IDLE;
        HP = 2;
    }

    private void Update()
    {
        // Update state based on animations.
        // There should be a better way to do this...
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            state = CharacterState.IDLE;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Light Attack"))
        {
            state = CharacterState.LIGHT_ATTACKING;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Heavy Attack"))
        {
            state = CharacterState.HEAVY_ATTACKING;
        }
    }

}
