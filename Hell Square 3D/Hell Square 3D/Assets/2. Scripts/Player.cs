using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public float speed;
    public float jumpPower;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public GameObject grenadeObject;
    public Camera followCamera;
    public GameManager gameManager;

    public AudioSource walkSound;
    public AudioSource dodgeSound;
    public AudioSource swapSound;
    public AudioSource throwSound;
    public AudioSource coinSound;
    public AudioSource weaponSound;
    public AudioSource itemSound;
    public AudioSource hitSound;
    public AudioSource dieSound;

    public int hp;
    public int ammo;
    public int coin;
    public int grenade;
    public int maxHp;
    public int maxAmmo;
    public int maxCoin;
    public int maxGrenade;

    public int score;

    float hAxis;
    float vAxis;

    public bool pause;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool fDown;
    bool gDown;
    bool rDown;

    bool isDodge;
    bool isFireReady = true;
    bool isReload;
    bool isBorder;
    bool isDamage;
    bool isShop;
    bool isDead;

    Vector3 moveVec; // 이동방향

    Rigidbody rigid;
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject; // 얻을 아이템
    public Weapon equipWeapon; // 장착한 아이템
    float fireDelay;
    public float reloadTime;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs= GetComponentsInChildren<MeshRenderer>();

        PlayerPrefs.SetInt("MaxScore", score);
    }

    void Update()
    {
        if (!isDead && !pause)
        {
            GetInput(); // 입력
            Move(); // 이동
            Turn(); // 방향전환
            Dodge(); // 회피
            Interaction(); // 상호작용
            Swap(); // 무기교체
            Attack(); // 공격
            Grenade(); // 수류탄
            if (rDown)
            {
                Reload(); // 재장전
            }
        }
    }

    void GetInput() // 입력
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
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
            // 달리기
            transform.position += moveVec * speed * Time.deltaTime;
        }

        // 걷기, 달리기 애니메이션
        anim.SetBool("isRun", moveVec != Vector3.zero);
        if (!walkSound.isPlaying && !isDodge)
        {
            walkSound.Play();
        }

        if (moveVec == Vector3.zero)
        {
            walkSound.Stop();
        }
    }

    void Turn() // 방향전환
    {
        // 키보드
        transform.LookAt(transform.position + moveVec);

        // 마우스
        if (fDown && !isShop)
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

    void Dodge() // 회피
    {
        if (jDown && !isDodge)
        {
            isDodge = true;
            if (moveVec == Vector3.zero && isDodge)
            {
                transform.DOLocalMove(transform.position + transform.forward * 10, 1.6f / 3).SetEase(Ease.OutSine);
                Invoke("DodgeFalse", 0.5f);
            }
            else
            {
                speed *= 2;
                Invoke("DodgeEnd", 0.5f);
            }
            anim.SetTrigger("doDodge");
            dodgeSound.Play();
        }
    }

    void DodgeEnd()
    {
        speed /= 2;
        DodgeFalse();
    }
    void DodgeFalse()
    {
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

                if (hasWeapons[weaponIndex])
                {
                    ammo += 30;
                    if (ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                }
                else
                {
                    hasWeapons[weaponIndex] = true;
                    if (equipWeapon != null)
                    {
                        equipWeapon.gameObject.SetActive(false);
                    }
                    equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
                    weapons[weaponIndex].SetActive(true);
                }
                weaponSound.Play();
                Destroy(nearObject);
            }
            else if (nearObject.tag == "Shop")
            {
                isShop = true;
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
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

        if ((sDown1 || sDown2 || sDown3) && !isReload)
        {
            if (equipWeapon != null)
            {
                equipWeapon.gameObject.SetActive(false);
            }
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            weapons[weaponIndex].SetActive(true);
            swapSound.Play();
        }
    }

    void Attack() // 공격
    {
        if (equipWeapon == null)
        {
            return;
        }

        if(equipWeapon.curAmmo == 0)
        {
            Reload();
            return;
        }

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isShop && !isReload)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Grenade() // 수류탄
    {
        if (grenade < 1)
        {
            return;
        }

        if(gDown && !isReload && !isDodge && !isShop)
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
            throwSound.Play();
        }
    }

    void Reload() // 재장전
    {
        if(equipWeapon == null || equipWeapon.type == Weapon.Type.Melee || ammo == 0)
        {
            return;
        }
        if (!isDodge && isFireReady && !isShop && !isReload)
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
                    itemSound.Play();
                    break;
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                    itemSound.Play();
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                    {
                        coin = maxCoin;
                    }
                    coinSound.Play();
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
                    itemSound.Play();
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

        if (hp <= 0 && !isDead)
        {
            dieSound.Play();
            OnDie();
        }
        else
        {
            hitSound.Play();
        }

        yield return new WaitForSeconds(1f);
        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
    }

    void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        gameObject.layer = 13;
        gameManager.GameOver();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")
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
        else if (other.tag == "Shop")
        {
            isShop = false;
            if (nearObject != null)
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Exit();
            }
            nearObject = null;
        }
    }
}
