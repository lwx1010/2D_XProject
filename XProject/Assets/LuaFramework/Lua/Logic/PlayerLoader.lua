
local PlayerLoader = {}
local this = PlayerLoader

local coDelay = nil

PlayerLoader.mount_model = 1
PlayerLoader.lingqin_model = 2
PlayerLoader.lingyi_model = 3
PlayerLoader.partnerhorse_model = 4
PlayerLoader.pet_model = 5
PlayerLoader.weapon_model = 6
PlayerLoader.shenyi_model = 7
PlayerLoader.jingmai_model = 8
PlayerLoader.beauty_model = 9


--围墙模型编号
PlayerLoader.DynData = {[13013] = 0 , [13012] = 0, [13021] = 0 }

--是否需要转变实体类型
function PlayerLoader.GetCustomEntityType(entityType, nmsg, syncNpc)
    if entityType == EntityType.EntityType_Npc then -- entityType为npc
        --是否需要转变为符文类型
        local staticNpcXls = require("xlsdata/Npc/StaticNpcXls")
        if staticNpcXls[nmsg.char_no] then
            if staticNpcXls[nmsg.char_no].NpcType == 2 then
                return EntityType.EntityType_Rune --符文
            end
        end
    end
    return nil
end

function PlayerLoader.SetCustomEntityInfo(entity, npcType, nmsg, syncPlayer, syncNpc)
    -- body
    local shape = syncNpc and syncNpc.shape or 0
    if this.DynData[shape] then
        local dynAirBlock = entity.gameObject:AddComponent(typeof(DynAirBlock))
        if dynAirBlock ~= nil then
            if shape > 13020 and shape < 13030 then
                dynAirBlock:GetAllGrid(nmsg.x, nmsg.z, 2, true)
            else
                dynAirBlock:GetAllGrid(nmsg.x, nmsg.z, 1, true)
            end
        end
        entity.gameObject.layer = LayerMask.NameToLayer("Selectable")
        entity.talk_distance = 2
    end
    if shape == 13011 then
        local staticAirBlock = entity.gameObject:AddComponent(typeof(StaticAirBlock))
        if staticAirBlock ~= nil then
            staticAirBlock:GetAllGrid(nmsg.x, nmsg.z, 1, true)
        end
        -- entity.gameObject.layer = LayerMask.NameToLayer("Selectable")
        -- entity.talk_distance = 2
    end

    -- 如果为自定义实体类型，需要设置实体类型字符串
    if entity.entityType == EntityType.EntityType_Custom then
        -- This is a example
        --entity.typeValue = "Custom"
        -- types your logic in this place
    end
    if entity.entityType == EntityType.EntityType_Monster then
        local targetXls = require("xlsdata/Npc/BattleNpcXls")[entity.char_id]
        if not targetXls then return end
        if targetXls.PlayRefreshAni ~= nil and targetXls.PlayRefreshAni == 1 then
            entity.roleAction:SetMonsterAppearImmediate()
        end

        if targetXls.CantHide then
            entity:SetKeyValue("CantHide", targetXls.CantHide)
            entity:UpdateBlockSetting(false)
        end
    end
    if entity.entityType == EntityType.EntityType_Role then
        entity:ShowComboKill(syncPlayer.multikill > 0, syncPlayer.multikill)
    end
end

