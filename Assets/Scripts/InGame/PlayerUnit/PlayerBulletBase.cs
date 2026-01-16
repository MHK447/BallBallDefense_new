using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class PlayerBulletBase : MonoBehaviour
{
    private float MoveSpeed = 10f;

    private EnemyUnit TargetEnemy = null;

    private PlayerUnit PlayerUnit = null;

    private Vector3 moveDirection = Vector3.zero;

    private bool isReturningToPlayer = false;

    private System.Action<PlayerBulletBase> DeleteAction = null;

    [SerializeField]
    private TrailComponent TrailComponent;


    void Awake()
    {
        if(TrailComponent != null)
        {
            TrailComponent.InitTrail(Color.white, 12);
        }
    }

    public void Set(EnemyUnit targetenemy, PlayerUnit unit, System.Action<PlayerBulletBase> deleteaction)
    {
        TargetEnemy = targetenemy;
        PlayerUnit = unit;


        // 타겟 방향으로 초기 방향 설정 (적이 죽거나 이동해도 이 방향 유지)
        if (targetenemy != null)
        {
            moveDirection = (targetenemy.transform.position - transform.position).normalized;
        }

        DeleteAction = deleteaction;
    }

    void Update()
    {
        Move();
    }


    public void Move()
    {
        if (isReturningToPlayer && PlayerUnit != null)
        {
            // PlayerUnit으로 이동
            Vector3 directionToPlayer = (PlayerUnit.transform.position - transform.position).normalized;
            transform.position += directionToPlayer * MoveSpeed * Time.deltaTime;

            // PlayerUnit에 도착했는지 확인 (거리가 0.1f 이하면 도착으로 간주)
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            EnemyUnit enemy = collision.gameObject.GetComponent<EnemyUnit>();
            if (enemy != null)
            {
                enemy.Damage(PlayerUnit.PlayerUnitInfoData.AttackDamage);
                // 충돌 지점의 법선 벡터를 사용하여 반사 방향 계산
                ContactPoint2D contact = collision.contacts[0];
                Vector2 reflectDir = Vector2.Reflect(moveDirection, contact.normal);
                moveDirection = reflectDir.normalized;
            }
        }
        // Wall layer와 충돌 시 반사
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // 충돌 지점의 법선 벡터를 사용하여 반사 방향 계산
            ContactPoint2D contact = collision.contacts[0];
            Vector2 reflectDir = Vector2.Reflect(moveDirection, contact.normal);
            moveDirection = reflectDir.normalized;
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("BottomWall"))
        {
            // PlayerUnit으로 돌아가도록 설정
            isReturningToPlayer = true;
            moveDirection = Vector3.zero; // 기존 방향은 더 이상 사용하지 않음
        }
    }

    public void DeleteBullet()
    {
        PlayerUnit.PlayerUnitInfoData.InBaseBallCount.Value += 1;
        ProjectUtility.SetActiveCheck(this.gameObject, false);
        DeleteAction?.Invoke(this);
    }

}

