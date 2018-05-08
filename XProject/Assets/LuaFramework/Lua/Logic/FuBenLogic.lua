local allJinJieFuBenXls = {}
local allChapterFBXls = {}
local allVipFBXls = {}
local allZhuLingFBXls = {}
local allExpFBXls = {}
local allJiangHuXls = {}
local allShenQiXls = {}
local allWuLinXls = {}
local allAiderXls = {}
local allBHFuBenXls = {}
local allXianJueXls = {}
local allLingYuanXls = {}

local FuBenLogic = {}
local this = FuBenLogic
local GAMECONST = GAMECONST

local changeSceneFunc = nil

--获取进阶副本信息
function FuBenLogic.GetJinJieFbXls(gateno)
	for k, v in pairs(allJinJieFuBenXls) do
		if v.GateNo == gateno then
			return v
		end
	end

	return nil
end

--获取章节副本信息
function FuBenLogic.GetChapterFbXls(gateno)
	for k, v in pairs(allChapterFBXls) do
		if v.GateNo == gateno then
			return v
		end
	end

	return nil
end

--获取vip副本信息
function FuBenLogic.GetVipFbXls(gateno)
	for k, v in pairs(allVipFBXls) do
		if v.GateNo == gateno then
			return v
		end
	end

	return nil
end

--获取注灵副本信息
function FuBenLogic.GetZhuLingFbXls(gateno)
	for k, v in pairs(allZhuLingFBXls) do
		if v.GateNo == gateno then
			return v
		end
	end

	return nil
end

--获取经验副本关卡信息
function FuBenLogic.GetExpFbXls(gateno)
	for k, v in pairs(allExpFBXls) do
		if v.GateNo == gateno then
			return v
		end
	end

	return nil
end

--获取经验副本关卡信息
function FuBenLogic.GetExpFbXlsByLayer(layer)
	for k, v in pairs(allExpFBXls) do
		if v.Layer == layer then
			return v
		end
	end

	return nil
end

--获取江湖试炼表格信息
function FuBenLogic.GetJiangHuFbXls(gateno)
	for k, v in pairs(allJiangHuXls) do
		if v.GateNo == gateno then
			return v
		end
	end

	return nil
end


--获取江湖试炼表格信息
function FuBenLogic.GetJiangHuFbXlsByLayer(layer)
	for k, v in pairs(allJiangHuXls) do
		if v.Layer == layer then
			return v
		end
	end

	return nil
end

--获取神器副本表格信息
function FuBenLogic.GetShenQiFbXls(gateno)
	for k, v in pairs(allShenQiXls) do
		if v.GateNo == gateno then
			return v
		end
	end

	return nil
end


--获取神器副本表格信息
function FuBenLogic.GetShenQiFbXlsByLayer(layer)
	for k, v in pairs(allShenQiXls) do
		if v.Layer == layer then
			return v
		end
	end

	return nil
end

--获取武林试炼表格信息
function FuBenLogic.GetWuLinFbXls(gateno)
	for k, v in pairs(allWuLinXls) do
		if v.GateNo == gateno then
			return v
		end
	end

	return nil
end


--获取武林试炼表格信息
function FuBenLogic.GetWuLinFbXlsByLayer(layer)
	for k, v in pairs(allWuLinXls) do
		if v.Layer == layer then
			return v
		end
	end

	return nil
end

--获取助战伙伴信息
function FuBenLogic.GetAiderInfo(partnerno)
	for k, v in pairs(allAiderXls) do
		if v.PartnerNo == partnerno then
			return v
		end
	end
end

--获取帮派副本表格信息
function FuBenLogic.GetBangHuiFbXls(gateno)
	for k, v in pairs(allBHFuBenXls) do
		if v.GateNo == gateno then
			return v
		end
	end

	return nil
end

--获取帮派副本表格信息
function FuBenLogic.GetBangHuiFbXlsByLayer(layer)
	for k, v in pairs(allBHFuBenXls) do
		if v.Layer == layer then
			return v
		end
	end

	return nil
end

--获取仙决副本信息
function FuBenLogic.GetXianJueFbXls(gateno)
	for k, v in pairs(allXianJueXls) do
		if v.GateNo == gateno then
			return v
		end
	end

	return nil
end

--守卫灵源副本
function FuBenLogic.GetLingYuanFbXls(gateno)
	for k, v in pairs(allLingYuanXls) do
		if v.GateNo == gateno then
			return v
		end
	end
end

function FuBenLogic.GetLingYuanFbXlsByLayer(layer)
	for k, v in pairs(allLingYuanXls) do
		if v.Layer == layer then
			return v
		end
	end
end

