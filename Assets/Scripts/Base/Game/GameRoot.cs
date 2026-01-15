using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BanpoFri;
using UniRx;
using UniRx.Async;

public class GameRoot : Singleton<GameRoot>
{
	[SerializeField]
	private Transform MainUITrans;
	[SerializeField]
	private Transform HUDUITrans;
	[SerializeField]
	public Canvas MainCanvas;
	[SerializeField]
	private Canvas WorldCanvas;
	[SerializeField]
	private GameObject CheatWindow;

	[SerializeField]
	private GameObject DebugConsoleObj;


	[HideInInspector]
	public LoadingBasic Loading;

	[HideInInspector]
	public HudCurrencyTop CurrencyTop;


	public InAppPurchaseManager InAppPurchaseManager;


	public RectTransform GetMainCanvasTR { get { return MainCanvas.transform as RectTransform; } }
	public UISystem UISystem { get; private set; } = new UISystem();
	public UserDataSystem UserData { get; private set; } = new UserDataSystem();
	public TutorialSystem TutorialSystem { get; private set; } = new TutorialSystem();
	public PlayTimeSystem PlayTimeSystem { get; private set; } = new PlayTimeSystem();
	public InGameSystem InGameSystem { get; private set; } = new InGameSystem();
	public PluginSystem PluginSystem { get; private set; } = new PluginSystem();
	public BoostSystem BoostSystem { get; private set; } = new BoostSystem();
	public EffectSystem EffectSystem { get; private set; } = new EffectSystem();

	public PlayerSystem PlayerSystem { get; private set; } = new PlayerSystem();

	public GameNotificationSystem GameNotification { get; private set; } = new GameNotificationSystem();

	public ContentsOpenSystem ContentsOpenSystem { get; private set; } = new ContentsOpenSystem();

	public ShopSystem ShopSystem { get; private set; } = new ShopSystem();

	public SceneSystem SceneSystem { get; private set; } = new SceneSystem();

	public ActionQueueSystem ActionQueueSystem { get; private set; } = new ActionQueueSystem();

	public StageRewardSystem StageRewardSystem { get; private set; } = new StageRewardSystem();

	public InGameUpgradeSystem InGameUpgradeSystem { get; private set; } = new InGameUpgradeSystem();

	public GameSpeedSystem GameSpeedSystem { get; private set; } = new GameSpeedSystem();

	public AlimentSystem AlimentSystem { get; private set; } = new AlimentSystem();

	public UnitSystem UnitSystem { get; private set; } = new UnitSystem();

	public TrainingSystem TrainingSystem { get; private set; } = new TrainingSystem();

	public LobbyBoxSystem LobbyBoxSystem { get; private set; } = new LobbyBoxSystem();

	public DamageTextSystem DamageTextSystem { get; private set; } = new DamageTextSystem();

	public TileSystem TileSystem { get; private set; } = new TileSystem();

	public CardSystem CardSystem { get; private set; } = new CardSystem();


	public UnityMainThreadDispatcher MainThreadDispatcher;

	private Queue<System.Action> PauseActions = new Queue<System.Action>();

	public GameObject UILock;
	private static bool InitTry = false;
	public static bool LoadComplete { get; private set; } = false;
	private float deltaTime = 0f;
	public GameType CurInGameType { get; private set; } = GameType.Main;
	private Queue<System.Action> TouchStartActions = new Queue<System.Action>();
	public Queue<System.Action> TitleCloseActions = new Queue<System.Action>();

	private int loadcount = 0;
	public static bool IsInit()
	{


		if (instance != null && !InitTry)
			Load();

		return instance != null;


	}

	public static void Load()
	{
		InitTry = true;
		Addressables.InstantiateAsync("GameRoot").Completed += (handle) =>
		{
			instance = handle.Result.GetComponent<GameRoot>();
			instance.name = "GameRoot";
		};
	}

	public void AddTouchAction(System.Action cb)
	{
		TouchStartActions.Enqueue(cb);
	}

	public void AddTitleCloseAction(System.Action cb)
	{
		TitleCloseActions.Enqueue(cb);
	}


	IEnumerator waitEndFrame(System.Action callback)
	{
		yield return new WaitForEndOfFrame();

		callback?.Invoke();
	}

	public void WaitEndFrameCallback(System.Action callback)
	{
		StartCoroutine(waitEndFrame(callback));
	}

	void InitUILoading()
	{
		// 로딩 팝업 어드레서블 로드
		var path = UISystem.GetUIPath<LoadingBasic>();
		if (!string.IsNullOrEmpty(path))
		{
			Addressables.InstantiateAsync(path, MainUITrans, false).Completed += (handle) =>
			{
				Loading = handle.Result.GetComponent<LoadingBasic>();
				loadcount++;
			};
		}
	}

