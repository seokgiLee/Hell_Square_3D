using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, Boss };
    public Type enemyType;
    public int maxHp;
    public int curHp;
    public int score;
    public GameManager gameManager;
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject enemyBullet;
    public GameObject[] coins;
    public bool isChase;
    public bool isAttack;
    public bool isDead;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public NavMeshAgent nav;
    public Animator anim;
    public MeshRenderer[] meshs;

    public AudioSource attackSound;
    public AudioSource hitSound;
    public AudioSource dieSound;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = GetComponent<NavMeshAgent>();
        anim= GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        curHp = maxHp;

        if (enemyType != Type.Boss)
        {
            Invoke("ChaseStart", 2f);
        }
    }

    void Update()
    {
        if (nav.enabled && enemyType != Type.Boss && target != null)
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
        if (target != null)
        {
            Targetting();
            FreezeVelocity();
        }
    }

    void Targetting()
    {
        if (target.gameObject.layer == 13)
        {
            target = null;
            anim.SetBool("isWalk", false);
        }

        if (enemyType != Type.Boss && !isDead)
        {
            float targetRadius = 0;
            float targetRange = 0;

            switch (enemyType)
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
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward * 0.5f, targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.3f);
                meleeArea.enabled = true;
                attackSound.Play();

                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = false;
                break;

            case Type.B:
                foreach (MeshRenderer mesh in meshs)
                {
                    mesh.material.color = Color.cyan;
                }

                yield return new WaitForSeconds(0.3f);
                rigid.AddForce(transform.forward * 30, ForceMode.Impulse);
                meleeArea.enabled = true;
                gameObject.layer = 11;
                attackSound.Play();

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
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(enemyBullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;
                attackSound.Play();

                yield return new WaitForSeconds(2f);
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
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }
        yield return new WaitForSeconds(0.1f);

        if (curHp > 0)
        {
            foreach (MeshRenderer mesh in meshs)
            {
                mesh.material.color = Color.white;
            }
            reactVec = reactVec.normalized;
            rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            hitSound.Play();
        }
        else
        {
            foreach (MeshRenderer mesh in meshs)
            {
                mesh.material.color = Color.gray;
            }
            gameObject.layer = 11; // 다른 물체와 상호작용 X

            if (!isDead)
            {
                switch (enemyType)
                {
                    case Type.A:
                        for (int i = 0; i < Random.Range(1, 3); i++)
                        {
                            Instantiate(coins[0], transform.position + new Vector3(Random.Range(0, 3), 0, Random.Range(0, 3)), Quaternion.identity);
                        }
                        gameManager.enemyCntA--;
                        break;
                    case Type.B:
                        for (int i = 0; i < Random.Range(3, 6); i++)
                        {
                            Instantiate(coins[0], transform.position + new Vector3(Random.Range(0, 3), 0, Random.Range(0, 3)), Quaternion.identity);
                        }
                        gameManager.enemyCntB--;
                        break;
                    case Type.C:
                        Instantiate(coins[1], transform.position, Quaternion.identity);
                        for (int i = 0; i < Random.Range(3, 6); i++)
                        {
                            Instantiate(coins[0], transform.position + new Vector3(Random.Range(0, 3), 0, Random.Range(0, 3)), Quaternion.identity);
                        }
                        gameManager.enemyCntC--;
                        break;
                    case Type.Boss:
                        for (int i = 0; i < 10; i++)
                        {
                            Instantiate(coins[2], transform.position + new Vector3(Random.Range(0, 3), 0, Random.Range(0, 3)), Quaternion.identity);
                        }
                        gameManager.enemyCntBoss--;
                        break;
                }
            }

            isDead = true;
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");
            Player player = target.GetComponent<Player>();
            player.score += score;
            dieSound.Play();

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
