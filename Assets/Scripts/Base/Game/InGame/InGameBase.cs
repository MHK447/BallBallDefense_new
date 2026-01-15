using UnityEngine;
using UnityEngine.AddressableAssets;

public class InGameBase : InGameMode
{
    private InGameBaseStage stage;

    public InGameBaseStage Stage { get { return stage; } }


    public override void Load()
    {
        base.Load();

        Addressables.InstantiateAsync($"ChapterMap_Base").Completed += (handle) =>
              {
                  var igls = handle.Result.GetComponent<InGameBaseStage>();
                  if (igls != null)
                  {
                      stage = igls;
                      igls.Init();
                  }
              };
    }

    protected override void LoadUI()
    {
        base.LoadUI();

        GameRoot.Instance.InGameSystem.InitPopups();
    }

    public override void UnLoad(bool nextStage = false)
    {
        base.UnLoad(nextStage);
        if (stage != null)
        {
            stage.UnLoad();

            if (!Addressables.ReleaseInstance(stage.gameObject))
                GameObject.Destroy(stage);

            stage = null;
        }
    }


    private void OnDestroy()
    {
        if (stage != null)
        {
            if (!Addressables.ReleaseInstance(stage.gameObject))
                GameObject.Destroy(stage);
        }
    }
}
