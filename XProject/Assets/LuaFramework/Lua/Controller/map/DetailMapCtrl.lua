-- @Author: LiangZG
-- @Date:   2017-02-28 11:01:08
-- @Last Modified time: 2017-12-12 16:34:41
-- @Desc    当前地图及世界地图界面逻辑处理

local MiniMapXls = require("xlsdata/Map/MiniMapXls")

DetailMapCtrl = {}
local this = DetailMapCtrl

local behaviour

function DetailMapCtrl.New()
	return this
end

function DetailMapCtrl.Awake(closeMainCamera)
	createPanel('map/DetailMap', this.OnCreate, closeMainCamera)
end

function DetailMapCtrl.OnCreate(go)
	this.gameObject = go
	this.transform = go.transform
	this.currentSceneID = roleMgr.curSceneNo

	local curMapData = MiniMapXls[this.currentSceneID]
	if curMapData then
		DetailMapPanel.worldMapBtn.gameObject:SetActive(curMapData.isWorldPage == 1)
		DetailMapPanel.rightPageTab.repositionNow = true
	end
end


--按钮点击

function DetailMapCtrl.OnCloseBtnClick()
	panelMgr:ClosePanel("DetailMap")

	--this.view.sceneMapPage:onDestroy()
end


function DetailMapCtrl.onSceneMap( toggle )
	if not toggle.value then	return 	end
	--Debugger.Log("------onSceneMap------")
	this.initScenePage(this.currentSceneID)
end

--初始化场景详细分布
function DetailMapCtrl.initScenePage( sceneId )
	DetailMapPanel.btnBack.gameObject:SetActive(false)
	this.view.worldMapPage:setActivePage(false)
	this.view.sceneMapPage:initPage( sceneId)
end


function DetailMapCtrl.onWorldMap( toggle )
	if not toggle.value then	return 	end
	--Debugger.Log("------onWorldMap------")
	this.view.sceneMapPage:setActivePage(false)
	this.view.worldMapPage:initPage()
end

--------------------------------------------------------------------

function DetailMapCtrl.OnDestroy()
	this.view.sceneMapPage:onDestroy()
end

return DetailMapCtrl