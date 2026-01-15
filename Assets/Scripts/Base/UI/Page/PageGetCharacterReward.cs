using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Page/PageGetCharacterReward")]
public class PageGetCharacterReward : CommonUIBase
{

    [SerializeField]
    private TextMeshProUGUI DescText;

    [SerializeField]
    private TextMeshProUGUI WeaponNameText;

    [SerializeField]
    private Image WeaponImg;

    
    private int CardIdx = 0;

    protected override void Awake()
    {
        base.Awake();

    }

    public void Set(int cardidx)
    {
        
    }

    public void OnClickAdReward()
    {
        Hide();
        //GameRoot.Instance.UserData.Unitgroupdata.AddUnit(UnitIdx);

    }
}

