-- @Author: LiangZG
-- @Last Modified time: 2017-07-13 10:48:42
-- @Desc: 服务器列表选择

local LoginCtrl = require("Controller.LoginCtrl")

ServersPanel = {}
local this = ServersPanel

--由LuaBehaviour自动调用
function ServersPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="leftCell",path="LeftViewGroup2.cell",src="GameObject"},
		{field="serverCell",path="RightViewGroup1.item",src="GameObject"},
		{field="btnClose",path="BtnClose",src=LuaButton, onClick = this._onClickClose},
		{field="leftviewport",path="LeftViewGroup2.viewport",src="GameObject"},
		{field="leftContent",path="LeftViewGroup2.viewport.leftContent",src="GameObject"},
		{field="rightViewport",path="RightViewGroup1.viewport",src="GameObject"},
		{field="rightContent",path="RightViewGroup1.viewport.rightContent",src="GameObject"},

	}
	LuaUIHelper.bind(this.gameObject , ServersPanel )
end

--由LuaBehaviour自动调用
function ServersPanel.Start()

	this._initLeftTab()


	--默认最近登录
	--this._initRightServers()

	this.leftCells[1].mainToggle.isOn = true
	ServersPanel._initRightServers(this.leftCells[1].cellData.serverInfos)

end

function ServersPanel.show( )
	-- CtrlManager.panelDepth = CtrlManager.panelDepth + 30
	createPanel('Prefab/Gui/Login/ServersPanel', 1, nil)
end

--由LuaBehaviour自动调用
function ServersPanel.OnInit()
	LuaUIHelper.addUIDepth(this.gameObject , ServersPanel)
	if this.leftCells and #this.leftCells > 0 then
		this.leftCells[1].mainToggle.isOn = true
		ServersPanel._initRightServers(this.leftCells[1].cellData.serverInfos)
	end

end

