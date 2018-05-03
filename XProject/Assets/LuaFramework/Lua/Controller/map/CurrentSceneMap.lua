-- @Author: liangzg
-- @Date:   2017-02-28 15:12:08
-- @Last Modified time: 2018-02-24 11:51:02
-- @Desc 当前场景分页逻辑

local CurrentSceneMap = class("CurrentSceneMap")
local UpdateBeat = UpdateBeat

CurrentSceneMap.CellType = { ["Monster"] = "Monster", ["Npc"] = "Npc", ["Player"] = "Player",
                             ["Position1"] = "Position1",["Position2"] = "Position2",["Position3"] = "Position3",
                             ["Gold"] = "Gold", ["Pickup"] = "Pickup"}

CurrentSceneMap.MapItemTypes = {
    ["Monster"] = "ui_wangfdb_dt04",
    ["Npc"] = "ui_ditu_zuobiao_2",
    ["Position1"] = "ui_ditu_zuobiao_3", --普通跳转点
    ["Position2"] = "ui_ditu_csd",  --剧情跳转点
    ["Position3"] = "ui_ditu_zuobiao_5",  --死亡跳转点
    ["Player"] = "ui_ditu_zuobiao",
    ["Gold"] = "ui_wangfdb_dt02",
    ["Pickup"] =  "ui_wangfdb_dt03",
 }

 CurrentSceneMap.MapItemColor = {
    [CurrentSceneMap.CellType.Monster] = "ee4f4f",
    [CurrentSceneMap.CellType.Npc] = "2fd95b",
    [CurrentSceneMap.CellType.Position1] = "42deff",
    [CurrentSceneMap.CellType.Position2] = "42deff",
    [CurrentSceneMap.CellType.Position3] = "ff5050",
    [CurrentSceneMap.CellType.Gold] = "2fd95b",
    [CurrentSceneMap.CellType.Pickup] =  "2fd95b",
}

require "Common/ConfigUtils"
local MapDataXls = require("xlsdata/Map/MapDataXls")
-- local MapJumpGateXls = require("xlsdata/Map/MapJumpGateXls")
-- local MapFakeJumpGateXls = require("xlsdata/Map/MapFakeJumpGateXls")
-- local StaticNpc = require("xlsdata/Npc/StaticNpcXls")
-- local AutoCreateXls = require("xlsdata/Npc/AutoCreateXls")
-- local NpcBattleXls = require("xlsdata/Npc/BattleNpcXls")
-- local MiniMapXls = require("xlsdata/Map/MiniMapXls")


function CurrentSceneMap:ctor(  )

    self.cells = {}

end

function CurrentSceneMap:Awake(go)
    self.gameObject = go
    self.transform = go.transform

    self.widgets = {
        {field="leftMapRoot",path="leftMap",src=LuaPanel},
        {field="rightView",path="RightTypeScrollview",src=LuaPanel},
        {field="miniMapContent",path="leftMap.viewport.content",src="Transform"},
        {field="miniMap",path="leftMap.viewport.content.miniMap",src=LuaButton , onClick = handler(self , self.onClickTouchScreen)},
        {field="destPoint",path="leftMap.viewport.content.dest",src=LuaButton, onClick = handler(self , self.onClickDestFinalPosition) },
        {field="roleItem",path="leftMap.viewport.content.itemSelf",src="GameObject" },
        {field="mapItem",path="leftMap.item",src="GameObject"},
        {field="mapItemMonster",path="leftMap.itemMonster",src="GameObject"},
        {field="mapItemGoldMonster",path="leftMap.itemGoldMonster",src="GameObject"},
        {field="mapTypeTable",path="RightTypeScrollview.Table",src="UITable"},
        {field="monster",path="RightTypeScrollview.Table.0_item_monster",src=LuaToggle , onChange = handler(self , self.toggleChange)},
        {field="monsterRoot",path="RightTypeScrollview.Table.0_item_monster.tween.viewcontext",src="UIGrid"},
        {field="player",path="RightTypeScrollview.Table.1_item_player",src=LuaToggle , onChange = handler(self , self.toggleChange)},
        {field="playerRoot",path="RightTypeScrollview.Table.1_item_player.tween.viewcontext",src="UIGrid"},
        {field="position",path="RightTypeScrollview.Table.2_item_position",src=LuaToggle , onChange = handler(self, self.toggleChange)},
        {field="positionRoot",path="RightTypeScrollview.Table.2_item_position.tween.viewcontext",src="UIGrid"},
        {field="pickup",path="RightTypeScrollview.Table.3_item_pickup",src=LuaToggle , onChange = handler(self, self.toggleChange)},
        {field="pickupRoot",path="RightTypeScrollview.Table.3_item_pickup.tween.viewcontext",src="UIGrid"},
        {field="cell",path="RightTypeScrollview.cell",src="GameObject"},
        {field="mapName",path="LeftMapPanel.MapName.Label",src=LuaText},
        {field="linearItem",path="linearItem",src=LuaImage},

    }
    LuaUIHelper.bind(self.gameObject , self)

end

--------------------------------------
--- 添加事项注册
function CurrentSceneMap:addEventListener( )
    self.event:AddListener("minimap_add_monster" , handler(self , self.addMonsterListener))
    self.event:AddListener("minimap_del_monster" , handler(self , self.delMonsterListener))

    if self.sub then
        self.sub:addEventListener(self.event)
    end
end

