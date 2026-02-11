using System;
using Google.FlatBuffers;
using UniRx;
using UnityEngine;

public partial class UserDataSystem
{

}



public class InGamePlayerData
{
    public string Playername { get; set; } = "Player";

    public int Playerlevel = 1;
    public int Playerexp = 0;

    public IReactiveProperty<int> StartHpProperty { get; private set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> CurShiledProperty { get; private set; } = new ReactiveProperty<int>(0);

    public IReactiveProperty<int> CurHpProperty { get; private set; } = new ReactiveProperty<int>(0);

    public IReactiveProperty<int> RemainingEnemyCountProperty { get; private set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> InGameExpProperty { get; private set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> InGameUpgradeCountProperty { get; private set; } = new ReactiveProperty<int>(1);


    public IReactiveProperty<int> KillCountProperty = new ReactiveProperty<int>(0);

    public IReactiveProperty<bool> IsGameStartProperty = new ReactiveProperty<bool>(false);

    public IReactiveProperty<int> WaveTimePorperty = new ReactiveProperty<int>(0);

    public IReactiveProperty<int> InGameMoneyProperty { get; private set; } = new ReactiveProperty<int>(0);
    public int InGameReRollCount = 0;


    public void SetPlayerLevel(int level)
    {
        level = Mathf.Max(level, Playerlevel);
        Playerlevel = level;
    }

    public void StageClear()
    {
        RemainingEnemyCountProperty.Value = 0;
        InGameExpProperty.Value = 0;
        InGameUpgradeCountProperty.Value = 1;
        InGameMoneyProperty.Value = 0;
        KillCountProperty.Value = 0;
        IsGameStartProperty.Value = false;
        WaveTimePorperty.Value = 0;
    }


    public void SetPlayerHp(int hp)
    {
        StartHpProperty.Value = hp;
    }

}
