using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHp;
    public int curHp;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponent<MeshRenderer>().material;
        curHp = maxHp;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHp -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec));
        }
        else if (other.tag == "Bullet")
        {
            Bullet weapon = other.GetComponent<Bullet>();
            curHp -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);
            StartCoroutine(OnDamage(reactVec));
        }
    }

    public void HitByGrenade(Vector3 explosionPosition)
    {
        curHp -= 100;
        Vector3 reactVec = transform.position - explosionPosition;
        StartCoroutine(OnDamage(reactVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade = false)
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHp > 0)
        {
            mat.color = Color.white;
            reactVec = reactVec.normalized;
            rigid.AddForce(reactVec * 5, ForceMode.Impulse);
        }
        else
        {
            mat.color = Color.gray;
            gameObject.layer = 11; // 다른 물체와 상호작용 X

            if(isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;
                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 8, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 20, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 8, ForceMode.Impulse);
            }

            Destroy(gameObject, 3);
        }
    }
}
