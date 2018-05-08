--[===[
Author: 柯明余
Time:   2017-02-21
Note:   工具类
]===]

ToolHelper = {};
local this = ToolHelper

--设置panel渲染深度
function ToolHelper.SetPanelDepthRenderer(panel, depth, startingRenderQueue)
    panel.depth = depth;
    panel.startingRenderQueue = startingRenderQueue;
end

--重置面板位置
function ToolHelper.ResetPanelPosition(panel)
    panel.transform.localPosition = Vector3.zero
    panel.clipOffset = Vector2.zero
end

--清除root目录下的所有子物体
function ToolHelper.ClearGameObject(root)
    local count = root.childCount
    for i=count-1, 0, -1 do
        local go = root:GetChild(i).gameObject
        go:SetActive(false)
        destroy(go)
    end
end

--阿拉伯数字转大写
function ToolHelper.NumberToUpper(num)
    local str = LANGUAGE_TIP.CaptitalNumbers[1]
    if num >= 1 and num <= 10 then
        str = LANGUAGE_TIP.CaptitalNumbers[num]
    end
    return str
end 

--设置货币图标
--@currencyType  货币类型
--@spr           货币精灵
function ToolHelper.SetCurrencySprite(currencyType, spr)
    spr.atlas = Util.loadAtlas("Atlas/common/common")
    
    local spriteName = "ui_icon_l_gold"
	if currencyType == 1 then
        spriteName = "ui_icon_l_gold"
    elseif currencyType == 2 then
        spriteName = "ui_icon_l_goldlock"
    elseif currencyType == 3 then
        spriteName = "ui_icon_shengwang"
    elseif currencyType == 4 then
        spriteName = "ui_icon_rongyu"
    elseif currencyType == 5 then
        spriteName = "ui_icon_gongxun"        
    elseif currencyType == 6 then
        spriteName = "ui_icon_lilian"
    elseif currencyType == 7 then
        spriteName = "ui_icon_bossjifen"
        spr.atlas = Util.loadAtlas("Atlas/interface1/interface1")
    end
	spr.spriteName = spriteName
    spr:MakePixelPerfect()
end

--设置物品稀有度
function ToolHelper.SetItemRare(rare, spr)
    local spriteName = ""
	if rare == 1 then 
		spriteName = "ui_box_green"
	elseif rare == 2 then
		spriteName = "ui_box_blue"
    elseif rare == 3 then
    	spriteName = "ui_box_purple"
    elseif rare == 4 then
    	spriteName = "ui_box_orange"
    elseif rare == 5 then
    	spriteName = "ui_box_golden"
    else
        spriteName = "ui_box_white"
    end
    spr.atlas = Util.loadAtlas("Atlas/common/common")
    spr.spriteName = spriteName
end


--设置道具名字颜色       
function ToolHelper.SetItemNameColor(text ,rare)
    local str = nil
    if rare == 1 then       --绿
        str = "[13af3f]"..text.."[-]"
    elseif rare == 2 then   --蓝
        str = "[1f93d6]"..text.."[-]"
    elseif rare == 3 then   --紫
        str = "[be28cc]"..text.."[-]"
    elseif rare == 4 then   --金
        str = "[e86800]"..text.."[-]"
    elseif rare == 5 then   --橙
        str = "[ff0000]"..text.."[-]"
    end
    return str
end

--设置道具名字颜色       
function ToolHelper.SetItemNameColorByBlack(text ,rare)
    local str = nil
    if rare == 1 then       --绿
        str = "[a7ff77]"..text.."[-]"
    elseif rare == 2 then   --蓝
        str = "[2bd5ff]"..text.."[-]"
    elseif rare == 3 then   --紫
        str = "[f161ff]"..text.."[-]"
    elseif rare == 4 then   --金
        str = "[ff9946]"..text.."[-]"
    elseif rare == 5 then   --橙
        str = "[ff5050]"..text.."[-]"
    end
    return str
end

--奖励列表数据转换
function ToolHelper.AwardListConvert(list)
    local itemList = {}
    local ItemXls = require("xlsdata/Item/ItemXls")
    --经验图标编号：10102004
    if list.exp ~= nil and list.exp ~= 0 then
        table.insert( itemList, {itemId=10102004, count=list.exp} )
    end
    for k,v in pairs(list.item) do
        if type(k) == "number" then
            table.insert( itemList, {itemId=k, count=v} )
        end
    end
    return itemList
