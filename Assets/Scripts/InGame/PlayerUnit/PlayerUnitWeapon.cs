using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class PlayerUnitWeapon : MonoBehaviour
{
    [SerializeField]
    private Transform BulletFireTr;

    [SerializeField]
    private SpriteRenderer WeaponImg;

    [SerializeField]
    private PlayerUnit PlayerUnit;

    [SerializeField]
    private GameObject BulletPrefab;

    [SerializeField]
    private WeaponDirectionController WeaponDirectionController;

    public PrefabPool<PlayerBulletBase> BulletPool = new();

    private List<PlayerBulletBase> ActiveBullets = new List<PlayerBulletBase>();

    private int WeaponIdx = 0;


    private InGameBaseStage BaseStage;

    [HideInInspector]
    public EnemyUnit TargetEnemy = null;

    private Coroutine BulletWeaponCoroutine = null;

    // 최대 반사 횟수 설정
    [SerializeField]
    private int maxReflections = 1;


    // 마지막 반사 충돌이 Enemy인지 여부
    [HideInInspector]
    public bool IsLastReflectionEnemy = false;

    // 마지막 반사 Enemy의 위치
    [HideInInspector]
    public Vector2 LastEnemyPosition = Vector2.zero;


    void Awake()
    {
        BaseStage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;
    }

    public void Set(int weaponidx)
    {
        WeaponIdx = weaponidx;

        TargetEnemy = null;

        if (BulletWeaponCoroutine != null)
        {
            GameRoot.Instance.StopCoroutine(BulletWeaponCoroutine);
        }

        BulletWeaponCoroutine = GameRoot.Instance.StartCoroutine(StartBulletWeapon());

        WeaponDirectionController.Init();

        OnSpawn();

    }



    public IEnumerator StartBulletWeapon()
    {
        while (true)
        {
            if (TargetEnemy == null || TargetEnemy.IsDead)
            {
                EnemyFindTarget();
            }

            if (TargetEnemy != null && PlayerUnit.PlayerUnitInfoData.InBaseBallCount.Value > 0)
            {
                FireBullet();
                PlayerUnit.PlayerUnitInfoData.InBaseBallCount.Value -= 1;
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void EnemyFindTarget()
    {
        var findenemytarget = BaseStage.EnemyUnitGroup.FindEnemyTarget(transform.position, PlayerUnit.PlayerUnitInfoData.AttackRange);
        if (findenemytarget != null)
        {
            TargetEnemy = findenemytarget;
        }
    }



    protected virtual void OnSpawn()
    {
        BulletPool.Init(BulletPrefab, null, 4);
        ActiveBullets.Clear();
    }

    void Update()
    {
        if (TargetEnemy != null)
        {
            transform.position = TargetEnemy.transform.position;

            // WeaponImg가 TargetEnemy를 바라보게 함
            Vector3 direction = TargetEnemy.transform.position - WeaponImg.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            WeaponImg.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }


    protected virtual void FireBullet()
    {
        if (PlayerUnit.IsDead) return;

        if (TargetEnemy == null) return;


        PlayerBulletBase instance = BulletPool.Get();
        if (instance == null) return;

        instance.transform.position = BulletFireTr.transform.position;
        instance.Set(TargetEnemy, PlayerUnit, OnBulletHit);
    }

    protected virtual void OnBulletHit(PlayerBulletBase bullet)
    {
        ActiveBullets.Remove(bullet);
        BulletPool.Return(bullet);
    }





    public List<Vector2> GetFuturePath(List<Vector2> path, Vector2 inLinearVelocity)
    {
        float deltaTime = 0.05f; // 더 큰 단계로 변경하여 경로 점 간격 증가
        float maxDistance = 100f; // 최대 거리 증가
        int maxSteps = 150;
        path.Clear();

        Vector2 lastPos = BulletFireTr.position;
        Vector2 lastVelocity = inLinearVelocity;

        LayerMask wallLayer = LayerMask.GetMask("Wall");
        LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        LayerMask bottomwall = LayerMask.GetMask("BottomWall");
        LayerMask collisionLayers = wallLayer | enemyLayer | bottomwall;

        // 디버그: 레이어 마스크 확인
        if (collisionLayers.value == 0)
        {
            Debug.LogWarning("WeaponBase: Wall 또는 Enemy 레이어가 존재하지 않습니다!");
        }

        float totalDistance = 0f;
        int stepCount = 0;
        int reflectionCount = 0;

        // 마지막 반사 정보 초기화
        IsLastReflectionEnemy = false;
        LastEnemyPosition = Vector2.zero;

        // 시작점 추가
        path.Add(lastPos);

        while (totalDistance < maxDistance && stepCount < maxSteps && reflectionCount < maxReflections)
        {
            Vector2 nextPos = lastPos + lastVelocity * deltaTime;
            Vector2 direction = nextPos - lastPos;
            float stepDistance = direction.magnitude;

            // stepDistance가 0에 가까우면 루프 종료 (속도가 0에 가까울 때)
            if (stepDistance < 0.001f)
            {
                break;
            }

            RaycastHit2D hit = Physics2D.Raycast(lastPos, direction.normalized, stepDistance, collisionLayers);

            if (hit.collider != null)
            {
                // Enemy와 충돌한 경우 - 해당 지점을 추가하고 종료
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    path.Add(hit.point);
                    IsLastReflectionEnemy = true;
                    LastEnemyPosition = hit.point;

                    // 디버그 로그 (필요시 주석 해제)
                    // Debug.Log($"Enemy 충돌: {hit.collider.name}, 위치: {hit.point}");

                    break; // Enemy와 만나면 경로 종료
                }

                // Wall과 충돌한 경우 - 반사 계속
                // 반사 벡터 계산
                lastVelocity = Vector2.Reflect(lastVelocity, hit.normal);

                // 충돌 지점에서 약간 떨어진 위치 계산 (경로 겹침 방지)
                Vector2 offsetPoint = hit.point + hit.normal * 0.1f;

                // 오프셋된 충돌 지점 추가
                path.Add(offsetPoint);

                // 다음 시작 위치 설정 (관통 방지)
                lastPos = offsetPoint;

                reflectionCount++;
                totalDistance += hit.distance;
                stepCount++;

                // 디버그 로그 (필요시 주석 해제)
                // Debug.Log($"Wall 반사 {reflectionCount}: {hit.collider.name}, Normal: {hit.normal}");

                continue;
            }

            // 충돌이 없으면 다음 위치 추가
            path.Add(nextPos);

            lastPos = nextPos;
            totalDistance += stepDistance;
            stepCount++;
        }

        // 디버그 로그 (필요시 주석 해제)
        // Debug.Log($"경로 점 개수: {path.Count}, 반사 횟수: {reflectionCount}, Enemy 충돌: {IsLastReflectionEnemy}");

        return path;
    }

}

