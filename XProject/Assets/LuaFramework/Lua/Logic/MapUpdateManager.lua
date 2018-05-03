--[===[
Author: 柯明余
Time:   2017-12-14
Note:   场景更新入口管理
]===]
local BangHuiLogic = require "Logic/BangHuiLogic"
MapUpdateManager = {}
local this = MapUpdateManager

function MapUpdateManager.Update(mapNo)
    WangfuSnatchManager.SceneUpdate(mapNo)
    MultiCopyManager.SceneUpdate(mapNo)
    FieldBossManager.SceneUpdate(mapNo)
    KillingFieldsManager.SceneUpdate(mapNo)
    BangHuiLogic.SceneUpdate(mapNo)
    ThreeUnitRacesModel.inst:IsInReadyMap(mapNo)
end 

