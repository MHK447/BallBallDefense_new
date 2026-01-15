using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UniRx;
using Unity.VisualScripting;

public class InGameBtnGroup : MonoBehaviour
{
    [SerializeField]
    private Button RerollBtn;

    public Button GetRerollBtn { get { return RerollBtn; } }

    [SerializeField]
    private Button SlotBtn;

    [SerializeField]
    private Button BattleBtn;

    public Button GetBattleBtn { get { return BattleBtn; } }

    [SerializeField]
    private Transform SlotAddMoneyRoot;

    public Transform GetSlotAddMoneyRoot { get { return SlotAddMoneyRoot; } }

    [SerializeField]
    private TextMeshProUGUI RerollPriceText;

    [SerializeField]
    private TextMeshProUGUI BuySlotPriceText;

    [SerializeField]
    private TextMeshProUGUI BuySlotDescText;

    private int RerollPrice = 0;

    private int BuySlotPrice = 0;

    private int RerollCount = 0;

    private int BuySlotCount = 0;

    private CompositeDisposable disposables = new CompositeDisposable();

    void Awake()
    {
        SlotBtn.onClick.AddListener(OnClickBuySlot);
        RerollBtn.onClick.AddListener(OnClickReroll);
        BattleBtn.onClick.AddListener(OnClickBattle);
    }

    public void Init()
    {
        RerollCount = 0;

        BuySlotCount = 0;

        SetRerollPrice();

        SetBuySlotPrice();

        RerollPriceText.text = RerollPrice.ToString();

        BuySlotPriceText.text = BuySlotPrice.ToString();


        disposables.Clear();

        // GameRoot.Instance.UserData.Ingamesilvercoin.Subscribe(x =>
        // {
        //     RerollPriceText.color = x >= RerollPrice ? Color.white : Color.red;
        //     BuySlotPriceText.color = x >= BuySlotPrice ? Color.white : Color.red;
        // }).AddTo(disposables);

        ProjectUtility.SetActiveCheck(SlotBtn.gameObject, GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.INGAMESLOTCARDOPEN));
        ProjectUtility.SetActiveCheck(RerollBtn.gameObject, GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.INGAMEREROLL));

        ProjectUtility.SetActiveCheck(SlotAddMoneyRoot.gameObject, GameRoot.Instance.UserData.Stageidx.Value != 2);
    }


    public void OnClickReroll()
    {
        if (GameRoot.Instance.UserData.Ingamesilvercoin.Value >= RerollPrice)
        {
            GameRoot.Instance.UserData.Ingamesilvercoin.Value -= RerollPrice;
            RerollCount += 1;
            SetRerollPrice();

            var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

            if (stageidx == 1)
            {
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.AddTileWeapon(1, 2);
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.AddTileWeapon(109, 1);
            }
            else
            {
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.StartRandSelectWeapon(2);
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.StartRandSelectBag();
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.RandAdCheck();
            }
        }
        else
        {
            GameRoot.Instance.UISystem.OpenUI<PopupInsufficientCoin>(popup => popup.Init(() =>
            {
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.StartRandSelectWeapon(2);
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.StartRandSelectBag();
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.RandAdCheck();
            }));
        }
    }

    public void OnClickBuySlot()
    {
        if (GameRoot.Instance.TutorialSystem.IsActive(TutorialSystem.Tuto_4))
        {
            ProjectUtility.SetActiveCheck(SlotAddMoneyRoot.gameObject, true);
            GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.StartRanSelectTile(1002);
        }
        else
        {
            if (GameRoot.Instance.UserData.Ingamesilvercoin.Value >= BuySlotPrice)
            {
                GameRoot.Instance.UserData.Ingamesilvercoin.Value -= BuySlotPrice;

                BuySlotCount += 1;
                SetBuySlotPrice();

                BuySlotDescText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_buy_slot_desc", 10 - BuySlotCount);

                if (BuySlotCount == 1)
                {
                    var randidx = Random.Range(1001, 1003);

                    GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.StartRanSelectTile(randidx);
                }
                else
                {
                    GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.StartRanSelectTile();
                }
                ProjectUtility.SetActiveCheck(SlotBtn.gameObject, BuySlotCount > 0);
            }
            else
            {
                GameRoot.Instance.UISystem.OpenUI<PopupInsufficientCoin>(popup => popup.Init(() =>
                {
                    var randidx = Random.Range(1001, 1003);
                    GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.StartRanSelectTile(randidx);
                }));
            }
        }
    }


    public void SetRerollPrice()
    {
        RerollPrice = RerollCount * 10;
        RerollPriceText.text = RerollPrice == 0 ? Tables.Instance.GetTable<Localize>().GetString("str_free") : RerollPrice.ToString();
    }


    public void SetBuySlotPrice()
    {
        BuySlotPrice = 10 + (BuySlotCount * 10);
        BuySlotPriceText.text = BuySlotPrice.ToString();
    }

    void OnDestroy()
    {
        disposables.Dispose();
    }

    void OnDisable()
    {
        disposables.Clear();
    }

    public void OnClickBattle()
    {
        var finditem = GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.GetTileWeaponComponentList.Find(x => x.IsEquip &&
        Tables.Instance.GetTable<EquipInfo>().GetData(x.EquipIdx).equip_purpose == 1);


        if (finditem == null)
        {
            // equip_purpose가 1인 장착 아이템이 없으면 PopupResultSure 오픈
            GameRoot.Instance.UISystem.OpenUI<PopupResultSure>(popup =>
                popup.Init(() =>
                {
                    NextWaveStart();
                }));
        }
        else
        {
            SoundPlayer.Instance.PlaySound("sfx_wave_Start");
            NextWaveStart();
        }


    }


    public void NextWaveStart()
    {
        GameRoot.Instance.UserData.InGamePlayerData.IsWaveRestProperty.Value = false;

        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.StartWave();

        GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.RefreshAllWeaponsMergeCheck();

        GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.SellNoneEquipWeapon();
    }
}

