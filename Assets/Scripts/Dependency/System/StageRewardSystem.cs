using UnityEngine;
using BanpoFri;
public class StageRewardSystem
{

    public double StageRewardpercent(int stageidx)
    {

        var stagetdlist = Tables.Instance.GetTable<StageInfo>().DataList.FindAll(x => x.stage_idx == stageidx);


        var unitcount = 0;



        // foreach (var stagetd in stagetdlist)
        // {
        //     unitcount += stagetd.enemy_idx.Count;
        // }


        return ProjectUtility.PercentGet(unitcount, GameRoot.Instance.UserData.InGamePlayerData.KillCountProperty.Value);
    }


    public System.Numerics.BigInteger GetStageRewardMoney(int count = 1)
    {
        System.Numerics.BigInteger value = 0;

        var curstageidx = GameRoot.Instance.UserData.Stageidx.Value;


        for (int i = 0; i < count; ++i)
        {
            var rewardtd = Tables.Instance.GetTable<StageRewardInfo>().GetData((int)Config.CurrencyID.Money);
        }
        

        return curstageidx == 1 ? 50 : value / 100;
    }




    public System.Numerics.BigInteger GetFailedReward(int rewaridx)
    {
        var reward = GetStageRewardMoney();

        return reward / GameRoot.Instance.UserData.InGamePlayerData.KillCountProperty.Value;
    }

}
