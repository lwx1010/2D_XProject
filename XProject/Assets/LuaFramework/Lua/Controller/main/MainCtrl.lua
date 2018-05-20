local MainCtrl = {}
local this = MainCtrl

this.init = false

local notifyList = {}	--弹出信息列表

--构建函数--
function MainCtrl.New()
	return this
end

--启动事件--
function MainCtrl.OnCreate(obj)

	print("=====================MainCtrl========================")

	roleMgr.mainRole.roleState:AddListener(CtrlNames.Main, this.OnStateChanged)

	SkillUiPanel.show()
	FunctionBtnPanel.show( )
	MainUiTaskPanel.show( )
	MiniMapPanel.show( )
	JoystickPanel.show( )
	MainUiChatPanel.show( )
	PlayerBar.show( )
end


function MainCtrl.OnStateChanged(state, action)
	if not roleMgr.mainRole then return end
	-- print("****", action, "****", state)
	local roleState = roleMgr.mainRole.roleState
	if action == "addstate" then
	
	elseif action == "removestate" then
		if state == GAMECONST.RoleState.SkillMoving and roleState:IsInState(GAMECONST.RoleState.SkillActing) then
			roleMgr.mainRole.move:StopPath()
		end
	end
end

-- 中屏弹出浮动提示
function MainCtrl.PopUpNotifyText(msg, effectColor, color, spriteSize, parentPanel, msgSize)
	local parent = parentPanel and parentPanel or panelMgr:GetNotifyTrans()
	if not parent then return end

	local prefab = resMgr.LoadPrefabBundle("Prefab/Gui/main/NotifyTextUi")
	local notify = {}
	notify.window = newObject(prefab)
	if not notify.window then
		error("创建弹出信息预设报错！")
		return
	end

	notify.window.transform:SetParent(parent)
	notify.window.transform.localPosition = Vector3.zero
	notify.window.transform.localScale = Vector3.New(0.1, 0.1, 0.1)
	notify.msgLabel = notify.window.transform:Find("Panel/Text"):GetComponent("Text")
	notify.msgBgSpr = notify.window.transform:Find("Panel/Image"):GetComponent("Image")
	notify.panel = notify.window.transform:Find("Panel")
	notify.bgSprRectTrans = notify.msgBgSpr.gameObject:GetComponent("RectTransform")
	notify.msgLabel.text = msg

	if effectColor then
		if color then
			notify.msgLabel.color = color
		end
		notify.window.AddComponent("Outline")
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
	sq:Append(notify.panel.transform:DOScale(1, 0.2))
	  :AppendInterval(1.2)
	  :Append(DOTween.To(DG.Tweening.Core.DOGetter_float(function()return notify.msgBgSpr.color.a end), 
	  	DG.Tweening.Core.DOSetter_float(function(x) notify.msgBgSpr.color.a = x end), 0, 0.5))
	  :AppendCallback(function()
			for i=1, #notifyList do
				if notifyList[i] == notify then
					table.remove(notifyList, i)
					break
				end
			end
			GameObject.DestroyImmediate(notify.window)
		end)



	local offset = notify.bgSprRectTrans.rect.height
	if #notifyList > 0 then
		offset = (offset + notifyList[#notifyList].bgSprRectTrans.rect.height)/2
	end

	--移动之前的提示
	local dist = Vector3.New(0, offset + 5, 0)
	for i=1, #notifyList do
		if notifyList[i] then
			notifyList[i].panel.transform:DOBlendableLocalMoveBy(dist, 0.3, false)
		end
	end

	table.insert(notifyList, notify)
end

return this