-- 加载主角模型
function PlayerLoader.LoadMainRole(obj, nmsg)
    HERO.SetData(obj, nmsg)
    if not HERO.Shape or HERO.Shape == 0 then
        logError("MAINROLE SHAPE IS NOT AVALIABLE!!")
        return
    end
    if not HERO.Sex or HERO.Sex == 0 then
        logError("MAINROLE SEX IS NOT AVALIABLE!!")
        return
    end
    if obj.model ~= nil then
        obj:RecycleRoleModel()
    end
    local path
    if HERO.Fashion and HERO.Fashion > 0 then
        path = "Prefab/Model/player/"..HERO.Fashion
    else
        path = "Prefab/Model/player/"..HERO.Shape
    end
    local ObjectPool = Riverlake.ObjectPool.instance
    obj.model = ObjectPool:PushToPool(path, ObjectPool.POOL_INIT_SIZE, obj.transform)
    if obj.model == nil then
        obj.model = Util.LoadBucket(obj.transform)
        obj.isBucket = true
    end
    if obj.model ~= nil then
        NGUITools.SetLayer(obj.model, obj.gameObject.layer)
        obj.model.transform.localScale = Vector3.one
        --obj:SetJumpTrailEffectParent()
        --回收这个模型上的武器和翅膀
        local weaponParent = Util.Find(obj.model.transform, 'wuqi01')
        if weaponParent and weaponParent.childCount > 0 then
            destroy(weaponParent:GetChild(0).gameObject)
        end
        local wingsParent = Util.Find(obj.model.transform, 'chibang01')
        if wingsParent and wingsParent.childCount > 0 then
            destroy(wingsParent:GetChild(0).gameObject)
        end
    end

    return obj.model
end

-- 加载角色模型
function PlayerLoader.GetPlayerAndNpcResPath(type, sex, npcType, shape)
    local resStr = "Prefab/Model/"
    --npcType 0,普通npc， 1.雕像，2.玩家模型怪物
    if type == 2 or npcType > 0 then
        if not shape or shape == 0 then
            logWarn("PLAYER SHAPE IS NOT AVALIABLE!!")
            shape = 100001
        end
        resStr = resStr.."player/"..shape
    elseif type == 13 then
        resStr = resStr.."fuwen/"..shape
    else
        resStr = resStr.."npc/"..shape
    end
    return resStr
end

--加载npc模型
function PlayerLoader.LoadNpc(obj, shape)
    local isBlock = obj.model.activeSelf
    if obj.model ~= nil then
        obj:RecycleRoleModel()
    end
    local path 
    if obj.entityType == EntityType.EntityType_Rune then
        path = "Prefab/Model/fuwen/" .. shape 
    else
        path = "Prefab/Model/npc/" .. shape 
    end

    local ObjectPool = Riverlake.ObjectPool.instance
    obj.model = ObjectPool:PushToPool(path, ObjectPool.POOL_INIT_SIZE, obj.transform)
    if obj.model == nil then
        obj.model = Util.LoadBucket(obj.transform)
        obj.isBucket = true
    end
    if obj.model ~= nil then
        obj.model.transform.localScale = Vector3.one
        obj.model:SetActive(isBlock)
        --obj:SetJumpTrailEffectParent()
    end
    return obj.model
end

--加载玩家模型
function PlayerLoader.LoadPlayer(obj, shape)
    local isBlock = obj.model.activeSelf
    if obj.model ~= nil then
        obj:RecycleRoleModel()
    end
    local path = "Prefab/Model/player/" .. shape
    local ObjectPool = Riverlake.ObjectPool.instance
    obj.model = ObjectPool:PushToPool(path, ObjectPool.POOL_INIT_SIZE, obj.transform)
    if obj.model == nil then
        obj.model = Util.LoadBucket(obj.transform)
        obj.isBucket = true
    end
    if obj.model ~= nil then
        obj.model.transform.localScale = Vector3.one
        obj.model:SetActive(isBlock)
        --obj:SetJumpTrailEffectParent()
    end
    return obj.model
end

