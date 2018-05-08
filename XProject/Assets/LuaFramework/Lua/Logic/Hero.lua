local Hero = {}
local this = Hero


function Hero.initPlayerInfo( pb)
	this.Id = pb.id;
    this.Uid = pb.uid;
    this.Name = pb.name;
    this.Sex = pb.sex;
    this.Grade = pb.grade;
    this.CreateTime = pb.create_time
    this.IsDownAward = pb.isdownreward      --是否领取下载奖励  0 未领取 1已领取
    this.ServerDay = pb.engine_day          --服务器开服天数
    User_Config.LoadCharactorConfig(this.Uid);
    this.HostId = pb.host_id                -- 区服编号
    this.merge_time = pb.merge_time

    --载入聊天设置
    -- CHATLOGIC.LoadChatSettings()
    -- 保存等级
    if User_Config.internal_sdk == 0 then
       local serverAccess = User_Config.GetString("DevTemp" , "serverAccess")
       local startIndex , endIndx = string.find(serverAccess, "s=" .. User_Config.default_server)
       if startIndex then
           local nextStartIndex = string.find(serverAccess , "|" , endIndx) or (string.len(serverAccess) + 1)
           local curServerAccess = string.sub(serverAccess , startIndex - 1, nextStartIndex - 1)
           serverAccess = string.gsub(serverAccess , curServerAccess , "")
       end
       serverAccess = serverAccess .. "|s=" .. User_Config.default_server .. "=" .. pb.grade
       User_Config.SetString("DevTemp" , "serverAccess", serverAccess)
       User_Config.Save()
    end

    --请求主角道具列表
    -- local ItemLogic = require("Logic/ItemLogic")
    -- ItemLogic.SendToGetAllItemsOnline()
end

