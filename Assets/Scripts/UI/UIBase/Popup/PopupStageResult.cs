using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using BanpoFri;
using UnityEngine;
using UnityEngine.UI;

[UIPath("UI/Popup/PopupStageResult")]
public class PopupStageResult : CommonUIBase
{
    [SerializeField]
    private GameObject FailedObj;

    [SerializeField]
    private GameObject VictoryObj;

    [SerializeField]
    private Button CloseBtn;

    [SerializeField]
    private AdsButton AdRewardBtn;

    [SerializeField]
    private List<ResultrewardComponent> ResultCurrencyRewardList = new();

    [SerializeField]
    private GameObject CloseBtnRoot;


    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private GameObject RewardMultiRoot;

    [SerializeField]
    private Button FreeBtn;

    [SerializeField]
    private GameObject HideBtnRoot;

    private bool IsSuccess = false;

    private bool IsFree = false;

    private int CoinAmount = 0;
    private int MaterialAmount = 0;

    private int CoinAdAmount = 0;
    private int MaterialAdAmount = 0;

    protected override void Awake()
    {
        base.Awake();
        CloseBtn.onClick.AddListener(OnClickBaseBtn);
        AdRewardBtn.AddListener(TpMaxProp.AdRewardType.StageResult, OnClickAdReward);
        FreeBtn.onClick.AddListener(OnClickBaseBtn);
    }

    public void Set(bool issuccess)
    {
        CloseBtn.interactable = true;
        AdRewardBtn.interactable = true;

        IsSuccess = issuccess;

        IsFree = !GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.RESULTREWARDAD);

        ProjectUtility.SetActiveCheck(FailedObj, !IsSuccess);
        ProjectUtility.SetActiveCheck(VictoryObj, IsSuccess);

        ProjectUtility.SetActiveCheck(CloseBtnRoot, IsSuccess);

        ProjectUtility.SetActiveCheck(AdRewardBtn.gameObject, !IsFree);

        ProjectUtility.SetActiveCheck(FreeBtn.gameObject, IsFree);

        ProjectUtility.SetActiveCheck(HideBtnRoot, !IsFree);

        SetCurrencyValue();

        ProjectUtility.SetActiveCheck(ResultCurrencyRewardList[(int)Config.CurrencyID.Material - 1].GetBaseRewardRoot,
        GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.CARDOPEN));

        ProjectUtility.SetActiveCheck(ResultCurrencyRewardList[(int)Config.CurrencyID.Material - 1].GetAdRewardRoot,
       GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.CARDOPEN));



        ResultCurrencyRewardList[0].Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, CoinAmount, CoinAdAmount);
        ResultCurrencyRewardList[(int)Config.CurrencyID.Material - 1].Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, MaterialAmount, MaterialAdAmount);

    }

    public void OnClickAdReward()
    {
        CloseBtn.interactable = false;
        AdRewardBtn.interactable = false;
        GameRoot.Instance.WaitTimeAndCallback(1.5f, () =>
               {
                   if (CoinAmount > 0)
                   {
                       GameRoot.Instance.EffectSystem.MultiPlay<RewardEffect>(transform.position, x =>
                                         {
                                             //적에 전체체력에 퍼센트

                                             var endtr = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>().GetRewardEndTr((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money);


                                             x.Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, endtr, () =>
                                             {
                                                 GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, CoinAdAmount);
                                             });
                                             x.SetAutoRemove(true, 4f);
                                         });
                   }
                   if (MaterialAmount > 0)
                   {
                       GameRoot.Instance.EffectSystem.MultiPlay<RewardEffect>(transform.position, x =>
                         {
                             //적에 전체체력에 퍼센트

                             var endtr = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>().GetRewardEndTr((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material);


                             x.Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, endtr, () =>
                             {
                                 GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, MaterialAdAmount);
                             });
                             x.SetAutoRemove(true, 4f);
                         });
                   }
               });


        GameRoot.Instance.CurrencyTop.SyncReward();


        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.ReturnMainScreen();
        Hide();

    }



    public void SetCurrencyValue()
    {

    }


    private void OnClickBaseBtn()
    {

        ConfirmBtn();
    }

    public void PlaySparkleSound()
    {
        if (IsSuccess)
            SoundPlayer.Instance.PlaySound("effect_result_sparkle");
        else
            SoundPlayer.Instance.PlaySound("effect_game_over");
    }





    public void ConfirmBtn()
    {
        // GameRoot.Instance.UserData.IsStartBattleProeprty.Value = false;
        CloseBtn.interactable = false;
        AdRewardBtn.interactable = false;

        GameRoot.Instance.WaitTimeAndCallback(1.5f, () =>
        {
            if (CoinAmount > 0)
            {
                GameRoot.Instance.EffectSystem.MultiPlay<RewardEffect>(transform.position, x =>
                                  {
                                      //적에 전체체력에 퍼센트

                                      var endtr = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>().GetRewardEndTr((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money);
                                      SoundPlayer.Instance.PlaySound("get_coin");

                                      x.Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, endtr, () =>
                                      {
                                          GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, CoinAmount);
                                      });
                                      x.SetAutoRemove(true, 4f);
                                  });
            }
            if (MaterialAmount > 0)
            {
                GameRoot.Instance.EffectSystem.MultiPlay<RewardEffect>(transform.position, x =>
                  {
                      //적에 전체체력에 퍼센트

                      var endtr = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>().GetRewardEndTr((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material);
                      SoundPlayer.Instance.PlaySound("get_coin");

                      x.Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, endtr, () =>
                      {
                          GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, MaterialAmount);
                      });
                      x.SetAutoRemove(true, 4f);
                  });
            }
        });


        Hide();
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.ReturnMainScreen();


        GameRoot.Instance.CurrencyTop.SyncReward();

    }


    public override void Hide()
    {
        base.Hide();

        if (IsSuccess)
        {
            GameRoot.Instance.UserData.Playerdata.Playerlevel += 1;
            GameRoot.Instance.UserData.Stageidx.Value += 1;
        }
        else
        {
            GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.StageFailedCount, 1, GameRoot.Instance.UserData.Stageidx.Value);
            // failed_count 증가 후 트레이닝 오픈 조건 체크를 위해 RefreshContentsOpen 호출
            GameRoot.Instance.ContentsOpenSystem.RefreshContentsOpen();
        }
    }
}