--设置信息
function PlayerLoader.SetXlsInfo(obj, entityType, charId, isyunbiao)
    if not obj then return end

    if entityType == 1 then

    elseif entityType == 2 then

    elseif entityType == 3 or entityType == 13 then
        local staticNpcXls = require("xlsdata/Npc/StaticNpcXls")
        if staticNpcXls[charId] then
            obj.gameObject.transform.localScale = Vector3.New(staticNpcXls[charId].Scale, staticNpcXls[charId].Scale, staticNpcXls[charId].Scale)
            if staticNpcXls[charId].headNameYoffset then
                obj:SetKeyValue("headNameYoffset", staticNpcXls[charId].headNameYoffset)
            end
            if staticNpcXls[charId].TitleSpr and #staticNpcXls[charId].TitleSpr>0 then
                obj:SetKeyValue("title", 1)
                obj:SetKeyValue("titleSpr", staticNpcXls[charId].TitleSpr)
                obj:SetKeyValue("title_effect", "")
            end
        end
    elseif entityType == 4 then
        local allNpcXls = require("xlsdata/Npc/BattleNpcXls")
        if allNpcXls[charId] then
            obj.gameObject.transform.localScale = Vector3.New(allNpcXls[charId].Scale, allNpcXls[charId].Scale, allNpcXls[charId].Scale)
            obj.isBoss = (allNpcXls[charId].BossType and (allNpcXls[charId].BossType == 1 or allNpcXls[charId].BossType == 19)) and true or false
            obj.punchAway = (allNpcXls[charId].PunchAway ~= nil and allNpcXls[charId].PunchAway == 1) and true or false
            obj:SetKeyValue("XlsNpcType", allNpcXls[charId].BossType)
            obj:SetKeyValue("ShowBlood", allNpcXls[charId].ShowBlood or 0)
            -- print("----------------", allNpcXls[charId].SelectScale or 1)
            obj:SetKeyValue("SelectScale", allNpcXls[charId].SelectScale or 1)
            obj:SetKeyValue("ClickWalk", allNpcXls[charId].ClickWalk or 0)

            if allNpcXls[charId].headNameYoffset then
                obj:SetKeyValue("headNameYoffset", allNpcXls[charId].headNameYoffset)
            end

            -- print("|============================", allNpcXls[charId].TitleSpr)
            if allNpcXls[charId].TitleSpr and #allNpcXls[charId].TitleSpr>0 then
                obj:SetKeyValue("title", 1)
                obj:SetKeyValue("titleSpr", allNpcXls[charId].TitleSpr)
                obj:SetKeyValue("title_effect", "")
            end
        end
    end
end


function PlayerLoader.LoadOtherShape(obj, entityType, weaponId, shenyiId)
    if not weaponId or weaponId <= 0 then
        weaponId = obj.weapon
    end

    this.LoadWeapon(obj, entityType, weaponId)
    this.LoadShenyi(obj, shenyiId)
end

function PlayerLoader.LoadOtherShapeOfPartner(obj, entityType, lingqinId, lingyiId)
    -- body
    this.LoadLingqin(obj, entityType, lingqinId)
    this.Loadlingyi(obj, entityType, lingyiId)
end


function PlayerLoader.LoadWeapon(obj, entityType, weaponId)
    if not obj then return end
    local id = obj.sex == 1 and 21000 or 22000
    weaponId = obj.showShenjianState > 0 and weaponId or id
    if not weaponId or weaponId == 0 then return end
    local xlsData = require("xlsdata/WeaponPointXls")
    local data = nil
    for k, v in pairs(xlsData) do
        if v.WeaponId == weaponId then
            data = v
            break
        end
    end
    if not data then
        logError(string.format("no weapon point avaliable in xlsdata: %d", weaponId))
        return
    end
    local prefabPath = "Prefab/Model/weapon/"..weaponId
    local isBlock = true
    if obj.model then
        isBlock = obj.model.activeSelf
    end
    if obj.weapon_model ~= nil then
        obj:RecycleOtherModel(this.weapon_model)
    end
    if obj.model == nil then
        return
    end
    local ObjectPool = Riverlake.ObjectPool.instance
    local weaponParent = Util.Find(obj.model.transform, 'wuqi01')
    if not weaponParent then
        logError(string.format("no weapon parent point avaliable in model: %d", weaponId))
        return
    end
    if #data.Offset > 0 then
        obj.weapon_model = ObjectPool:PushToPool(prefabPath, 5, weaponParent, data.Offset[1], data.Offset[2], data.Offset[3])
    else
        obj.weapon_model = ObjectPool:PushToPool(prefabPath, 5, weaponParent)
    end
    if obj.weapon_model == nil then
        return
    end
    NGUITools.SetLayer(obj.weapon_model, obj.gameObject.layer)
    obj.weapon_model.transform.localRotation = Quaternion.Euler(data.Rotation[1], data.Rotation[2], data.Rotation[3])
    -- print("----------------", obj.id, isBlock)
    -- obj.weapon_model:SetActive(isBlock)
    if obj.weapon ~= weaponId then
        obj.weapon = weaponId
    end
