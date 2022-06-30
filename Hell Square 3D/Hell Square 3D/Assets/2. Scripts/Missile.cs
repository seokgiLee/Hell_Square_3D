using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public int speed;

    void Update()
    {
        transform.Rotate(Vector3.right * speed * Time.deltaTime);
    }
}
