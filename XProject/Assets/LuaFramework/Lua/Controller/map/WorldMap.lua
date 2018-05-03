-- @Author: LiangZG
-- @Date:   2017-02-28 15:12:25
-- @Last Modified time: 2017-06-28 17:32:43
-- @Desc  世界地图分页

local MapDataXls = require("xlsdata/Map/MapDataXls")
-- local MiniMapXls = require("xlsdata/Map/MiniMapXls")

local WorldMap = class("WorldMap")

function WorldMap:ctor(  )
    self.WORLDMAP_WIDTH = 2458
    self.WORLDMAP_HEIGHT = 690
end

function WorldMap:Awake(go)
    self.gameObject = go
    self.transform = go.transform

    self.widgets = {
        {field="worldScrollview",path="ScrollView",src=LuaPanel},
        {field="uiLayer",path="ScrollView.UILayer",src="Transform"},
        {field="sceneItem",path="ScrollView.SceneSimpleItem",src=LuaToggle },
        {field="vSBar",path="Vertical Scroll Bar",src="UIScrollBar"},
        {field="hSBar",path="Horizontal Slider",src="UISlider"},
    }
    LuaUIHelper.bind(self.gameObject , self)
end


function WorldMap:onCreate( )

    self.cells = self.cells or {}

    if table.nums(self.cells) <= 0 then
        local childCount = self.uiLayer.transform.childCount
        for i=1,childCount do
            local cellObj = self.uiLayer.transform:GetChild(i - 1)
            local cell = WorldMap.MapCell.new(self , cellObj.gameObject)
            local nameArr = string.split(cellObj.gameObject.name , "_")
            cell.sceneId = tonumber(nameArr[2])

            local mapItem = MapDataXls[cell.sceneId]
            local openTipMsg = MiniMapXls[cell.sceneId]
            --设置名字信息
            cell:setInfo(mapItem.Name , openTipMsg and openTipMsg.miniMapLvTips or "null", mapItem.Level)

            self.cells[cell.sceneId] = cell
        end
    end

    for _,cell in pairs(self.cells) do
        cell:setRoleLv(HERO.Grade)
    end
end


function WorldMap:lookatScene( sceneId )
    if not self.cells[sceneId] then     return      end

    local localPos = self.cells[sceneId].mainObj.transform.localPosition

    self.hSBar.value = localPos.x / self.WORLDMAP_WIDTH + 0.5
    self.worldScrollview:asScrollView():UpdatePosition()
end

function WorldMap:initPage(  )
    self:setActivePage(true)

    self:onCreate()


    self:lookatScene(DetailMapCtrl.currentSceneID)

    DetailMapPanel.btnBack.gameObject:SetActive(false)

    if self.cells[DetailMapCtrl.currentSceneID] then
        self.cells[DetailMapCtrl.currentSceneID]:enable(true)
    end
end


function WorldMap:setActivePage( active )
    DetailMapPanel.worldMapPage.gameObject:SetActive(active)
end

function WorldMap:onClickCell( sceneId )
    -- DetailMapCtrl.initScenePage(sceneId)

    DetailMapCtrl.initScenePage(sceneId)

    DetailMapPanel.btnBack.gameObject:SetActive(true)
end

function WorldMap:onDestroy(  )

end

---------------------------------------------------

WorldMap.MapCell = class("WorldMap_MapCell")

function WorldMap.MapCell:ctor( parent , gObj )
    self.parent = parent
    self.mainObj = gObj
    self.openLevel = 0

    self.toggle = self.mainObj:GetComponent(typeof(UIToggle))
    self.imgBg = gObj.transform:Find("BgSprite"):GetComponent(typeof(UISprite))
end

function WorldMap.MapCell:enable( enableState )
    self.toggle.enabled = true
    self.toggle.value = enableState
end

--设置场景是否开放
function WorldMap.MapCell:setInfo( sceneName , openTip , openLv)
    local sceneLab = self.mainObj.transform:Find("SceneName");
    sceneLab = sceneLab.gameObject:GetComponent(typeof(UILabel))
    sceneLab.text = sceneName

    local lvLab = self.mainObj.transform:Find("Level");
    lvLab = lvLab.gameObject:GetComponent(typeof(UILabel))
    lvLab.text = openTip

    self.openLevel = openLv
end

function WorldMap.MapCell:setRoleLv( lv )

    self.roleLv = lv
    local isOpen = self.openLevel <= lv

    self.parent.behaviour:AddClick(self.mainObj , handler(self , self.onClick))


    --置灰
    self.imgBg.color = isOpen and Color.white or Color.black
    self.toggle.enabled = isOpen
    self.toggle.activeSprite.gameObject:SetActive(isOpen)
end

function WorldMap.MapCell:onClick( )
    if self.openLevel > self.roleLv then
        CtrlManager.PopUpNotifyText(string.format(LANGUAGE_TIP.worldMapNotOpen , self.openLevel))
        return
    end
    self.parent:onClickCell(self.sceneId)
end


return WorldMap