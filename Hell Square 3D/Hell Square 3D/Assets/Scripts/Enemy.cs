using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C };
    public Type enemyType;
    public int maxHp;
    public int curHp;
    public Transform target;
    public BoxCollider meleeArea;
    public bool isChase;
    public bool isAttack;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;
    Animator anim;
    MeshRenderer[] meshs;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim= GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        curHp = maxHp;

        Invoke("ChaseStart", 2f);
    }

    void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void FixedUpdate()
    {
        Targetting();
        FreezeVelocity();
    }

    void Targetting()
    {
        float targetRadius = 0;
        float targetRange = 0;

        switch(enemyType)
        {
            case Type.A:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;

            case Type.B:
                targetRadius = 1f;
                targetRange = 6f;
                break;

            case Type.C:

                break;
        }

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        if (rayHits.Length > 0 && !isAttack)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch(enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.3f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;
                break;

            case Type.B:
                foreach (MeshRenderer mesh in meshs)
                {
                    mesh.material.color = Color.yellow;
                }

                yield return new WaitForSeconds(0.3f);
                rigid.AddForce(transform.forward * 30, ForceMode.Impulse);
                meleeArea.enabled = true;
                gameObject.layer = 11;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;
                gameObject.layer = 10;
                foreach (MeshRenderer mesh in meshs)
                {
                    mesh.material.color = Color.white;
                }

                yield return new WaitForSeconds(1f);
                break;

            case Type.C:

                break;
        }

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);

    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
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
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");

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
