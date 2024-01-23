using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public float maxSpeed;
    public float jumpPower;

    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;
    CapsuleCollider2D capsuleCollider2D;
    AudioSource audioSource;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void PlaySound(string action)
    {
        switch(action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
        }
        audioSource.Play();
    }

    void Update()
    {
        // 점프
        if (Input.GetButtonUp("Jump") && !animator.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            animator.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        // 키를 떼었을 때 멈추게 하기
        if (Input.GetButtonUp("Horizontal")) 
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y); // normalized : 벡터 크기를 1로 만든 상태 (단위벡터)

        // 방향 전환
        if (Input.GetButton("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        // 움직이는 애니메이션 설정
        if (Mathf.Abs(rigid.velocity.x) < 0.4)
            animator.SetBool("isWalking", false);
        else
            animator.SetBool("isWalking", true);
    }

    void FixedUpdate()
    {
        // 이동하기
        float h = Input.GetAxisRaw("Horizontal");        

        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if(rigid.velocity.x > maxSpeed) // 오른쪽 속도 제한
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity. x < maxSpeed*(-1)) // 왼쪽 속도 제한
            rigid.velocity = new Vector2(maxSpeed*(-1), rigid.velocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform") // 바닥에 닿을 때 점프를 다시 할 수 있도록 함
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform")); // Ray에 닿은 오브젝트 중 Platform 레이어를 가진 오브젝트만 반환함
            if (rayHit.collider != null) // 빔에 맞은 것이 있다면
                if (rayHit.distance < 1f)
                    animator.SetBool("isJumping", false);
        }

        if (collision.gameObject.layer == 7)
        {
            if (gameObject.layer == 8) // Enemy layer와 충돌하고 무적이 아닐 때
            {
                if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y
                    && collision.gameObject.tag == "Enemy") // Enemy를 밟았을 경우
                {
                    OnAttack(collision.transform);
                }
                else // 밟지 않고 충돌했을 경우
                    OnDamaged(collision.transform.position);
            }
            else // Enemy layer와 충돌하고 무적일 때
            {
                if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y
                   && collision.gameObject.tag == "Enemy") // Enemy를 밟았을 경우
                {
                    OnAttack(collision.transform);
                }
            }
        }
    }

    public void OnDamaged(Vector2 targetPosition)
    {
        PlaySound("DAMAGED");
        gameManager.HealthDown();

        gameObject.layer = 9; // playerDamaged layer로 변경 (무적 시간으로 변경)

        spriteRenderer.color = new Color(1, 1, 1, 0.4f); // 투명하게 변경

        // 튕기는 방향 설정
        int dirc = (transform.position.x - targetPosition.x) > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7f, ForceMode2D.Impulse);

        animator.SetTrigger("Damaged");

        Invoke("OffDamaged", 3); // 3초후에 되돌림
    }

    public void OnDamaged()
    {
        PlaySound("DAMAGED");
        gameManager.HealthDown();

        gameObject.layer = 9; // playerDamaged layer로 변경 (무적 시간으로 변경)

        spriteRenderer.color = new Color(1, 1, 1, 0.4f); // 투명하게 변경

        Invoke("OffDamaged", 3); // 3초후에 되돌림
    }

    void OffDamaged()
    {
        gameObject.layer = 8;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    void OnAttack(Transform enemy)
    {
        PlaySound("ATTACK");
        gameManager.stagePoint += 100;

        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse); // 충돌 시 플레이어에게도 반발력 주

        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Item")
        {
            PlaySound("ITEM");
            bool isBronze = collision.gameObject.name.Contains("Bronze"); // 이름에 Bronze를 포함하고 있는 것을 확인함
            bool isSilver = collision.gameObject.name.Contains("Silver"); 
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSilver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 300;

            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.tag == "Finish") 
        {
            PlaySound("FINISH");
            gameManager.NextStage();
        }
    }

    public void OnDie()
    {
        PlaySound("DIE");
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        spriteRenderer.flipY = true;

        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
 
        capsuleCollider2D.enabled = false;
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero; // 낙하속도 0으로 만들기
    }
}