	private void Update()
	{
		if (!LoadComplete)
			return;

		UserData.Update();
		PlayTimeSystem.Update();
		AlimentSystem.Update();

		if (deltaTime >= 1f) // one seconds updates;
		{
			deltaTime -= 1f;
			ShopSystem.UpdateOneSecond();
			ShopSystem.UpdateOneTimeSecond();
			LobbyBoxSystem.UpdateOneSecond();
		}
		deltaTime += Time.deltaTime;

		if (Input.GetMouseButtonUp(0))
		{
			if (InGameSystem.CurInGame != null)
			{
				Vector2 localPos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(MainCanvas.transform as RectTransform, Input.mousePosition,
					MainCanvas.worldCamera, out localPos);


				if (!IsPointerOverUIObject(Input.mousePosition))
				{
				}


				if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
				{


					if (TouchStartActions.Count > 0)
					{
						var cnt = TouchStartActions.Count;
						for (int i = 0; i < cnt; i++)
						{
							TouchStartActions.Dequeue().Invoke();
						}
					}
				}
			}
		}



	}

	public bool IsPointerOverUIObject(Vector2 touchPos)
	{
		UnityEngine.EventSystems.PointerEventData eventDataCurrentPosition
			= new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);

		eventDataCurrentPosition.position = touchPos;

		List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();


		UnityEngine.EventSystems.EventSystem.current
		.RaycastAll(eventDataCurrentPosition, results);