--- 添加动态创建NPC
function CurrentSceneMap:addMonsterListener( monsters )
    if not monsters then  return    end

    print(" CurrentSceneMap , addMonsterListener!!")
    for _ , monster in pairs(monsters) do

        if monster.type == 0 then
            -- 只有图标
            self:addEntityIconMapItem(monster , monster.iconName)
        elseif monster.type == 1 then
            -- 一行文本
            self:addEntityLabelIconMapItem(monster , monster.titleLab , monster.titleCol , monster.iconName , monster.dir)
        elseif monster.type == 2 then
            -- 二行文本
            self:addEntityMulTitleMapItem( monster , monster.titleLab , monster.titleCol , monster.subTitleLab , monster.subTitleCol ,
                                           monster.iconName , monster.dir )
        end

    end
end

--- 删除动态NPC
function CurrentSceneMap:delMonsterListener( monsters )
    if not monsters then  return    end
    for _ , monster in pairs(monsters) do
        self:removeMapItem(monster.npcNo)
    end
end

-------------------------------
function CurrentSceneMap:toggleChange( toggle )

    self:playTween(toggle , toggle.value)
end

function CurrentSceneMap:playTween( toggle , state)
    if not toggle then  return    end

    local tween = toggle.transform:Find("tween")
    tween = tween.gameObject:GetComponent(typeof(TweenScale))

    if state then
        local viewContext = toggle.transform:Find("tween/viewcontext")
        local activeChildCount = self:_getActiveChildCount(viewContext)

        tween.gameObject:SetActive(true)
        self.curTweenTab = tween
                -- uigrid.transform.localPosition = Vector3.New(0 , 175 , 0)

        local scrollPanel = tween.gameObject:GetComponent(typeof(UIScrollView))

        scrollPanel.panel.clipOffset = Vector2.zero
        local offset = activeChildCount <= 6 and ((580 - activeChildCount * 98) * 0.5) or 0
        scrollPanel.panel.cachedTransform.localPosition = Vector3.New(380 , -265 + offset, 0)

        local uigrid = tween.gameObject:GetComponentInChildren(typeof(UIGrid))
        uigrid:Reposition()

       EventDelegate.Add(tween.onFinished , EventDelegate.Callback(function (  )
            self.rightView:Refresh()
            SpringPanel.Begin(tween.gameObject , Vector3.New(380 , -270 + offset, 0) , 8)
            end) , true)

       TweenScale.Begin(tween.gameObject , 0.3 , Vector3.New(1 , 1, 1))
    else
        tween.gameObject:SetActive(false)
        self.mapTypeTable.repositionNow = true
        -- EventDelegate.Add(tween.onFinished , EventDelegate.Callback(function (  )

        --     end) , true)

        -- TweenScale.Begin(tween.gameObject , 0.15 , Vector3.New(1 , 0, 1))
    end
end


function CurrentSceneMap:_getActiveChildCount( root )
    local childCount = root.childCount
    local count = 0
    for i = 1 , childCount do
        local childTrans = root:GetChild(i - 1)
        if childTrans.gameObject.activeSelf then
            count = count + 1
        end
    end
    return count
end

------------------------------------------

function CurrentSceneMap:initPage( sceneId )
    self.mapScrollview = self.leftMapRoot.gameObject:GetComponent(typeof(UIScrollView))
    self.mapScrollview.onDragStarted =  UIScrollView.OnDragNotification(handler(self , self.onDragMap))
    self.mapScrollview.onDragFinished =  UIScrollView.OnDragNotification(handler(self , self.onDragMap))

    self.mapItem.gameObject:SetActive(false)
    self.isLookatPlayer = true
    self.isDragEnd = true

    local curMiniMap = MiniMapXls[sceneId]
    if not curMiniMap then
        Debugger.LogError("Cant find MiniMapXls for scene . scene id is " .. sceneId)
        return
    end

    self.gameObject:SetActive(true)

    local lastSceneId = self.sceneId
    self.sceneId = sceneId

    self.centerOffset = ConfigUtils.toVector3(curMiniMap.center)
    self.scaleMapWidth = curMiniMap.miniMapWidth / curMiniMap.sceneWidth
    self.scaleMapHeight = curMiniMap.miniMapHeight / curMiniMap.sceneHeight

    self.scaleSceneWidth =  curMiniMap.sceneWidth / curMiniMap.miniMapWidth
    self.scaleSceneHeight =  curMiniMap.sceneHeight / curMiniMap.miniMapHeight

    self.curMiniMap = curMiniMap
    self.miniMapContent.localRotation = Quaternion.Euler(0 , 0, self.curMiniMap.rotation)
    self.destPoint.transform.rotation = Quaternion.identity

    if lastSceneId ~= sceneId then
        self.mapItems = GoPool.reset(self.mapItems)
        self.mapItemMonsters = GoPool.reset(self.mapItemMonsters)

        self:initMonsterList()
        self:initPlayerList()
        self:initPositionList()

        -- 采集
        self.pickup.gameObject:SetActive(self.curMiniMap.mapType == 1)

        self.tempCells = self.tempCells or {}
        self.mapItemTempCaches = GoPool.reset(self.mapItemTempCaches)
        self.mapItemGoldCaches = GoPool.reset(self.mapItemGoldCaches)
        self.mapItemMonsterCaches = GoPool.reset(self.mapItemMonsterCaches)
        if self.curMiniMap.mapType == 1 then
            -- 王府夺宝
            self.sub = WFDeposerMiniMap.new(self)
        elseif self.curMiniMap.mapType == 2 then
            -- 野外Boss
            self.sub = FieldBossMiniMap.new(self)
        elseif self.curMiniMap.mapType == 3 then
            -- 帮派领地
            self.sub = SocityDemesneMiniMap.new(self)
        elseif self.curMiniMap.mapType == 4 then
            -- 帮派攻城战
            self.sub = SocitySieyeMiniMap.new(self)
        elseif self.curMiniMap.mapType == 5 then
            -- 杀戮战场
            self.sub = PlatoonMiniMap.new(self)
        elseif self.curMiniMap.mapType == 6 then
            -- 帮派秘境
            self.sub = SocityUnchartedMiniMap.new(self)
        end
    end
    self:initLeftMap(sceneId)

    if self.sub then
        self.sub:init()
    end

    self:setActivePage(true)
    -- 初始化完成后再监听事件
    self:addEventListener()
