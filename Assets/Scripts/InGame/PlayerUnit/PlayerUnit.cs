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

    private CastleHpProgress CastleHpProgress;

    private int PlayerUnitIdx = 0;

    public void Set(int unitidx)
    {
        PlayerUnitIdx = unitidx;

        var td = Tables.Instance.GetTable<UnitInfo>().GetData(unitidx);

        if (td != null)
        {
            PlayerUnitInfoData.StartHp = 1000;
            PlayerUnitInfoData.CurHp = 1000;
            PlayerUnitInfoData.InBaseBallCount.Value = 5;
            PlayerUnitInfoData.AttackRange = 15f;

            UnitState = PlayerUnitState.Idle;

            UnitWeapon.Set(PlayerUnitIdx);

            SetHpProgress(PlayerUnitInfoData.CurHp);
        }


    }

    public void SetHpProgress(int hp)
    {
        if (CastleHpProgress == null)
        {
            GameRoot.Instance.UISystem.LoadFloatingUI<CastleHpProgress>(x =>
            {
                CastleHpProgress = x;
                CastleHpProgress.SetHpText(hp, PlayerUnitInfoData.StartHp);
                CastleHpProgress.Init(transform);
            });
        }
        else
        {
            CastleHpProgress.SetHpText(hp, PlayerUnitInfoData.StartHp);
        }
    }



    public void Damage(int damage)
    {
        PlayerUnitInfoData.CurHp -= damage;
        CastleHpProgress.SetHpText(PlayerUnitInfoData.CurHp, PlayerUnitInfoData.StartHp);
        DamageColorEffect();

        if (PlayerUnitInfoData.CurHp <= 0)
        {
            UnitState = PlayerUnitState.Dead;
        }
    }




    private bool IsDamageDirect = false;
    public virtual void DamageColorEffect()
    {
        if (!IsDamageDirect)
        {
            IsDamageDirect = true;

            PlayerUnitImg.EnableHitEffect();

            // 피격 효과 적용


            GameRoot.Instance.WaitTimeAndCallback(0.15f, () =>
            {
                if (this != null)
                {
                    // 효과 종료 후 원래 머티리얼로 복귀
                    PlayerUnitImg.DisableHitEffect();

                    IsDamageDirect = false;
                }
            });
        }
    }




}