		return results.Count > 0;
	}

	public void DestroyObj(GameObject obj)
	{
		Destroy(obj);
	}

	public void InitCurrencyTop(System.Action successcallback = null)
	{
		if (CurrencyTop == null)
		{
			UISystem.OpenUI<HudCurrencyTop>(x =>
			{
				CurrencyTop = x;
				successcallback?.Invoke();
			});
		}
	}

	IEnumerator Start()
	{
		if (instance == null)
		{
			Load();
			Destroy(this.gameObject);
			yield break;
		}

#if BANPOFRI_LOG
		DebugConsoleObj.SetActive(true);
#else
		DebugConsoleObj.SetActive(false);
#endif

		//TouchStartActions.Clear();
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		PluginSystem.Init();
		InAppPurchaseManager = GetComponent<InAppPurchaseManager>();
		if (InAppPurchaseManager != null)
			InAppPurchaseManager.Init();
		//SnapshotCam = SnapshotCamera.MakeSnapshotCamera("SnapShot");
		//SnapshotCam.transform.SetParent(this.transform);
		//SnapshotCam.transform.position = new Vector3(0f, 0f, -1f);
		yield return TimeSystem.GetGoogleTime(null);
		UISystem.SetMainUI(MainUITrans);
		UISystem.SetHudUI(HUDUITrans);
		UISystem.SetWorldCanvas(WorldCanvas);
		UISystem.LockScreen = UILock;
		yield return LoadGameData();
		//Application.deepLinkActivated += OnDeepLinkActivated;
		if (!string.IsNullOrEmpty(Application.absoluteURL))
		{
			// Cold start and Application.absoluteURL not null so process Deep Link.
			//OnDeepLinkActivated(Application.absoluteURL);
		}
	}

	private IEnumerator LoadGameData()
	{

		AtlasManager.Instance.Init();
		yield return Config.Create();
		yield return Tables.Create();
		yield return SoundPlayer.Create();

		// 로딩 팝업 어드레서블 로드
		loadcount = 0;
		InitUILoading();

		yield return new WaitUntil(() => loadcount == 1);
		UserData.Load();
		InGameSystem.ChangeMode(CurInGameType);

		LoadComplete = true;

		InitSystem();

		GameNotification.Create();
		ShopSystem.Create();
		LobbyBoxSystem.Create();
		CardSystem.Create();
		GameRoot.instance.InAppPurchaseManager.InitializePurchasing();

		InitRequestAtlas();



		GameRoot.instance.WaitTimeAndCallback(1f, () =>
		{
			//JoyStick.Init();
			BgmOn();
			// 로딩 숨김 추가!
			Loading.Hide(true);
		});


	}

	public void BgmOn()
	{
		if (GameRoot.instance.CurInGameType == GameType.Event)
		{
			SoundPlayer.Instance.PlayBGM("bgm_pirate", true);
			SoundPlayer.Instance.BgmSwitch(UserData.Bgm);
		}
		else
		{
			SoundPlayer.Instance.PlayBGM("bgm", true);
			SoundPlayer.Instance.BgmSwitch(UserData.Bgm);
		}
	}


	void InitRequestAtlas()
	{
		AtlasManager.Instance.ReLoad(false);
	}

	public void ChangeIngameType(GameType type, bool changeData = false)
	{

		CurInGameType = type;
		//InGameSystem.ChangeMode(CurInGameType);
		if (changeData)
		{
			DataState dataState = DataState.None;
			switch (CurInGameType)
			{
				case GameType.Main:
					{
						dataState = DataState.Main;
					}
					break;
				case GameType.Event:
					{
						dataState = DataState.Event;
					}
					break;
			}
			if (dataState != DataState.None)
			{
				UserData.ChangeDataMode(dataState);
				GameRoot.Instance.UserData.CurMode.LastLoginTime = TimeSystem.GetCurTime();
			}
		}
	}

	void InitSystem()
	{
		QualitySettings.vSyncCount = 0;  // VSync 비활성화
		Application.targetFrameRate = 60;  // (제한 없음)

		var count = GameRoot.instance.UserData.GetRecordCount(Config.RecordCountKeys.Init);

		if (GameRoot.Instance.UserData.Stageidx.Value == 1 && count == 0)
		{
			if (UserData.UUID == 0)
				UserData.SetUUID(ProjectUtility.GetUUID());

			GameRoot.instance.UserData.AddRecordCount(Config.RecordCountKeys.Init, 1);
			SetNativeLanguage();

			PluginSystem.InitMax(() =>
			{
				PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, "install");
			});

		}
		else
		{

			PluginSystem.InitMax();

			Config.Instance.UpdateFallbackOrder(UserData.Language);
		}


		InGameUpgradeSystem.Init();
		GameSpeedSystem.Create();
		GameNotification.Create();
		GameNotification.Create();
		ContentsOpenSystem.Create();
		GameSpeedSystem.Create();
		InGameUpgradeSystem.Init();
		UnitSystem.Create();
		TrainingSystem.Create();
		DamageTextSystem.Create();

	}

	private void SetNativeLanguage()
	{
	}

	public void SetCheatWindow(bool value)
	{
		if (CheatWindow != null)
			CheatWindow.SetActive(value);

		// DebugLogManager가 있으면 팝업도 함께 활성화/비활성화
		if (IngameDebugConsole.DebugLogManager.Instance != null)
		{
			IngameDebugConsole.DebugLogManager.Instance.PopupEnabled = value;
		}
	}

	IEnumerator waitFrameAndCallback(int frame, System.Action callback)
	{
		for (int i = 0; i < frame; i++)
			yield return new WaitForEndOfFrame();
		callback?.Invoke();


	}

	IEnumerator waitTimeAndCallback(float time, System.Action callback)
	{
		yield return new WaitForSeconds(time);
		callback?.Invoke();
	}

	IEnumerator waitRealTimeAndCallback(float time, System.Action callback)
	{
		yield return new WaitForSecondsRealtime(time);
		callback?.Invoke();
	}

	public Coroutine WaitStageAndCallback(float time, System.Action callback)
	{
		return StartCoroutine(waitTimeAndCallback(time, callback));
	}

	public Coroutine WaitTimeAndCallback(float time, System.Action callback)
	{
		return StartCoroutine(waitTimeAndCallback(time, callback));
	}

	public Coroutine WaitRealTimeAndCallback(float time, System.Action callback)
	{
		return StartCoroutine(waitRealTimeAndCallback(time, callback));
	}

	public void WaitFrameAndCallback(int frame, System.Action callback)
	{
		StartCoroutine(waitFrameAndCallback(frame, callback));
	}


	private void OnApplicationPause(bool pause)
	{
		if (!LoadComplete)
			return;

		if (pause)
		{
			PluginSystem.OnApplicationPause(true);

			GameRoot.Instance.UserData.mainData.LastLoginTime = TimeSystem.GetCurTime();
		}
		else
		{
			if (InGameSystem.GetInGame<InGameBase>() == null) return;


			if (GameRoot.Instance.TutorialSystem.IsActive())
				return;

			var time = GameRoot.Instance.UserData.mainData.LastLoginTime;

			if (time.Equals(default(System.DateTime)))
				return;




			System.Action NextAction = () =>
			{
				if (PauseActions.Count < 1)
					return;

				var action = PauseActions.Dequeue();
				action.Invoke();
			};


			NextAction.Invoke();
		}



	}


#if UNITY_EDITOR
	private void OnApplicationQuit()
	{
		PluginSystem.OnApplicationPause(true);

		UnityEditor.AssetDatabase.SaveAssets();
		UnityEditor.AssetDatabase.Refresh();
	}
#else
	private void OnApplicationQuit()
	{
		PluginSystem.OnApplicationPause(true);
		
	}
#endif

}