end


function CurrentSceneMap:initLeftMap( sceneId )

    local mapData = MapDataXls[sceneId]
    local curMiniMap = MiniMapXls[sceneId]

    self.mainRole = roleMgr.mainRole
    local mainRolePos = self.mainRole.transform.position
    --print(print_lua_table(self.miniMap , 0 , 3 , true))
    local miniMapTex = self.miniMap:asTexture()
    miniMapTex:Load("Texture/miniMap/minimap" .. curMiniMap.miniMapId .. ".jpg")
    miniMapTex:SetDimensions(curMiniMap.miniMapWidth , curMiniMap.miniMapHeight)

    self.mapName.text = mapData.Name

    --清空寻路记录
    self.destPoint.gameObject:SetActive(false)
    if self.moveLinear then
        self.moveLinear:popAllLinear()
    end

    --玩家浮标
    self.roleItem.gameObject:SetActive(DetailMapCtrl.currentSceneID == sceneId)

    if DetailMapCtrl.currentSceneID == sceneId then
        local curPlayPos = self:worldToMapPoint(mainRolePos)
        self.roleItem.transform.localPosition = curPlayPos
        self.roleItem.transform.rotation = Quaternion.identity

        --self:lookatPlayer(self.roleItem.transform)
        self.mapScrollview.horizontalScrollBar.value =  0.5
        self.mapScrollview.verticalScrollBar.value = 0.5
        self.mapScrollview:UpdatePosition()

        if self.mainRole.roleState:IsInState(RoleStateTransition.RoleState.AutoPathfinding:ToInt()) then
            self.readInitNavigat = true
        end
    else
        self.mapScrollview.horizontalScrollBar.value =  0.5
        self.mapScrollview.verticalScrollBar.value = 0.5
        self.mapScrollview:UpdatePosition()
    end
end


function CurrentSceneMap:worldToMapPoint(worldPos)
    worldPos = worldPos - self.centerOffset

    worldPos = self:scaleToMapPoint(worldPos)

    return worldPos * -1
end

--聚焦玩家
function CurrentSceneMap:lookatPlayer( playerTrans )
    if not (self.isLookatPlayer and self.isDragEnd) then     return      end

    -- self.mapScrollview.horizontalScrollBar.value = playerMapPos.x / self.curMiniMap.miniMapWidth + 0.5
    -- self.mapScrollview.verticalScrollBar.value = -playerMapPos.y / self.curMiniMap.miniMapHeight + 0.5
    local relativeParentPos = self.miniMapContent.parent.transform:InverseTransformPoint(playerTrans.position)
    local subSize = NGUIMath.CalculateRelativeWidgetBounds(self.leftMapRoot.transform)
    subSize = subSize.size / 2

    self.mapScrollview.horizontalScrollBar.value = relativeParentPos.x / subSize.x + 0.5
    self.mapScrollview.verticalScrollBar.value = -relativeParentPos.y / subSize.y + 0.5

    self.mapScrollview:UpdatePosition()
end


function CurrentSceneMap:mapToWorldPoint( mapPos )
    mapPos = mapPos * -1
    mapPos =  Vector3.New(mapPos.x * self.scaleSceneWidth , 0 , mapPos.y * self.scaleSceneHeight)
    return mapPos + self.centerOffset
end

function CurrentSceneMap:scaleToMapPoint( worldPos )
    return Vector3.New(worldPos.x * self.scaleMapWidth , worldPos.z * self.scaleMapHeight , 0)
end

function CurrentSceneMap:createMapItemScreenPoint( screenPos )

    local uiWorldPos = UICamera.currentCamera:ScreenToWorldPoint(screenPos)
    uiWorldPos = self.miniMapContent:InverseTransformPoint(uiWorldPos)

    local worldPos = self:mapToWorldPoint(uiWorldPos)
    local gridPos = Util.Convert2MapPosition(worldPos.x , worldPos.z , worldPos.y)
    if gridPos == Vector3.zero then     return  end

    self:onClickWalkTo(gridPos)
end

--- 创建小地图左侧显示项
function CurrentSceneMap:createMapItemWorldPoint( worldPos , labName , itemType , dir)

    local itemObj = GoPool.swapnGameObject( self.mapItems , self.mapItem ,self.miniMapContent)
    local relativeMapPos = self:worldToMapPoint(worldPos)
    itemObj.transform.localPosition = relativeMapPos
    itemObj.transform.rotation = Quaternion.identity

    local iconName = CurrentSceneMap.MapItemTypes[itemType]
    local labColor = CurrentSceneMap.MapItemColor[itemType]

    return self:setMapItemLabelAndIcon( itemObj , labName , labColor , iconName , dir)