end

function PlayerLoader.LoadLingqin(obj, entityType, lingqinId)
    if not obj then return end
    local id = lingqinId
    if entityType ~= 6 then
        logError("LoadLingqin error, wrong entityType "..entityType)
        return
    end
    if not id or id == 0 then
        id = 31000
    end
    local prefabPath = "Prefab/Model/lingqin/"..id
    local isBlock = true
    if obj.model then
        isBlock = obj.model.activeSelf
    end
    if obj.model == nil then
        return
    end
    if obj.lingqin_model ~= nil then
        obj:RecycleOtherModel(this.lingqin_model)
    end
    local ObjectPool = Riverlake.ObjectPool.instance
    obj.lingqin_model = ObjectPool:PushToPool(prefabPath, 2, obj.transform)
    if obj.lingqin_model == nil then
        return
    end
    if obj.lingqin ~= id then
        obj.lingqin = id
    end

    if obj.horse and obj.horse.ridePosition then
        obj.lingqin_model.transform.localPosition = obj.horse.ridePosition
    end
    obj.roleAction:ResetLingQinAnimation(obj.lingqin_model)
    obj.lingqin_model:SetActive(isBlock)
end

function PlayerLoader.LoadShenyi(obj, shenyiId)
    if not obj then return end
    if not shenyiId or shenyiId == 0 then return end
    local prefabPath = "Prefab/Model/wings/"..shenyiId
    local shenyi_model = nil
    if obj.shenyi_model ~= nil then
        obj:RecycleOtherModel(this.shenyi_model)
    end
    local isBlock = true
    if obj.model then
        isBlock = obj.model.activeSelf
    end
    local xlsData = require("xlsdata/WingsPointXls")
    local data = nil
    for k, v in pairs(xlsData) do
        if v.WingsId == shenyiId then
            data = v
            break
        end
    end
    if obj.model == nil then
        return
    end
    local ObjectPool = Riverlake.ObjectPool.instance
    obj.shenyi_model = ObjectPool:PushToPool(prefabPath, 5, Util.Find(obj.model.transform, 'chibang01'), 0, 0, 0, 0, -90, 0)
    if obj.shenyi_model == nil then
        return
    end
    NGUITools.SetLayer(obj.shenyi_model, obj.gameObject.layer)
    if data ~= nil and #data.Offset > 0 then
        obj.shenyi_model.transform.localPosition = Vector3.New(data.Offset[1], data.Offset[2], data.Offset[3])
    end
    if obj.shenyi ~= shenyiId then
        obj.shenyi = shenyiId
    end
    obj.roleAction:ResetWingsAnimation(obj.shenyi_model)
    obj.shenyi_model:SetActive(obj.showShenyiState ~= 0)  --isBlock and
end

