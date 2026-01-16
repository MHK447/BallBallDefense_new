using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
[UIPath("UI/Popup/PopupRevival")]
public class PopupRevival : CommonUIBase
{
    [SerializeField]
    private TextMeshProUGUI RevivalTimeText;




    [SerializeField]
    private Button CloseBtn;

    [SerializeField]
    private AdsButton RevivalBtn;



    private int RevivalTime = 0;

    protected override void Awake()
    {
        base.Awake();
        CloseBtn.onClick.AddListener(RevivalFailed);

        RevivalBtn.AddListener(TpMaxProp.AdRewardType.Revival, RevivalSuccess);

        RevivalTime = Tables.Instance.GetTable<Define>().GetData("invicible_time").value;
    }


    public void Init()
    {
        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(true, false);

        RevivalTimeText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_name_revival_1", RevivalTime.ToString());


        var revivalcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.FREEREVIVALCOUNT);

        RevivalBtn.SetFree(revivalcount == 0);

    }

    public override void Hide()
    {
        base.Hide();
        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false, false);
    }

    public void RevivalSuccess()
    {
        Hide();

        GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.FREEREVIVALCOUNT, 1);
    }

    public void RevivalFailed()
    {
        Hide();
        GameRoot.Instance.UISystem.OpenUI<PopupStageResult>(popup => popup.Set(false));
    }
}

