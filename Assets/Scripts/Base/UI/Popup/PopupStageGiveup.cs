using UnityEngine;
using BanpoFri;
using UniRx;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Popup/PopupStageGiveup")]
public class PopupStageGiveup : UIBase
{
    [SerializeField]
    private Button GiveupBtn;


    protected override void Awake()
    {
        base.Awake();
        GiveupBtn.onClick.AddListener(OnClickGiveup);
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(true);
    }


    public void OnClickGiveup()
    {
        Hide();
        
        // 모든 유닛 비활성화
        DeactivateAllUnits();
        
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.ReturnMainScreen();
    }

    private void DeactivateAllUnits()
    {
        var stage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;
        
        if (stage == null) return;

    }

    public override void Hide()
    {
        base.Hide();
        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false);
    }
}
