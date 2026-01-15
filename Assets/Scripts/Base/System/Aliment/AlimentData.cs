using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class AlimentData
{
    public AlimentType Type = AlimentType.None;

    public float Time = 0f;

    public EnemyUnit Target = null;

    public double Damage = 0f;
    public float DamageDelay = 0f;

    public int StackCount = 1; // 중첩 카운트
    public int MaxStackCount = 1; // 최대 중첩 수

    protected float deltatime = 0f;


    public virtual void Set(AlimentType type, float time, EnemyUnit target, double damage, float damagedelay, int maxStackCount = 1)
    {
        this.Type = type;
        this.Time = time;
        this.Target = target;
        this.Damage = damage;
        this.DamageDelay = damagedelay;
        this.MaxStackCount = maxStackCount;

        deltatime = 0f;
    }


    public virtual void Update()
    {
        if (Time <= 0f) return;

        deltatime += UnityEngine.Time.deltaTime;
        if (deltatime >= DamageDelay)
        {
            Time -= DamageDelay;
            deltatime = 0f;
            OnDamage();
        }
    }


    public virtual void OnDamage()
    {
        if (Target == null) return;


        Effect();
    }

    public void Effect()
    {
        if (Target == null) return;

        switch (Type)
        {
            case AlimentType.Poison:
                {
                    GameRoot.Instance.EffectSystem.MultiPlay<PoisonEffect>(Target.transform.position, (x) =>
                    {
                        x.SetAutoRemove(true, 1.5f);
                    });
                    break;
                }
        }
    }

}
