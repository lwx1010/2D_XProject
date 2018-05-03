
local LoginCtrl = require "Controller/LoginCtrl"
local MainCtrl = require "Controller/MainCtrl"

CtrlManager = {};
local this = CtrlManager;
CtrlManager.panelId2TabMap = {}  --界面ID映射表
local ctrlList = {};	--控制器列表--
local notifyList = {}	--弹出信息列表
local popupList = {}	--弹出窗口列表
local guideList = {}	--引导窗口列表

this.panelDepth = 30    --起始panel深度为20(主界面和聊天占用),后面每打开一个界面深度加10
this.startRenderQ = 3200 -- 普通界面起始渲染队列深度
this.headPanelRenderQ = 2900 --场景对象头顶容器渲染队列深度
this.panelStartDepth = 30
this.PANEL_DEPTH_OFFSET = 40

this.guideDepth = 200
this.guideRenderQ = 4000

this.tabGroupNo = 10	--初始tabGroup
this.tabGroupGap = 20	--每次打开界面添加

ctrlList[CtrlNames.Login] = LoginCtrl.New();
ctrlList[CtrlNames.Main] = MainCtrl.New();


function CtrlManager.Init()
	log("CtrlManager.Init----->>>");


	return this;
end

--添加控制器--
function CtrlManager.AddCtrl(ctrlName, ctrlObj)
	ctrlList[ctrlName] = ctrlObj;
end

--获取控制器--
function CtrlManager.GetCtrl(ctrlName)
	return ctrlList[ctrlName];
end

--移除控制器--
function CtrlManager.RemoveCtrl(ctrlName)
	ctrlList[ctrlName] = nil;
end

--关闭控制器--
function CtrlManager.Close()
	log('CtrlManager.Close---->>>');
end

