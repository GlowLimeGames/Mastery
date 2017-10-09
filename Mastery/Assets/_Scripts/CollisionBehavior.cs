using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionBehavior : MonoBehaviour
{

    public delegate void Attack(GameObject attacker, GameObject defender);
    public static event Attack AttackResolution;

    public delegate void Kick(GameObject attacker, GameObject defender);
    public static event Kick KickResolution;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("Sword"))
        {
            GameObject attacker = collision.collider.gameObject.transform.parent.gameObject;
            GameObject defender = collision.otherCollider.gameObject.transform.parent.gameObject;
            if (attacker != defender)          // Lots of self collisions happening, ignore those
            {
                AttackResolution(attacker, defender);
            }
        }

        if (collision.collider.gameObject.CompareTag("Leg"))
        {
            GameObject attacker = collision.collider.gameObject.transform.parent.gameObject;
            GameObject defender = collision.otherCollider.gameObject.transform.parent.gameObject;
            if (attacker != defender)          // Lots of self collisions happening, ignore those
            {
                KickResolution(attacker, defender);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {

    }

    private void OnCollisionExit2D(Collision2D collision)
    {

    }
}