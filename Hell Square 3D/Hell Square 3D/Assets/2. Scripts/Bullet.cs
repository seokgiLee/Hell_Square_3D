using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public bool isBoss;
    public bool isMelee;
    public bool isRock;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor" && !isRock)
        {
            Destroy(gameObject, 3f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wall" && !isMelee)
        {
            Destroy(gameObject);
        }
    }
}
