DetailMapPanel = {};
local this = DetailMapPanel;
local DetailMapCtrl = require("Controller.map.DetailMapCtrl")

--由LuaBehaviour自动调用
function DetailMapPanel.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	this.widgets = {
		{field="sceneMapPage",path="CurrentMapContext",src=LuaScript , requirePath="Controller.map.CurrentSceneMap"},
		{field="worldMapPage",path="WorldMapContext",src=LuaScript , requirePath="Controller.map.WorldMap"},
        {field="rightPageTab",path="rightTab",src="UITable" },
		{field="curSceneMapBtn",path="rightTab.1currentScene",src=LuaToggle, onChange = DetailMapCtrl.onSceneMap },
		{field="worldMapBtn",path="rightTab.0worldMap",src=LuaToggle, onChange = DetailMapCtrl.onWorldMap },
		{field="btnClose",path="TopPanel.btnClose",src=LuaButton, onClick = DetailMapCtrl.OnCloseBtnClick },
		--{field="mask",path="mask",src=LuaButton, onClick = DetailMapCtrl.OnCloseBtnClick },
		{field="btnBack",path="TopPanel.btnBack",src=LuaButton, onClick = this.onBackWorldMap },
	}
	LuaUIHelper.bind(this.gameObject , DetailMapPanel )
end

--由LuaBehaviour自动调用
function DetailMapPanel.OnInit()
    LuaUIHelper.addUIDepth(this.gameObject , DetailMapPanel)

    DetailMapCtrl.view = this  --注入Controller

    this.initCurrentPage()

    this.currentSceneID = roleMgr.curSceneNo
    MinimapHandler.sendMinimapState(this.currentSceneID , true)
end


function DetailMapPanel.initCurrentPage(  )
    local lastCurVal = this.curSceneMapBtn.value
    this.curSceneMapBtn.value = true

    if lastCurVal then
        DetailMapCtrl.onSceneMap(this.curSceneMapBtn)
    end
end

--返回世界地图
function DetailMapPanel.onBackWorldMap( )
    this.sceneMapPage:setActivePage(false)
    this.worldMapPage:initPage()
end

--由LuaBehaviour自动调用
function DetailMapPanel.OnClose()
    LuaUIHelper.removeUIDepth(this.gameObject)
    MinimapHandler.sendMinimapState(this.currentSceneID , false)
    MinimapModel.inst:clear()
    DetailMapCtrl.OnDestroy()
end

--单击事件--
function DetailMapPanel.OnDestroy()
	this.mStart = false
end