end

--- 设置小地图左侧显示项
--@param itemObj GameObject
--@param labName  string   标签名称
--@param labColor Color     标签颜色
--@param iconName string    icon名称
--@param dir     int        标签的方向   0-上  1-下  2-左  3-右
function CurrentSceneMap:setMapItemLabelAndIcon( itemObj, labName , labColor , iconName , dir)

    local iconSpr = itemObj:GetComponentInChildren(typeof(UISprite))
    iconSpr.spriteName = iconName
    iconSpr:MakePixelPerfect()

    local titleHLab = itemObj.transform:Find("titleH")  --水平上下的文本
    local titleVLab = itemObj.transform:Find("titleV")  --左右垂直文本

    dir = dir or 0
    titleHLab.gameObject:SetActive(dir < 2)
    titleVLab.gameObject:SetActive(dir > 1)

    local titleLab = dir > 1 and titleVLab or titleHLab
    titleLab = titleLab.gameObject:GetComponent(typeof(UILabel))
    if dir == 0 or dir == 1 then
        titleLab:SetDimensions(170 , 25)
        local losY = dir == 1 and -iconSpr.height or iconSpr.height
        titleLab.transform.localPosition = Vector3.New(0 , losY , 0)
    else
        titleLab:SetDimensions(25 , 170)
        local losX = dir == 2 and -iconSpr.width or iconSpr.width
        titleLab.transform.localPosition = Vector3.New(losX , 0 , 0)
    end

    titleLab.gameObject:SetActive(labName and true or false)
    if titleLab.gameObject.activeSelf then
        titleLab.text = labName
        titleLab.color = Color.Hex2RGBA(labColor)
    end


    return itemObj
end

--左侧地图的怪物结点
function CurrentSceneMap:createMapItemMonsterWorldPoint( worldPos , name , grade , dir , monsterType)
    local relativeMapPos = self:worldToMapPoint(worldPos)

    local itemObj = GoPool.swapnGameObject(self.mapItemMonsters , self.mapItemMonster ,self.miniMapContent)
    itemObj.transform.rotation = Quaternion.identity
    itemObj.transform.localPosition = relativeMapPos

    local iconName = monsterType or CurrentSceneMap.MapItemTypes.Monster
    local titleColor = Color.Hex2RGBA(CurrentSceneMap.MapItemColor.Monster)

    return self:setMapItemMulTitle(itemObj , name , titleColor , grade , titleColor , iconName , dir)
end

--- 设置多标题MapItem信息
-- @param itemObj     GameObject 实例对象
-- @param title1      string    标题1
-- @param titleColor  color     标题1Color
-- @param title2      string    副标题2
-- @param title2Color color     副标题颜色
-- @param iconName    string    图标名称
-- @param dir         int       标签的方向   0-上  1-下  2-左  3-右
function CurrentSceneMap:setMapItemMulTitle( itemObj , title1, title1Color , title2 , title2Color , iconName , dir)

    local iconSpr = itemObj:GetComponentInChildren(typeof(UISprite))
    iconSpr.spriteName = iconName
    iconSpr:MakePixelPerfect()

    local titleH = itemObj.transform:Find("titleH")
    local titleNameLab = itemObj.transform:Find("titleH/titleName")
    local titleLvLab = itemObj.transform:Find("titleH/titleLv")

    titleNameLab = titleNameLab.gameObject:GetComponent(typeof(UILabel))
    titleNameLab.fontSize = 24
    titleNameLab.text = title1
    titleNameLab.color = title1Color

    --print("--->title1:" .. tostring(title1) .. ", dir:" .. tostring(dir))
    dir = dir or 0
    if dir == 0 or dir == 1 then
        local losY = dir == 1 and -iconSpr.height * 0.6 or iconSpr.height
        titleH.transform.localPosition = Vector3.New(0 , losY * 1.2, 0)
    else
        local size = titleNameLab.drawingDimensions * 1.5
        local losX = dir == 2 and -size.z or size.z
        titleH.transform.localPosition = Vector3.New(losX , iconSpr.height * 0.25 , 0)
    end

    titleLvLab.gameObject:SetActive( title2 and true or false)
    if titleLvLab.gameObject.activeSelf then
        titleLvLab = titleLvLab.gameObject:GetComponent(typeof(UILabel))
        titleLvLab.text = title2
        titleLvLab.color = title2Color
    end

    return itemObj
end

--创建移动导航线
function CurrentSceneMap:createMoveLinear( corners )
    --如何是世界分页，则不显示导航线
    if DetailMapPanel.btnBack.gameObject.activeSelf then    return  end

    if roleMgr.curSceneNo ~= self.sceneId then
        DetailMapPanel.initCurrentPage()
    end

    if not self.moveLinear then
        local linear = NavigationLinear.new()
        linear.root = self.miniMap.transform
        linear.sceneMap = self
        self.moveLinear = linear
    end

    self.moveLinear.DobberTrans = self.roleItem.transform
    self.moveLinear:calLinearPath(corners , self.curMiniMap.isFly == 1)

end

