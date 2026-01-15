using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using DG.Tweening;

public class PlayerUnitGroup : MonoBehaviour
{
    [HideInInspector]
    public List<PlayerUnit> ActiveUnits = new List<PlayerUnit>();

    [HideInInspector]
    public List<PlayerUnit> DeadUnits = new List<PlayerUnit>();

    [SerializeField]
    private Transform UnitRootTr;

    private List<PlayerUpgradeStateModifier> StatModifierList = new();


    // 승리 연출 진행 중 여부
    [HideInInspector]
    public bool IsWinAnimationPlaying = false;

    public void Init()
    {
        ActiveUnits.Clear();
        DeadUnits.Clear();
        IsWinAnimationPlaying = false;
    }

    public void AddUnit(int unit_idx, int grade, Transform spawntr, int blockOrder = 0)
    {
        // 승리 연출 중에는 유닛 추가 불가
        if (IsWinAnimationPlaying)
            return;
        
        var td = Tables.Instance.GetTable<UnitInfo>().GetData(unit_idx);

        if (td != null)
        {
            SoundPlayer.Instance.PlaySound("item_get");

            var find = DeadUnits.Find(x => x.PlayerUnitIdx == unit_idx && x.PlayerGrade == grade);
            if (find != null)
            {
                // 재활용 전 이전 애니메이션 정리
                find.transform.DOKill();
                
                ActiveUnits.Add(find);
                DeadUnits.Remove(find);

                find.transform.localScale = Vector3.one;
                
                // spawntr 위치에서 시작
                find.transform.position = spawntr.position;
                
                // Set() 호출 - 비활성 상태에서 먼저 초기화
                find.Set(unit_idx, grade, blockOrder);

                // 모든 설정이 완료된 후 활성화
                ProjectUtility.SetActiveCheck(find.gameObject, true);
            }
            else
            {
                var unit = Addressables.InstantiateAsync(td.prefab, UnitRootTr);

                var result = unit.WaitForCompletion();

                PlayerUnit instance = result.GetComponent<PlayerUnit>();

                // 생성 직후 바로 spawntr 위치로 설정
                result.transform.position = spawntr.position;
                
                // 초기화 전에 비활성화 (화면에 안 보이도록)
                ProjectUtility.SetActiveCheck(instance.gameObject, false);

                ActiveUnits.Add(instance);
                
                // Set() 호출 - 비활성 상태에서 먼저 초기화
                instance.Set(unit_idx, grade, blockOrder);

                // 모든 설정이 완료된 후 활성화 (올바른 위치에서)
                ProjectUtility.SetActiveCheck(instance.gameObject, true);
            }
        }
    }

    public void DeleteUnit(PlayerUnit unit)
    {
        // DOTween 애니메이션 정리
        unit.transform.DOKill();
        
        ProjectUtility.SetActiveCheck(unit.gameObject, false);

        ActiveUnits.Remove(unit);
        DeadUnits.Add(unit);
    }


    public void ClearData()
    {
        // 모든 활성 유닛 비활성화
        foreach (var unit in ActiveUnits)
        {
            if (unit != null)
            {
                // DOTween 애니메이션 정리
                unit.transform.DOKill();
                // HpProgress 비활성화
                unit.HideHpProgress();
                ProjectUtility.SetActiveCheck(unit.gameObject, false);
            }
        }

        // 모든 죽은 유닛 비활성화
        foreach (var unit in DeadUnits)
        {
            if (unit != null)
            {
                // DOTween 애니메이션 정리
                unit.transform.DOKill();
                // HpProgress 비활성화
                unit.HideHpProgress();
                ProjectUtility.SetActiveCheck(unit.gameObject, false);
            }
        }

        // 리스트 클리어
        ActiveUnits.Clear();
        DeadUnits.Clear();
        IsWinAnimationPlaying = false;
    }
}