function PlayerLoader.Loadlingyi(obj, entityType, lingyiId)
    if not obj then return end
    local id = lingyiId
    if entityType ~= 6 then
        logError("Loadlingyi error, wrong entityType "..entityType)
        return
    end
    if not id or id == 0 then return end
    local prefabPath = "Prefab/Model/lingyi/"..id
    local isBlock = true
    if obj.model then
        isBlock = obj.model.activeSelf
    end
    if obj.lingyi_model ~= nil then
        obj:RecycleOtherModel(this.lingyi_model)
    end
    local xlsData = require("xlsdata/WingsPointXls")
    local data = nil
    for k, v in pairs(xlsData) do
        if v.WingsId == id then
            data = v
            break
        end
    end
    if obj.model == nil then
        return
    end
    local ObjectPool = Riverlake.ObjectPool.instance
    obj.lingyi_model = ObjectPool:PushToPool(prefabPath, 2, Util.Find(obj.model.transform, 'chibang01'), 0, 0, 0, 90, 0, 0)
    if obj.lingyi_model == nil then
        return
    end
    if data ~= nil and #data.Offset > 0 then
        obj.lingyi_model.localPosition = Vector3.New(data.Offset[1], data.Offset[2], data.Offset[3])
    end
    if obj.lingyi ~= id then
        obj.lingyi = id
    end
    obj.roleAction:ResetWingsAnimation(obj.lingyi_model)
    obj.lingyi_model:SetActive(isBlock)
end

function PlayerLoader.LoadHorse(obj, entityType, horseId)
    if not obj then return end
    local id = horseId
    if entityType == EntityType.EntityType_Self and horseId == 0 then
        id = HERO.MountModel
    end
    if not id or id == 0 then return end
    local prefabPath = "Prefab/Model/horse/"..id
    local isBlock = true
    if obj.model then
        isBlock = obj.model.activeSelf
    end
    if obj.model == nil then
        return
    end
    if obj.horse.horseGo ~= nil then
        if entityType == EntityType.EntityType_Partner then
            obj:RecycleOtherModel(this.partnerhorse_model)
        else
            obj:RecycleOtherModel(this.mount_model)
        end
    end
    local initSize = 3
    if id == 30001 then
        initSize = 10
    end
    local xlsData = require("xlsdata/HorsePointXls")
    local data = nil
    for k, v in pairs(xlsData) do
        if v.HorseId == id then
            data = v
            break
        end
    end
    local ObjectPool = Riverlake.ObjectPool.instance
    obj.horse.horseGo = ObjectPool:PushToPool(prefabPath, initSize, obj.transform)
    if obj.horse.horseGo == nil then
        return
    end
    NGUITools.SetLayer(obj.horse.horseGo, obj.gameObject.layer)
    if data ~= nil then
        obj.horse:SetHorseData(data.Offset[1], data.Offset[2], data.Offset[3], data.RideRunAudio)
    else
        obj.horse:SetHorseData(0, 0, 0, "")
    end
    obj.horse.horseGo:SetActive(isBlock)
end

function PlayerLoader.LoadLingqi(obj, lingqiId, callback)
    if not obj then
        if callback ~= nil then roleMgr.entityCreate:OnModelCreateFailed(callback) end
        return
    end
    local id = lingqiId
    if not id or id == 0 then
        if callback ~= nil then roleMgr.entityCreate:OnModelCreateFailed(callback) end
        return
    end
    local prefabPath = "Prefab/Model/lingqi/"..id
    if obj.lingqiObj and obj.lingqiObj.model ~= nil then
        obj.lingqiObj:RecycleRoleModel()
    end
    Riverlake.ObjectPool.instance:AsyncPushToPool(prefabPath, 2, obj.lingqiObj.transform, 0, 0, 0, callback)
end

function PlayerLoader.LoadPartner(obj, partnerId, callback)
    if not obj then
        if callback ~= nil then roleMgr.entityCreate:OnModelCreateFailed(callback) end
        return
    end
    local id = partnerId
    if not id or id == 0 then
        if callback ~= nil then roleMgr.entityCreate:OnModelCreateFailed(callback) end
        return
    end
    local prefabPath = "Prefab/Model/huoban/"..id
    if obj.partnerObj and obj.partnerObj.model ~= nil then
        obj.partnerObj:RecycleRoleModel()
    end
    Riverlake.ObjectPool.instance:AsyncPushToPool(prefabPath, 2, obj.partnerObj.transform, 0, 0, 0, callback)
end

