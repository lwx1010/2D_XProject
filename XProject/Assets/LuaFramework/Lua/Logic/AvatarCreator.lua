local AvatarCreator = {}
local roleMgr = roleMgr
local this = AvatarCreator
local ModelTransform = {} --require('xlsdata/ModelTransformXls')

------------------------------------------------------------------------
--异步
local avatarBehaviour 
local coroutineTbl
local gameObject

function AvatarCreator.OnCreate()
    gameObject = GameObject.New("AvatarCreator")
    avatarBehaviour = gameObject:AddComponent(typeof(LuaFramework.LuaBehaviour))
    coroutineTbl = {}
end

function AvatarCreator.AddCoToTabel(panelName, co)
    if not coroutineTbl[panelName] then
        coroutineTbl[panelName] = {}
    end

    table.insert(coroutineTbl[panelName], co)
end

function AvatarCreator.PanelCloseStopCoroutine(panelName)
    if not coroutineTbl then return end

    if not coroutineTbl[panelName] then
        return 
    end

    for k, v in pairs(coroutineTbl[panelName]) do
        avatarBehaviour:StopCoroutine(v)
    end

    coroutineTbl[panelName] = nil
end

function AvatarCreator.StopOneCoroutine(panelName, co)
    if not coroutineTbl then return end

    if not coroutineTbl[panelName] then
        return 
    end

    for k, v in pairs(coroutineTbl[panelName]) do
        if v == co then
            avatarBehaviour:StopCoroutine(v)
            table.remove(coroutineTbl[panelName], k)
            return
        end
    end
end

function AvatarCreator.SceneChangeClear()
    if avatarBehaviour then
        avatarBehaviour:StopAllCoroutines()
    end

    destroy(gameObject)
end

function AvatarCreator.OnDestroy()
    AvatarCreator.SceneChangeClear()

    gameObject = nil
    coroutineTbl = {}
    avatarBehaviour = nil
end
------------------------------------------------------------------------

