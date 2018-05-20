-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc:

SkillUiPanel = {}
local this = SkillUiPanel

--- 由LuaBehaviour自动调用
function SkillUiPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="btnSkill1",path="skill01",src=LuaButton, onClick = function (gObj)  this.SkillBtnClick(1)  end },
		{field="graySkill1",path="skill01.gray",src=LuaImage},
		{field="btnSkill2",path="skill02",src="GameObject"},
		{field="graySkill2",path="skill02.gray",src=LuaImage},
		{field="btnSkill3",path="skill03",src="GameObject"},
		{field="graySkill3",path="skill03.gray",src=LuaImage},
		{field="btnSkill4",path="skill04",src="GameObject"},
		{field="graySkill4",path="skill04.gray",src=LuaImage},
		{field="btnSkill5",path="skill05",src="GameObject"},
		{field="graySkill5",path="skill05.gray",src=LuaImage},
		{field="selectGo",path="selectbg",src="GameObject"},
		{field="posTrans",path="selectbg.select",src="Transform"},
		{field="btnBreakSkill",path="breakskill",src="GameObject"},
		{field="imgBreak",path="breakskill.gray",src=LuaImage},
		{field="btnChange",path="btnchange",src=LuaButton, onClick = function (gObj)  this.ChangeSkillTest()  end },
		{field="textChange",path="btnchange.Text",src=LuaText},
		{field="btnSkill0",path="bigskill",src="GameObject"},
		{field="graySkill0",path="bigskill.gray",src=LuaImage},
		---custom extendsion
		{field="selectTrans",path="selectbg",src="Transform"},

	}

	LuaUIHelper.bind(this.gameObject , SkillUiPanel )

	for i = 0, 5 do
		if i ~= 1 then
			this.behaviour:AddOnDown(this["btnSkill" ..i], function(go, data) this.OnBtnSkillDown(i, go, data) end)
			this.behaviour:AddOnUp(this["btnSkill" ..i], function(go, data) this.OnBtnSkillUp(i, go, data) end)
			this.behaviour:AddOnDrag(this["btnSkill" ..i], function(go, data) this.OnBtnSkillDrag(i, go, data) end)
		end
	end	
end

function SkillUiPanel.show( )
    createPanel("Prefab/Gui/main/SkillUiPanel", 1)
end

--- 由LuaBehaviour自动调用
function SkillUiPanel.OnInit()
    --每次显示自动修改UI中所有Panel的depth
    LuaUIHelper.addUIDepth(this.gameObject , SkillUiPanel)
	this._registeEvents(this.event)

end

-- 注册界面事件监听
function SkillUiPanel._registeEvents(event)
    
end

--- 关闭界面
function SkillUiPanel._onClickClose( )
    panelMgr:ClosePanel("SkillUi")
end

--- 由LuaBehaviour自动调用
function SkillUiPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end

--- 由LuaBehaviour自动调用--
function SkillUiPanel.OnDestroy()

end

------------------------------------------------------------------------------------------
--cd以及连招图片显示
--普通攻击不显示cd
function SkillUiPanel.UpdateShowMartial(martial)
	
end

------------------------------------------------------------------------------------------
--上一个技能hassend变化之前，点击技能按钮直接返回
function SkillUiPanel.SkillBtnClick(index)
	print("------------SkillBtnClick---------", index)

	HEROSKILLMGR.SetHeroNextMartial(index)
end

function SkillUiPanel.OnBreakSkillClick()
	if not roleMgr.mainRole then return end

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

	local martial = HEROSKILLMGR.GetMartialByBtnIndex(index)
	-- print("===========OnBtnSkillDown2222==================", index, martial)
	if not martial or not HEROSKILLMGR.JudgeMartialCdOk(martial) then
		return
	end
	-- print("===========OnBtnSkillDown3333==================")
	local skillId = martial.skills[martial.Combo]
	-- print("-----------OnBtnSkillDown---111-------", index)
	--设置技能类型
	local type, angle = HEROSKILLMGR.GetSkillShowType(skillId)
	-- print("-----------OnBtnSkillDown---222-------", type, SkillAreaType.OuterCircle_InnerSector)
	if not type then return end
	roleMgr.mainRole.skillArea:SetSkillAreaType(type, 60)

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

	HEROSKILLMGR.SetHeroNextMartial(index, draged and roleMgr.mainRole.skillArea:GetDirToLookAt(movePos) or nil)
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
--测试切换技能
local skillType = 1

function SkillUiPanel.ChangeSkillTest()
	if skillType == 1 then
		skillType = 2
		this.textChange.text = "5-8"
	else
		skillType = 1
		this.textChange.text = "1-4"
	end

	HEROSKILLMGR.TestChangeSkillAdd(skillType == 1 and 0 or 4)
end