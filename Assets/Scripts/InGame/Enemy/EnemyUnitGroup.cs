using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class EnemyUnitGroup : MonoBehaviour
{

    [SerializeField]
    private List<Transform> EnemySpawnTrList = new List<Transform>();

    public HashSet<EnemyUnit> ActiveUnits = new HashSet<EnemyUnit>();
    public HashSet<EnemyUnit> DeadUnits = new HashSet<EnemyUnit>();

    public void AddEnemyUnit(int enemyidx, int order, int hp)
    {
        var find = DeadUnits.FirstOrDefault();

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
            var handle = Addressables.InstantiateAsync("EnemyUnit_Base", transform);
            var result = handle.WaitForCompletion();
            instance = result.GetComponent<EnemyUnit>();
            instance.Set(enemyidx, order, hp);

            instance.transform.position = EnemySpawnTrList[order].position;
            ActiveUnits.Add(instance);
        }

    }


    public EnemyUnit FindEnemyTarget(Vector3 position, float range)
    {
        var findenemytarget = ActiveUnits.FirstOrDefault(x => x.transform.position.x >= position.x - range && x.transform.position.x <= position.x + range && x.transform.position.y >= position.y - range && x.transform.position.y <= position.y + range);
        return findenemytarget;
    }

}