-- name:界面名字
-- closeMainCamera:弹出这个界面的时候是否可以关闭主摄像机
function CtrlManager.PopUpPanel(name, closeMainCamera, x, y, z)
	local temp = string.split(name, '/')
	if #temp > 0 then
		name = temp[#temp]
	end

	-- if CtrlManager.PanelIsPopuped(name) then
	-- 	return
	-- end
	if Game.currentStage.isLoadingScene then 
		Game.currentStage:CacheWindow(name)
		return nil
	end
	local ctrl = CtrlManager.GetCtrl(name .. "Ctrl")
	if not ctrl then
		error("界面Ctrl:[" .. name .."没有预先加载进CtrlManager!")
		return 
	else
		-- this.panelDepth = this.panelDepth + this.PANEL_DEPTH_OFFSET
		ctrl.Awake(closeMainCamera, x, y, z)
		-- ctrl.showDepth = this.panelDepth
		-- _G[name .."Panel"].depth = this.panelDepth
    end
    table.insert(popupList, name)
    return ctrl
end

function CtrlManager.ClosePanel(name)
	for i=1,#popupList do
		--print("----------", popupList[i], name)
		if popupList[i] == name then
			
			-- this.panelDepth = this.panelDepth - this.PANEL_DEPTH_OFFSET
			if this.panelDepth < this.panelStartDepth then
				this.panelDepth = this.panelStartDepth
                --Debugger:LogWarning("CtrlManager ClosePanel reset: 30")
			end

			AvatarCreator.PanelCloseStopCoroutine(name)
			panelMgr:ClosePanel(name)
			table.remove(popupList, i)
			break
		end
	end
end

--跨场景清理所有界面
function CtrlManager.Clear( maxDepth)
	popupList = {}

    maxDepth = maxDepth or 0
	CtrlManager.panelDepth = Mathf.Max(maxDepth , this.panelStartDepth)
    --Debugger.LogWarning("CtrlManager Clear:" .. CtrlManager.panelDepth)

	guideList = {}
	--panelMgr:Clear()

	this.tabGroupNo = 10

	AvatarCreator.SceneChangeClear()
end

-- 返回这个界面是否弹出显示
function CtrlManager.PanelIsPopuped(name)
	for i=1,#popupList do
		if popupList[i] == name then
			return true
		end
	end
	return false
end

-- 中屏弹出浮动提示
function CtrlManager.PopUpNotifyText(msg, spritename ,effectColor , color , spriteSize, parentPanel , msgSize)
	local parent = parentPanel and parentPanel or panelMgr:GetNotifyTrans()
	if not parent then return end

	local prefab = Util.LoadPrefab("Prefab/Gui/NotifyTextUi")
	local notify = {}
	notify.window = newObject(prefab)
	if not notify.window then
		error("创建弹出信息预设报错！")
		return
	end

	notify.window.transform:SetParent(parent)
	notify.window.transform.localPosition = Vector3.zero
	notify.window.transform.localScale = Vector3.New(0.1, 0.1, 0.1)
	notify.msgLabel = notify.window.transform:Find("NotifyText"):GetComponent("UILabel")
	notify.msgBgSpr = notify.window.transform:Find("Sprite"):GetComponent("UISprite")
	notify.msgLabel.text = msg
	notify.msgBgSpr.height = notify.msgLabel.height + 16

	if spritename then
		notify.msgBgSpr.spriteName = spritename
		notify.msgBgSpr:MakePixelPerfect()
	end
	if effectColor then
		if color then
			notify.msgLabel.color = color
		end
		notify.msgLabel.effectStyle = UILabel.Effect.Outline
		notify.msgLabel.effectColor = effectColor
	end
	if spriteSize then
		notify.msgBgSpr.height = spriteSize[2]
		notify.msgBgSpr.width = spriteSize[1]
	end
	if msgSize then
		notify.msgLabel.fontSize = msgSize
	end
	local sq = Sequence()
	local widget = notify.window.transform:GetComponent("UIWidget")
	sq:Append(notify.window.transform:DOScale(1, 0.2))
	  :AppendInterval(1.0)
	  :Append(DOTween.To(DG.Tweening.Core.DOGetter_float(function()return widget.alpha end), DG.Tweening.Core.DOSetter_float(function(x) widget.alpha = x end), 0, 0.5))
	  :AppendCallback(function()
			for i=1, #notifyList do
				if notifyList[i] == notify then
					table.remove(notifyList, i)
					break
				end
			end
			GameObject.DestroyImmediate(notify.window)
		end)



	local offset = notify.msgBgSpr.height
	if #notifyList > 0 then
		offset = (offset + notifyList[#notifyList].msgBgSpr.height)/2
	end

	--移动之前的提示
	local dist = Vector3.New(0, offset+5, 0)
	for i=1, #notifyList do
		if notifyList[i] then
			notifyList[i].window.transform:DOBlendableLocalMoveBy(dist, 0.3, false)
		end
	end

	table.insert(notifyList, notify)
end

function CtrlManager.CSPopUpNotifyText(key)
	this.PopUpNotifyText(LANGUAGE_TIP[key])
end

-----------------------------------------------------
--引导处理

-- name:界面名字
-- closeMainCamera:弹出这个界面的时候是否可以关闭主摄像机
function CtrlManager.PopUpGuidePanel(name, closeMainCamera, x, y, z)
	local temp = string.split(name, '/')
	if #temp > 0 then
		name = temp[#temp]
	end
	local ctrl = CtrlManager.GetCtrl(name .. "Ctrl")
	if not ctrl then
		error("界面Ctrl:[" .. name .."没有预先加载进CtrlManager!")
		return
	else
		ctrl.Awake(closeMainCamera, x, y, z)
    end
    table.insert(guideList, name)
    return ctrl
end

function CtrlManager.CloseGuidePanel(name)
	for i=1,#guideList do
		-- print("----------", guideList[i], name)
		if guideList[i] == name then
			panelMgr:ClosePanel(name)

			AvatarCreator.PanelCloseStopCoroutine(name)
			table.remove(guideList, i)
			break
		end
	end
end

-- 返回这个引导界面是否弹出显示
function CtrlManager.GuidePanelIsPopuped(name)
	for i=1,#guideList do
		if guideList[i] == name then
			return true
		end
	end
	return false
end

function CtrlManager.IsPanelVisible(name)
	return panelMgr:IsPanelVisible(name)
end

require "View.init"
require "Model.init"