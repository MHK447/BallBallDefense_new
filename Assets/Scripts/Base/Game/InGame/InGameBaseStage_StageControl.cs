using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Collections;
using System.IO.Compression;
using System.Linq;
using UnityEngine.AddressableAssets;

public partial class InGameBaseStage : MonoBehaviour
{

    private Coroutine currentWaveCoroutine = null;

    // 웨이브가 완전히 끝났는지 확인 (스폰이 모두 완료되었는지)
    public bool IsWaveSpawnComplete { get { return currentWaveCoroutine == null; } }

    public EnemyUnitGroup EnemyUnitGroup;

    [HideInInspector]
    public PlayerUnit PlayerUnit;

    [SerializeField]
    private Transform PlayerUnitSpawnRoot;


    public void InitStage()
    {
        EquipTutorialCheck();
    }

    public void StartBattle()
    {

        GameRoot.Instance.UserData.Waveidx.Value = 0;
        GameRoot.Instance.UserData.Ingamesilvercoin.Value = 0;
        GameRoot.Instance.UserData.InGamePlayerData.KillCountProperty.Value = 0;
        SoundPlayer.Instance.SetBGMVolume(0f);
        GameRoot.Instance.UISystem.GetUI<HUDTotal>()?.Hide();
        GameRoot.Instance.UserData.InGamePlayerData.IsGameStartProperty.Value = true;

        var trainingvalue = GameRoot.Instance.TrainingSystem.GetBuffValue(TrainingSystem.TrainingType.CastleHpIncrease);

        var hpvalue = Tables.Instance.GetTable<Define>().GetData("base_hp").value + trainingvalue;

        SetHp((int)hpvalue);

        GameRoot.Instance.UISystem.OpenUI<PopupInGame>(x =>
        {
            x.Init();
        });
        
        InitStage();

        SetPlayerUnit(1);

        StartNextWaveCheck();



    }


    public void SetHp(int hp)
    {
        GameRoot.Instance.UserData.InGamePlayerData.StartHpProperty.Value = hp;
        GameRoot.Instance.UserData.InGamePlayerData.CurHpProperty.Value = hp;
    }

    public void TutorialCheck()
    {

    }

    public void SetPlayerUnit(int playerunitidx)
    {
        if (PlayerUnit != null)
        {
            Destroy(PlayerUnit.gameObject);
        }

        var handle = Addressables.InstantiateAsync("PlayerUnit_Base", transform);
        var result = handle.WaitForCompletion();
        PlayerUnit = result.GetComponent<PlayerUnit>();
        PlayerUnit.Set(playerunitidx);

        PlayerUnit.transform.position = PlayerUnitSpawnRoot.position;
    }

    public void StageClear()
    {
        GameRoot.Instance.UserData.InGamePlayerData.StageClear();
        GameRoot.Instance.UserData.InGamePlayerData.IsGameStartProperty.Value = false;
        GameRoot.Instance.UserData.Ingamesilvercoin.Value = 0;
        GameRoot.Instance.AlimentSystem.Clear();
        GameRoot.Instance.InGameUpgradeSystem.ClearData();

        GameRoot.Instance.GameSpeedSystem.ResetGameSpeed();
        GameRoot.Instance.InGameUpgradeSystem.Reset();

        // PopupInGame의 TileWeaponGroup 초기화
        var popupInGame = GameRoot.Instance.UISystem.GetUI<PopupInGame>();
        // if (popupInGame != null && popupInGame.TileWeaponGroup != null)
        // {
        //     popupInGame.TileWeaponGroup.ClearData();
        // }
    }

    public bool GameFinishSequenceStarted = false;
    public IEnumerator GameOverSequence()
    {
        if (GameFinishSequenceStarted) yield break;
        GameFinishSequenceStarted = true;


        //slow mo
        GameRoot.Instance.GameSpeedSystem.CurGameSpeedValue.Value = 0.3f;
        yield return new WaitForSecondsRealtime(1f);
        GameRoot.Instance.GameSpeedSystem.CurGameSpeedValue.Value = 1;

        yield return new WaitForSeconds(1f);

        //not really dead
        if (this != null)
        {
            GameFinishSequenceStarted = false;
            yield break;
        }

        // //show ui
        // if (GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.CARDOPEN))
        // {
        //     GameRoot.Instance.UISystem.OpenUI<PopupRevival>(popup => popup.Init());
        // }
        // else
        // {
        //     GameRoot.Instance.WaitRealTimeAndCallback(1f, () =>
        //     {
        //         GameRoot.Instance.UISystem.OpenUI<PopupStageResult>(popup => popup.Init(false));
        //     });
        // }
        GameFinishSequenceStarted = false;
    }




