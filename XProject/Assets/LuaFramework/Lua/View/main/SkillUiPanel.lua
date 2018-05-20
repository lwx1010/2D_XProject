-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc:

SkillUiPanel = {}
local this = SkillUiPanel

local curTime = 0			--当前时间
local maxCd = 0				--最长的cd





--- 由LuaBehaviour自动调用
function SkillUiPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="JiNengZhanKai",path="zhujiemian_skill.JiNengZhanKai",src="GameObject"},
		{field="btnNSkill",path="zhujiemian_skill.JiNengZhanKai.BtnSkill1",src=LuaButton, onClick = function (gObj)  this.SkillBtnClick(1)  end },
		{field="btnSkill1",path="zhujiemian_skill.JiNengZhanKai.BtnSkill2",src="GameObject"},
		{field="imgSkill1",path="zhujiemian_skill.JiNengZhanKai.BtnSkill2.bg",src=LuaImage},
		{field="imgCdSkill1",path="zhujiemian_skill.JiNengZhanKai.BtnSkill2.gray",src=LuaImage},
		{field="btnSkill2",path="zhujiemian_skill.JiNengZhanKai.BtnSkill3",src="GameObject"},
		{field="imgSkill2",path="zhujiemian_skill.JiNengZhanKai.BtnSkill3.bg",src=LuaImage},
		{field="imgCdSkill2",path="zhujiemian_skill.JiNengZhanKai.BtnSkill3.gray",src=LuaImage},
		{field="btnSkill3",path="zhujiemian_skill.JiNengZhanKai.BtnSkill4",src="GameObject"},
		{field="imgSkill3",path="zhujiemian_skill.JiNengZhanKai.BtnSkill4.bg",src=LuaImage},
		{field="imgCdSkill3",path="zhujiemian_skill.JiNengZhanKai.BtnSkill4.gray",src=LuaImage},
		{field="btnSkill4",path="zhujiemian_skill.JiNengZhanKai.BtnSkill5",src="GameObject"},
		{field="imgSkill4",path="zhujiemian_skill.JiNengZhanKai.BtnSkill5.bg",src=LuaImage},
		{field="imgCdSkill4",path="zhujiemian_skill.JiNengZhanKai.BtnSkill5.gray",src=LuaImage},
		{field="btnBSkill",path="zhujiemian_skill.JiNengZhanKai.BtnBigSkill",src=LuaButton, onClick = function (gObj)  this.SkillBtnClick(0)   end },
		{field="imgBSkill",path="zhujiemian_skill.JiNengZhanKai.BtnBigSkill.bg",src=LuaImage},
		{field="imgCdBSkill",path="zhujiemian_skill.JiNengZhanKai.BtnBigSkill.gray",src=LuaImage},
		{field="btnDSkill",path="zhujiemian_skill.JiNengZhanKai.BtnDodge",src=LuaButton, onClick = function (gObj)  this.OnDodgeSkillClick()  end },
		{field="imgCdDodge",path="zhujiemian_skill.JiNengZhanKai.BtnDodge.gray",src=LuaImage},
		{field="textDodge",path="zhujiemian_skill.JiNengZhanKai.BtnDodge.dodge",src=LuaText},
		{field="btnChange",path="zhujiemian_skill.JiNengZhanKai.BtnChange",src=LuaButton, onClick = function (gObj) this.OnChangeBtnClick()  end },
		{field="btnGuaJi",path="zhujiemian_skill.JiNengZhanKai.BtnGuaJi",src=LuaButton, onClick = function (gObj)  this.OnGuaJiBtnClick()   end },
		{field="goGJ",path="zhujiemian_skill.JiNengZhanKai.BtnGuaJi.checkmark",src="GameObject"},
		{field="selectGo",path="zhujiemian_skill.JiNengZhanKai.selectbg",src="GameObject"},
		{field="posTrans",path="zhujiemian_skill.JiNengZhanKai.selectbg.select",src="Transform"},
		{field="btnTest",path="zhujiemian_skill.JiNengZhanKai.btnTest",src=LuaButton, onClick = function (gObj)  this.ChangeSkillTest() end },
		{field="textChange",path="zhujiemian_skill.JiNengZhanKai.btnTest.Text",src=LuaText},

	}
	LuaUIHelper.bind(this.gameObject , SkillUiPanel )

	this.selectTrans = this.selectGo.transform

	for i = 1, 4 do
		this.behaviour:AddOnDown(this["btnSkill" ..i], function(go, data) this.OnBtnSkillDown(i, go, data) end)
		this.behaviour:AddOnUp(this["btnSkill" ..i], function(go, data) this.OnBtnSkillUp(i, go, data) end)
		this.behaviour:AddOnDrag(this["btnSkill" ..i], function(go, data) this.OnBtnSkillDrag(i, go, data) end)
	end	

	UpdateBeat:Add(this.Update, this)