--加载助战小伙伴
function PlayerLoader.LoadAider(obj, aiderId, callback)
    --print("-------------", obj, model)
    local prefabPath = "Prefab/Model/npc/"..aiderId

    Riverlake.ObjectPool.instance:AsyncPushToPool(prefabPath, 1, obj.transform, 0, 0, 0, callback)
end

--加载巡逻npc
function PlayerLoader.LoadXunLuoNpc(obj, shape, callback)
    --print("-------------", obj, model)
    local prefabPath = "Prefab/Model/npc/"..shape

    Riverlake.ObjectPool.instance:AsyncPushToPool(prefabPath, 1, obj.transform, 0, 0, 0, callback)
end


--加载宠物
function PlayerLoader.LoadPet(obj, petId, callback)
    if not obj then
        if callback ~= nil then roleMgr.entityCreate:OnModelCreateFailed(callback) end
        return
    end
    local id = 0
    if obj.entityType == EntityType.EntityType_Self then
        id = HERO.PetModel
    else
        id = petId
    end
    if not id or id == 0 then
        if callback ~= nil then roleMgr.entityCreate:OnModelCreateFailed(callback) end
        return
    end
    if obj.petObj and obj.petObj.model ~= nil then
        obj:RecycleOtherModel(this.pet_model)
    end
    local prefabPath = "Prefab/Model/pet/"..id
    obj.pet = petId
    Riverlake.ObjectPool.instance:AsyncPushToPool(prefabPath, 2, obj.petObj.transform, 0, 0, 0, callback)
end

--加载神使
function PlayerLoader.LoadBeauty(obj, yunbiao, callback)

end

---------------------------------------------------------------------------
--其他玩家更新处理
function PlayerLoader.UpdatePlayerIntShape(obj, attrs)
    if not obj then return end

    -- 更新角色模型相关绑定属性
    if attrs.fashion and attrs.fashion ~= 0 then
        if obj.fashion ~= attrs.fashion then
            PLAYERLOADER.LoadPlayer(obj, attrs.fashion)
            obj:ChangeShader(obj.model)
            obj.fashion = attrs.fashion
            PLAYERLOADER.LoadOtherShape(obj, 2, obj.weapon, obj.shenyi)
            obj:ResetHeadTitle()

            if not obj.isShapeChanged then
                roleMgr.entityCreate:AddFastShadow(obj, EntityType.EntityType_Role)
                obj.roleAction:ResetRoleAnimation(obj.model)
            else
                obj.model:SetActive(false)
            end
        end
    end
    if attrs.weapon and attrs.weapon ~= 0 then
        if obj.weapon ~= attrs.weapon then
            if obj.showShenjianState == 0 then
                obj.weapon = attrs.weapon
            else
                PLAYERLOADER.LoadWeapon(obj, 2, attrs.weapon)
            end
        end
    end
    if attrs.mount_model and attrs.mount_model ~= 0 then
        if obj.horse.id ~= attrs.mount_model and obj.isRide == true then
            if obj.isShapeChanged then
                obj.horse.changedId = attrs.mount_model
            else
                obj.horse:ChangeHorse(attrs.mount_model)
            end
        else
            if obj.isShapeChanged then
                obj.horse.changedId = attrs.mount_model
            else
                obj.horse.id = attrs.mount_model
            end
        end
    end

    if attrs.up_mount and obj.horse then
        obj.isRide = attrs.up_mount == 1
    end

    if attrs.partnerhorse_model and attrs.partnerhorse_model ~= 0 then
        if obj.partnerObj then
            if obj.partnerObj.horse.id ~= attrs.partnerhorse_model and obj.partnerObj.isRide == true then
                obj.partnerObj.horse:ChangeHorse(attrs.partnerhorse_model)
            else
                obj.partnerObj.horse.id = attrs.partnerhorse_model
                obj.partnerObj.isRide = true
            end
        end
    end

    if attrs.up_horse and obj.partnerObj and obj.partnerObj.horse then
        obj.partnerObj.isRide = attrs.up_horse == 1
    end

    if attrs.shenyi_model ~= nil and attrs.shenyi_model ~= 0 then
        if obj.shenyi ~= attrs.shenyi_model then
            PLAYERLOADER.LoadShenyi(obj, attrs.shenyi_model)
        end
    end
    if attrs.lingyi_model ~= nil and attrs.lingyi_model ~= 0 then
        if obj.lingyi ~= attrs.lingyi_model then
            PLAYERLOADER.Loadlingyi(obj.partnerObj, 6, attrs.lingyi_model)
        end
    end
    if attrs.lingqin_model ~= nil and attrs.lingqin_model ~= 0 then
        if obj.lingqin ~= attrs.lingqin_model then
            PLAYERLOADER.LoadLingqin(obj.partnerObj, 6, attrs.lingqin_model)
        end
    end
    if attrs.pet_model ~= nil and attrs.pet_model ~= 0 then
        -- local function callback(model, name)
        --     obj.petObj.model = model
        --     if User_Config.blockOtherPet == 1 and obj.entityType ~= EntityType.EntityType_Self then
        --         obj.petObj.model:SetActive(false)
        --     end
        --     obj.petObj.roleAction:ResetRoleAnimation(model)
        --     roleMgr.entityCreate:AddFastShadow(model, EntityType.EntityType_Pet)
        --     obj:ChangeShader(model)
        -- end
        if obj.pet ~= attrs.pet_model then
            roleMgr:CreatePet(obj, attrs.pet_model)
        end
    end
    if attrs.shenjian_model ~= nil and attrs.shenjian_model ~= 0 then
        obj.shenjian = attrs.shenjian_model
    end
