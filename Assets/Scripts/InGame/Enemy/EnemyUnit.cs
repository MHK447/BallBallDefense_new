using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
using System.Numerics;

public class EnemyUnit : MonoBehaviour
{
    public enum EnemyState
    {
        Dead,
        Move,
        Sturn
    }

    protected InGameHpProgress InGameHpProgress;


    [HideInInspector]
    public EnemyInfoData EnemyInfoData;

    [SerializeField]
    private SpriteRenderer UnitImg;


    private int EnemyIdx = 0;

    private EnemyState CurState = EnemyState.Move;

    protected InGameBaseStage BaseStage;

    protected int Order = 0;

    public bool IsDead { get { return CurState == EnemyState.Dead; } }





    public void Set(int enemyidx, int order, int hp)
    {
        EnemyIdx = enemyidx;

        EnemyInfoData.StartHp = hp;
        EnemyInfoData.CurHp = hp;
        EnemyInfoData.MoveSpped = 2f;

        BaseStage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;

        Order = order;

        SetState(EnemyState.Move);


        SetHpprogress(hp);

        this.transform.DOKill();
        this.transform.localScale = UnityEngine.Vector3.zero;
        this.transform.DOScale(UnityEngine.Vector3.one, 0.3f).SetEase(Ease.OutBack);

        UnitImg.DisableHitEffect();
    }



    public void SetHpprogress(int hp)
    {
        if (InGameHpProgress == null)
        {
            GameRoot.Instance.UISystem.LoadFloatingUI<InGameHpProgress>(hpprogress =>
                    {
                        InGameHpProgress = hpprogress;
                        // 먼저 비활성화하여 잘못된 위치에서 보이지 않도록 함
                        ProjectUtility.SetActiveCheck(hpprogress.gameObject, true);
                        hpprogress.Init(transform);
                        hpprogress.SetHpText(hp, EnemyInfoData.StartHp);
                    });
        }
        else
        {
            InGameHpProgress.SetHpText(hp, EnemyInfoData.StartHp);
            ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, true);
        }
    }


    void Update()
    {
        Move();
    }


    public virtual void Damage(int damage)
    {

        GameRoot.Instance.DamageTextSystem.ShowDamage(damage,
        new UnityEngine.Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Color.white);
    }


    public virtual void Dead()
    {
        SetState(EnemyState.Dead);
        ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, false);

        this.transform.DOKill();

        this.transform.localScale = UnityEngine.Vector3.one;
        this.transform.DOScale(UnityEngine.Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            BaseStage.EnemyUnitGroup.DeadUnits.Add(this);
            ProjectUtility.SetActiveCheck(this.gameObject, false);
        });
    }



    public virtual void SetState(EnemyState state)
    {
        if (CurState == state) return;

        CurState = state;
    }

    public void Move()
    {
        if (CurState != EnemyState.Move) return;

        transform.position -= new UnityEngine.Vector3(0, EnemyInfoData.MoveSpped * Time.deltaTime, 0);
    }


    private bool IsDamageDirect = false;

    public virtual void DamageColorEffect()
    {
        if (!IsDamageDirect)
        {
            IsDamageDirect = true;

            UnitImg.EnableHitEffect();

            // 피격 효과 적용


            GameRoot.Instance.WaitTimeAndCallback(0.15f, () =>
            {
                if (this != null)
                {
                    // 효과 종료 후 원래 머티리얼로 복귀
                    UnitImg.DisableHitEffect();

                    IsDamageDirect = false;
                }
            });
        }
    }

}

