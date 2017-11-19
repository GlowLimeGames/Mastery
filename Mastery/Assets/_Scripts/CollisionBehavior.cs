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
        // Two things that can hit: an active sword, and a leg
        if (collision.collider.gameObject.CompareTag("SwordActive"))
        {
            //// If it hit an inactive shield, ignore it
            //if (collision.otherCollider.gameObject.CompareTag("ShieldInactive"))
            //{
            //    return;
            //}

            // Also ignore inactive sword
            // TODO: Testing not doing this, since the swords are positioned differently now
            //if (collision.otherCollider.gameObject.CompareTag("SwordInactive"))
            //{
            //    return;
            //}

            GameObject attacker = collision.collider.transform.root.gameObject;
            GameObject defender = collision.otherCollider.transform.root.gameObject;

            if (attacker != defender)          // Lots of self collisions happening, ignore those
            {
                print(attacker);
                print(defender);
                AttackResolution(attacker, defender);
            }
        }

        if (collision.collider.gameObject.CompareTag("Leg"))
        {
            // Still include inactiveshield when it's a leg.
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

    private bool _collisionIsBetweenDifferentPlayers(Collision2D collision)
    {
        GameObject collOne = collision.collider.gameObject.transform.parent.gameObject;
        GameObject collTwo = collision.otherCollider.gameObject.transform.parent.gameObject;

        return collOne != collTwo;
    }
}