end

function SkillUiPanel.show( )
    createPanel("Prefab/Gui/main/SkillUiPanel", 1, this.OnInit)
end

--- 由LuaBehaviour自动调用
function SkillUiPanel.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , SkillUiPanel)
	this._registeEvents(this.event)

	this.UpdateMartialIcons()
end

-- 注册界面事件监听
function SkillUiPanel._registeEvents(event)
    
end

--- 关闭界面
function SkillUiPanel._onClickClose( )
    panelMgr:ClosePanel("SkillUiPanel")
end

--- 由LuaBehaviour自动调用
function SkillUiPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--- 由LuaBehaviour自动调用--
function SkillUiPanel.OnDestroy()

end

function SkillUiPanel.ShowHide(visible)
	-- body
	this.JiNengZhanKai.gameObject:SetActive(visible)
end

-----------------------------------------------------------------------------------
--Update:技能cd处理


function SkillUiPanel.Update()
	curTime = TimeManager.GetRealTimeSinceStartUp() 

	this.UpdateDodgeShow(curTime)

	--技能cd处理
	if curTime > maxCd then return end

	for k, v in pairs(HEROSKILLMGR.GetCurMartials()) do
		this.UpdateShowMartialCd(v)
	end
end

------------------------------------------------------------------------------------------
--cd以及连招图片显示
--普通攻击不显示cd
function SkillUiPanel.UpdateShowMartial(martial)
	if martial.Mtype ~= 1 and (martial.cdTime + martial.receiveCd + 1) > maxCd then
		maxCd = martial.cdTime + martial.receiveCd + 1
	end

	--更新cd
	this.UpdateShowMartialCd(martial)

	--更新icons
	this.UpdateShowMartialIcon(martial)
end

function SkillUiPanel.UpdateShowMartialCd(martial)
	-- print("=================UpdateShowMartialCd==================")
	if not HEROSKILLMGR.JudgeMartialCdOk(martial) then
		if martial.martialId == 1030000 then
			this.imgCdBSkill.gameObject:SetActive(true)
			this.imgCdBSkill.fillAmount = (martial.cdTime + martial.receiveCd - curTime) /martial.cdTime
		elseif (martial.SkillShow - this.testSkillAdd) > 1 and (martial.SkillShow - this.testSkillAdd) < 6 then
			this["imgCdSkill" ..(martial.SkillShow - this.testSkillAdd -1)].gameObject:SetActive(true)
			this["imgCdSkill" ..(martial.SkillShow - this.testSkillAdd -1)].fillAmount = (martial.cdTime + martial.receiveCd - curTime) /martial.cdTime
		end
	else
		if martial.martialId == 1030000 then
			this.imgCdBSkill.gameObject:SetActive(false)
			this.imgCdBSkill.fillAmount = 1
		elseif (martial.SkillShow - this.testSkillAdd) > 1 and (martial.SkillShow - this.testSkillAdd) < 6 then
			this["imgCdSkill" ..(martial.SkillShow - this.testSkillAdd -1)].gameObject:SetActive(false)
			this["imgCdSkill" ..(martial.SkillShow - this.testSkillAdd -1)].fillAmount = 1
		end
	end
