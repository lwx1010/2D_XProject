-- @Author: luweixing
-- @Date:   2017-12-19 16:25:28
-- @Last Modified by:   luweixing
-- @Last Modified time: 2017-12-19 16:25:37

local HeadTitle = {}

local this = HeadTitle
local LANGUAGE_TIP = LANGUAGE_TIP

local playerNameColors = { "[bdf2ff]", "[ffeb66]", "[ff3e24]" }

function HeadTitle.SetEntityName(titleScript, entity, name, colorType)
	if entity.entityType == EntityType.EntityType_Npc then
        titleScript.nameLab.fontSize = 32
        titleScript.nameLab.text = string.format("[21f7ff]%s[-]", name)
        if COMMONCTRL.JudgeNpcNeedTaskFlag(entity.char_id) then
        	titleScript:ShowTaskFlag()
        end
    elseif entity.entityType == EntityType.EntityType_Self then
        titleScript.nameLab.fontSize = 30
        if colorType == 0 or colorType == 1 then
            titleScript.nameLab.text = string.format("[ffffff]%s[-]", name)
        else
            titleScript.nameLab.text = string.format("%s%s[-]", playerNameColors[colorType], name)
        end
        if entity.husongTitle ~= nil and entity.husongTitle ~= "" then
            titleScript:SetHusongSprite(entity.husongTitle)
        end
    elseif entity.entityType == EntityType.EntityType_Role or entity.entityType == EntityType.EntityType_Robot then
        titleScript.nameLab.fontSize = 30
        
        --判断是否为仇人
        local FriendLogic = require('Logic/FriendLogic')
        local enemyList = FriendLogic.GetBriefEnemy()
        local HuaShanLunJianLogic = require("Logic/HuaShanLunJianLogic")
        if enemyList[entity.id] and not HuaShanLunJianLogic.IsInHSLJ then
            titleScript.nameLab.text = string.format("%s%s%s[-]", LANGUAGE_TIP.friendtext1,playerNameColors[colorType], name)
        else
            titleScript.nameLab.text = string.format("%s%s[-]", playerNameColors[colorType], name)
        end

        if entity.husongTitle ~= nil and entity.husongTitle ~= "" then
            titleScript:SetHusongSprite(entity.husongTitle)
        end
    elseif entity.entityType == EntityType.EntityType_Beauty then
        titleScript.nameLab.fontSize = 32
        titleScript.nameLab.text = string.format("[21f7ff]%s[-]", name)
    elseif entity.entityType == EntityType.EntityType_XunLuo then
        titleScript.nameLab.fontSize = 32
        titleScript.nameLab.text = string.format("[21f7ff]%s[-]", name)
    elseif entity.entityType == EntityType.EntityType_Statue then
        titleScript.nameLab.fontSize = 30
        titleScript.nameLab.text = string.format("[21f7ff]%s[-]", name)
    elseif entity.entityType == EntityType.EntityType_Monster then
        titleScript.nameLab.fontSize = 32
        titleScript.nameLab.text = string.format("[bdf2ff]%s[-]", name)
    elseif entity.entityType == EntityType.EntityType_Rune then
        titleScript.nameLab.fontSize = 32
        titleScript.nameLab.text = string.format("[bdf2ff]%s[-]", name)
    end
    if tonumber(entity:GetValueByKey("weddingshapestate")) == 1 then
        entity:ShowHideTitle(false)
    end
end

function HeadTitle.UpdateExtendInfo(titleScript, entity, leftTime, receiveTime)
	if titleScript.showType == 0 then
        this.SetClubAndMateName(titleScript, entity)
    elseif titleScript.showType == 1 then
    	this.SetUpTipText(titleScript, entity, leftTime, receiveTime)
   	end

    if titleScript.extendLab.gameObject.activeSelf and titleScript.extendLab.text ~= nil then
        titleScript.autoShow.transform.localPosition = Vector3.New(0, 45 + (titleScript.extendLab.height / 32 - 1) * 32, 0)
    else
        titleScript.autoShow.transform.localPosition = Vector3.New(0, 13 + (titleScript.extendLab.height / 32 - 1) * 32, 0)
    end

    this.SetVipText(titleScript, entity)
    this.LoadTitleSprite(titleScript, entity)
end

function HeadTitle.UpdateWeddingInfo(titleScript, entity, leftTime, receiveTime, sex)
	local female = sex == 2 and true or false
	if titleScript.showType == 0 then
        this.SetClubAndMateName(titleScript, entity, female , sex)
    elseif titleScript.showType == 1 then
        this.SetUpTipText(titleScript, entity, leftTime, receiveTime)
    end

    if titleScript.extendLab.gameObject.activeSelf and titleScript.extendLab.text ~= nil then
        titleScript.autoShow.transform.localPosition = Vector3.New(0, 45 + (titleScript.extendLab.height / 32 - 1) * 32, 0)
    else
        titleScript.autoShow.transform.localPosition = Vector3.New(0, 13 + (titleScript.extendLab.height / 32 - 1) * 32, 0)
    end

    this.SetVipText(titleScript, entity, female)
    this.LoadTitleSprite(titleScript, entity, female)
end

