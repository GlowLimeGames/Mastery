using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionBehavior : MonoBehaviour {

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("Sword"))
        {
            GameObject attacker = collision.collider.gameObject.transform.parent.gameObject;
            GameObject defender = collision.otherCollider.gameObject.transform.parent.gameObject;
            if (attacker != defender)          // Lots of self collisions happening, ignore those
            {
                print(attacker.state);
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
