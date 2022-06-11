using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public float speed;
    public float jumpPower;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;


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

    bool isJump;
    bool isDodge;

    Vector3 moveVec; // 이동방향

    Rigidbody rigid;
    Animator anim;

    GameObject nearObject; // 얻을 아이템
    GameObject equipWeapon; // 장착한 아이템

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
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
    }

    void Move() // 이동
    {
        if (!isDodge) // 회피 중 방향전환 방지
        {
            moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        }

        if (wDown) // 걷기
        {
            transform.position += moveVec * speed * 0.3f * Time.deltaTime;
        }
        else // 달리기
        {
            transform.position += moveVec * speed * Time.deltaTime;
        }

        // 걷기, 달리기 애니메이션
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn() // 방향전환
    {
        transform.LookAt(transform.position + moveVec);
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
                ItemManager item = nearObject.GetComponent<ItemManager>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                if (equipWeapon != null)
                {
                    equipWeapon.SetActive(false);
                }
                equipWeapon = weapons[weaponIndex];
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
                equipWeapon.SetActive(false);
            }
            equipWeapon = weapons[weaponIndex];
            weapons[weaponIndex].SetActive(true);
        }
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
            ItemManager item = other.GetComponent<ItemManager>();
            switch(item.type)
            {
                case ItemManager.Type.Heart:
                    hp += item.value;
                    if(hp>maxHp)
                    {
                        hp = maxHp;
                    }
                    break;
                case ItemManager.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                    break;
                case ItemManager.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                    {
                        coin = maxCoin;
                    }
                    break;
                case ItemManager.Type.Grenade:
                    if (grenade == 2)
                    {
                        grenades[1].SetActive(false);
                        grenades[4].SetActive(true);
                        grenades[5].SetActive(true);
                    }
                    else if (grenade == maxGrenade)
                    {
                        grenades[1].SetActive(true);
                        grenades[2].SetActive(true);
                        grenades[3].SetActive(true);
                        grenades[4].SetActive(false);
                        grenades[5].SetActive(false);
                    }
                    else
                    {
                        grenades[grenade].SetActive(true);
                    }
                    grenade += item.value;
                    if (grenade > maxGrenade)
                    {
                        grenade = maxGrenade;
                    }
                    break;
            }
            Destroy(other.gameObject);
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
