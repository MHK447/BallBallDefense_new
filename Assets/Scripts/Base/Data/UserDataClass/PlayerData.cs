using System;
using Google.FlatBuffers;
using UniRx;
using UnityEngine;

public partial class UserDataSystem
{
    public PlayerData Playerdata { get; private set; } = new PlayerData();
    private void SaveData_PlayerData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Playerdata 단일 저장
        // Playerdata 최종 생성 및 추가
        var playerdata_Offset = BanpoFri.Data.PlayerData.CreatePlayerData(
            builder,
            Playerdata.Playerlevel,
            Playerdata.Playerexp,
            builder.CreateString(Playerdata.Playername)
        );


        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddPlayerdata(builder, playerdata_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_PlayerData()
    {
        // 로드 함수 내용

        // Playerdata 로드
        var fb_Playerdata = flatBufferUserData.Playerdata;
        if (fb_Playerdata.HasValue)
        {
            Playerdata.Playerlevel = fb_Playerdata.Value.Playerlevel;
            Playerdata.Playerexp = fb_Playerdata.Value.Playerexp;
            Playerdata.Playername = fb_Playerdata.Value.Playername;
        }
    }

}



public class PlayerData
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

    public IReactiveProperty<bool> IsWaveRestProperty = new ReactiveProperty<bool>(false);

    public IReactiveProperty<bool> IsGameStartProperty = new ReactiveProperty<bool>(false);



    public IReactiveProperty<int> InGameMoneyProperty { get; private set; } = new ReactiveProperty<int>(0);
    public int InGameReRollCount = 0;

    public void UpdatePlayerLevelFromStageIndex()
    {
        GameRoot.Instance.UserData.Playerdata.SetPlayerLevel(GameRoot.Instance.UserData.Stageidx.Value);
    }

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
        // InGameReRollCount = GameRoot.Instance.CardSystem.GetSkillCardTypeValue((int)CardSystem.CardType.FreeRerollCount);
        IsWaveRestProperty.Value = false;
        IsGameStartProperty.Value = false;
    }


    public void SetPlayerHp(int hp)
    {
        StartHpProperty.Value = hp;
    }

}
