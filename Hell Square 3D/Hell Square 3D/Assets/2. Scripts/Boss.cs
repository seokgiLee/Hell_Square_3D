using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePortA, missilePortB;

    Vector3 lookVec; // 플레이어 방향예측
    Vector3 tauntVec; // 내려찍기
    public bool isLook; // 플레이어 바라보기

    public AudioSource rockSound;
    public AudioSource jumpSound;
    public AudioSource landSound;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        curHp = maxHp;

        nav.isStopped = true;
        StartCoroutine(Think());
    }

    void Update()
    {
        if(isDead)
        {
            StopAllCoroutines();
            return;
        }

        if(isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 3f;
            transform.LookAt(target.position + lookVec);
        }
        else
        {
            nav.SetDestination(tauntVec);
        }
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(1f);

        int ranAction = Random.Range(0, 5);

        switch(ranAction)
        {
            case 0:
            case 1: // 미사일
                StartCoroutine(MissileShot());
                break;
            case 2:
            case 3: // 돌
                StartCoroutine(RockShot());
                break;
            case 4: // 내려찍기
                StartCoroutine(Taunt());
                break;
        }
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossBullet bossMissileA = instantMissileA.GetComponent<BossBullet>();
        bossMissileA.target = target;
        attackSound.Play();

        yield return new WaitForSeconds(0.5f);
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossBullet bossMissileB = instantMissileB.GetComponent<BossBullet>();
        bossMissileB.target = target;
        attackSound.Play();

        yield return new WaitForSeconds(2f);

        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");
        Instantiate(enemyBullet, transform.position, transform.rotation);
        rockSound.Play();

        yield return new WaitForSeconds(3f);

        isLook = true;
        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec;
        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");
        jumpSound.Play();

        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;
        landSound.Play();

        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(1f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;

        StartCoroutine(Think());
    }
}
