using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;

    public PlayerMove player;
    public GameObject[] Stages;
    public BoxCollider2D boxCollider2D;

    public Image[] UIHealth;
    public Text UIScore;
    public Text UIStage;
    public GameObject RetryButton;
    public GameObject Congratulations;

    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        UIScore.text = (totalPoint + stagePoint).ToString();
    }

    public void NextStage()
    { 
        if (stageIndex < Stages.Length-1) // 스테이지의 남은 갯수가 있다면
        { // 스테이지 이동
            Stages[stageIndex++].SetActive(false);
            Stages[stageIndex].SetActive(true);
            PlayerReposition();

            UIStage.text = "STAGE " + (stageIndex + 1);

            if (stageIndex == 4)
                boxCollider2D.offset = new Vector2(0, -30);
        }
        else // 마지막 스테이지라면
        {
            Time.timeScale = 0;
            Text buttonText = RetryButton.GetComponentInChildren<Text>();
            buttonText.text = "RESTART?";
            RetryButton.SetActive(true);
            Congratulations.SetActive(true);
        }

        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        health--;
        UIHealth[health].color = new Color(1, 0, 0, 0.2f);
        if (health < 1) { // health가 0일 경우
            player.OnDie();
            Time.timeScale = 0;
            
            RetryButton.SetActive(true);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            player.OnDamaged();

            if (health > 0)
                PlayerReposition();
        }
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(0, 0, -1);
        player.VelocityZero();
    }

    public void Retry()
    {
        Time.timeScale = 1; // 시간 복구
        SceneManager.LoadScene(0);
    }
}
