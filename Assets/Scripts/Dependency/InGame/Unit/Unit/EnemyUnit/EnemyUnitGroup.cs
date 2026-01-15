using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BanpoFri;
using UnityEngine.AddressableAssets;
using DG.Tweening;
using System.Linq;

public class EnemyUnitGroup : MonoBehaviour
{
    public HashSet<EnemyUnit> ActiveUnits = new HashSet<EnemyUnit>();
    public HashSet<EnemyUnit> DeadUnits = new HashSet<EnemyUnit>();

    [SerializeField]
    private List<Transform> UnitSpawnList = new List<Transform>();

    public EnemyBlockSpawner EnemyBlockSpawner;

    [HideInInspector]
    public bool IsEnemyBlockSpawnerActive = false;

    public bool IsAllDeadCheck
    {
        get
        {
            // 일반 적 유닛이 모두 죽었는지 확인
            if (ActiveUnits.Count > 0) return false;

            // EnemyBlockSpawner가 활성화되어 있고 아직 살아있으면 false
            if (IsEnemyBlockSpawnerActive && EnemyBlockSpawner != null && !EnemyBlockSpawner.IsDead)
                return false;

            return true;
        }
    }

    private int SpawnOrder = 0;

    public void Init()
    {
        SpawnOrder = 0;


    }



    public void AddUnit(int enemyidx , int deadexpvalue, int unitdmg, int unithp)
    {
        var td = Tables.Instance.GetTable<EnemyInfo>().GetData(enemyidx);

        if (td != null)
        {
            // DeadUnits에서 같은 enemyidx를 가진 유닛을 찾아 재활용
            var find = DeadUnits.FirstOrDefault(x => x.EnemyIdx == enemyidx);

            EnemyUnit instance;

            if (find != null)
            {
                // 재활용
                instance = find;
                DeadUnits.Remove(find);
                ActiveUnits.Add(instance);
            }
            else
            {
                // 새로 생성
                var handle = Addressables.InstantiateAsync(td.prefab, transform);
                var result = handle.WaitForCompletion();
                instance = result.GetComponent<EnemyUnit>();

                ActiveUnits.Add(instance);
            }

            if (td.boss_unit == 1)
            {
                ProjectUtility.SetActiveCheck(GameRoot.Instance.UISystem.GetUI<PopupInGame>().DifficultyRoot, true);
            }

            Vector3 landingPos;
            Vector3 spawnPos;
            float landingY = 0f;

            if (IsEnemyBlockSpawnerActive && EnemyBlockSpawner != null && EnemyBlockSpawner.UnitSpawner != null)
            {
                // EnemyBlockSpawner가 활성화된 경우 GetUnitSpawnerPoint로 순차적으로 스폰 포인트 가져오기
                Transform spawnPoint = EnemyBlockSpawner.GetUnitSpawnerPoint();
                var randx = Random.Range(-0.3f, 0.3f);
                var randy = Random.Range(-0.3f, 0.3f);

                landingPos = new Vector3(spawnPoint.position.x + randx, spawnPoint.position.y + randy, spawnPoint.position.z);
                spawnPos = landingPos;
                landingY = landingPos.y;

            }
            else
            {
                // 착지 위치는 UnitSpawnList의 원래 위치에 SpawnOrder에 따른 간격 추가
                landingPos = UnitSpawnList[SpawnOrder].position;
                landingPos.x += SpawnOrder * 1.0f; // x축으로 1씩 간격 추가
                landingY = landingPos.y;

                // 스폰 위치는 착지 위치에서 공중으로 올림 (SpawnOrder에 따라 0.5~1f씩 차이)
                spawnPos = landingPos;
                spawnPos.y += 2f + (SpawnOrder * 0.5f); // 공중에서 시작 (0.5f씩 차이)
            }


            instance.transform.position = spawnPos;

            // Set 호출 시 SpawnOrder와 착지 y값, 그리고 WaveInfo의 dmg, hp 전달 (활성화 전에 초기화)
            instance.Set(enemyidx , unitdmg, unithp, deadexpvalue, SpawnOrder, landingY);


            // 초기화 완료 후 활성화
            ProjectUtility.SetActiveCheck(instance.gameObject, true);

            SpawnOrder++;

            if (SpawnOrder >= UnitSpawnList.Count)
            {
                SpawnOrder = 0;
            }
        }
    }

