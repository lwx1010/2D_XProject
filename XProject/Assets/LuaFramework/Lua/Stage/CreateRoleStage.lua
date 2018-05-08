-- @Author: LiangZG
-- @Date:   2017-04-27 15:06:26
-- @Last Modified time: 2018-02-08 16:55:51
-- @Desc :  创角场景切换逻辑


local CreateRoleStage = class("CreateRoleStage" , BaseStage)

-- local userXls = require("xlsdata/UserXls")

function CreateRoleStage:ctor(  )
    CreateRoleStage.super.ctor(self , "CreateRoleScene" , "PreLoadingScene")
end

function CreateRoleStage:onEnter( transiter , stageLoader )
    print("-------> CreateRoleStage onEnter:" .. self.stageName)

    -- 预加载资源
    -- stageLoader:AddLoader(resMgr:LoadAssetAsync("Prefab/Model/player/" .. userXls[1].CreateMan) , 2)
    -- stageLoader:AddLoader(resMgr:LoadAssetAsync("Prefab/Model/player/" .. userXls[1].CreateFemale) , 2)

    -- stageLoader:AddLoader(resMgr:LoadAssetAsync("Prefab/Model/weapon/" .. userXls[1].CreateManWeapon) , 2)
    -- stageLoader:AddLoader(resMgr:LoadAssetAsync("Prefab/Model/weapon/" .. userXls[1].CreateFemaleWeapon) , 2)

    stageLoader:AddLoader(resMgr:LoadAssetAsync("Prefab/Gui/Login/CreateRolePanel") , 5)
end


function CreateRoleStage:onShow( ... )
    --预加载
    --resMgr:LoadPrefab("Gui/Cutscene/WorldView1")
    --resMgr:LoadPrefab("Gui/Cutscene/Curtain")

    local ctrl = CtrlManager.GetCtrl(CtrlNames.CreateRole);
    if ctrl ~= nil then
        ctrl:Awake();
    end

    CtrlManager.curIndex = 2
end



function CreateRoleStage:onExit( )
    print("CreateRoleStage onExit <----")
    panelMgr:ClosePanel("CreateRole")


end

return CreateRoleStage