end

-- function PlayerLoader.UpdatePlayerStringShape(obj, attrs)
--     if not obj then return end

--     local roleMgr = roleMgr
--     if attrs.magic and #attrs.magic > 0  then
--         roleMgr:CreateLingqi(obj, attrs.magic)
--     end
--     if attrs.partner and #attrs.partner > 0 then
--         roleMgr:CreatePartner(obj, attrs.partner)
--     end
-- end

function PlayerLoader.ShowHidePlayerShape(obj, attrs)
    if not obj then return end

    if attrs.lingqin_model_state and obj.lingqin > 0 then
        if attrs.lingqin_model_state == 0 then
            PLAYERLOADER.LoadLingqin(obj.partnerObj, 6, 31000)
        else
            PLAYERLOADER.LoadLingqin(obj.partnerObj, 6, obj.lingqin)
        end
    end

    if attrs.lingyi_model_state and obj.partnerObj and obj.partnerObj.lingyi_model then
        obj.partnerObj.lingyi_model:SetActive(attrs.lingyi_model_state ~= 0)
    end

    if attrs.partnerhorse_model_state and obj.partnerhorse > 0 then
        if attrs.partnerhorse_model_state == 0 then

        end
    end
    if attrs.pet_model_state and obj.petObj and obj.petObj.model then
        obj.petObj.model:SetActive(attrs.pet_model_state ~= 0)
    end

    if attrs.shenjian_model_state and obj.shenjian > 0 then
        local id = attrs.shenjian_model_state == 0 and (obj.sex == 1 and 21000 or 22000) or obj.shenjian
        PLAYERLOADER.LoadWeapon(obj, 2, id)
    end

    if attrs.shenyi_model_state and obj.shenyi > 0 and obj.shenyi_model then
        obj.shenyi_model:SetActive(attrs.shenyi_model_state ~= 0)
    end
    if attrs.magic_model_state and obj.lingqiObj and obj.lingqiObj.model then
        obj.lingqiObj.model:SetActive(attrs.magic_model_state ~= 0)
    end
    if attrs.partner_model_state and obj.partnerObj and obj.partnerObj.model then
        obj.partnerObj.gameObject:SetActive(attrs.partner_model_state ~= 0)
    end
end

----------------------------------------------------------------------------

return PlayerLoader