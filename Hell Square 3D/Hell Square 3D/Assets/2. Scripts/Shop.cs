using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public RectTransform uiGroup;
    public Animator anim;

    public GameObject[] item;
    public int[] itemPrice;
    public Transform[] itemPos;
    public Text talkText;
    public string talkData;

    Player enterPlayer;
    bool isShop;

    public AudioSource clickSound;
    public AudioSource errorSound;
    public AudioSource enterSound;
    public AudioSource exitSound;

    public Button[] weaponBuyButtons;

    public void Enter(Player player)
    {
        if (!isShop)
        {
            isShop = true;
            anim.SetTrigger("doHello");
            enterPlayer = player;
            uiGroup.anchoredPosition = Vector3.zero;
            enterSound.Play();
        }
    }

    public void Exit()
    {
        if (isShop)
        {
            isShop = false;
            anim.SetTrigger("doHello");
            uiGroup.anchoredPosition = Vector3.down * 1000;
            exitSound.Play();
        }
    }

    public void Buy(int index)
    {
        int pirce = itemPrice[index];

        if (pirce > enterPlayer.coin)
        {
            StopAllCoroutines();
            StartCoroutine(Talk());
            return;
        }
        enterPlayer.coin -= pirce;
        clickSound.Play();

        Vector3 ranVec = Vector3.right * Random.Range(-3, 3) + Vector3.forward * Random.Range(-3, 3);
        Instantiate(item[index], itemPos[index].position + ranVec, itemPos[index].rotation);

        weaponBuyButtons[index].interactable = false;
    }

    IEnumerator Talk()
    {
        talkText.text = "돈이 부족합니다!";
        yield return new WaitForSeconds(2f);
        talkText.text = talkData;
    }
}
