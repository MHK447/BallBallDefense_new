using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

using DG.Tweening;
using Unity.VisualScripting;
[UIPath("UI/Popup/PopupInGame", true)]
public class PopupInGame : CommonUIBase
{


    [SerializeField]
    private Button PauseBtn;

    public Transform SilverCoinRoot;

    private CompositeDisposable disposables = new CompositeDisposable();


    protected override void Awake()
    {
        base.Awake();

        PauseBtn.onClick.AddListener(OnClickPause);
    }


    public void Init()
    {
        

        
    }

    public void OnClickPause()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupStageGiveup>();
    }

    void OnDisable()
    {
        disposables.Clear();
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

}
