using System;
using System.Collections.Generic;
using UnityEngine;

public class CommonTools
{
    /// <summary>
    /// 把颜色码转换成color
    /// </summary>
    /// <param name="colorStr"></param>
    /// <returns></returns>
    public static Color ConvertStringToColor(string colorStr)
    {
        if (colorStr.Length != 9)
        {
            Debug.LogWarning("颜色码位数不对");
            return Color.white;
        }

        float r = Convert.ToInt32(colorStr.Substring(1, 2), 16);
        float g = Convert.ToInt32(colorStr.Substring(3, 2), 16);
        float b = Convert.ToInt32(colorStr.Substring(5, 2), 16);
        float a = Convert.ToInt32(colorStr.Substring(7, 2), 16);

        return new Color(r / 255, g / 255, b / 255, a / 255);
    }

    /// <summary>
    /// 根据shape，套装，时装获取模型
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="setno"></param>
    /// <param name="fashion"></param>
    /// <returns></returns>
    public static int GetPlayerShape(int shape, int setno, int fashion)
    {
        //羊不处理套装
        if (shape == 2010019)
            return shape;

        if (fashion != 0)
        {
            return fashion;
        }
        else if (setno != 0)
        {
            //EquipSetXls setXls = Table.Search<EquipSetXls>(
            //delegate(EquipSetXls target)
            //{
            //    return target.SetNo == setno;
            //});
            //return shape + setXls.ShapeNo;
        }
        return shape;
    }

    /// <summary>
    /// 将奖励字符串转换成奖励
    /// </summary>
    /// <param name="rewardStr"></param>
    /// <returns></returns>
    //public static List<Reward_info> ConvertStringToRewardList(string rewardStr)
    //{
    //    List<Reward_info> retList = new List<Reward_info>();
    //    string[] rewardArr = rewardStr.Split('&');
    //    int len = rewardArr.Length;
    //    for (int i = 0; i < len; i++)
    //    {
    //        string[] reward = rewardArr[i].Split('=');
    //        if (reward.Length < 2)
    //            continue;

    //        Reward_info rewardInfo = new Reward_info();
    //        rewardInfo.tagtype = reward[0];
    //        if (reward[0].Equals(GameConsts.REWARD_TAG_CASH))
    //        {
    //            rewardInfo.name = LanguageZYF.cash;
    //            rewardInfo.count = Convert.ToInt32(reward[1]);
    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_EXP))
    //        {
    //            rewardInfo.name = LanguageZYF.experience;
    //            rewardInfo.count = Convert.ToInt32(reward[1]);

    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_EXP_PARTNER))
    //        {
    //            //这个暂时没有
    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_ITEM))
    //        {
    //            ItemXls xlsInfo = Table.Search<ItemXls>(
    //            delegate(ItemXls target)
    //            {
    //                return target.ItemNo == Convert.ToInt32(reward[1]);
    //            });

    //            if (xlsInfo == null)
    //            {
    //                Debug.Log("找不到奖励的道具" + Convert.ToInt32(reward[1]));
    //                continue;
    //            }

    //            rewardInfo.name = string.Format("<color={0}>{1}</color>", ItemData.RARE_COLOR_CODES[xlsInfo.Rare], xlsInfo.Name);
    //            rewardInfo.count = reward.Length > 2 ? Convert.ToInt32(reward[2]) : 1;
    //            rewardInfo.photo = xlsInfo.IconNo;
    //            rewardInfo.step = xlsInfo.Rare;
    //            rewardInfo.no = xlsInfo.ItemNo;
    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_PARTNER))
    //        {
    //            PartnerXls xlsInfo = Table.Search<PartnerXls>(
    //            delegate(PartnerXls target)
    //            {
    //                return target.PartnerNo == Convert.ToInt32(reward[1]);
    //            });

    //            if (xlsInfo == null)
    //            {
    //                Debug.Log("找不到奖励的随从" + Convert.ToInt32(reward[1]));
    //                continue;
    //            }

    //            rewardInfo.name = string.Format("<color={0}>{1}</color>", ItemData.RARE_COLOR_CODES[xlsInfo.Step], xlsInfo.Name);
    //            rewardInfo.count = reward.Length > 2 ? Convert.ToInt32(reward[2]) : 1;
    //            rewardInfo.photo = xlsInfo.Photo;
    //            rewardInfo.step = xlsInfo.Step;
    //            rewardInfo.no = xlsInfo.PartnerNo;
    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_VIGOR))
    //        {
    //            rewardInfo.name = LanguageZYF.vigor;
    //            rewardInfo.count = Convert.ToInt32(reward[1]);
    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_PHYSICAL))
    //        {
    //            rewardInfo.name = LanguageZYF.physical;
    //            rewardInfo.count = Convert.ToInt32(reward[1]);
    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_BINDYUANBAO))
    //        {
    //            rewardInfo.name = LanguageZYF.lanZhuan;
    //            rewardInfo.count = Convert.ToInt32(reward[1]);
    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_MOPO))
    //        {
    //            rewardInfo.name = LanguageZYF.moPo;
    //            rewardInfo.count = Convert.ToInt32(reward[1]);
    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_MOHUN))
    //        {
    //            rewardInfo.name = LanguageZYF.moHun;
    //            rewardInfo.count = Convert.ToInt32(reward[1]);
    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_XUESHI))
    //        {
    //            rewardInfo.name = LanguageZYF.xueShi;
    //            rewardInfo.count = Convert.ToInt32(reward[1]);
    //        }
    //        else if (reward[0].Equals(GameConsts.REWARD_TAG_JINGTIE))
    //        {
    //            rewardInfo.name = LanguageZYF.jingTie;
    //            rewardInfo.count = Convert.ToInt32(reward[1]);
    //        }
    //        else
    //            continue;

    //        retList.Add(rewardInfo);
    //    }

    //    return retList;
    //}

    /// <summary>
    /// 设置粒子特效go的ui sortorder
    /// </summary>
    public static void SetGameObjectSortOrder(GameObject go, int order)
    {
        if (go == null)
            return;

        Renderer[] renders = go.GetComponentsInChildren<Renderer>();
        foreach (Renderer render in renders)
        {
            render.sortingOrder = order;
        }
    }
}