--获取副本信息
function FuBenLogic.GetFuBenXlsInfo(ftype, gateno)
	if ftype == GAMECONST.FuBenFType.TYPE_JINJIE then
		return this.GetJinJieFbXls(gateno)
	elseif ftype == GAMECONST.FuBenFType.TYPE_JINGYAN then
		return this.GetExpFbXls(gateno)
	elseif ftype == GAMECONST.FuBenFType.TYPE_ZHANGJIE then
		return this.GetChapterFbXls(gateno)
	elseif ftype == GAMECONST.FuBenFType.TYPE_VIP then
		return this.GetVipFbXls(gateno)
	elseif ftype == GAMECONST.FuBenFType.TYPE_JIANGHU then
		return this.GetJiangHuFbXls(gateno)
	elseif ftype == GAMECONST.FuBenFType.TYPE_SHENQI then
		return this.GetShenQiFbXls(gateno)
	elseif ftype == GAMECONST.FuBenFType.TYPE_WULIN then
		return this.GetWuLinFbXls(gateno)
	elseif ftype == GAMECONST.FuBenFType.TYPE_ZHULING then
		return this.GetZhuLingFbXls(gateno)
	elseif ftype == GAMECONST.FuBenFType.TYPE_BANGHUI then
		return this.GetBangHuiFbXls(gateno)
	elseif ftype == GAMECONST.FuBenFType.TYPE_XIANJUE then
		return this.GetXianJueFbXls(gateno)
	elseif ftype == GAMECONST.FuBenFType.TYPE_LINGYUAN then
		return this.GetLingYuanFbXls(gateno)
	end
end

--获取最大关卡数量
function FuBenLogic.GetFBGateMax(ftype)
	if ftype == GAMECONST.FuBenFType.TYPE_JINJIE then
		return #allJinJieFuBenXls
	elseif ftype == GAMECONST.FuBenFType.TYPE_JINGYAN then
		return #allExpFBXls
	elseif ftype == GAMECONST.FuBenFType.TYPE_ZHANGJIE then
		return #allChapterFBXls
	elseif ftype == GAMECONST.FuBenFType.TYPE_VIP then
		return #allVipFBXls
	elseif ftype == GAMECONST.FuBenFType.TYPE_JIANGHU then
		return #allJiangHuXls
	elseif ftype == GAMECONST.FuBenFType.TYPE_SHENQI then
		return #allShenQiXls
	elseif ftype == GAMECONST.FuBenFType.TYPE_WULIN then
		return #allWuLinXls
	elseif ftype == GAMECONST.FuBenFType.TYPE_ZHULING then
		return #allZhuLingFBXls
	elseif ftype == GAMECONST.FuBenFType.TYPE_BANGHUI then
		return #allBHFuBenXls
	elseif ftype == GAMECONST.FuBenFType.TYPE_XIANJUE then
		return #allXianJueXls
	elseif ftype == GAMECONST.FuBenFType.TYPE_LINGYUAN then
		return #allLingYuanXls
	end
end

---------------------------------------------------------------------------------------
--客户端保存爬塔最大层
function FuBenLogic.UpdatePataFuBenLayer(ftype, curlayer, maxlayer)
	if ftype == GAMECONST.FuBenFType.TYPE_JINGYAN then
		if not this.maxLayerExp then this.maxLayerExp = 0 end
		this.maxLayerExp = maxlayer > this.maxLayerExp and maxlayer or this.maxLayerExp
		this.maxLayerExp = this.maxLayerExp >= (curlayer - 1) and this.maxLayerExp or (curlayer - 1)
		this.curLayerExp = curlayer
	elseif ftype == GAMECONST.FuBenFType.TYPE_JIANGHU then
		if not this.maxLayerJH then this.maxLayerJH = 0 end
		this.maxLayerJH = maxlayer > this.maxLayerJH and maxlayer or this.maxLayerJH
		this.maxLayerJH = this.maxLayerJH >= (curlayer - 1) and this.maxLayerJH or (curlayer - 1)
		this.curLayerJH = curlayer
	elseif ftype == GAMECONST.FuBenFType.TYPE_SHENQI then
		if not this.maxLayerSQ then this.maxLayerSQ = 0 end
		this.maxLayerSQ = maxlayer > this.maxLayerSQ and maxlayer or this.maxLayerSQ
		this.maxLayerSQ = this.maxLayerSQ >= (curlayer - 1) and this.maxLayerSQ or (curlayer - 1)
		this.curLayerSQ = curlayer
	elseif ftype == GAMECONST.FuBenFType.TYPE_WULIN then
		if not this.maxLayerWL then this.maxLayerWL = 0 end
		this.maxLayerWL = maxlayer > this.maxLayerWL and maxlayer or this.maxLayerWL
		this.maxLayerWL = this.maxLayerWL >= (curlayer - 1) and this.maxLayerWL or (curlayer - 1)
		this.curLayerWL = curlayer
	elseif ftype == GAMECONST.FuBenFType.TYPE_BANGHUI then
		if not this.maxLayerBH then this.maxLayerBH = 0 end
		this.maxLayerBH = maxlayer > this.maxLayerBH and maxlayer or this.maxLayerBH
		this.maxLayerBH = this.maxLayerBH >= (curlayer - 1) and this.maxLayerBH or (curlayer - 1)
		this.curLayerBH = curlayer
	elseif ftype == GAMECONST.FuBenFType.TYPE_LINGYUAN then
		if not this.maxLayerLY then this.maxLayerLY = 0 end
		this.maxLayerLY = maxlayer > this.maxLayerLY and maxlayer or this.maxLayerLY
		this.maxLayerLY = this.maxLayerLY >= (curlayer - 1) and this.maxLayerLY or (curlayer - 1)
		this.curLayerLY = curlayer
	end