--创建时间
this.CreateTime = 0
--是否领取下载奖励  0 未领取 1已领取
this.IsDownAward = false
---
-- 玩家Runtime ID
-- @field [parent=#model.HeroAttr] #string Id
--
this.Id = nil

---
-- 姓名
-- @field [parent=#model.HeroAttr] #string Name
--
this.Name = nil

---
-- 性别
-- @field [parent=#model.HeroAttr] #string Sex
--
this.Sex = nil

---
-- 等级
-- @field [parent=#model.HeroAttr] #string Grade
--
this.Grade = nil

---
-- 显示经验
-- @field [parent=#model.HeroAttr] #string ShowExp
--
this.ShowExp = 0

---
-- 显示最大经验
-- @field [parent=#model.HeroAttr] #string MaxExp
--
this.MaxExp = nil

---
-- 银两
-- @field [parent=#model.HeroAttr] #string Cash
--
this.Cash = nil

---
-- 元宝
-- @field [parent=#model.HeroAttr] #string YuanBao
--
this.YuanBao = 0

---
-- 绑定元宝
-- @field [parent=#model.HeroAttr] #string BindYuanBao
--
this.BindYuanBao = 0

---
-- 体力
-- @field [parent=#model.HeroAttr] #string Physical
--
this.Physical = nil

---
-- 最大体力
-- @field [parent=#model.HeroAttr] #string PhysicalMax
--
this.PhysicalMax = nil

---
-- 精力
-- @field [parent=#model.HeroAttr] #string Vigor
--
this.Vigor = nil

---
-- 最大精力
-- @field [parent=#model.HeroAttr] #string VigorMax
--
this.VigorMax = nil

this.ShengWang = 0            --声望
this.JJCHonor = 0             --荣誉
this.GongXun = 0              --功勋
this.LiLian = 0               --历练
this.BossScore = 0            --boss积分

this.Shape = nil
this.Fashion = nil

this.Photo = nil

this.Ap = nil
this.Hp = nil
this.Ma = nil
this.Dp = nil
this.Mr = nil
this.HpMax = nil

this.Speed = nil
this.Double = nil
this.Tenacity = nil
this.Parry = nil
this.Reparry = nil
this.HitRate = nil
this.Dodge = nil
this.Score = nil
this.PkMode = nil

this.DoubleHurt = nil
this.ReDoubleHurt = nil
this.Hurt = nil
this.ReHurt = nil
this.AbsHurt = nil

this.LingqiModel = nil      --养成之灵器外形
this.ThugModel = nil        --养成之伙伴外形
this.MountModel = nil       --坐骑外形
this.ThugHorseModel = nil   --养成之灵骑外形
this.LingqinModel = nil     --养成之灵琴外形
this.LingyiModel = nil      --养成之灵翼外形
this.PetModel = nil			--养成之宠物外形
this.ShenjianModel = nil	--养成之神剑外形
this.ShenyiModel = nil		--养成之神翼外形
this.JingmaiModel = nil		--养成之经脉外形
this.ActivateWeapon = nil	--激活的武器外形

this.LingqiModelState = 1     --显示/隐藏灵器外形
this.LingqinModelState = 1    --显示/隐藏灵琴外形
this.LingyiModelState = 1     --显示/隐藏灵翼外形
this.ThugModelState = 1       --显示/隐藏伙伴外形
this.ThugHorseModelState = 1  --显示/隐藏灵骑外形
this.PetModelState = 1        --显示/隐藏宠物外形
this.ShenjianModelState = 1   --显示/隐藏神剑外形
this.ShenyiModelState = 1     --显示/隐藏神翼外形


this.JewelSet = nil         --宝石套装是否已激活
this.MeltingGrade = nil     --锻造熔炼等级
this.MeltingVal = nil       --当前锻造熔炼值
this.MaxGrade = 0

this.IsYunBiao = 0        --护送的神使的品阶

this.SevenState = nil         --控制七天入口开关

--跳闪值
this.FlyDodge = nil

this.Vip = 0

this.ClubId = nil 		--帮派id
this.ClubDonate = nil 	--帮贡
this.ClubPost = nil     --职位
this.ClubName = nil     --帮名

this.EvilValue = nil  --善恶值
this.Charm = nil   --魅力
this.Marriage = nil --姻缘

this.UpMountModel = nil --主角上下马状态
this.UpHorseModel = nil --伙伴上下马状态
this.Title = nil --称号

this.ChangeMolelBuffId = nil        --变身buff编号
this.MateName = nil    --伴侣名称
this.WeddingShapeState = nil   --婚礼游行是否隐藏自身模型 
this.MarriageRing = nil  --姻缘戒等级
this.MarriageStep = nil   --伴侣姻缘最低等级

this.Fid = 0

function Hero.SetData(obj, nmsg)
	-- body
	if not obj then return end
    this.Fid = nmsg.fid
	obj:SetData(this.Id, 0, this.Fid, this.Sex or 0, this.Speed or 0, this.ActivateWeapon or 0,
		this.MountModel or 0, this.ThugHorseModel or 0, this.LingqinModel or 0, this.LingyiModel or 0, this.PetModel or 0,
        this.ShenjianModel or 0, this.ShenyiModel or 0, this.JingmaiModel or 0, this.Score or 0,
        this.UpMountModel or 0, this.UpHorseModel or 0, this.Fashion or 0,
        this.DaZuo or 0, 0, 0, this.IsYunBiao or 0, this.ShenyiModelState, this.ShenjianModelState, this.ThugModelState,
        this.LingqiModelState, this.LingqinModelState, this.ThugHorseModelState, this.LingyiModelState, this.PetModelState)
end

-----------------------------------------------------
--客户端用主角信息
this.midPointWalk = false

function Hero.SetMidPointWalk(mid)
    this.midPointWalk = mid
end

--是否在安全区域 1安全场景不可敌对 2安全场景可敌对 3安全区域 4杀戮区域
this.inSafeArea = 0

--上传用户数据
function Hero.SubmitExtraData(dataType)
    local csm = CenterServerManager.Instance
    local data = ExtraGameData()
    data.dataType = dataType
    data.roleID = this.Uid
    data.roleName = this.Name
    data.roleLevel = this.Grade
    data.serverID = csm.Sid
    data.serverName = csm.ServerName
    data.moneyNum = this.YuanBao
    data.vipLv = this.Vip
    data.unionName = this.ClubName == nil and "无" or this.ClubName
    data.createTime = this.CreateTime
    local sdk = SDKInterface.Instance:SubmitGameData(data)
end

function Hero.Clear()
    this.CreateTime = 0
    this.Id = nil
    this.Name = nil
    this.Sex = nil
    this.Grade = nil
    this.ShowExp = 0
    this.MaxExp = nil
    this.Cash = nil
    this.YuanBao = 0
    this.BindYuanBao = 0
    this.Physical = nil
    this.PhysicalMax = nil
    this.Vigor = nil

    this.VigorMax = nil
    this.ShengWang = 0            --声望
    this.JJCHonor = 0             --荣誉
    this.GongXun = 0              --功勋
    this.LiLian = 0               --历练
    this.BossScore = 0            --boss积分

    this.Shape = nil
    this.Fashion = nil

    this.Photo = nil

    this.Ap = nil
    this.Hp = nil
    this.Ma = nil
    this.Dp = nil
    this.Mr = nil
    this.HpMax = nil

    this.Speed = nil
    this.Double = nil
    this.Tenacity = nil
    this.Parry = nil
    this.Reparry = nil
    this.HitRate = nil
    this.Dodge = nil
    this.Score = nil
    this.PkMode = nil

    this.DoubleHurt = nil
    this.ReDoubleHurt = nil
    this.Hurt = nil
    this.ReHurt = nil
    this.AbsHurt = nil

    this.LingqiModel = nil      --养成之灵器外形
    this.ThugModel = nil        --养成之伙伴外形
    this.MountModel = nil       --坐骑外形
    this.ThugHorseModel = nil   --养成之灵骑外形
    this.LingqinModel = nil     --养成之灵琴外形
    this.LingyiModel = nil      --养成之灵翼外形
    this.PetModel = nil         --养成之宠物外形
    this.ShenjianModel = nil    --养成之神剑外形
    this.ShenyiModel = nil      --养成之神翼外形
    this.JingmaiModel = nil     --养成之经脉外形
    this.ActivateWeapon = nil   --激活的武器外形
    this.JewelSet = nil         --宝石套装是否已激活
    this.MeltingGrade = nil     --锻造熔炼等级
    this.MeltingVal = nil       --当前锻造熔炼值
    this.MaxGrade = 0

    this.IsYunBiao = 0        --护送的神使的品阶

    this.SevenState = nil         --控制七天入口开关

    --跳闪值
    this.FlyDodge = nil

    this.Vip = 0

    this.ClubId = nil       --帮派id
    this.ClubDonate = nil   --帮贡
    this.ClubName = nil     --帮名

    this.EvilValue = nil  --善恶值
    this.Charm = nil   --魅力
    this.Marriage = nil --姻缘

    this.UpMountModel = nil --主角上下马状态
    this.UpHorseModel = nil --伙伴上下马状态
    this.title = nil --称号
    this.matename = nil  --伴侣名字
    this.WeddingShapeState = nil
    this.MarriageRing = nil  --姻缘戒等级
    this.MarriageStep = nil   --伴侣姻缘最低等级

    this.ChangeMolelBuffId = nil
end

--获得当前的模型ID
function Hero.getModelShape()
    if this.Fashion and this.Fashion > 0 then   return this.Fashion    end

    return this.Shape
end

function Hero.UpdateHeroAttrs(attrs)
    -- body
end

return Hero