function CurrentSceneMap:setActivePage( active )
    self.isUpdateNavgation = roleMgr.curSceneNo == self.sceneId

    if active then
        -- Debugger.Log("setActivePage :" .. tostring(active))
        local childCount = self.mapTypeTable.transform.childCount
        local isDefault = true
        for i=1,childCount do
            local item = self.mapTypeTable.transform:GetChild(i - 1)
            local tween = item.transform:Find("tween")
            tween.transform.localScale = Vector3.one
            tween = tween.gameObject:GetComponent(typeof(TweenScale))
            tween.gameObject:SetActive(false)

            if item.gameObject.activeSelf then
                local def = item:GetComponent(typeof(UIToggle))
                def:Set(isDefault)
                if isDefault then
                    self:playTween(def , true)
                    isDefault = false
                end
            end
        end
        --self.mapTypeTable.repositionNow = true

        self.screenInputHandler = handler(self , self.update)
        UpdateBeat:Add(self.screenInputHandler)

        self.roleStateTransHandler = RoleStateTransition.OnStateChanged(handler(self , self.roleStateTrans))
        roleMgr.mainRole.roleState:AddListener( "minMapListener" , self.roleStateTransHandler)
    else
        if self.curTweenTab then
            self.curTweenTab:ResetToBeginning()
        end
        self.gameObject:SetActive(active)
        if self.screenInputHandler then
            UpdateBeat:Remove(self.screenInputHandler)
            self.screenInputHandler = nil
            roleMgr.mainRole.roleState:RemoveListener("minMapListener")
        end

        self.event:Clear() --清空当前的事件
    end
end

function CurrentSceneMap:update( )

    if self.readInitNavigat and self.mainRole.move and self.mainRole.move:InMoving()  then
        self.readInitNavigat = false
        self.isLookatPlayer = true

        local corners = self.mainRole.move.vectorLeftPath
        self.finalDestPos = self.mainRole.move.finalDestination
        local uiWorldPos = self:worldToMapPoint(self.finalDestPos)

        -- Debugger.Log("Create Novnation")
        --如果是直接跳转
        if self.switch then
            -- local gridPos = Util.Convert2MapPosition(self.finalDestPos.x , self.finalDestPos.z , self.finalDestPos.y)
            local gridPos = self.targetGridPos
            MAPLOGIC.FlyToAnyWhere(self.sceneId , gridPos , 0.1 , nil , function ( result )
                -- Debugger.Log("Fly any where : " .. result .. ",sceneId:" .. self.sceneId)
                if result ~= 1 then
                    if not self.isWorldPage then
                        self.destPoint.transform.localPosition = uiWorldPos
                        self:createMoveLinear(corners)
                    end
                elseif result == 1 then
                    DetailMapCtrl.OnCloseBtnClick()
                end
            end)
            --MAPLOGIC.SendFlyShoe()

            self.switch = false
        else
            self.destPoint.transform.localPosition = uiWorldPos
            self:createMoveLinear(corners)
        end
    end

    if self.moveLinear and self.isUpdateNavgation then
        --只有在当前场景才更新导航
        if self.moveLinear.isMoveing then
            self.moveLinear:update()
        end

        if self.isFinish then
            self.isFinish = false
            self.moveLinear.isMoveing = false
            -- self.destPoint.gameObject:SetActive(false)
        end
    end

    -- 野外Boss
    if self.curMiniMap.mapType == 2 and self.sub then
           self.sub:update()
    end
end

function CurrentSceneMap:roleStateTrans( state , action )
    local pathComputeComplete = RoleStateTransition.RoleState.PathComputeComplete:ToInt()
    -- Debugger.Log("roleState Trans:" .. state .. ",action:" .. action)
    if action == "addstate" then
        if state == pathComputeComplete then
            self.readInitNavigat = true
            -- self.mainRole.roleState:RemoveState(state)
        end
    elseif action == "removestate" then
        local findPath = RoleStateTransition.RoleState.AutoPathfinding:ToInt()
        if state == findPath and self.moveLinear then
            self.moveLinear.isMoveing = false
            self.destPoint.gameObject:SetActive(false)
        end
    end
    self.lastRoleState = state
end