end
------------------------------------------------------------------------------------------
--技能图标处理
function SkillUiPanel.UpdateMartialIcons()
	for k, v in pairs(HEROSKILLMGR.GetCurMartials()) do
		this.UpdateShowMartialIcon(v)
	end
end

function SkillUiPanel.UpdateShowMartialIcon(martial)
	-- if martial.martialId == 1030000 then
	-- 	this.imgBSkill.sprite = HEROSKILLMGR.GetSkillIcon(martial.skills[martial.Combo])
	if (martial.SkillShow - this.testSkillAdd) > 1 and (martial.SkillShow - this.testSkillAdd) < 6 then
		this["imgSkill" ..(martial.SkillShow - this.testSkillAdd -1)].sprite = HEROSKILLMGR.GetSkillIcon(martial.skills[martial.Combo])
		this["imgCdSkill" ..(martial.SkillShow - this.testSkillAdd -1)].sprite = HEROSKILLMGR.GetSkillIcon(martial.skills[martial.Combo])
	end
end

------------------------------------------------------------------------------------------
--上一个技能hassend变化之前，点击技能按钮直接返回
function SkillUiPanel.SkillBtnClick(index)
	-- print("------------SkillBtnClick---------", index)

	local martial = HEROSKILLMGR.GetMartialByBtnIndex(index)
	if not martial or not HEROSKILLMGR.JudgeMartialCdOk(martial) then
		-- print("---------------------cd没有好")
		return
	end

	HEROSKILLMGR.SetHeroNextMartial(index)
end

function SkillUiPanel.OnDodgeSkillClick()
	if not roleMgr.mainRole then return end

	if HEROSKILLMGR.curDodge <= 0 then
		MainCtrl.PopUpNotifyText("技能cd中...")
		return
	end

	HEROSKILLMGR.SkillDodge()
	-- roleMgr.mainRole.skillCtrl
end


---技能处理
local radius = 100
local isInSkill = false
local btnPosition = Vector2.zero
local lastCanHit = true
local draged = false

function SkillUiPanel.OnBtnSkillDown(index, go, data)
	if isInSkill then return end
	-- print("===========OnBtnSkillDown1111==================")
	if not roleMgr.mainRole or HEROSKILLMGR.HasSendSkill() then return end

	local martial = HEROSKILLMGR.GetMartialByBtnIndex(index + 1 + this.testSkillAdd)
	-- print("===========OnBtnSkillDown2222==================", index, martial)
	if not martial or not HEROSKILLMGR.JudgeMartialCdOk(martial) then
		-- print("---------------------cd没有好")
		MainCtrl.PopUpNotifyText("cd没有好,稍后再点!")
		return
	end
	-- print("===========OnBtnSkillDown3333==================")
	local skillId = martial.skills[martial.Combo]
	-- print("-----------OnBtnSkillDown---111-------", index)
	--设置技能类型 type:类型 angle:角度 r:外圈半径 len:内圈或者箭头长 widht:内圈或者箭头宽
	local type, angle, r, len, width = HEROSKILLMGR.GetSkillShowType(skillId)
	-- print("-----------OnBtnSkillDown---222-------", type, SkillAreaType.OuterCircle_InnerSector)
	if not type then return end
	roleMgr.mainRole.skillArea:SetSkillAreaType(type, angle, r, len, width)

	isInSkill = true

	btnPosition = this["btnSkill"..index].transform.position -- Vector2.New(this["btnSkill"..index].transform.position.x, this["btnSkill"..index].transform.position.y)

	-- print("-----------OnBtnSkillDown----333------", index, btnPosition.x, btnPosition.y, data.position.x, data.position.y)
	this.selectGo:SetActive(true)
	this.selectTrans.localPosition = this["btnSkill"..index].transform.localPosition

	roleMgr.mainRole.skillArea:OnJoystickDownEvent(Vector2.zero)
