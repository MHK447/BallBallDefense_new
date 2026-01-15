using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

[EffectPath("Effect/FireballEffect", false, false)]
public class FireballEffect : Effect
{
    [SerializeField]
    private SpriteRenderer FireballEffectimg;

    //private FactionType Type = FactionType.None;

    private double Damage = 0;

    private float Radius = 0f;

    //private PlayerUnit Unit = null;

    public void Set(float radius, double damage, Color color)
    {
        Damage = damage;
        Radius = radius;

        FireballEffectimg.color = color;

        StartDirection();

    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = collision.gameObject.layer;


    }


    public void StartDirection()
    {
        this.transform.localScale = Vector3.zero;

        this.transform.DOScale(Radius, 1f)
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            ProjectUtility.SetActiveCheck(this.gameObject, false);
        });
    }


}
