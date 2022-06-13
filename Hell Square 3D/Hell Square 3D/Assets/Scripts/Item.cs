using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon };
    public Type type;
    public int value;

    float height;
    float top, bottom;
    float direction = 0.5f;

    void Awake()
    {
        height = transform.position.y;
        top = height + 0.5f;
        bottom = height - 0.5f;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * 10 * Time.deltaTime);
        height += Time.deltaTime * direction;

        if (height > top || height < bottom)
        {
            direction *= -1;
        }

        transform.position = new Vector3(transform.position.x, height, transform.position.z);
    }
}
