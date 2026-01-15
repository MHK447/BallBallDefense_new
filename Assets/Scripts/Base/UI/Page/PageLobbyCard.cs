using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UniRx;


[UIPath("UI/Page/PageLobbyCard")]
public class PageLobbyCard : CommonUIBase
{
    [SerializeField]
    private List<GameObject> LobbyCardList = new List<GameObject>();


    [SerializeField]
    private Transform WeaponCardRoot;

    [SerializeField]
    private Transform UnitCardRoot;

    [SerializeField]
    private GameObject CardPrefab;

    [SerializeField]
    private VerticalLayoutGroup LobbyCardLayout;


    [SerializeField]
    private Button CardBuyBtn;

    [SerializeField]
    private TextMeshProUGUI CardButtonText;

    [SerializeField]
    private TextMeshProUGUI CardBuyText;

    private int BuyOneCardPrice = 0;

    private CompositeDisposable disposables = new CompositeDisposable();


    protected override void Awake()
    {
        base.Awake();
        CardBuyBtn.onClick.AddListener(OnClickPurchase);
    }


    public void Init(bool isanim = true)
    {

        BuyOneCardPrice = 20;

        CardSet();

        foreach (var obj in LobbyCardList)
        {
            ProjectUtility.SetActiveCheck(obj, false);
        }

        var cardlist = Tables.Instance.GetTable<CardInfo>().DataList.OrderBy(x => x.unlock_stage).ToList();

        // 먼저 모든 카드를 정상 스케일로 생성하고 활성화
        List<LobbyCardComponent> cardComponents = new List<LobbyCardComponent>();
        foreach (var card in cardlist)
        {
            var getcachedobj = GetCachedObject(card.card_type).GetComponent<LobbyCardComponent>();

            if (getcachedobj != null)
            {
                // 레이아웃 계산을 위해 반드시 활성화 상태여야 함
                ProjectUtility.SetActiveCheck(getcachedobj.gameObject, true);
                getcachedobj.Set(card.card_idx);
                getcachedobj.transform.localScale = Vector3.one; // 먼저 정상 크기로

                // CanvasGroup으로 투명하게 (레이아웃은 유지)
                var canvasGroup = getcachedobj.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = getcachedobj.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f; // 보이지 않게

                cardComponents.Add(getcachedobj);
            }
        }

        // DOTween의 DelayedCall을 사용하여 레이아웃이 잡힌 후 애니메이션 시작
        DOVirtual.DelayedCall(0.01f, () =>
        {
            // 다시 한번 레이아웃 갱신
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(LobbyCardLayout.transform as RectTransform);

            // 이제 애니메이션 시작
            int index = 0;
            foreach (var component in cardComponents)
            {
                // 스케일 애니메이션과 알파 애니메이션 동시에
                component.transform.localScale = Vector3.zero;
                float delay = index * 0.05f;
                component.transform.DOScale(Vector3.one, 0.3f)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBack);

                var canvasGroup = component.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(1f, 0.3f)
                        .SetDelay(delay)
                        .SetEase(Ease.OutQuad);
                }

                index++;
            }
        });

        disposables.Clear();

        GameRoot.Instance.UserData.Material.Subscribe(x =>
        {
            CardSet();
        }).AddTo(disposables);
    }

    private GameObject GetCachedObject(int type)
    {
        var inst = LobbyCardList.Find(x => !x.gameObject.activeSelf);

        if (inst == null)
        {
            inst = GameObject.Instantiate(CardPrefab);
        }

        if (type == 1)
        {
            inst.transform.SetParent(UnitCardRoot);
        }
        else if (type == 2)
        {
            inst.transform.SetParent(WeaponCardRoot);
        }


        inst.transform.localScale = UnityEngine.Vector3.one;


        if (!LobbyCardList.Contains(inst))
        {
            LobbyCardList.Add(inst);
        }

        return inst;
    }

    private int GetBuyCount()
    {
        int result = (int)(GameRoot.Instance.UserData.Material.Value / BuyOneCardPrice);
        result = Mathf.Clamp(result, 0, 10);
        return result;
    }


    public void OnClickPurchase()
    {
        if (GameRoot.Instance.TutorialSystem.IsActive("6"))
        {
            int buyCount = GetBuyCount();

            GameRoot.Instance.UISystem.OpenUI<PageGachaSkillCard>(page => page.ChoiceCard(103, 1), () => Init(false));

            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, -BuyOneCardPrice * buyCount);
        }
        else if (GameRoot.Instance.UserData.Material.Value >= BuyOneCardPrice)
        {
            int buyCount = GetBuyCount();

            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, -BuyOneCardPrice * buyCount);

            GameRoot.Instance.UISystem.OpenUI<PageGachaSkillCard>(page => page.Init(buyCount), () =>
            {
                Init(false);
            });
        }
    }



    public void CardSet()
    {
        int buyCount = GetBuyCount();
        int showBuyCount = Mathf.Max(1, buyCount);
        int buyPrice = showBuyCount * BuyOneCardPrice;

        CardBuyText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_card_puchase_1", showBuyCount);

        CardButtonText.text = buyPrice.ToString();
        CardBuyBtn.interactable = GameRoot.Instance.UserData.Material.Value >= buyPrice || GameRoot.Instance.TutorialSystem.IsActive("2");
    }


    void OnDestroy()
    {
        disposables.Clear();
    }


    void OnDisable()
    {
        disposables.Clear();
    }

}

