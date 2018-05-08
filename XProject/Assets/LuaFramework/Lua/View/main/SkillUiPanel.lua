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
		{field="btnSkill2",path="skill02",src=LuaButton, onClick = function (gObj)  this.SkillBtnClick(2)  end },
		{field="graySkill2",path="skill02.gray",src=LuaImage},
		{field="btnSkill3",path="skill03",src=LuaButton, onClick = function (gObj)  this.SkillBtnClick(3)  end },
		{field="graySkill3",path="skill03.gray",src=LuaImage},
		{field="btnSkill4",path="skill04",src=LuaButton, onClick = function (gObj)  this.SkillBtnClick(4)  end },
		{field="graySkill4",path="skill04.gray",src=LuaImage},
		{field="btnSkill5",path="skill05",src=LuaButton, onClick = function (gObj)  this.SkillBtnClick(5)  end },
		{field="graySkill5",path="skill05.gray",src=LuaImage},
		{field="selectGo",path="selectbg",src="GameObject"},
		{field="selectTrans",path="selectbg",src="Transform"},
		{field="posTrans",path="selectbg.select",src="Transform"},
	}

	LuaUIHelper.bind(this.gameObject , SkillUiPanel )

	for i = 1, 5 do
		this.behaviour:AddOnDown(this["btnSkill" ..i].gameObject, function(go, data) this.OnBtnSkillDown(i, go, data) end)
		this.behaviour:AddOnUp(this["btnSkill" ..i].gameObject, function(go, data) this.OnBtnSkillUp(i, go, data) end)
		this.behaviour:AddOnDrag(this["btnSkill" ..i].gameObject, function(go, data) this.OnBtnSkillDrag(i, go, data) end)
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
function SkillUiPanel.UpdateSkillCd(cdTbl)

end

------------------------------------------------------------------------------------------
function SkillUiPanel.SkillBtnClick(index)
	print("------------SkillBtnClick---------", index)

	HEROSKILLMGR.SetHeroNextMartial(index)
end

---技能处理
local radius = 100
local isInSkill = false
local btnPosition = Vector2.zero
local lastCanHit = true
local draged = false

function SkillUiPanel.OnBtnSkillDown(index, go, data)
	if isInSkill then return end

	if not roleMgr.mainRole then return end

	local skillId = HEROSKILLMGR.GetSkillIdByBtnIndex(index)
	if HEROSKILLMGR.JudgeSkillIsInCd(skillId) then
		print("==============技能冷却中")
		return
	end

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
