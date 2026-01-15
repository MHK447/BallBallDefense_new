using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TileComponent : MonoBehaviour
{

    [SerializeField]
    private Image TileImg;

    [SerializeField]
    public GameObject TileUnLockObj;
    
    [SerializeField]
    private int TileOrder = 0;

    public int GetTileOrder { get { return TileOrder; } }


    [HideInInspector]
    public TileWeaponComponent TargetTileWeaponComponent;

    

    [HideInInspector]
    public bool IsUnLock = false;

    public Vector2 TileOrderVec = Vector2.zero;

    public void Init()
    {
        var curstageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var stagetd = Tables.Instance.GetTable<StageInfo>().GetData(curstageidx);

        IsUnLock = stagetd.spawn_tile_order.Contains(TileOrder);

        ProjectUtility.SetActiveCheck(TileUnLockObj, false);
        ProjectUtility.SetActiveCheck(this.gameObject , IsUnLock);

        
    }


    public void UnLockOnTile()
    {
        ProjectUtility.SetActiveCheck(TileUnLockObj, true);
        TileImg.color = Config.Instance.GetImageColor("AddTileEquip_Color");
    }


    public void SetTileComponent(TileWeaponComponent tileweaponcomponent)
    {
        if(TargetTileWeaponComponent != null)
        {
            TargetTileWeaponComponent.IsEquip = false;
        }

        TargetTileWeaponComponent = tileweaponcomponent;
        
    }

    




    public void TileColorChange(Color color)
    {
        TileImg.color = color;
    }
}

