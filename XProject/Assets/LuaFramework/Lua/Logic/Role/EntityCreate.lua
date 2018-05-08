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

function EntityCreate.SetExtendInfo(obj, entityType, nmsg, syncPlayer, syncNpc, npcType, iswedding)
	-- body
    local shape = syncPlayer and syncPlayer.shape or syncNpc.shape
    if syncPlayer ~= nil and syncPlayer.fashion > 0 then
        shape = syncPlayer.fashion
    end
    local grade = syncPlayer and syncPlayer.grade or syncNpc.grade
    local comp = syncPlayer and syncPlayer.comp or syncNpc.comp
    local canAttack = syncPlayer and true or syncNpc.canattk == 1
    local pkMode = syncPlayer and syncPlayer.pkinfo or ""
    local weapon, fashion, shenyiModel, sex, mountModel = 0
    local vip = syncPlayer and syncPlayer.vip or 0
    local weddingshapestate = syncPlayer and syncPlayer.weddingshapestate or 0
    
    obj.IsWeddingCar = iswedding
    if npcType > 0 then
        local clubname, clubpost = ""
        local title = 0
        local name, matename, enemyname = ""
        local keys = string.split(syncNpc.extend, ',')
        for i = 1, #keys do
            local values = string.split(keys[i], '=')
            if values[1] == "shape" then
                shape = tonumber(values[2])
            elseif values[1] == "weapon" then
                weapon = tonumber(values[2])
            elseif values[1] == "setno" then
                fashion = tonumber(values[2])
            elseif values[1] == "shenyi_model" then
                shenyiModel = tonumber(values[2])
            elseif values[1] == "sex" then
                sex = tonumber(values[2])
            elseif values[1] == "mount_model" then
                mountModel = tonumber(values[2])
            elseif values[1] == "name" then
                name = values[2]
            elseif values[1] == "clubname" then
                clubname = values[2]
            elseif values[1] == "clubpost" then
                clubpost = values[2]
            elseif values[1] == "title" then
                title = tonumber(values[2])
            elseif values[1] == "matename" then
                matename = values[2]
            elseif values[1] == "enemyname" then
                enemyname = values[2]
            end
        end
        
        obj:SetData(nmsg.rid, nmsg.char_no, nmsg.fid, sex, 0, weapon or 0, mountModel or 0, 0, 0, 0, 0, 0, shenyiModel or 0, 0, syncNpc.score, 0, 1)
        obj:SetKeyValue("name", name)
        obj:SetKeyValue("clubname", clubname)
        obj:SetKeyValue("clubpost", clubpost)
        obj:SetKeyValue("vip", vip)
        obj:SetKeyValue("title", title)
        obj:SetKeyValue("matename", matename)
        obj:SetKeyValue("enemyname", enemyname)
        obj:SetKeyValue("sex", sex)
        obj:SetKeyValue("weddingshapestate", weddingshapestate)
        local titleSpr, title_effect = COMMONCTRL.GetTitleInfo(title)
        if titleSpr ~= nil then 
            obj:SetKeyValue("titleSpr", titleSpr)
        end

       	if title_effect ~= nil then
        	obj:SetKeyValue("title_effect", title_effect)
        end
        if fashion > 0 then shape = fashion end
        if npcType == 1 then canAttack = false end
    else
    	PLAYERLOADER.SetXlsInfo(obj, entityType, nmsg.char_no, syncPlayer and syncPlayer.isyunbiao or 0)
        if syncPlayer ~= nil then
            obj:SetData(nmsg.rid, nmsg.char_no, nmsg.fid, syncPlayer.sex, syncPlayer.speed, syncPlayer.weapon,
                syncPlayer.mount_model, syncPlayer.partnerhorse_model, syncPlayer.lingqin_model,
                syncPlayer.lingyi_model, syncPlayer.pet_model, syncPlayer.shenjian_model, syncPlayer.shenyi_model,
                syncPlayer.jingmai_model, syncPlayer.score, syncPlayer.up_mount, syncPlayer.up_horse, syncPlayer.fashion,
                syncPlayer.dazuo, syncPlayer.shield_hp or 0, syncPlayer.shield_hpmax or 0, syncPlayer.isyunbiao, syncPlayer.showShenyiState, syncPlayer.showShenjianState,
                syncPlayer.showThugState, syncPlayer.showLingqiState, syncPlayer.showLingqinState, syncPlayer.showThugHorseState,
                syncPlayer.showLingyiState, syncPlayer.showPetState);
            obj:SetKeyValue("clubname", syncPlayer.clubname);
            obj:SetKeyValue("clubpost", syncPlayer.clubpost);
            obj:SetKeyValue("vip", syncPlayer.vip);
            obj:SetKeyValue("title", syncPlayer.title);
            obj:SetKeyValue("titleSpr", syncPlayer.titleSpr);
            obj:SetKeyValue("title_effect", syncPlayer.title_effect);
            obj:SetKeyValue("photo", syncPlayer.photo);
            obj:SetKeyValue("matename", syncPlayer.matename);
            obj:SetKeyValue("enemyname", syncPlayer.enemyname);
            obj:SetKeyValue("sex", syncPlayer.sex);
            obj:SetKeyValue("weddingshapestate", syncPlayer.weddingshapestate);
            obj.isShowShuangxiuEffect = syncPlayer.dx_effect == 1;
            obj.doubleXiulianDir360 = syncPlayer.dx_dir360;
            if syncPlayer.double_xiulian == 1 then
                obj.daZuo = 2
            end
        else
            obj:SetData(nmsg.rid, nmsg.char_no, nmsg.fid, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, syncNpc.score, 0, 0, 0, 0, syncNpc.shield_hp, syncNpc.shield_hpmax, 0, 1, 1,1, 1,1,1,1,1, syncNpc.endline)
            if obj.IsWeddingCar then
                local keys = string.split(syncNpc.extend, ',')
                local title, bvip, ftitle, fvip = 0
                local name, mate, club, job, fmate, fclub, fjob, fname = ""
                for i = 1, #keys do
                    local values = string.split(keys[i], '=')
                    if (values[1] == "name") then
                        name = values[2]
                    elseif values[1] == "title" then
                        title = tonumber(values[2])
                    elseif values[1] == "mate" then
                        mate = values[2]
                    elseif values[1] == "club" then
                        club = values[2]
                    elseif values[1] == "job" then
                        job = values[2]
                    elseif values[1] == "vip" then
                        bvip = tonumber(values[2])
                    elseif values[1] == "ftitle" then
                        ftitle = tonumber(values[2])
                    elseif values[1] == "fmate" then
                        fmate = values[2]
                    elseif values[1] == "fclub" then
                        fclub = values[2]
                    elseif values[1] == "fjob" then
                        fjob = values[2]
                    elseif values[1] == "fname" then
                        fname = values[2]
                    elseif values[1] == "fvip" then
                        fvip = tonumber(values[2])
                    end
                end
                local s_job = ""
                local s_fjob = ""
                if club ~= nil then
                    s_job = COMMONCTRL.GetPostNameByPost(job)
                end
                if fclub ~= nil then
                    s_fjob = COMMONCTRL.GetPostNameByPost(fjob)
                end
                -- print("-----------", title, name, mate, bvip , ftitle ,fname,fmate , fvip)
                obj:SetKeyValue("title", title);
                obj:SetKeyValue("name", name);
                obj:SetKeyValue("mate", mate);
                obj:SetKeyValue("club", club);
                obj:SetKeyValue("job", s_job);
                obj:SetKeyValue("vip", bvip);
                obj:SetKeyValue("ftitle", ftitle);
                obj:SetKeyValue("fmate", fmate);
                obj:SetKeyValue("fclub", fclub);
                obj:SetKeyValue("fjob", s_fjob);
                obj:SetKeyValue("fname", fname);
                obj:SetKeyValue("fvip", fvip);
                local mTitleSpr, mTitle_effect = COMMONCTRL.GetTitleInfo(title)
                if mTitleSpr ~= nil then
                    obj:SetKeyValue("titleSpr", mTitleSpr)
                end
                obj:SetKeyValue("title_effect", mTitle_effect)

                local oTitleSpr, oTitle_effect = COMMONCTRL.GetTitleInfo(ftitle)
                if oTitleSpr ~= nil then
                    obj:SetKeyValue("ftitleSpr", oTitleSpr)
                end
                obj:SetKeyValue("ftitle_effect", oTitle_effect)
            else
                if syncNpc.extend ~= nil then
                    local keys = string.split(syncNpc.extend, ',')
                    local values = string.split(keys[1], '=');
                    if #values == 2 and values[1] =="clubName" then
                        obj:SetKeyValue("clubname", values[2])
                    end
                end
            end

            --守卫npc 特殊处理
            if syncNpc.clublist ~= nil then
                if FIGHTMGR.NpcCreateClubListDeal(syncNpc.clublist) then
                    comp = roleMgr.mainRole.comp
                    obj:SetKeyValue("title", 0)
                    obj:SetKeyValue("titleSpr", "")
                    obj:SetKeyValue("title_effect", "")
                end
            end
        end
    end
    obj.npcType = npcType;
    return shape, grade, comp, canAttack, pkMode, weapon, shenyiModel, mountModel
end

return EntityCreate