function ServersPanel._initLeftTab( )
	this.leftCells = {}


	--my nearest login servers
	this._createLeftCell("000cell" , this._getMyServers())

	--Recommend
	this._createLeftCell("001cell" , this._getRecommendServers())


	--分组列表
	local serverList = User_Config.serverList
	if not serverList then
		-- this.leftviewport.repositionNow = true
		return
 	end

	local count = serverList.Count
	local group = Mathf.Max( 1, math.ceil(count / 10 )) - 1

	-- 开发工程中的数据记录
	if User_Config.internal_sdk == 0 then
		local serverAccess = User_Config.GetString("DevTemp" , "serverAccess")
		if not string.isEmptyOrNil(serverAccess) then
			local saArr = string.split(serverAccess , '|')
			this.serverAccessMap = {}
			for _,v in pairs(saArr) do
				if not string.isEmptyOrNil(v) then
					local sa = string.split(v , '=')
					local sno = tonumber(sa[2])
					local grade = tonumber(sa[3])
					this.serverAccessMap[sno] = grade
				end
			end
		end
	end

	for i=0,group do
		local cellData = {}
		cellData["title"] = string.format("%s-%s区" , i * 10 + 1, i * 10 + 10)

		local serverInfos = {}
		local hasNew = false
		for j = i * 10 , (i + 1) * 10 - 1 do
			if j >= count then 	break end
			local server = serverList:get_Item(count - j - 1)
			if this.serverAccessMap then
				server.playerLevel = this.serverAccessMap[server.serverNo] or 0
			end

			serverInfos[#serverInfos + 1] = server

			if server.isNew == 1 then	hasNew = true	end
		end

		table.sort( serverInfos, function ( a , b)
			return a.serverNo > b.serverNo
		end)
		cellData["hasNew"] = hasNew
		cellData["serverInfos"] = serverInfos

		--if #serverInfos <= 0 then	break   end

		this._createLeftCell(string.format("%03dcell" , group - i + 2) , cellData)
	end

	-- this.leftviewport.repositionNow = true
end

function ServersPanel.filterServerList( srcServerList )
	local serverList = srcServerList
	local count = serverList.Count - 1

	local finalServers = {}
	for i=0,count do
		local server = serverList:get_Item(i)
		if server.serverNo > 0 then
			finalServers[#finalServers + 1] = server
		end
	end
	return finalServers
end

--获得当前用户已有账号服务器列表
function ServersPanel._getMyServers(  )
	--todo
	local cellData = {title = "我的服务器"}
	local serverList = User_Config.lastServerList
	if serverList then
		local count = serverList.Count
		local serverGroups = {}
		for i = 1 , count do
			local serverInfo = serverList:get_Item(i - 1)
			serverGroups[#serverGroups + 1] = serverInfo
		end

		cellData["serverInfos"] = serverGroups
	end
	return cellData
end


--获得推荐服务器
function ServersPanel._getRecommendServers( )

	local cellData = {title = "推荐"}
	local serverList = User_Config.serverList
	if not serverList then	return cellData	end

	local count = serverList.Count

	--推荐服，使用最近的10个服
	local serverGroups = {}
	for i = 1 , 10 do
		if i > count then	break   end

		local serverInfo = serverList:get_Item(i - 1)
		--if serverInfo.serverNo > 0 then
			serverGroups[#serverGroups + 1] = serverInfo
		--end
	end

	cellData["serverInfos"] = serverGroups

	return cellData
end


function ServersPanel._createLeftCell( name , cellData )
	local myServerCell = newObject(this.leftCell)
	myServerCell.name = name
	GoPool.addChild(myServerCell , this.leftContent.transform)

	local newCell = ServersPanel.LeftCell.new(myServerCell , cellData)
	newCell:Awake()

	this.leftCells[#this.leftCells + 1] = newCell

	return newCell
end

--右侧的服务器列表
function ServersPanel._initRightServers( servers )

	this.rightCells = {}
	this.serverCellPool = GoPool.reset(this.serverCellPool)

	if not servers then	return end

	local count = #servers
	for i=1,count do
		local cellData = {serverInfo = servers[i]}
		this._createRightCell(string.format("%02dCell" , i) , cellData)
	end

	-- this.rightViewport.repositionNow = true
end


function ServersPanel._createRightCell( name , cellData )
	local myServerCell = GoPool.swapnGameObject(this.serverCellPool , this.serverCell , this.rightContent.transform)
	myServerCell.name = name

	local newCell = ServersPanel.ServerCell.new(myServerCell , cellData)
	newCell:Awake()

	this.rightCells[#this.rightCells + 1] = newCell

	return newCell
end

function ServersPanel._onClickClose( )
	-- CtrlManager.panelDepth = CtrlManager.panelDepth - 30
	panelMgr:ClosePanel("ServersPanel")
end

--- 设置当前的服务器
function ServersPanel.setCurrentServer( serverData )

	local serverInfo = serverData.serverInfo

	if User_Config.internal_sdk == 1 then
		--只有SDK模式，才能取到时间戳
		local curTotalSeconds = TimeManager.TotalSecondsToCurrentTime()

		if tonumber(serverInfo.openTime) > curTotalSeconds then
			MessageBox.DisplayMessageBox(LANGUAGE_TIP.Servers_NotOpen ,0 , nil ,nil)
			return
		end
	end

	LoginCtrl.setCurrentServer(serverInfo)

	this._onClickClose()
end

--由LuaBehaviour自动调用
function ServersPanel.OnClose()
	LuaUIHelper.removeUIDepth(this.gameObject)  --还原全局深度
end



--由LuaBehaviour自动调用--
function ServersPanel.OnDestroy()
	this.serverCellPool = {}
end


----------------------左侧Cell-------------------------------
ServersPanel.LeftCell = class("ServersPanel_LeftCell")

function ServersPanel.LeftCell:ctor( gObj , cellData )
	self.gameObject = gObj
	self.cellData = cellData

	self.mainToggle = gObj:GetComponent(typeof(UnityEngine.UI.Toggle))
	ServersPanel.behaviour:AddToggleChange(gObj , handler(self , self._onValueChange))
end

function ServersPanel.LeftCell:_onValueChange( toggle )

	-- local labColor = toggle.value and Color.Hex2RGBA("405B78") or Color.white
	-- self.

	if not toggle.isOn then 	return 		end

	ServersPanel._initRightServers(self.cellData.serverInfos)
end

function ServersPanel.LeftCell:Awake()
    self.widgets = {
		{field="titleNormal",path="title",src=LuaText},
		{field="titleActiveLab",path="checkmark.title",src=LuaText},
		{field="imgNew",path="new",src=LuaImage},
    }
    LuaUIHelper.bind(self.gameObject , self)
    self.titleNormal.text = self.cellData.title
    self.titleActiveLab.text = self.cellData.title
    self.imgNew.gameObject:SetActive(self.cellData.hasNew)

end

function ServersPanel.LeftCell:OnDestroy()

end

----------------------右则服务器Cell--------------------------------

ServersPanel.ServerCell = class("ServersPanel_ServerCell")

function ServersPanel.ServerCell:ctor( gObj , cellData )
	self.gameObject = gObj
	self.cellData = cellData --ServerInfo

	self.mainToggle = gObj:GetComponent(typeof(UnityEngine.UI.Toggle))
	self.mainToggle.isOn = cellData.serverInfo.serverNo == User_Config.default_server
	ServersPanel.behaviour:AddClick(gObj , handler(self , self._onClick))
end

function ServersPanel.ServerCell:Awake()
    self.widgets = {
		{field="serverNameLab",path="serverName",src=LuaText},
		{field="imgServerStatus",path="statusIcon",src=LuaImage},
		{field="imgNewServer",path="newServer",src=LuaImage},
		{field="accentIcon",path="accentIcon",src=LuaImage},
		{field="accentLvLab",path="accentIcon.lv",src=LuaText},

    }
    LuaUIHelper.bind(self.gameObject , self)

    self:_initCell()
end


function ServersPanel.ServerCell:_initCell( )

	local serverInfo = self.cellData.serverInfo
	self.serverNameLab.text = serverInfo.serverName
	self.imgServerStatus.spriteName = self:_getServerImgStatus(serverInfo.status)
	self.imgNewServer.gameObject:SetActive(serverInfo.isNew == 1)

	if serverInfo.playerLevel then
		local lv = serverInfo.playerLevel
		self.accentIcon.gameObject:SetActive(lv > 0)
		self.accentLvLab.text = string.format("等级:%d",lv)
	else
		self.accentIcon.gameObject:SetActive(false)
	end
end

function ServersPanel.ServerCell:_getServerImgStatus( state )

	--正常
	if state == 2 then	return "ui_denglu_lightdot_normal"	end
	--爆满
	if state == 3 then  return "ui_denglu_lightdot_busy" end
	--维护
	if state == 4 then	return "ui_denglu_lightdot_fix"	end

	return "ui_denglu_lightdot_fluent"
end

function ServersPanel.ServerCell:_onClick( )
	ServersPanel.setCurrentServer(self.cellData)
end

function ServersPanel.ServerCell:OnDestroy()

end
