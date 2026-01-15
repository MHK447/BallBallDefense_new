using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using BanpoFri;


public class ActionQueueSystem
{
    #region System
    private readonly LinkedList<Action> actions = new();

    public void Append(Action action)
    {
        actions.AddLast(action);
    }

    public void InsertNext(Action action)
    {
        actions.AddFirst(action);
    }

    public void NextAction()
    {
        if (actions.Count == 0)
        {
            BpLog.Log($"ActionQueue finished");
            return;
        }

        var action = actions.First.Value;
        actions.RemoveFirst();
        action?.Invoke();

        BpLog.Log($"ActionQueue left : {actions.Count}");
    }

    public void ClearActions()
    {
        actions.Clear();
    }
    #endregion

    #region Actions

    public struct GameFinishContext
    {
        public bool isStageClear;
        public bool shouldRestoreSkill;

        public int passKeyCount;
    }

    public void OnFirstInitCall()
    {
        ClearActions();

        // AttendancePopupCheck();
        // TowerRacePopupCheck();
        // StarterPackageRewardPopupCheck();
        // NoAdsPopupCheck();
        // GoldGrabPopupCheck();
        // FirstTreasureHunterPopupCheck();

        NextAction();
    }

    public void OnGameFinishCall()
    {
        ClearActions();
        NewChapterCheck();
        MainRewardOpenTutorialCheck();
        ReviewPopupCheck();
        BoxOpenTutorialCheck();
        UnLocCardTutorialCheck();
        UnLocTrainingTutorialCheck();
        NextAction();
    }


    public void SpearManTutorialCheck()
    {

        // var finddata = GameRoot.Instance.UserData.Unitgroupdata.FindUnit((int)Config.WeaponUnit.Spear);
        // if (GameRoot.Instance.UserData.Stageidx.Value >= 2 && finddata == null)
        // {
        //     Append(() =>
        //     {
        //         GameRoot.Instance.UserData.Unitgroupdata.AddUnit((int)Config.WeaponUnit.Spear);
        //         //GameRoot.Instance.UISystem.OpenUI<PageGetCharacterInfo>(page => page.Set((int)Config.WeaponUnit.Spear), NextAction);
        //         SoundPlayer.Instance.PlaySound("unit_unlock");
        //     });
        // }
    }

    public void UnLocTrainingTutorialCheck()
    {
        // if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_5) &&
        //  GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.TRAININGOPEN))
        // {
        //     Append(() =>
        //     {
        //         //GameRoot.Instance.UISystem.GetUI<HUDTotal>().HudBottomBtnList[(int)HudBottomBtnType.TRAINING].SetLocked(false);
        //         GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_5);
        //         GameRoot.Instance.TutorialSystem.OnActiveTutoEnd = NextAction;
        //     });
        // }
    }


    public void UnLocCardTutorialCheck()
    {
        // if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_6) &&
        //  GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.CARDOPEN))
        // {
        //     Append(() =>
        //     {
        //         GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency , (int)Config.CurrencyID.Material , 20, false);
        //         GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_6);
        //         GameRoot.Instance.TutorialSystem.OnActiveTutoEnd = NextAction;
        //     });
        // }
    }



    public void UnLockStageUnitCheck()
    {
        // if (GameRoot.Instance.UserData.Stageidx.Value >= 3 &&
        //  GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.UNITUPGRADEOPEN))
        // {
        //     Append(() =>
        //     {
        //         var tdlist = Tables.Instance.GetTable<UnitInfo>().DataList.ToList();

        //         var selectstageidx = 0;

        //         var finddata = tdlist.FirstOrDefault(x => GameRoot.Instance.UserData.Unitgroupdata.FindUnit(x.idx) == null);

        //         if (finddata != null)
        //         {
        //             selectstageidx = finddata.idx;

        //             GameRoot.Instance.UISystem.OpenUI<PageGetCharacterReward>(page => page.Set(selectstageidx), NextAction);
        //         }
        //         else
        //         {
        //             NextAction();
        //         }
        //     });
        // }
    }


    #endregion

    // private void ShowTouchLock()
    // {
    //     Append(() =>
    //     {
    //         GameRoot.Instance.UISystem.OpenUI<PopupTouchLock>(x => NextAction(), NextAction);
    //     });
    // }

    // private void HideTouchLock()
    // {
    //     Append(() =>
    //     {
    //         var touchlock = GameRoot.Instance.UISystem.GetUI<PopupTouchLock>();
    //         if (touchlock == null) { NextAction(); return; }
    //         touchlock.Hide();
    //     });
    // }

    private void MainRewardOpenTutorialCheck()
    {
        //메인 리워드 버튼 체크 
        if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_3) && GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.LobbyReward))
        {
            Append(() =>
             {
                 GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_3);
                 GameRoot.Instance.TutorialSystem.OnActiveTutoEnd = NextAction;
             });
        }
    }
    private void BoxOpenTutorialCheck()
    {
        // //스테이지 보상 바운스볼 획득 튜토리얼
        // if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_3)
        //     && GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.BOXOPEN))
        // {
        //     Append(() =>
        //      {
        //          GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_3);
        //          GameRoot.Instance.TutorialSystem.OnActiveTutoEnd = NextAction;
        //      });
        // }
    }

    public void NewChapterCheck()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var recordcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.NewChapter, stageidx);

        if (stageidx % 7 == 0 && recordcount == 0)
        {
            var stagetd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);
            GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.NewChapter, 1, stageidx);
            Append(() =>
             {
                 GameRoot.Instance.UISystem.OpenUI<PopupNewChapter>(x => x.Set(stagetd.ingame_map_idx), NextAction);
             });

        }
    }



    private void ReviewPopupCheck()
    {
        if (GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.ReviewPopup)
        && GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.ReviewPopup) == 0)
        {
            Append(() =>
             {
                 GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.ReviewPopup, 1);
                 GameRoot.Instance.UISystem.OpenUI<PopupReview>(null, NextAction);
             });
        }
    }


}