    public void ReturnMainScreen(System.Action fadeaction = null)
    {
        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false, false);
        GameRoot.Instance.PluginSystem.HideBanner();

        StageClear();

        fadeaction += StartMainUI;

        GameRoot.Instance.UISystem.OpenUI<PageFade>(page =>
        {
            page.Set(fadeaction);
        }, null, false);
    }

    public void StartMainUI()
    {
        // GameRoot.Instance.UISystem.OpenUI<HUDTotal>(popup =>
        // {
        //     popup.OpenPage(HudBottomBtnType.Fight);
        //     popup.UpdateButtonLock();
        // });

        SoundPlayer.Instance.SetBGMVolume(0.125f);
        SoundPlayer.Instance.RestartBGM();

        GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.Hide();
        GameRoot.Instance.UISystem.OpenUI<PageLobbyBattle>(popup => popup.Init());

        var hud = GameRoot.Instance.UISystem.GetUI<HUDTotal>();

        GameRoot.Instance.UISystem.OpenUI<HUDTotal>();

        hud.RegisterContentsOpen();
        hud.EnqueueTutorialContentsOpen();
        hud.OpenPage(HudBottomBtnType.BATTLE, forceOpen: true);


        GameRoot.Instance.ActionQueueSystem.OnGameFinishCall();
    }

    public void EquipTutorialCheck()
    {
        //리롤 튜토리얼 체크
        if (GameRoot.Instance.UserData.Waveidx.Value == 1 && GameRoot.Instance.UserData.Stageidx.Value == 1)
        {
            if (GameRoot.Instance.UserData.Tutorial.Contains(TutorialSystem.Tuto_2))
            {
                GameRoot.Instance.UserData.Tutorial.Remove(TutorialSystem.Tuto_2);
            }

            GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_2);
        }
        else if (GameRoot.Instance.UserData.Waveidx.Value == 1 && GameRoot.Instance.UserData.Stageidx.Value == 2)
        {
            if (GameRoot.Instance.UserData.Tutorial.Contains(TutorialSystem.Tuto_4))
            {
                GameRoot.Instance.UserData.Tutorial.Remove(TutorialSystem.Tuto_4);
            }

            GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_4);
        }
    }

    public void StopWave()
    {
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
            currentWaveCoroutine = null;
        }

        // 웨이브 중지 시 휴식 상태로 전환하여 TileWeaponComponent 드래그 가능하도록 설정
    }


    public void StartWave()
    {
        // 이미 웨이브가 진행 중이면 무시
        if (currentWaveCoroutine != null)
        {
            return;
        }

        currentWaveCoroutine = StartCoroutine(StartWaveCoroutine());

    }

    public void StartNextWaveCheck()
    {
        GameRoot.Instance.UserData.Waveidx.Value += 1;


        var wavecount = Tables.Instance.GetTable<WaveInfo>().DataList.FindAll(x => x.stage == GameRoot.Instance.UserData.Stageidx.Value).Count;

        bool isend = GameRoot.Instance.UserData.Waveidx.Value > wavecount;


        if (isend)
        {
            GameRoot.Instance.UISystem.OpenUI<PopupStageResult>(popup => popup.Set(true));
        }
        else
        {
            StartWave();
        }
    }


    private int StartWaveOrder = 1;

    private IEnumerator StartWaveCoroutine()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var Waveidx = GameRoot.Instance.UserData.Waveidx.Value;

        StartWaveOrder = 1;

        var tdlists = Tables.Instance.GetTable<WaveInfo>().DataList.FindAll(x => x.stage == stageidx && x.wave == Waveidx);

        foreach(var td in tdlists)
        {
            while(StartWaveOrder <= td.order)
            {
                for(int i = 0; i < td.unit_idx.Count; ++i)
                {
                    EnemyUnitGroup.AddEnemyUnit(td.unit_idx[i], i, td.unit_hp[i]);
                }

                StartWaveOrder++;

                yield return new WaitForSeconds(td.unit_appear_time / 100);

            }
        }
    }


    private float oneseconddeltime = 0f;    

    void Update()
    {
        if(!GameRoot.Instance.UserData.InGamePlayerData.IsGameStartProperty.Value) return;


        oneseconddeltime += Time.deltaTime;

        if(oneseconddeltime >= 1)
        {
            oneseconddeltime = 0f;
            GameRoot.Instance.UserData.InGamePlayerData.WaveTimePorperty.Value += 1;

            if(GameRoot.Instance.UserData.InGamePlayerData.WaveTimePorperty.Value >= 20)
            {
                GameRoot.Instance.UserData.InGamePlayerData.WaveTimePorperty.Value = 0;
                StartNextWaveCheck();
            }
        }
    }
}
