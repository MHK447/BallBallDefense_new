using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UniRx;

public class PlayerUnitInfoData
{
    public int StartHp = 0;
    public int CurHp = 0;
    public float AttackRange = 0f;
    
    public int AttackDamage = 1;



    public ReactiveProperty<int> InBaseBallCount = new ReactiveProperty<int>(0);





}