end

--设置奖励数据
function ToolHelper.SetAwardData(itemList, dataList, grid, count)
    local awardList = this.AwardListConvert(dataList)
    for i=1,count do
        local item = awardList[i]
        if item ~= nil then
            itemList[i]:SetData(item.itemId, item.count)
            itemList[i]:Refresh()
            itemList[i].gameObject:SetActive(true)
        else
            itemList[i].gameObject:SetActive(false)
        end
    end
    grid.repositionNow = true
end

function ToolHelper.SetAwardData2(itemList, dataList, grid, count)
    for i=1,count do
        local item = itemList[i]
        local data = dataList[i]
        if data then
            item:SetData(data.id, data.count, data.isBind)
            item:Refresh()
            item.gameObject:SetActive(true)
        else
            item.gameObject:SetActive(false)
        end
    end
    grid.repositionNow = true
end

--保留N位小数
function ToolHelper.GetPreciseDecimal(nNum, n)
    if type(nNum) ~= "number" then
        return 0;
    end
    
    n = n or 0;
    n = math.floor(n)
    local fmt = '%.' .. n .. 'f'
    local nRet = tonumber(string.format(fmt, nNum))

    return nRet;
end

function ToolHelper.GetDan(level)
    local name = "无"
    if level >= 1 and level <= 5 then
        name = LANGUAGE_TIP.DanGrading[level]
    end
    return name
end


function ToolHelper.GetGodNickName(step)
    local str = ""
    if step and step >= 1 and step <= 5 then
        str = LANGUAGE_TIP.GodNickName[step]
    end
    return str
end

function ToolHelper.SetFighScore(Ap , Dp , HpMax , HitRate , Dodge , Double , Tenacity , DoubleHurt, Hurt , ReHurt)
    local countScore = nil
    countScore =  math.floor(Ap * 5 + Dp * 5 + HpMax * 0.5 + (HitRate + Dodge + Double + Tenacity)*15 + DoubleHurt*2 + Hurt*3 + ReHurt*3)
    return countScore
end

--互斥状态检查
function ToolHelper.CheckState()
    local BangHuiLogic = require("Logic/BangHuiLogic")
    if  HERO.IsYunBiao > 0 then CtrlManager.PopUpNotifyText(LANGUAGE_TIP.playMutexTip2)  return true end
    if BangHuiLogic.isInFactionScene then CtrlManager.PopUpNotifyText(LANGUAGE_TIP.playMutexTip4)  return true end
    if MultiCopyManager.isInMultiCopyScene then CtrlManager.PopUpNotifyText(LANGUAGE_TIP.playMutexTip3)  return true end
    local ClientFightManager = require('Logic/ClientFightManager')
    if WangfuSnatchManager.isInWangfuSnatchScene then CtrlManager.PopUpNotifyText(LANGUAGE_TIP.playMutexTip5)  return true end
    if FIGHTMGR.JudgeIsClientFight() then CtrlManager.PopUpNotifyText(LANGUAGE_TIP.playMutexTip3) return true end
    if MarriageLogic.ISBANQUET then CtrlManager.PopUpNotifyText(LANGUAGE_TIP.playMutexTip8)  return true end
    if ImperialExamManager.state == 1 then CtrlManager.PopUpNotifyText(LANGUAGE_TIP.playMutexTip9) return true end
    if MarriageLogic.MarriageCheck() then return true end
    if BangHuiLogic.isInFactionWarScene then
        CtrlManager.PopUpNotifyText(LANGUAGE_TIP.playMutexTip7)
        return true
    end
    local BPMiJingLogic = require("Logic/BPMiJingLogic")
    if BPMiJingLogic.IsInBPMJ then
         CtrlManager.PopUpNotifyText(LANGUAGE_TIP.playMutexTip13)
        return true
    end
    if SecretBossLogic.IsSecretMap then CtrlManager.PopUpNotifyText(LANGUAGE_TIP.playMutexTip12) return true end
    return false
end