--------------------------------------------------------------------------------------
--初始化怪物列表
function CurrentSceneMap:initMonsterList( )
    self.monster.gameObject:SetActive(self.curMiniMap.isMonster == 1)
    if self.curMiniMap.isMonster == 0 then    return  end

    self.monsterCells = GoPool.reset(self.monsterCells)

    local monsterList = {}


    for _,monsterCreate in pairs(AutoCreateXls) do
        local canSee = monsterCreate.canSee or 1
        if monsterCreate.MapNo == self.sceneId and canSee ~= 0 then

            local monster = {}
            local mapPos = string.split(monsterCreate.miniMapPos , ",")
            local pos = Vector3.New(tonumber(mapPos[1]) , 0 , tonumber(mapPos[3]))
            monster.mapPos = pos
            monster.nameDirection = monsterCreate.nameDirection

            pos = string.split(monsterCreate.miniGridPos , ",")
            monster.gridPos = Vector3.New(tonumber(pos[1]) , 0 , tonumber(pos[2]))
            monster.battle = NpcBattleXls[monsterCreate.NpcNo]
			monster.topTitleName = monster.battle.Name

			if self.curMiniMap.mapType == 0 then
				if monster.battle.BossType == 15 or monster.battle.BossType == 19 then
					--特殊处理帮派攻城站
					monster.titleName = monster.battle.Name
                    monster.Grade = ""
                else
					monster.titleName = string.format("%s(%d级)" , monster.battle.Name , monster.battle.Grade)
					monster.Grade = string.format("%s级" , monster.battle.Grade)
                end
            else
                monster.titleName = monster.battle.Name
            end
            monsterList[#monsterList + 1] = monster
        end
    end

    table.sort( monsterList, function ( x,y )
        return x.battle.Grade < y.battle.Grade
    end )

    self:addRightTabGroup(monsterList , self.monsterRoot , self.monsterCells , CurrentSceneMap.CellType.Monster)
end

--- 添加标签组
function CurrentSceneMap:addRightTabGroup( tabList , parent , cachePool , cellType )
    local parentTrans = parent.transform
    for i , cellTab in ipairs(tabList) do

        local newCellObj = GoPool.swapnGameObject( cachePool ,  self.cell.gameObject , parentTrans)
        newCellObj.name = string.format("%03dCell" , i)

        local newCell = CurrentSceneMap.Cell.new(self , newCellObj , cellType)
        newCell.data = cellTab
        newCell.worldPos = cellTab.mapPos
        newCell.gridPos = cellTab.gridPos
		newCell:setTitle(cellTab.titleName)
		--add mapItem
		if cellType == CurrentSceneMap.CellType.Monster then
			newCell.mapItem = self:createMapItemMonsterWorldPoint(cellTab.mapPos , cellTab.topTitleName ,
                                                                  cellTab.Grade , cellTab.nameDirection )
        else
            newCell.mapItem = self:createMapItemWorldPoint(cellTab.mapPos , cellTab.titleName ,
                                                           cellType , cellTab.nameDirection)
        end
        self.cells[#self.cells + 1] = newCell
    end

    local groupTrans = parentTrans.parent.parent
    self:adjustScrollView(groupTrans , #tabList)

    parent:Reposition()
    groupTrans.gameObject:SetActive(#tabList > 0)
end


--初始化角色列表
function CurrentSceneMap:initPlayerList( )
    self.player.gameObject:SetActive(self.curMiniMap.isPlay == 1)
    if self.curMiniMap.isPlay == 0 then    return  end

    self.playerCells = GoPool.reset(self.playerCells)

    local npcList = {}
    for _,npc in pairs(StaticNpc) do
        if npc.PosMapId == self.sceneId and npc.NpcType == 0 and npc.MinimapCanSee == 1 then
            local cell = {}

            cell.mapPos = ConfigUtils.toVector3(npc.miniMapPos)
            cell.nameDirection = npc.nameDirection

            cell.gridPos = Vector3.New(npc.Position[1] , npc.Position[3] , npc.Position[2])
            cell.battle = npc

			cell.titleName = npc.Name
			cell.topTitleName = npc.Name
			cell.Grade = ""

            npcList[#npcList + 1] = cell
        end
    end

    table.sort(npcList , function(x , y)

        local lenX = string.len(x.battle.TitleSpr)
        local lenY = string.len(y.battle.TitleSpr)

        return lenX > lenY
    end)

    self:addRightTabGroup(npcList , self.playerRoot , self.playerCells , CurrentSceneMap.CellType.Npc)
end


--初始化地点列表
function CurrentSceneMap:initPositionList(  )
    self.position.gameObject:SetActive(self.curMiniMap.isMapPoint == 1)
    if self.curMiniMap.isMapPoint == 0 then    return  end

    local mapConveyData = {}
    for _,jump in pairs(MapJumpGateXls) do
        if jump.StartMap == self.sceneId then
            local jumpData = { data = jump , jumpType = jump.Type , nameDirection = jump.nameDirection}
            jumpData.cellType = jump.Type == 1 and CurrentSceneMap.CellType.Position1 or CurrentSceneMap.CellType.Position2
            jumpData.worldPos = ConfigUtils.toVector3(jump.miniMapPos)
            jumpData.gridPos = Vector3.New(jump.ConveyPos[1] , jump.ConveyPos[3] , jump.ConveyPos[2])
			jumpData.titleName = jump.Type == 2 and "" or jump.EndName
            mapConveyData[#mapConveyData + 1] = jumpData
        end
    end
    --跳转点
    for _,jump in pairs(MapFakeJumpGateXls) do
        if jump.StartMap == self.sceneId then
            local jumpData = { data = jump , jumpType = 1 , nameDirection = jump.nameDirection}
            jumpData.cellType = CurrentSceneMap.CellType.Position1
            jumpData.worldPos = ConfigUtils.toVector3(jump.miniMapPos)
            jumpData.titleName = jump.EndName
            jumpData.gridPos = Vector3.New(jump.ConveyPos[1] , jump.ConveyPos[3] , jump.ConveyPos[2])
            mapConveyData[#mapConveyData + 1] = jumpData
        end
    end

    self:addPositionTabGroup(mapConveyData)

end

--- 添加地图标签和小地图图标
--@params mapConveyDatas 包装的数据
function CurrentSceneMap:addPositionTabGroup(mapConveyDatas)

    self.positionCells = GoPool.reset(self.positionCells)

    if #mapConveyDatas <= 0 then
        self.position.gameObject:SetActive(false)
        return
    end

    local parentTrans = self.positionRoot.transform
    local jumpTabCount = 0
    for _ , jump in ipairs(mapConveyDatas) do
        local mcd = jump.data
        local cellType = jump.cellType
        local mapItem = self:createMapItemWorldPoint(jump.worldPos , jump.titleName , cellType , jump.nameDirection)
        if jump.jumpType == 1 then
            local newCellObj = GoPool.swapnGameObject(self.positionCells ,self.cell.gameObject , parentTrans)

            local newCell = CurrentSceneMap.Cell.new(self , newCellObj , cellType)
            newCell.data = mcd
            newCell.worldPos = jump.worldPos
            newCell.gridPos = jump.gridPos
            newCell:setTitle(jump.titleName)
            --add mapItem
            newCell.mapItem= mapItem
            self.cells[#self.cells + 1] = newCell
            jumpTabCount = jumpTabCount + 1
        end
    end

    self:adjustScrollView(self.position.transform , jumpTabCount)

    self.positionRoot:Reposition()
    self.position.gameObject:SetActive(jumpTabCount > 0)

end

--- 查找满足条件的多个Cell
function CurrentSceneMap:findCells( findFunc )
    local _cells = {}
    for _,cell in ipairs(self.cells) do
        local result = findFunc(cell.data)
        if result then
            _cells[#cells + 1] = cell
        end
    end
    return _cells
end

--- 查找满足条件的单个Cell
function CurrentSceneMap:findCell( findFunc )

    for _,cell in ipairs(self.cells) do
        local result = findFunc(cell.data)
        if result then
            return cell
        end
    end

    return nil
end


---------------部分界面临时数据----------------

--- 添加左侧地图列表项
function CurrentSceneMap:addMapItem( itemNo , mapWorldPosX , mapWorldPosY  , itemPrefab)

    return self:_addMapItem(self.mapItemTempCaches , itemNo , mapWorldPosX , mapWorldPosY , itemPrefab)
end


function CurrentSceneMap:_addMapItem( cachePool , itemNo , mapWorldPosX , mapWorldPosY  , itemPrefab )
    if self.tempCells[itemNo] then  return self.tempCells[itemNo].mainGO end

    local worldPos = Util.Convert2RealPosition(mapWorldPosX , mapWorldPosY)
    local relativeMapPos = self:worldToMapPoint(worldPos)

    local itemObj = GoPool.swapnGameObject(cachePool , itemPrefab ,self.miniMapContent)
    itemObj.transform.localPosition = relativeMapPos
    itemObj.transform.rotation = Quaternion.identity

    self.tempCells[itemNo] = { pool = cachePool , mainGO = itemObj}

    return itemObj
end


function CurrentSceneMap:addEntityIconMapItem( npcInfo , iconName )

    local itemPrefab = self.mapItemGoldMonster
    local itemPool = self.mapItemGoldCaches
    local itemObj = self:_addMapItem(itemPool , npcInfo.rid ,npcInfo.mapPos.x , npcInfo.mapPos.y , itemPrefab)
    local iconSpr = itemObj:GetComponentInChildren(typeof(UISprite))
    iconSpr.spriteName = iconName
    iconSpr:MakePixelPerfect()

    local tweenIcon = itemObj.transform:Find("icon"):GetComponent(typeof(TweenScale))
    tweenIcon.enabled = false

    local iconBg = itemObj.transform:Find("iconBG")
    iconBg.gameObject:SetActive(false)

    return itemObj
end


function CurrentSceneMap:addEntityLabelIconMapItem( npcInfo , labName , labColor , iconName , dir)

    local itemPrefab = self.mapItem
    local itemPool = self.mapItemTempCaches
    local itemObj = self:_addMapItem(itemPool , npcInfo.rid ,npcInfo.mapPos.x , npcInfo.mapPos.y , itemPrefab)
    self:setMapItemLabelAndIcon(itemObj , labName , labColor , iconName , dir)
    return itemObj
end


function CurrentSceneMap:addEntityMulTitleMapItem( npcInfo , titleLab , titleCol , subTitleLab , subTitleCol , iconName , dir )
    local itemPrefab = self.mapItemMonster
    local itemPool = self.mapItemMonsterCaches
    local itemObj = self:_addMapItem(itemPool , npcInfo.rid ,npcInfo.mapPos.x , npcInfo.mapPos.y , itemPrefab)
    self:setMapItemMulTitle(itemObj , titleLab , titleCol , subTitleLab , subTitleCol , iconName , dir)
    return itemObj
end


--- 删除左侧地图列表项
function CurrentSceneMap:removeMapItem( itemNo )
    if not self.tempCells[itemNo] then return end

	local cellObj = self.tempCells[itemNo]
    GoPool.remove(cellObj.pool , cellObj.mainGO)
	cellObj.mainGO:SetActive(false)

    self.tempCells[itemNo] = nil

end

--- 清空临时地图小单元
function CurrentSceneMap:clearTempMapItems( )
    if not self.tempCells then  return  end

    for _,item in pairs(self.tempCells) do
        if item then
            item.mainGO.gameObject:SetActive(false)
        end
    end
    self.tempCells = {}
end

--自适应子结点的高度
function CurrentSceneMap:adjustScrollView( gObj , childCount )
    local sv = gObj:Find("tween"):GetComponent(typeof(UIScrollView))
    sv.enabled = childCount >= 6
    local svPanel = sv.gameObject:GetComponent(typeof(UIPanel))
    local finalClip = svPanel.finalClipRegion
    svPanel.clipOffset = Vector2.zero
    svPanel:SetRect(0 , -65 , finalClip.z , sv.enabled and 580 or (childCount * 100))
end

--- 获得指定no序号的地图左侧的单元项
-- @param no 序号
function CurrentSceneMap:getMapItemGameObject(no)

    if not no then  return nil end

    for _ , cell in ipairs(self.cells) do

        local dataNo = false
        if cell.cellType == CurrentSceneMap.CellType.Npc then
            dataNo = cell.data.NpcNo
        elseif cell.cellType == CurrentSceneMap.CellType.Monster then
            dataNo = cell.data.battle.NpcNo
        end

        if dataNo == no then
            return cell.mapItem
        end
    end

    return nil
end
-----------------------------------------按钮点击-----------------------------------------------

function CurrentSceneMap:onClickWalkTo( gridPos )
    -- local gridPos = Util.Convert2MapPosition(worldPos.x , worldPos.z , worldPos.y)
    if MarriageLogic.MarriageCheck() then
        return
    end

    self.targetGridPos = gridPos
    if roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.DigTreasure) then
        roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.DigTreasure)
    end
    if roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.Task) then
        roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.Task)
    end
    worldPathfinding:BeginWorldPathfinding(self.sceneId , gridPos.x , gridPos.y , gridPos.z, 0.1,
                      Move.PathFinished(function ()
                         self.isFinish = true
                      end), false)
end


function CurrentSceneMap:onClickSwitch( gridPos )

    --飞鞋是否足够
    local num = ITEMLOGIC.GetItemCountByNo(10105003)
    -- Debugger.Log("num:" .. tostring(num) .. ",vip:" .. tostring(HERO.Vip))
    if num <= 0 and HERO.Vip < 2 then
        local BuyLogic = require('Logic/BuyLogic')
        BuyLogic.BuyItem(10105003, 1)
        return
    end

    --是否在押镖
    local husong = RoleStateTransition.RoleState.Husong:ToInt()
    if roleMgr.mainRole.roleState:IsInState(husong) then
        CtrlManager.PopUpNotifyText(LANGUAGE_TIP.roleProjectFollow)
        return
    end

    if MarriageLogic.MarriageCheck() then
        return
    end

    if roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.DigTreasure) then
        roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.DigTreasure)
    end
    if roleMgr.mainRole.roleState:IsInState(GAMECONST.RoleState.Task) then
        roleMgr.mainRole.roleState:RemoveState(GAMECONST.RoleState.Task)
    end

    local needFly = MAPLOGIC.JudgeActNeedFly(self.sceneId, gridPos.x, gridPos.y, gridPos.z)
    if needFly then
        MAPLOGIC.FlyToAnyWhere(self.sceneId , gridPos , 0.1 , nil , function ( result )
                -- Debugger.Log("FlyToAnyWhere-------> result:" .. result)
                if result == 1 then
                    DetailMapCtrl.OnCloseBtnClick()
                end
            end)
    else
        CtrlManager.PopUpNotifyText(LANGUAGE_TIP.nearstDestPos)
        self:onClickWalkTo(gridPos)
    end
end

--点击小地图上的飞鞋
function CurrentSceneMap:onClickDestFinalPosition( )

    local gridPos = Util.Convert2MapPosition(self.finalDestPos.x , self.finalDestPos.z , self.finalDestPos.y)

    self:onClickSwitch(gridPos)

end

--屏幕点击左侧地图
function CurrentSceneMap:onClickTouchScreen(  )
    if self.curMiniMap.isNavigation == 0 or self.curMiniMap.No ~= roleMgr.curSceneNo then    return  end

    local screenPos = UICamera.lastTouchPosition
    screenPos.z = 1
    -- Debugger.Log("--------------onClickTouchScreen-----------")

    self:createMapItemScreenPoint(screenPos)
end

function CurrentSceneMap:onDragMap( )
    self.isDragEnd = not self.isDragEnd
end

----------------------------------------------------------

function CurrentSceneMap:onDestroy( )
    if self.screenInputHandler then
        UpdateBeat:Remove(self.screenInputHandler)
        self.screenInputHandler = nil

        if self.moveLinear then
            self.moveLinear:popAllLinear()
            self.destPoint.gameObject:SetActive(false)
            self.moveLinear = nil
        end
    end

    if self.mainRole and self.mainRole.roleState then
        self.mainRole.roleState:RemoveListener("minMapListener")
    end

    if self.sub then
        self.sub:destroy()
        self.sub = nil
    end

    self.sceneId = nil
    self.tempCells = nil
    GoPool.clear(self.mapItemTempCaches)
    GoPool.clear(self.mapItemGoldCaches)
    GoPool.clear(self.mapItemMonsterCaches)
end


-----------------------------------------------------------
--每个基本单元项的逻辑

CurrentSceneMap.Cell = class("CurrentSceneMap_Cell")

function CurrentSceneMap.Cell:ctor( sceneMap , gObj , type)
    self.sceneMap = sceneMap
    self.mainObj = gObj
    self.cellType = type


    DetailMapPanel.behaviour:AddClick(gObj , handler(self , self.onClick));

    local flyBtn = gObj.transform:Find("flyBtn")
    flyBtn.gameObject:SetActive(sceneMap.curMiniMap.isFly == 1)
    DetailMapPanel.behaviour:AddClick(flyBtn.gameObject , handler(self , self.onSwitch))
end

function CurrentSceneMap.Cell:setTitle( titleName )
    local titleLab = self.mainObj.transform:Find("Title"):GetComponent("UILabel")
    titleLab.text = titleName
end

function CurrentSceneMap.Cell:onClick( )
    -- self.mainObj.transform:SetAsFirstSibling()
    self.sceneMap:onClickWalkTo(self.gridPos)

    -- local parentTable = self.mainObj:GetComponentInParent(typeof(UITable))
    -- parentTable.repositionNow = true
end

function CurrentSceneMap.Cell:onSwitch( )
    self.sceneMap:onClickSwitch(self.gridPos)
end


function CurrentSceneMap.Cell:onDestroy(  )

end






return CurrentSceneMap