function AvatarCreator.CreateAvatar(path, parent, renderQueue, panel, createCallBack, scale, uidepth, extendFactor)
    --print("------------1111-------------", avatarBehaviour)
    if not avatarBehaviour then 
        this.OnCreate()
    end

    local co = avatarBehaviour:StartCoroutine(AL.ObjectPool.instance:AsyncPushAvatarToPool(path, function(prefab, prefabPath)
            if not prefab then
                CtrlManager.PopUpNotifyText(LANGUAGE_TIP.model_notexist)
                local model = this.CreateBucket(parent, renderQueue, uidepth)
                if createCallBack then
                    createCallBack(model)
                end
                logError("create avatar error!"..prefabPath.." is not exits!")
                return
            end

            if not parent then 
                logError("create avatar error! parent has been destroyed!")
                return
            end
            --回收人物武器和翅膀
            local pathTbl = string.split(path, '/')
            if pathTbl[3] == "player" then
                local weaponParent = Util.Find(prefab.transform, 'wuqi01')
                if weaponParent and weaponParent.childCount > 0 then
                    destroy(weaponParent:GetChild(0).gameObject)
                end
                local wingsParent = Util.Find(prefab.transform, 'chibang01')
                if wingsParent and wingsParent.childCount > 0 then
                    destroy(wingsParent:GetChild(0).gameObject)
                end
            elseif pathTbl[3] == "huoban" then
                local pWingsParent = Util.Find(prefab.transform, 'chibang01')
                if pWingsParent and pWingsParent.childCount > 0 then
                    destroy(pWingsParent:GetChild(0).gameObject)
                end
            end
            --local model = newObject(prefab)
            local model = prefab
            model.layer = LayerMask.NameToLayer("UIModel")
            local count = model.transform.childCount
            for i=0, count-1 do
                model.transform:GetChild(i).gameObject.layer = LayerMask.NameToLayer("UIModel")
            end
            model.transform:SetParent(parent)
            local avatar = parent:GetComponent(typeof(AvatarBase))
            if not avatar then
                avatar = parent.gameObject:AddComponent(typeof(AvatarBase))
            end
            local arr = string.split(path,'/')
            local shape = tonumber(arr[#arr])
            local xls = ModelTransform[shape]
            uidepth = uidepth and uidepth or CtrlManager.panelDepth
            local index = (uidepth - 30) / 40
            local depth = 10000
            if index >= 0 then
                depth = 10000 - 800 * index
            elseif index < 0 then
                depth = 10000 + 800 * index
            end
            if xls then
                model.transform.rotation = Quaternion.Euler(xls.Rotation.x, xls.Rotation.y, xls.Rotation.z)
                model.transform.localScale = Vector3(xls.Scale.x, xls.Scale.y, xls.Scale.z)
                model.transform.localPosition = Vector3.New(xls.CenterOffset.x, xls.CenterOffset.y, depth)
                avatar:SetAvatar(model, xls.CanClick == 1, xls.CanRotate==1)
                avatar.showAction = xls.ShowAction and xls.ShowAction or 0
                avatar.clickAction = xls.ClickAction and xls.ClickAction or 0
            else
                model.transform.rotation = Quaternion.Euler(0, 180, 0)
                model.transform.localPosition = Vector3.New(0, 0, depth)
                model.transform.localScale = scale and Vector3(scale, scale, scale) or Vector3.New(300,300,300)
                avatar:SetAvatar(model, false, false)
                avatar.showAction = 0
                avatar.clickAction = 0
            end
            
            if renderQueue ~= nil then
                ----包括粒子特效
                local scaleFactor = scale and scale or (xls and xls.Scale.x or 300)
                if extendFactor then scaleFactor = scaleFactor * extendFactor end
                Util.SetRenderQueue(model.transform, renderQueue, scaleFactor)
            end

            if createCallBack then
                createCallBack(model)
            end

            --return model, xls and xls.Scale.x or 300
        end))

    this.AddCoToTabel(panel, co)
    return co
end

function AvatarCreator.CreateBucket(parent, renderQueue, uidepth)
    local prefab = Util.LoadBucket(nil)
    if not prefab then return end
    prefab.layer = LayerMask.NameToLayer("UIModel")
    local count = prefab.transform.childCount
    for i=0, count-1 do
        prefab.transform:GetChild(i).gameObject.layer = LayerMask.NameToLayer("UIModel")
    end
    prefab.transform:SetParent(parent)
    local avatar = parent:GetComponent(typeof(AvatarBase))
    if not avatar then
        avatar = parent.gameObject:AddComponent(typeof(AvatarBase))
    end
    uidepth = uidepth and uidepth or CtrlManager.panelDepth
    local index = (uidepth - 20) / 30
    local depth = 3000
    if index >= 0 then
        depth = 3000 - 500 * index
    elseif index < 0 then
        depth = 3000 + 500 * index
    end
    prefab.transform.rotation = Quaternion.Euler(-90, 0, 0)
    prefab.transform.localPosition = Vector3.New(0, 0, depth)
    prefab.transform.localScale = Vector3.New(200,200,200)
    avatar:SetAvatar(prefab, false, false)
    avatar.showAction = 0
    avatar.clickAction = 0

    if renderQueue ~= nil then
        ----包括粒子特效
        Util.SetRenderQueue(prefab.transform, renderQueue, 200)
    end
    return prefab, 200
end

function AvatarCreator.CreateWeapon(weaponId, charModel, renderQueue, panel, createCallBack, scaleFactor)
    if not charModel then
        logError(string.format("charModel is nil: %d", weaponId or 0, debug.traceback()))
        return
    end
    --print("------------2222-------------", avatarBehaviour)
    if not avatarBehaviour then 
        this.OnCreate()
    end

    if string.find(charModel.name, "bucket") ~= nil then
        return
    end
	local xlsData = require("xlsdata/WeaponPointXls")
    local data = nil
    for k, v in pairs(xlsData) do
        if v.WeaponId == weaponId then
            data = v
            break
        end
    end
    if not data then
        logError(string.format("no weapon point avaliable in xlsdata: %d", weaponId or 0))
        return
    end
    
    local weaponPath = "Prefab/Model/weapon/"..weaponId
    local co = avatarBehaviour:StartCoroutine(AL.ObjectPool.instance:AsyncPushAvatarToPool(weaponPath
        , function(prefab, path)
            if not prefab then 
                logError("武器模型加载错误" .. path)
                return
            end

            if not charModel or not charModel.transform then 
                logError("武器模型父销毁")
                return
            end

            --local weaponGo = newObject(prefab)
            local weaponGo = prefab
            local parent = Util.Find(charModel.transform, 'wuqi01')
            if parent.childCount > 0 then
                destroy(parent:GetChild(0).gameObject)
            end
            weaponGo.transform:SetParent(parent)
            weaponGo.transform.localPosition = #data.Offset > 0 
                and Vector3.New(data.Offset[1], data.Offset[2], data.Offset[3])
                or Vector3.zero

            weaponGo.transform.localRotation = Quaternion.Euler(data.Rotation[1], data.Rotation[2], data.Rotation[3])
            weaponGo.transform.localScale = Vector3.one
            weaponGo.layer = LayerMask.NameToLayer("UIModel")
            Util.ChangeToNormalShader(weaponGo.transform)
            local count = weaponGo.transform.childCount
            for i=0, count-1 do
                weaponGo.transform:GetChild(i).gameObject.layer = LayerMask.NameToLayer("UIModel")
            end
            if renderQueue ~= nil then
                --包括粒子特效
                Util.SetRenderQueue(weaponGo.transform, renderQueue, scaleFactor)
            end

            if createCallBack then
                createCallBack(weaponGo)
            end
        end))

    this.AddCoToTabel(panel, co)
    return co
end

function AvatarCreator.CreateWings(id, charModel, renderQueue, panel, createCallBack, scaleFactor)
    --print("------------3333-------------", avatarBehaviour)
    if not avatarBehaviour then 
        this.OnCreate()
    end

    if not id or id == 0 then return end 
    if not id then
        return
    end

    if string.find(charModel.name, "bucket") ~= nil  then
        return
    end

    local xlsData = require("xlsdata/WingsPointXls")
    local data = nil
    for k, v in pairs(xlsData) do
        if v.WingsId == id then
            data = v
            break
        end
    end

    local prefabPath = "Prefab/Model/wings/"..id
    local co = avatarBehaviour:StartCoroutine(AL.ObjectPool.instance:AsyncPushAvatarToPool(prefabPath
        , function(prefab, path)
            if not prefab or not charModel then 
                logError("翅膀模型加载错误" + path)
                return
            end

            if not charModel or not charModel.transform then 
                logError("翅膀模型父销毁")
                return
            end
            --local shenyi_model = newObject(prefab)
            local shenyi_model = prefab
            local parent = Util.Find(charModel.transform, 'chibang01')
            if parent.childCount > 0 then
                destroy(parent:GetChild(0).gameObject)
            end
            shenyi_model.transform:SetParent(parent)
            shenyi_model.transform.localRotation = Quaternion.Euler(0, -90, 0)
            if data ~= nil and #data.Offset > 0 then
                shenyi_model.transform.localPosition = Vector3.New(data.Offset[1], data.Offset[2], data.Offset[3])
            else
                shenyi_model.transform.localPosition = Vector3.zero
            end
            shenyi_model.layer = LayerMask.NameToLayer('UIModel')
            shenyi_model.transform.localScale = Vector3.one
            Util.ChangeToNormalShader(shenyi_model.transform)
            local count = shenyi_model.transform.childCount
            for i=0, count-1 do
                shenyi_model.transform:GetChild(i).gameObject.layer = LayerMask.NameToLayer("UIModel")
            end
            if renderQueue ~= nil then
                --包括粒子特效
                Util.SetRenderQueue(shenyi_model.transform, renderQueue, scaleFactor)
            end

            if createCallBack then
                createCallBack(shenyi_model)
            end
        end))

    this.AddCoToTabel(panel, co)
    return co
end

function AvatarCreator.CreateHorse(horseId, parent, charModel, renderQueue, panel, createCallBack, scaleFactor, rotateFactor)
    if not charModel then
        logError(string.format("charModel is nil: %d", horseId or 0, debug.traceback()))
        return
    end
    if not avatarBehaviour then 
        this.OnCreate()
    end

    if string.find(charModel.name, "bucket") ~= nil then
        return
    end

    local xlsData = require("xlsdata/HorsePointXls")
    local data = nil
    for k, v in pairs(xlsData) do
        if v.HorseId == horseId then
            data = v
            break
        end
    end

    local prefabPath = "Prefab/Model/horse/" .. horseId
    local co = avatarBehaviour:StartCoroutine(AL.ObjectPool.instance:AsyncPushAvatarToPool(prefabPath
        , function(prefab, path)
            if not prefab then 
                logError("坐骑加载错误" .. path)
                return
            end

            if not charModel or not charModel.transform then 
                logError("坐骑模型父销毁")
                return
            end
            local horse_model = prefab
            local horseTrans = Util.Find(horse_model.transform, 'zuoqi01')
            local uidepth = CtrlManager.panelDepth
            local index = (uidepth - 30) / 40
            local depth = 10000
            if index >= 0 then
                depth = 10000 - 800 * index
            elseif index < 0 then
                depth = 10000 + 800 * index
            end
            horse_model.transform.position = Vector3.zero
            horse_model.transform.localScale = Vector3.one
            horse_model.transform.rotation = Quaternion.Euler(0, 0, 0)
            horse_model.transform:SetParent(parent)
            horse_model.transform.localPosition = Vector3.New(0, 0, depth)
            charModel.transform:SetParent(horseTrans)
            charModel.transform.localScale = Vector3.one
            charModel.transform.position = Vector3.zero
            charModel.transform.rotation = Quaternion.Euler(0, 0, 0)
            if data ~= nil and #data.Offset > 0 then
                charModel.transform.position = horseTrans.position + Vector3.New(data.Offset[1], data.Offset[2], data.Offset[3])
            end
            horse_model.layer = LayerMask.NameToLayer('UIModel')
            horse_model.transform.localScale = Vector3.New(scaleFactor, scaleFactor, scaleFactor)
            horse_model.transform.rotation = Quaternion.Euler(0, rotateFactor, 0)
            Util.ChangeToNormalShader(horse_model.transform)
            local count = horse_model.transform.childCount
            for i=0, count-1 do
                horse_model.transform:GetChild(i).gameObject.layer = LayerMask.NameToLayer("UIModel")
            end
            if renderQueue ~= nil then
                --包括粒子特效
                Util.SetRenderQueue(horse_model.transform, renderQueue, scaleFactor)
            end

            if createCallBack then
                createCallBack(horse_model)
            end
        end))

    this.AddCoToTabel(panel, co)
    return co
end

return AvatarCreator