    public void DeleteUnit(EnemyUnit unit)
    {
        if (ActiveUnits.Contains(unit))
        {
            ActiveUnits.Remove(unit);
            DeadUnits.Add(unit);
            ProjectUtility.SetActiveCheck(unit.gameObject, false);
        }

        // 모든 적 유닛을 처치했고, 웨이브 스폰이 완전히 끝났는지 확인
        var stage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;
        if (IsAllDeadCheck && stage.IsWaveSpawnComplete)
        {
            // 승리 애니메이션 후 웨이브 휴식 상태로 전환
            StartCoroutine(PlayWinActionAndStartRest());
        }

        //GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.NextWaveCheck();
    }

    // 웨이브 스폰 완료 후 적이 이미 모두 죽었는지 체크하는 메서드
    public void CheckAndStartRestIfAllDead()
    {
        if (IsAllDeadCheck)
        {
            StartCoroutine(PlayWinActionAndStartRest());
        }
    }

    public IEnumerator PlayWinActionAndStartRest()
    {
        // 승리 연출 플래그 설정 (플레이어 유닛 추가 방지)
        var playerUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup;
        playerUnitGroup.IsWinAnimationPlaying = true;

        // 플레이어 유닛 리스트 가져오기
        var untlist = playerUnitGroup.ActiveUnits;

        // 승리 연출 시작 시 모든 플레이어 유닛의 HP Progress 비활성화
        foreach (var unit in untlist)
        {
            if (unit != null)
            {
                unit.HideHpProgress();
            }
        }


        // 2초 대기
        yield return new WaitForSeconds(2f);

        EndStageReset();
    }

    public void EndStageReset()
    {
        var playerUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup;
        var untlist = playerUnitGroup.ActiveUnits;
        // 모든 플레이어 유닛 삭제 (역순으로 안전하게 삭제)
        for (int i = playerUnitGroup.ActiveUnits.Count - 1; i >= 0; i--)
        {
            GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup.DeleteUnit(untlist[i]);
        }

        // 웨이브 휴식 상태로 전환
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.StartRest();
    }

    public EnemyUnit FindTargetEnemy(Transform closetroottr, float attackrange = -1)
    {
        // ShooterTr과 가장 가까운 적을 찾기 (x좌표 기준)
        var query = ActiveUnits.Where(unit => unit.IsDead == false).ToList();

        // attackrange가 -1이 아니면 범위 내의 적만 필터링 (x좌표 기준)
        if (attackrange != -1)
        {
            query = query.Where(unit => Mathf.Abs(closetroottr.position.x - unit.transform.position.x) <= attackrange).ToList();
        }

        var closestEnemy = query
            .OrderBy(unit => Mathf.Abs(closetroottr.position.x - unit.transform.position.x))
            .FirstOrDefault();

        if (closestEnemy != null)
        {
            return closestEnemy;
        }

        return null;
    }

    public void CheckEnemyBlockSpawner()
    {
        ProjectUtility.SetActiveCheck(EnemyBlockSpawner.gameObject, false);
        IsEnemyBlockSpawnerActive = false;

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var waveidx = GameRoot.Instance.UserData.Waveidx.Value;

        var wavetd = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(stageidx, waveidx));

        if (wavetd != null)
        {
            var stagetd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

            if (stagetd != null)
            {
                if (stagetd.ingame_map_idx > 0)
                {
                    EnemyBlockSpawner.Set(stagetd.ingame_map_idx);
                    ProjectUtility.SetActiveCheck(EnemyBlockSpawner.gameObject, true);
                    IsEnemyBlockSpawnerActive = true;
                }
            }
        }
    }



    public void ClearData()
    {
        foreach (var block in ActiveUnits)
        {
            //block.Clear();
            Destroy(block.gameObject);
        }
        ActiveUnits.Clear();


        foreach (var block in DeadUnits)
        {
            //block.Clear();
            Destroy(block.gameObject);
        }
        DeadUnits.Clear();
    }


    public void ActiveUnitSetScale(float scale)
    {
    }

}