end

function SkillUiPanel.OnBtnSkillUp(index, go, data)
	-- print("----------OnBtnSkillUp-----------", index, data.position.x, data.position.y, data.pressPosition.x, data.pressPosition.y)
	if not isInSkill then return end

	this.selectGo:SetActive(false)
	local movePos = data.position - btnPosition
	this.posTrans.localPosition = Vector3.zero
	isInSkill = false
	btnPosition = Vector2.zero
	lastCanHit = true

	roleMgr.mainRole.skillArea:OnJoystickUpEvent()

	if movePos.magnitude > 2*radius then
		return
	end	

	HEROSKILLMGR.SetHeroNextMartial(index + 1 + this.testSkillAdd, draged and roleMgr.mainRole.skillArea:GetDirToLookAt(movePos) or nil)
	draged = false
end

function SkillUiPanel.OnBtnSkillDrag(index, go, data)
	-- print("-----------OnBtnSkillDrag----------", index)
	if not isInSkill then return end

	draged = true
	local movePos = data.position - btnPosition

	if lastCanHit ~= (movePos.magnitude < 2*radius) then
		lastCanHit = movePos.magnitude < 2*radius
		roleMgr.mainRole.skillArea:ChangeAreaColorByType(lastCanHit)
	end

	movePos = movePos.magnitude > radius and movePos.normalized*radius or movePos
	this.posTrans.localPosition = movePos

	roleMgr.mainRole.skillArea:OnJoystickMoveEvent(data.position - btnPosition)
end

-----------------------------------------------------------------------------------
--挂机逻辑
function SkillUiPanel.OnGuaJiBtnClick(  )
	HEROSKILLMGR.OnGuaJiBtnClick()
end

function SkillUiPanel.SetGuaJiBtnState(isGuaJi)
	this.goGJ:SetActive(isGuaJi)
end

-----------------------------------------------------------------------------------
--切换目标逻辑
function SkillUiPanel.OnChangeBtnClick()
	
end

------------------------------------------------------------------------------------
--更新闪显示
function SkillUiPanel.UpdateDodgeShow(time)
	if HEROSKILLMGR.curDodge >= HEROSKILLMGR.DODGE_MAX_COUNT then
		return
	end

	local curDodgeValue = HEROSKILLMGR.receiveDodgeValue + (time - HEROSKILLMGR.receiveDodgeTime) * HEROSKILLMGR.ONE_DODGE_VALUE / HEROSKILLMGR.DODGE_CHARGE_TIME
	HEROSKILLMGR.curDodge = math.floor(curDodgeValue/HEROSKILLMGR.ONE_DODGE_VALUE)

	this.textDodge.text = HEROSKILLMGR.curDodge > 0 and HEROSKILLMGR.curDodge or ""
	if HEROSKILLMGR.curDodge >= HEROSKILLMGR.DODGE_MAX_COUNT then
		this.imgCdDodge.fillAmount = 0
	else
		this.imgCdDodge.fillAmount = (HEROSKILLMGR.ONE_DODGE_VALUE*(HEROSKILLMGR.curDodge + 1) - curDodgeValue)/HEROSKILLMGR.ONE_DODGE_VALUE
	end
end

-----------------------------------------------------------------------------------
--测试切换技能
local skillType = 1
this.testSkillAdd = 0

function SkillUiPanel.ChangeSkillTest()
	if skillType == 1 then
		skillType = 2
		this.textChange.text = "5-8"
	else
		skillType = 1
		this.textChange.text = "1-4"
	end

	this.testSkillAdd = this.testSkillAdd == 0 and 4 or 0

	this.UpdateMartialIcons()
end