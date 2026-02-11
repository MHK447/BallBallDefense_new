using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;

public class PlayerBulletBase : MonoBehaviour
{
    [SerializeField]
    private ColliderAction ColAction;

    private float MoveSpeed = 10f;

    private EnemyUnit TargetEnemy = null;

    private PlayerUnit PlayerUnit = null;

    private Vector3 moveDirection = Vector3.zero;

    private bool isReturningToPlayer = false;

    private System.Action<PlayerBulletBase> DeleteAction = null;

    [SerializeField]
    private TrailComponent TrailComponent;

    private Rigidbody2D rb;
    
    private Collider2D lastCollider = null;
    private float lastCollisionTime = 0f;
    private const float collisionCooldown = 0.1f; // 같은 충돌체와 재충돌 방지 시간

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Rigidbody2D 설정 (충돌 감지를 위해 필요)
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // 중력 영향 제거
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        if (TrailComponent != null)
        {
            TrailComponent.InitTrail(Color.white, 12);
        }
    }

    public void Set(EnemyUnit targetenemy, PlayerUnit unit, System.Action<PlayerBulletBase> deleteaction)
    {
        TargetEnemy = targetenemy;
        PlayerUnit = unit;
        isReturningToPlayer = false;
        lastCollider = null;
        lastCollisionTime = 0f;


        // 타겟 방향으로 초기 방향 설정 (적이 죽거나 이동해도 이 방향 유지)
        if (targetenemy != null)
        {
            moveDirection = (targetenemy.transform.position - transform.position).normalized;
        }

        DeleteAction = deleteaction;

        ColAction.CollisionEnterAction = OnCollisionEnter2D;
    }

    // 방향을 직접 받는 Set 메서드
    public void Set(Vector3 direction, PlayerUnit unit, System.Action<PlayerBulletBase> deleteaction)
    {
        TargetEnemy = null;
        PlayerUnit = unit;
        isReturningToPlayer = false;
        moveDirection = direction.normalized;
        lastCollider = null;
        lastCollisionTime = 0f;

        DeleteAction = deleteaction;

        ColAction.CollisionEnterAction = OnCollisionEnter2D;
    }

    void Update()
    {
        Move();
    }


    public void Move()
    {
        if (rb == null)
        {
            // Rigidbody2D가 없으면 transform으로 이동
            if (isReturningToPlayer && PlayerUnit != null)
            {
                Vector3 directionToPlayer = (PlayerUnit.transform.position - transform.position).normalized;
                transform.position += directionToPlayer * MoveSpeed * Time.deltaTime;

                float distanceToPlayer = Vector3.Distance(transform.position, PlayerUnit.transform.position);
                if (distanceToPlayer < 0.1f)
                {
                    DeleteBullet();
                }
            }
            else if (moveDirection != Vector3.zero)
            {
                transform.position += moveDirection * MoveSpeed * Time.deltaTime;
            }
        }
        else
        {
            // Rigidbody2D가 있으면 velocity로 이동 (물리 기반)
            if (isReturningToPlayer && PlayerUnit != null)
            {
                Vector3 directionToPlayer = (PlayerUnit.transform.position - transform.position).normalized;
                rb.linearVelocity = directionToPlayer * MoveSpeed;

                float distanceToPlayer = Vector3.Distance(transform.position, PlayerUnit.transform.position);
                if (distanceToPlayer < 0.1f)
                {
                    DeleteBullet();
                }
            }
            else if (moveDirection != Vector3.zero)
            {
                rb.linearVelocity = moveDirection * MoveSpeed;
            }
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        // 같은 충돌체와의 재충돌 방지 (쿨다운 체크)
        if (collision.collider == lastCollider && Time.time - lastCollisionTime < collisionCooldown)
        {
            return;
        }
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            EnemyUnit enemy = collision.gameObject.GetComponent<EnemyUnit>();
            if (enemy != null)
            {
                enemy.Damage(PlayerUnit.PlayerUnitInfoData.AttackDamage);
                // Unity가 제공하는 정확한 법선 벡터 사용
                if (collision.contactCount > 0)
                {
                    Vector2 normal = collision.contacts[0].normal;
                    Vector2 reflectDir = Vector2.Reflect(moveDirection, normal);
                    moveDirection = reflectDir.normalized;
                    
                    // Rigidbody2D velocity도 즉시 업데이트
                    if (rb != null)
                    {
                        rb.linearVelocity = moveDirection * MoveSpeed;
                    }
                    
                    // 충돌 지점에서 충분히 밀어내어 재충돌 방지 (0.01f -> 0.15f로 증가)
                    transform.position = (Vector2)transform.position + normal * 0.15f;
                    
                    // 마지막 충돌 정보 저장
                    lastCollider = collision.collider;
                    lastCollisionTime = Time.time;
                }
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // Unity가 제공하는 정확한 충돌 법선 벡터 사용
            if (collision.contactCount > 0)
            {
                Vector2 normal = collision.contacts[0].normal;
                Vector2 reflectDir = Vector2.Reflect(moveDirection, normal);
                moveDirection = reflectDir.normalized;
                
                // Rigidbody2D velocity도 즉시 업데이트
                if (rb != null)
                {
                    rb.linearVelocity = moveDirection * MoveSpeed;
                }
                
                // 충돌 지점에서 충분히 밀어내어 벽 통과 방지 (0.01f -> 0.1f로 증가)
                transform.position = (Vector2)transform.position + normal * 0.1f;
                
                // 마지막 충돌 정보 저장
                lastCollider = collision.collider;
                lastCollisionTime = Time.time;
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("BottomWall"))
        {
            // PlayerUnit으로 돌아가도록 설정
            isReturningToPlayer = true;
            moveDirection = Vector3.zero; // 기존 방향은 더 이상 사용하지 않음
            
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // velocity 초기화
            }
        }
    }

    public void DeleteBullet()
    {
        PlayerUnit.PlayerUnitInfoData.InBaseBallCount.Value += 1;
        ProjectUtility.SetActiveCheck(this.gameObject, false);
        DeleteAction?.Invoke(this);
    }

}

