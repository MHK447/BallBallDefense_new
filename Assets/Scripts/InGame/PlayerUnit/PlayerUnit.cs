using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class PlayerUnit : MonoBehaviour
{
    public enum PlayerUnitState
    {
        Idle,
        Attack,
        Dead,
    }


    [SerializeField]
    private SpriteRenderer PlayerUnitImg;

    [SerializeField]
    private SpriteRenderer PlayerWeaponImg;

    [SerializeField]
    private PlayerUnitWeapon UnitWeapon;

    private PlayerUnitState UnitState = PlayerUnitState.Idle;

    public bool IsDead { get { return UnitState == PlayerUnitState.Dead; } }

    [HideInInspector]
    public PlayerUnitInfoData PlayerUnitInfoData = new PlayerUnitInfoData();

    private int PlayerUnitIdx = 0;

    public void Set(int unitidx)
    {
        PlayerUnitIdx = unitidx;

        var td = Tables.Instance.GetTable<UnitInfo>().GetData(unitidx);

        if(td != null)
        {
            PlayerUnitInfoData.StartHp = 1000;
            PlayerUnitInfoData.CurHp = 1000;
            PlayerUnitInfoData.InBaseBallCount.Value = 5;
            PlayerUnitInfoData.AttackRange = 5f;

            UnitState = PlayerUnitState.Idle;

            UnitWeapon.Set(PlayerUnitIdx);
        }


    }
}

