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

    public Animator anim;

	// Use this for initialization
	void Start () {
		
	}
}
