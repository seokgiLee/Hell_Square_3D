using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float jumpPower;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public GameObject grenadeObject;
    public Camera followCamera;

    public int hp;
    public int ammo;
    public int coin;
    public int grenade;
    public int maxHp;
    public int maxAmmo;
    public int maxCoin;
    public int maxGrenade;

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool fDown;
    bool gDown;
    bool rDown;

    bool isJump;
    bool isDodge;
    bool isFireReady = true;
    bool isReload;
    bool isBorder;
    bool isDamage;

    Vector3 moveVec; // 이동방향

    Rigidbody rigid;
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject; // 얻을 아이템
    Weapon equipWeapon; // 장착한 아이템
    float fireDelay;
    public float reloadTime;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs= GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        GetInput(); // 입력
        Move(); // 이동
        Turn(); // 방향전환
        Jump(); // 점프
        Dodge(); // 회피
        Interaction(); // 상호작용
        Swap(); // 무기교체
        Attack(); // 공격
        Grenade(); // 수류탄
        Reload(); // 재장전
    }

    void GetInput() // 입력
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButton("Reload");
    }

    void Move() // 이동
    {
        if (!isDodge) // 회피 중 방향전환 방지
        {
            moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        }

        if(!isFireReady || isReload) // 공격시 정지
        {
            moveVec = Vector3.zero;
        }

        if (!isBorder) // 경계에서 멈추기
        {
            if (wDown) // 걷기
            {
                transform.position += moveVec * speed * 0.3f * Time.deltaTime;
            }
            else // 달리기
            {
                transform.position += moveVec * speed * Time.deltaTime;
            }
        }

        // 걷기, 달리기 애니메이션
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn() // 방향전환
    {
        // 키보드
        transform.LookAt(transform.position + moveVec);

        // 마우스
        if (fDown)
        {
            // 바라보는 방향으로 회전
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump() // 점프
    {
        if (jDown && !isJump && moveVec == Vector3.zero && !isDodge)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge() // 회피
    {
        if (jDown && !isJump && moveVec != Vector3.zero && !isDodge)
        {
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeEnd", 0.5f);
        }
    }

    void DodgeEnd()
    {
        speed /= 2;
        isDodge = false;
    }

    void Interaction() // 상호작용
    {
        if (iDown && nearObject != null)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                if (equipWeapon != null)
                {
                    equipWeapon.gameObject.SetActive(false);
                }
                equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
                weapons[weaponIndex].SetActive(true);
                Destroy(nearObject);
            }
        }
    }

    void Swap() // 무기교체
    {
        int weaponIndex = -1;

        if (sDown1)
        {
            weaponIndex = 0;
        }
        if (sDown2)
        {
            weaponIndex = 1;
        }
        if (sDown3)
        {
            weaponIndex = 2;
        }

        if(weaponIndex < 0 || !hasWeapons[weaponIndex]) // 무기가 없거나 같은 무기로 교체할때 아무일 없음
        {
            return;
        }

        if (sDown1 || sDown2 || sDown3)
        {
            if (equipWeapon != null)
            {
                equipWeapon.gameObject.SetActive(false);
            }
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            weapons[weaponIndex].SetActive(true);
        }
    }

    void Attack() // 공격
    {
        if (equipWeapon == null)
        {
            return;
        }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Grenade() // 수류탄
    {
        if(grenade<1)
        {
            return;
        }

        if(gDown && !isReload && !isDodge)
        {
            // 바라보는 방향으로 던지기
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObject, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                grenade--;
                if (grenade == 3)
                {
                    grenades[2].SetActive(false);
                    grenades[3].SetActive(false);
                    grenades[4].SetActive(false);
                    grenades[5].SetActive(true);
                    grenades[6].SetActive(true);
                }
                else if (grenade == 2)
                {
                    grenades[2].SetActive(true);
                    grenades[5].SetActive(false);
                    grenades[6].SetActive(false);
                }
                else
                {
                    grenades[grenade + 1].SetActive(false);
                }
            }
        }
    }

    void Reload() // 재장전
    {
        if(equipWeapon == null || equipWeapon.type == Weapon.Type.Melee || ammo == 0)
        {
            return;
        }
        if(rDown)
        {
            Debug.Log(isFireReady);
        }
        if (rDown && !isDodge && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;
            Invoke("ReloadEnd", reloadTime);
        }
    }

    void ReloadEnd()
    {
        // 가지고 있는 탄창 또는 최대 탄창만큼 재장전
        int reAmmo = ammo + equipWeapon.curAmmo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo - equipWeapon.curAmmo;
        equipWeapon.curAmmo += reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 3, Color.green);
        isBorder = Physics.Raycast(transform.position, moveVec, 3, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        StopToWall();
    }

    void OnCollisionEnter(Collision collision) // 바닥착지감지
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Heart:
                    hp += item.value;
                    if (hp > maxHp)
                    {
                        hp = maxHp;
                    }
                    break;
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                    {
                        coin = maxCoin;
                    }
                    break;
                case Item.Type.Grenade:
                    grenade += item.value;
                    if (grenade > maxGrenade)
                    {
                        grenade = maxGrenade;
                    }
                    if (grenade == 3)
                    {
                        grenades[2].SetActive(false);
                        grenades[5].SetActive(true);
                        grenades[6].SetActive(true);
                    }
                    else if (grenade == maxGrenade)
                    {
                        grenades[2].SetActive(true);
                        grenades[3].SetActive(true);
                        grenades[4].SetActive(true);
                        grenades[5].SetActive(false);
                        grenades[6].SetActive(false);
                    }
                    else
                    {
                        grenades[grenade].SetActive(true);
                    }
                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            Bullet enemyAttack = other.GetComponent<Bullet>();
            if (!isDamage)
            {
                hp -= enemyAttack.damage;
                bool isBoss = enemyAttack.isBoss;

                StartCoroutine(OnDamage(isBoss, other.transform));
            }

            if (other.GetComponent<Rigidbody>() != null)
            {
                Destroy(enemyAttack.gameObject);
            }
        }
    }

    IEnumerator OnDamage(bool isBoss, Transform trans)
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }
        if(isBoss)
        {
            rigid.AddForce((transform.position - trans.position) * 2 + transform.up * 15, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(1f);
        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }
}
