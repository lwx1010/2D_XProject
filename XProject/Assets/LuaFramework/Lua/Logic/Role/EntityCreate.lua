local EntityCreate = {}

local this = EntityCreate

function EntityCreate.SetSelfExtendInfo(obj, syncPlayer)
	obj:SetKeyValue("clubname", syncPlayer.clubname)
    obj:SetKeyValue("clubpost", syncPlayer.clubpost)
    obj:SetKeyValue("vip", syncPlayer.vip)
    obj:SetKeyValue("title", syncPlayer.title)
    obj:SetKeyValue("titleSpr", syncPlayer.titleSpr)
    obj:SetKeyValue("title_effect", syncPlayer.title_effect)
    obj:SetKeyValue("matename", syncPlayer.matename)
    obj:SetKeyValue("enemyname", syncPlayer.enemyname)
    obj:SetKeyValue("sex", syncPlayer.sex)
end

function EntityCreate.InitPlayerData(entityData, cmd)
    -- body
end

function EntityCreate.InitSelfData(entityData, cmd)
    -- body
end

function EntityCreate.InitNpcData(entityData, cmd)
    -- body
end

function EntityCreate.RefreshEntityData(entity, key, value)
    -- body
    
end

return EntityCreate