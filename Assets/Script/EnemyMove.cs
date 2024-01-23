using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator animator;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider2D;

    public int nextMove;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>(); 
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();

        Think();
    }

    void FixedUpdate()
    {
        // 이동
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        // Platform 확인
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.2f, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform")); // Ray에 닿은 오브젝트 중 Platform 레이어를 가진 오브젝트만 반환함

        if (rayHit.collider == null && !spriteRenderer.flipY) // 빔에 맞은 것이 없다면 (낭떠러지라면)
            Turn();
    }

    // 재귀
    void Think()
    {
        nextMove = Random.Range(-1, 2);

        animator.SetInteger("walkSpeed", nextMove);

        if(nextMove != 0)
            spriteRenderer.flipX = nextMove == 1;

        float nextThinkTime = Random.Range(2f, 6f);
        Invoke("Think", nextThinkTime); // 2초~6초 뒤에 실행하도록 하는 함수
    }

    void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove == 1;

        CancelInvoke(); // 작동 중인 모든 Invoke 함수를 멈춤
        Invoke("Think", 3);
    }

    void DeActive()
    {
        gameObject.SetActive(false);
    }

    public void OnDamaged() // 몬스터가 데미지를 받았을 때
    {
        capsuleCollider2D.enabled = false;

        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        spriteRenderer.flipY = true;

        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        Invoke("DeActive", 2);
    }
}
