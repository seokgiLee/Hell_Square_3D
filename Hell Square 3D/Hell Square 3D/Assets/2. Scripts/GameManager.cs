using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject gameCam;
    public GameObject menuCam;
    public Player player;
    public Boss boss;
    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject nextStageZone;

    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntBoss;

    public Transform[] enemySpawnZones;
    public GameObject[] enemies;
    public List<int> enemyList;

    public GameObject gamePanel;
    public GameObject menuPanel;
    public GameObject overPanel;
    public Text maxScoreText;

    public Text scoreText;
    public Text stageText;
    public Text playTimeText;
    public Text playerHPText;
    public Text playerAmmoText;
    public Text playerCoinText;
    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weaponRImg;
    public Text enemyAText;
    public Text enemyBText;
    public Text enemyCText;
    public Text curScoreText;
    public Text highScoreText;

    public RectTransform bossHPGroup;
    public RectTransform bossHPBar;

    public AudioSource startSound;
    public AudioSource endSound;
    public AudioSource gameoverSound;

    void Awake()
    {
        enemyList = new List<int>();
        maxScoreText.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
    }

    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        player.gameObject.SetActive(true);
        nextStageZone.SetActive(true);
    }

    public void GameOver()
    {
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreText.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore");
        if(maxScore<player.score)
        {
            highScoreText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("MaxScore", player.score);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void StageStart()
    {
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        nextStageZone.SetActive(false);

        foreach(Transform zone in enemySpawnZones)
        {
            zone.gameObject.SetActive(true);
        }

        startSound.Play();
        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd()
    {
        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        nextStageZone.SetActive(true);

        foreach (Transform zone in enemySpawnZones)
        {
            zone.gameObject.SetActive(false);
        }

        endSound.Play();
        isBattle = false;
        stage++;
    }

    IEnumerator InBattle()
    {
        if (stage % 10 == 0)
        {
            enemyCntBoss++;
            GameObject istantEnemy = Instantiate(enemies[3], enemySpawnZones[0].position, enemySpawnZones[0].rotation);
            Enemy enemy = istantEnemy.GetComponent<Enemy>();
            enemy.target = player.transform;
            boss = istantEnemy.GetComponent<Boss>();
        }
        else
        {
            for (int i = 0; i < stage; i++)
            {
                int ran = Random.Range(0, 3);
                enemyList.Add(ran);

                switch (ran)
                {
                    case 0:
                        enemyCntA++;
                        break;
                    case 1:
                        enemyCntB++;
                        break;
                    case 2:
                        enemyCntC++;
                        break;
                }
            }

            while (enemyList.Count > 0)
            {
                int ranZone = Random.Range(0, 12);
                GameObject istantEnemy = Instantiate(enemies[enemyList[0]], enemySpawnZones[ranZone].position, enemySpawnZones[ranZone].rotation);
                Enemy enemy = istantEnemy.GetComponent<Enemy>();
                enemy.target = player.transform;
                enemy.gameManager = this;
                enemyList.RemoveAt(0);
                yield return new WaitForSeconds(1f);
            }
        }

        while (enemyCntA + enemyCntB + enemyCntC + enemyCntBoss > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(5f);
        boss = null;
        StageEnd();
    }

    void Update()
    {
        if(isBattle)
        {
            playTime += Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        // 상단 UI
        scoreText.text = string.Format("{0:n0}", player.score);
        stageText.text = "Stage " + stage;

        int hour = (int)(playTime / 3600);
        int min = (int)(playTime % 3600 / 60);
        int sec = (int)(playTime % 60);
        playTimeText.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) + ":" + string.Format("{0:00}", sec);

        // 플레이어 UI
        playerHPText.text = player.hp + " / " + player.maxHp;
        playerCoinText.text = string.Format("{0:n0}", player.coin);

        if (player.equipWeapon == null)
        {
            playerAmmoText.text = "- / " + player.ammo;
        }
        else if (player.equipWeapon.type == Weapon.Type.Melee)
        {
            playerAmmoText.text = "- / " + player.ammo;
        }
        else
        {
            playerAmmoText.text = player.equipWeapon.curAmmo + " / " + player.ammo;
        }

        // 무기 UI
        weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0);
        weaponRImg.color = new Color(1, 1, 1, player.grenade > 0 ? 1 : 0);

        // 몬스터 UI
        enemyAText.text = enemyCntA.ToString();
        enemyBText.text = enemyCntB.ToString();
        enemyCText.text = enemyCntC.ToString();

        // 보스 체력 UI
        if (boss != null)
        {
            bossHPGroup.anchoredPosition = Vector3.down * 25;
            bossHPBar.localScale = new Vector3((float)boss.curHp / boss.maxHp, 1, 1);
        }
        else
        {
            bossHPGroup.anchoredPosition = Vector3.up * 200;
        }
    }
}