end

--保存当前的副本类型
function FuBenLogic.SetCurFuBenInfo(ftype, gateno)
	this.curFubenFtype = ftype
	this.gateno = gateno
end

------------------------------------------------------------------
--灵源hp处理
local LINGYUAN_NPCID = 25001001

function FuBenLogic.UpdateLingYuanHp(target)
	if target.char_id ~= LINGYUAN_NPCID then
		return
	end

	this.LingYuanBarUpdateHp(target)
end

--灵源副本
function FuBenLogic.LingYuanEnter()
	local mainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)

	if not mainCtrl.LingYuanCtrl then
		mainCtrl.LingYuanCtrl = require("Controller/fuben/FBBarLingYuanCtrl")
		mainCtrl.LingYuanCtrl.OnCreate(MainPanel.rightUpViews, mainCtrl.GetBehaviour())
	end

	--主界面处理
	mainCtrl.SetOtherBarState(false)
	mainCtrl.ShowHideTopBtns(false)
	local TimeBarCtrl = require("Controller/TimeBarCtrl")
	TimeBarCtrl.gameObject:SetActive(false)
end

--更新当前关卡波数
function FuBenLogic.LingYuanBarUpdate(layer, wave, waveMax)
	local mainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
	if not mainCtrl.LingYuanCtrl then
		return
	end

	mainCtrl.LingYuanCtrl.SetFbInfo(layer, wave, waveMax)
end

--更新下一波来临倒计时
function FuBenLogic.LingYuanBarUpdateTime(time, nextwave, maxwave)
	local mainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
	if not mainCtrl.LingYuanCtrl then
		return
	end

	mainCtrl.LingYuanCtrl.UpdateLeftTimeTip(time, nextwave, maxwave)
end

function FuBenLogic.SetLingYuanFbNoneTimeTip()
	local mainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
	if not mainCtrl.LingYuanCtrl then
		return
	end

	mainCtrl.LingYuanCtrl.SetLingYuanFbNoneTimeTip()
end

--更新水晶血量
function FuBenLogic.LingYuanBarUpdateHp(target)
	local mainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
	if not mainCtrl.LingYuanCtrl then
		return
	end

	mainCtrl.LingYuanCtrl.UpdateShuiJingHp(target.hp, target.maxhp)
end

function FuBenLogic.LingYuanBarHpReset()
	local mainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
	if not mainCtrl.LingYuanCtrl then
		return
	end

	mainCtrl.LingYuanCtrl.ResetShuiJingHp()
end

--更新击杀怪物数量
function FuBenLogic.LingYuanBarUpdateMonsterInfo(name, diecnt, maxcnt)
	local mainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
	if not mainCtrl.LingYuanCtrl then
		return
	end

	mainCtrl.LingYuanCtrl.UpdateMonsterInfo(name, diecnt, maxcnt)
end

--更新波数奖励
function FuBenLogic.LingYuanBarEnterRewardWave(waveno)
	local mainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
	if not mainCtrl.LingYuanCtrl then
		return
	end

	mainCtrl.LingYuanCtrl.EnterFbUpdateRewardInfo(waveno)
end

--退出，删除信息
function FuBenLogic.LingYuanExit()
	local mainCtrl = CtrlManager.GetCtrl(CtrlNames.Main)
	if mainCtrl.LingYuanCtrl then
		mainCtrl.LingYuanCtrl.Clear()
		mainCtrl.LingYuanCtrl = nil
	end

	mainCtrl.SetOtherBarState(true)
	mainCtrl.ShowHideTopBtns(true)
end

------------------------------------------------------------------

return FuBenLogic