function HeadTitle.SetClubAndMateName(titleScript, entity, female, sex)
	local clubName = female == nil and entity:GetValueByKey("clubname") or not female and entity:GetValueByKey("club") or entity:GetValueByKey("fclub")
    local matename = female == nil and entity:GetValueByKey("matename") or not female and entity:GetValueByKey("mate") or entity:GetValueByKey("fmate")
    local enemyName = entity:GetValueByKey("enemyname")
    local hasClubName = clubName ~= nil and clubName ~= "" or false
    local hasMateName = matename ~= nil and matename ~= "" or false
    local hasEnemyName = enemyName ~= nil and enemyName ~= "" or false

    if hasClubName or hasMateName or hasEnemyName then
    	titleScript.extendLab.gameObject:SetActive(true)
    	local clubPost = female == nil and entity:GetValueByKey("clubpost") or not female and entity:GetValueByKey("job") or entity:GetValueByKey("fjob") or ""
        local matestr = ""
        local clubstr = ""
        local enemystr = ""
      	if hasClubName then
      		if entity.isHostileClub then
      			clubstr = string.format(LANGUAGE_TIP.titleInfo_2, clubName, clubPost)
            else
                clubstr = string.format("[63FF97]%s %s[-]", clubName, clubPost)
            end
        end
        if hasMateName then
            
            local sexTemp = female == nil and entity:GetValueByKey("sex") or 2
            sexTemp = sex == nil and sexTemp or sex

            if tonumber(sexTemp) == 1 then
                matestr = string.format(LANGUAGE_TIP.marriagetitle_1, matename)
            else
                matestr = string.format(LANGUAGE_TIP.marriagetitle_2, matename)
            end
        end
        if enemyName and enemyName ~= "" then
            enemystr = string.format(LANGUAGE_TIP.enemyTitle,enemyName)
        end

        titleScript.extendLab.text = ""

        if matestr ~= "" then
            if titleScript.extendLab.text == "" then
                titleScript.extendLab.text = matestr
            end
        end

        if enemystr ~= "" then
            if titleScript.extendLab.text == "" then
                titleScript.extendLab.text = enemystr
            else
                titleScript.extendLab.text = string.format("%s\n%s",titleScript.extendLab.text,enemystr)
            end
        end

        if clubstr ~= "" then
            if titleScript.extendLab.text == "" then
                titleScript.extendLab.text = clubstr
            else
                titleScript.extendLab.text = string.format("%s\n%s",titleScript.extendLab.text,clubstr)
            end
        end

        -- if matestr ~= "" and clubstr ~= "" then
        -- 	titleScript.extendLab.text = string.format("%s\n%s", matestr, clubstr)
        -- elseif matestr ~= "" and clubstr == "" then
        --     titleScript.extendLab.text = matestr
        -- elseif matestr == "" and clubstr ~= "" then
        --     titleScript.extendLab.text = clubstr
        -- end
    else
    	titleScript.extendLab.gameObject:SetActive(false)
    end
end

function HeadTitle.SetUpTipText(titleScript, entity, leftTime, receiveTime)
	titleScript.extendLab.gameObject:SetActive(false)
	if leftTime + receiveTime > TimeManager.GetRealTimeSinceStartUp() then
		titleScript.upTipLab.text = string.format("[ff341f]%s[-]", TimeConverter.CovertToString(leftTime + receiveTime - TimeManager.GetRealTimeSinceStartUp()))
	else
		titleScript.upTipLab.text = "[ff341f]00:00[-]"
	end
end

function HeadTitle.SetVipText(titleScript, entity, female)
	local vip = female == nil and entity:GetValueByKey("vip") or entity:GetValueByKey("fvip")
    if vip == nil or tonumber(vip) <= 0 then
        titleScript.VipSpr.gameObject:SetActive(false)
    else
        titleScript.VipSpr.gameObject:SetActive(true)
        titleScript.VipLevel.text = tostring(vip)
    end
end

function HeadTitle.LoadTitleSprite(titleScript, entity, female)
	local titlespr = not female and entity:GetValueByKey("title") or entity:GetValueByKey("ftitle")
    if titlespr == nil or tonumber(titlespr) == 0
        or (User_Config.blockAllTitleSpr ==1 and (entity.entityType == EntityType.EntityType_Role or entity.entityType == EntityType.EntityType_Robot)) then
        titleScript.titlespr.gameObject:SetActive(false)
        titleScript.autoShow.repositionNow = true
    else
        titleScript.titlespr.spriteName = female == nil and entity:GetValueByKey("titleSpr") or entity:GetValueByKey("ftitleSpr")
	    titleScript.titlespr.gameObject:SetActive(true)
	    if entity.entityType == EntityType.EntityType_Npc then
	        titleScript.titlespr.depth = 3
	    else
	        titleScript.titlespr.depth = 2
	    end
	    titleScript.titlespr:MakePixelPerfect()
	    titleScript.autoShow.repositionNow = true
	    if IsNil(titleScript.titleEffectObj) == false then
            Riverlake.ObjectPool.Recycle(titleScript.titleEffectObj)
	        titleScript.titleEffectObj = nil
	    end
	    local titleEffect = female == nil and entity:GetValueByKey("title_effect") or entity:GetValueByKey("ftitle_effect")
	    if titleEffect ~= "" then
	        titleScript.titleEffectObj = Riverlake.ObjectPool.instance:PushToPool("Prefab/UIEffect/title/"..titleEffect, 3, titleScript.titlespr.transform, 0, 0, 0)
	        local eft = titleScript.titleEffectObj.gameObject:GetComponent('UIParticles')
	        eft.parentWidget = titleScript.titlespr.transform:GetComponent('UIWidget')
	        eft.IsForward = true
	        titleScript.titleEffectObj.transform.parent = titleScript.titlespr.transform
	        titleScript.titleEffectObj.transform.localPosition = Vector3.zero
	        titleScript.titleEffectObj.transform.localScale = Vector3.one
	        eft:Play()
	    end

	    if tonumber(entity:GetValueByKey("weddingshapestate")) == 1 then
	        entity:ShowHideTitle(false)
	    end
    end
end

return HeadTitle