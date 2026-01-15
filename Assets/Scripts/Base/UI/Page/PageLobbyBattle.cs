using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DG.Tweening;

[UIPath("UI/Page/PageLobbyBattle")]
public class PageLobbyBattle : CommonUIBase
{
    [SerializeField]
    private Button StartBtn;

    [SerializeField]
    private Button SettingBtn;

    [SerializeField]
    private LobbyBattleBoxComponent LobbyBattleBoxComponent;

    public LobbyBattleBoxComponent GetLobbyBattleBoxComponent { get { return LobbyBattleBoxComponent; } }

    [SerializeField]
    private GameObject BoxRoot;

    [SerializeField]
    private TextMeshProUGUI StageNameText;

    [SerializeField]
    private TextMeshProUGUI StageText;

    [SerializeField]
    private LobbyStageRewardGroup RewardGroup;

    [SerializeField]
    private ChapterMapComponent MapComponent;

    [SerializeField]
    private GameObject BossStageImg;

    [SerializeField]
    private GameObject HardStageImg;

    [SerializeField]
    private TextMeshProUGUI BossDiffcultlyText;



    override protected void Awake()
    {
        base.Awake();
        StartBtn.onClick.AddListener(OnStartBtnClick);
        SettingBtn.onClick.AddListener(OnClickSetting);
    }

    public void OnStartBtnClick()
    {
        Hide();

        StartBtn.interactable = false;
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.StartBattle();
    }

    public override void OnShowBefore()
    {
        base.OnShowBefore();
        Init();
    }

    public void Init()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        StartBtn.interactable = true;

        LobbyBattleBoxComponent.Init();

        RewardGroup.Init();

        SetStage();

        MapComponent.transform.localScale = Vector3.zero;
        MapComponent.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        MapComponent.Set(stageidx);

        ProjectUtility.SetActiveCheck(RewardGroup.gameObject,
         GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.LobbyReward));

    }

    public void OnClickSetting()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupOption>();
    }


    public void SetStage()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var td = Tables.Instance.GetTable<StageInfo>().DataList.ToList().FindAll(x => x.stage_idx == stageidx).LastOrDefault();

       
        StageNameText.text = $"{stageidx}.{Tables.Instance.GetTable<Localize>().GetString($"str_stage_name_{stageidx}")}";
        StageText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_main_stage_desc", stageidx);

        ProjectUtility.SetActiveCheck(BossStageImg , td.diffiyculty == 2);
        ProjectUtility.SetActiveCheck(HardStageImg , td.diffiyculty == 1);

        BossDiffcultlyText.text = Tables.Instance.GetTable<Localize>().GetFormat($"str_difficultly_stage_{td.diffiyculty}");


    }

    public void OnClickOption()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupOption>();
    }

}
