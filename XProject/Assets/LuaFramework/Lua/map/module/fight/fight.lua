local string=string
local table=table
local debug=debug
local pairs=pairs
local ipairs=ipairs
local tostring=tostring
local tonumber=tonumber
local math=math
local assert=assert
local error=error
local tinsert=table.insert
local tremove=table.remove
local mfloor=math.floor
local mrandom=math.random
local mceil=math.ceil
local mabs = math.abs
local SINGLE_TARGET_TYPE=SINGLE_TARGET_TYPE
local MULTI_TARGET_TYPE=MULTI_TARGET_TYPE
local SKILL_SHAPE_LINE=SKILL_SHAPE_LINE
local SKILL_SHAPE_CIRCLE=SKILL_SHAPE_CIRCLE
local SKILL_SHAPE_SECTOR=SKILL_SHAPE_SECTOR
local SKILL_ATTAREA_MAX=SKILL_ATTAREA_MAX
local MAP_LAYER_CHAR=MAP_LAYER_CHAR
local MOVE_DIR=MOVE_DIR
local FIGHT_EFF_NAME=FIGHT_EFF_NAME
local COST_SP_TYPE=COST_SP_TYPE
local NOT_SEE_OTHER=NOT_SEE_OTHER
local laoi=laoi
local lua_time_sec=lua_time_sec
local pbc_send_msg=pbc_send_msg

local IsKuaFuServer = cfgData and cfgData.IsKuaFuServer

local CMD_DO 			= CMD_DO
local CMD_DO_HIT		= CMD_DO_HIT
local PASS_HP			= PASS_HP		--当自身hp低于，（注意:敌方的不判断）
local PASS_BATTLE		= PASS_BATTLE	--当进入战斗模式
local PASS_DO			= PASS_DO		--指令执行阶段
local PASS_SDO			= PASS_SDO		--技能指令执行阶段
local PASS_HIT			= PASS_HIT		--命中敌人时
local PASS_SHIT			= PASS_SHIT		--技能命中敌人时(前一个包含普通和合体技能)
local PASS_BEHIT		= PASS_BEHIT	--受击时

local SKILL_TYPE_INITIATIVE = SKILL_TYPE_INITIATIVE		--主动技能
local SKILL_TYPE_PASSIVE = SKILL_TYPE_PASSIVE			--被动技能

--伤害类型
local HURT_TYPE_00 	= HURT_TYPE_00 		--普通伤害
local HURT_TYPE_01	= HURT_TYPE_01		--暴击伤害
local HURT_TYPE_02	= HURT_TYPE_02		--破格伤害
local HURT_TYPE_03	= HURT_TYPE_03		--暴击破格伤害
local HURT_TYPE_04	= HURT_TYPE_04		--miss
local HURT_TYPE_05	= HURT_TYPE_05		--无敌
local HURT_TYPE_12	= HURT_TYPE_12		--隐身无敌

local FIGHT_PHYSIC_ATTACK	= FIGHT_PHYSIC_ATTACK
local FIGHT_MAGIC_ATTACK	= FIGHT_MAGIC_ATTACK

local EVENT_BEATTACK = EVENT_BEATTACK

local RANDOM_MAX = 10000
local NormalHit = 0
local NormalDodge = 0

SKILL_TIPS_ID = 0

local function GetSkillTipsId()
	SKILL_TIPS_ID = SKILL_TIPS_ID + 1
	return SKILL_TIPS_ID
end

function AddSkillTips(AttObj, TarObj, skillId, tx, ty)
	local tipsId = GetSkillTipsId()
	local skillData = SKILL_DATA.GetMartialSkill(skillId)
	if not skillData then return end
	local oneData = AttObj:GetOneSkillData(skillId)
	local areaCenter = skillData.AttAreaCenter
	local area = skillData.ClientAttArea
	if not area or not areaCenter then return end
	local martialLevel = AttObj:GetMartialLevelBySkillId(skillId) or 1
	area = area(martialLevel)
	
	local x, y = nil, nil
	if areaCenter == 1 then
		x, y = AttObj:GetX(), AttObj:GetY()
	else
		if TarObj then
			x, y = TarObj:GetX(), TarObj:GetY()
		else
			x, y = tx, ty
		end
	end
	
	local vfds = CHAR_MGR.GetAllPlayerVfds(AttObj:GetMapLayer())
	
	local circle = {}
	local line = {}
	local sector = {}
	local protoMsg = {
		tips_id = tipsId,
		circle = circle,
		line = line,
		sector = sector,
	}
	
	for _, _oneAreaInfo in ipairs(area) do
		local shape = _oneAreaInfo.shape
		if shape == SKILL_SHAPE_CIRCLE then
			local r = _oneAreaInfo.r
			r = r > SKILL_ATTAREA_MAX and SKILL_ATTAREA_MAX or r
			if not x or not y then return end
			tinsert(circle, {
				r = r,
				x = x,
				y = y,
			})
		elseif shape == SKILL_SHAPE_LINE then
			local len = _oneAreaInfo.len
			local len = len > SKILL_ATTAREA_MAX and SKILL_ATTAREA_MAX or len
			local accuracy = _oneAreaInfo.accuracy or 0.2
			local degreeTbl = {_oneAreaInfo.degree}
			assert(len and degreeTbl, string.format("not enought ele! att:%s, skillId:%s", AttObj:GetName(), skillId))
			local ax, ay = AttObj:GetX(), AttObj:GetY()	
			local dx, dy = TarObj and TarObj:GetX() or tx, TarObj and TarObj:GetY() or ty
			if dx == ax and dy == ay then
				dx, dy = ax + 1, ay + 1
			end
			tinsert(line, {
				ax = ax,
				ay = ay,
				tx = dx,
				ty = dy,
				atype = areaCenter,
				len = len,
				accu = accuracy,
				degree = degreeTbl,
			})	
		elseif shape == SKILL_SHAPE_SECTOR then	
			local len = _oneAreaInfo.r	
			len = len > SKILL_ATTAREA_MAX and SKILL_ATTAREA_MAX or len
			local ax, ay = AttObj:GetX(), AttObj:GetY()	
			local dx, dy = TarObj and TarObj:GetX() or tx, TarObj and TarObj:GetY() or ty
			if dx == ax and dy == ay then
				dx, dy = ax + 1, ay + 1
			end	
			tinsert(sector, {
				ax = ax,
				ay = ay,
				tx = dx,
				ty = dy,
				atype = areaCenter,
				len = len,
			})						
		end	
	end
	if oneData then
		oneData.TipsId = tipsId
	end
	if #vfds > 0 then
		pbc_send_msg(vfds, "S2c_aoi_skill_tips", protoMsg)
	end
	return tipsId
end

function DelSkillTips(AttObj, skillId)
	local oneData = AttObj:GetOneSkillData(skillId)
	if not oneData then return end	
	local tipsId = oneData.TipsId
	if not tipsId then return end
	oneData.TipsId = nil
	
	local vfds = CHAR_MGR.GetAllPlayerVfds(AttObj:GetMapLayer())
	if #vfds > 0 then
		pbc_send_msg(vfds, "S2c_aoi_skill_deltips", {tips_id=tipsId})
	end
end

function DelSkillTipsByTipsId(AttObj, tipsId)
	local vfds = CHAR_MGR.GetAllPlayerVfds(AttObj:GetMapLayer())
	if #vfds > 0 then
		pbc_send_msg(vfds, "S2c_aoi_skill_deltips", {tips_id=tipsId})
	end	
end

--获取8个方向
local function Get8Degree(x, y, dx, dy)
	local disX, disY = dx - x, dy - y
	if disX == 0 then
		return 90
	end
	if disY == 0 then
		return 0
	end
	if disX == disY then
		return 45
	elseif -disX == disY then
		return 135
	end
end
local function GetSectorDegree(x, y, dx, dy)
	local disX, disY = dx - x, dy - y
	local degree = Get8Degree(x, y, dx, dy)
	if not degree then
		degree = math.atan(disY/disX) * 180 / math.pi
		if degree < 0 then
			degree = degree + 180
		end
	end
	
	local upDegree = nil
	if disY == 0 then
		if disX > 0 then
			upDegree = 0
		else
			upDegree = 180
		end
	elseif disY > 0 then
		if disX > 0 then
			upDegree = degree
		elseif disX == 0 then
			upDegree = 90
		else
			upDegree = degree
		end
	else
		if disX > 0 then
			upDegree = degree + 180
		elseif disX == 0 then
			upDegree = 270
		else
			upDegree = degree + 180
		end		
	end
	return upDegree
end
--不能x==dx and y==dy
function GetSectorFunc(oDegreeLeft, oDegreeRight, x, y, dx, dy, mx, my, radius)
	local disX, disY = dx - x, dy - y
	assert(oDegreeLeft > oDegreeRight, "not oDegreeLeft > oDegreeRight")
	assert(mx and my and radius, "not mx or not my or not radius")
	assert(not (disX == 0 and disY == 0), "error same x, y, dx, dy")
	
	local dradius = radius ^ 2
	local nDegree = GetSectorDegree(x, y, dx, dy)
	local addDeg = 90 - nDegree
	return function(tx, ty)
		if tx == mx and ty == my then return true end
		if (tx - mx) ^ 2 + (ty - my) ^ 2 > dradius then return end		--不在圆内
		local tDegree = GetSectorDegree(mx, my, tx, ty)
		tDegree = (tDegree + addDeg) % 360
		if oDegreeRight <= tDegree and tDegree <= oDegreeLeft then
			return true
		end
	end
end

--求点是否在平面内函数和相切四边形顶点
function GetIsInFunc(rx, ry, dx, dy, accuracy, len)
	--assert((dx-rx==0 or dy-ry==0), "error GetIsInFunc rx, ry, dx, dy")
	
	--2点直线
--	local y0 = function(x)
--		return (x-rx)/(dx-rx)*(dy-ry)+ry
--	end
	
	local q1,q2,q3,q4 = nil,nil,nil,nil	--简化计算复杂度替代参数
	--转换一般式ax+by+c=0
	local a = ry-dy
	local b = dx-rx
	local c = rx*dy-dx*ry
	local aa = a*a
	local bb = b*b
	local sqrt_aabb = math.sqrt(aa+bb)
	
	--两条平行线ax+by+c+d√(a^2+b^2)=0和ax+by+c-d√(a^2+b^2)=0
--	local y_1 = function(x)	
--		return (a*x+c+accuracy*sqrt_aabb)/(-b)
--	end
--	local y_2 = function(x)
--		return (a*x+c-accuracy*sqrt_aabb)/(-b)
--	end
	
	--引出过(rx,ry)的垂线y=(b/a)x + y1 - (b/a)x1即bx - ay +ay1 - bx1 = 0
	local y_3 = function(x)
		return (b/a)*x + ry - (b/a)*rx
	end
	
	--1 3直线交点
	q1 = accuracy*sqrt_aabb
	--local x1_3 = (b*b*rx-a*(c+accuracy*math.sqrt(a*a+b*b)+b*ry))/(a*a+b*b)
	local x1_3 = (bb*rx-a*(c+q1+b*ry))/(aa+bb)
	local y1_3 = y_3(x1_3)
	--2 3直线交点
	--local x2_3 = (b*b*rx-a*(c-accuracy*math.sqrt(a*a+b*b)+b*ry))/(a*a+b*b)
	local x2_3 = (bb*rx-a*(c-q1+b*ry))/(aa+bb)
	local y2_3 = y_3(x2_3)
	
	local a1 = b
	local b1 = -a
	local c1 = a*ry-b*rx
	local a1a1 = a1*a1
	local b1b1 = b1*b1
	local sqrt_a1a1b1b1 = math.sqrt(a1a1+b1b1)
	
	local y_4 = function(x)
		return (a1*x+c1+len*sqrt_a1a1b1b1)/(-b1)
	end
	
	local v1x,v1y = dx-rx,dy-ry
	local v2x,v2y = v1x,y_4(dx)-ry
	local p = 1
	--判断2向量是否同向做点积 >0同向 <0反向 =0垂直
	if v1x*v2x+v1y*v2y <=0 then
		y_4 = function(x)
			return (a1*x+c1-len*sqrt_a1a1b1b1)/(-b1)
		end
		p=-1
	end
	
	--1 4直线交点
	q2 = c1*b+b*p*len*sqrt_a1a1b1b1
	q3 = b1*q1
	q4 = a*b1-a1*b
	--local x1_4 = (c1*b+b*p*len*math.sqrt(a1a1+b1b1)-c*b1-b1*accuracy*math.sqrt(aa+bb))/(a*b1-a1*b)
	local x1_4 = (q2-c*b1-q3)/(q4)
	local y1_4 = y_4(x1_4)
	--2 4直线交点
	--local x2_4 = (c1*b+b*p*len*math.sqrt(a1a1+b1b1)-c*b1+b1*accuracy*math.sqrt(aa+bb))/(a*b1-a1*b)
	local x2_4 = (q2-c*b1+q3)/(q4)
	local y2_4 = y_4(x2_4)
	
	--点到直线距离d=math.abs((ax0+by0+c)/math.sqrt(a*a+b*b)) 点到正方形两平行边距离判断是否在内
	local IsIn = function(x,y)
		if (x==rx and y==ry) or (x==dx and y==dy) then return true end
		
		local d1 = math.abs((a*x+b*y+c+q1)/sqrt_aabb)
		local d2 = math.abs((a*x+b*y+c-q1)/sqrt_aabb)
		
		local d = math.floor((d1+d2)*100)/100
		
		if d>accuracy*2 then
			return false
		end
		
		local d3 = math.abs((b*x-a*y+a*ry-b*rx)/sqrt_aabb)
		local d4 = math.abs((a1*x+b1*y+c1+p*len*sqrt_a1a1b1b1)/sqrt_a1a1b1b1)
		
		d = math.floor((d3+d4)*100)/100
		
		if d>len then
			return false
		end
		
		return true
	end
	
	--取相切四边形顶点
	local VertexX = {x1_3, x2_3, x1_4, x2_4}
	local VertexY = {y1_3, y2_3, y1_4, y2_4}
	
	local PosList = {}
	PosList.minx = VertexX[1]
	PosList.maxx = VertexX[1]
	PosList.miny = VertexY[1]
	PosList.maxy = VertexY[1]
	for _, x in pairs(VertexX) do
		PosList.minx = PosList.minx>x and x or PosList.minx
		PosList.maxx = PosList.maxx<x and x or PosList.maxx
	end
	
	for _, y in pairs(VertexY) do
		PosList.miny = PosList.miny>y and y or PosList.miny
		PosList.maxy = PosList.maxy<y and y or PosList.maxy
	end
	
	return PosList, IsIn 
end

function CheckGoodAttKind(attKind)
	if attKind == 1 or attKind == 3 or attKind == 4 then
		return true
	end
end

function CheckTarget(AttObj, TarObj, attKind, checkTarObj)
	if checkTarObj then
		if not TarObj then return end
	end
	if not TarObj then return true end
	
	if attKind == 1 then
--		return AttObj:GetComp() == TarObj:GetComp()
		if AttObj == TarObj then 
			return true 
		else
			return AttObj:IsSkillSameComp(TarObj)
		end
	elseif attKind == 2 then
		return AttObj:IsHitTarget(TarObj)
	elseif attKind == 3 then
		return AttObj == TarObj
	elseif attKind == 4 then
--		return (AttObj ~= TarObj) and (AttObj:GetComp() == TarObj:GetComp())
		return (AttObj ~= TarObj) and AttObj:IsSkillSameComp(TarObj)
	end
	error("not attKind:" .. attKind)
end

function NpcCheckTarget(AttObj, TarObj, attKind, attKind2)
	if not TarObj then return true end
	if attKind == 1 then
		return AttObj:GetComp() == TarObj:GetComp()
	elseif attKind == 2 then
		--还需要判断是否主角
		if attKind2 and attKind2 == 5 then
			if TarObj:IsPlayer() then
				return true
			else
				return
			end
		else
			return AttObj:IsHitTarget(TarObj)
		end
	elseif attKind == 3 then
		return AttObj == TarObj
	elseif attKind == 4 then
		return (AttObj ~= TarObj) and (AttObj:GetComp() == TarObj:GetComp())
	end
	error("not attKind:" .. attKind)
end

function GetMainTarget(AttObj, attKind, attKind2, radius)
	if attKind == 1 then
--		if attKind2 then
--			if attKind2 == 1 then
--				local retObj = AttObj:GetMinPropSelfCompCharObj(radius, mFIGHT_Hp)
--				if retObj then
--					return retObj
--				end
--			elseif attKind2 == 2 then
--				local retObj = AttObj:GetMaxPropSelfCompCharObj(radius, mFIGHT_Hp)
--				if retObj then
--					return retObj
--				end
--			elseif attKind2 == 3 then
--				local retObj = AttObj:GetMinRateHpSelfCompCharObj(radius)
--				if retObj then
--					return retObj
--				end
--			elseif attKind2 == 4 then
--				local retObj = AttObj:GetMaxRateHpSelfCompCharObj(radius)
--				if retObj then
--					return retObj
--				end				
--			end
--		end
		return AttObj
	elseif attKind == 2 then
		if attKind2 and attKind2 == 5 then
			local playerObj = CHAR_MGR.GetRandomUserObjByRadius(AttObj:GetMapLayer(), radius, AttObj:GetX(), AttObj:GetY())
			if playerObj then
				return playerObj
			end
		else
			local TarObj = AttObj:SearchOCompCharObj(radius)
			if TarObj then
				return TarObj
			end
		end
	elseif attKind == 3 then
		return AttObj
	elseif attKind == 4 then
		if AttObj:IsMagic() or AttObj:IsPartner() then
			return AttObj:GetOwner()
		elseif AttObj:IsNpc() then
			local TarObj = AttObj:SearchSelfCompCharObj(radius)		--不包含自己
			if TarObj then
				return TarObj
			end		
		elseif AttObj:IsPlayer() then
			return AttObj:GetOnePartner() or AttObj:GetOneMagic()
		end
		
--		if attKind2 then
--			if attKind2 == 1 then
--				local retObj = AttObj:GetMinPropSelfCompCharObj(radius, mFIGHT_Hp, AttObj:GetId())
--				if retObj then
--					return retObj
--				end
--			elseif attKind2 == 2 then
--				local retObj = AttObj:GetMaxPropSelfCompCharObj(radius, mFIGHT_Hp, AttObj:GetId())
--				if retObj then
--					return retObj
--				end
--			elseif attKind2 == 3 then
--				local retObj = AttObj:GetMinRateHpSelfCompCharObj(radius, AttObj:GetId())
--				if retObj then
--					return retObj
--				end
--			elseif attKind2 == 4 then
--				local retObj = AttObj:GetMaxRateHpSelfCompCharObj(radius, AttObj:GetId())
--				if retObj then
--					return retObj
--				end	
--			end
--		else
--			local TarObj = AttObj:SearchSelfCompCharObj(radius)		--不包含自己
--			if TarObj then
--				return TarObj
--			end
--		end
	end
end

local function GetLeftDir(dir)
	if dir >= 7 then
		return (dir + 2) % 8
	else
		return dir + 2
	end
end
local function GetRightDir(dir)
	local dir = dir - 2
	if dir <= 0 then
		return dir + 8
	end
	return dir
end
local function GetBackDir(dir)
	if dir <= 4 then
		return dir + 4
	else
		return (dir + 4) % 8
	end
end

--获取360度个的8方向
local function Get360_8Degree(x, y, dx, dy)
	local disX, disY = dx - x, dy - y
	if disX == 0 then
		if disY > 0 then
			return 90
		else
			return 270
		end
	end
	if disY == 0 then
		if disX > 0 then
			return 0
		else
			return 180
		end
	end
	if disX == disY then
		if disX > 0 then
			return 45
		else
			return 225
		end
	elseif -disX == disY then
		if disX > 0 then
			return 315
		else
			return 135
		end
	end
end
--不能x==dx and y==dy
function GetPushPullPos(x, y, dx, dy, rx, ry, cnt, pushPull)
	local disX, disY = dx - x, dy - y
	assert(pushPull == 1 or pushPull == -1, "error pushPull not 1 or -1")			--1是push推,-1是pull拉
	assert(not (disX == 0 and disY == 0), "error same x, y, dx, dy")
	
	local degree = Get360_8Degree(x, y, dx, dy)
	if degree then
		local xNum, yNum = 0, 0
		if degree == 0 then
			xNum = pushPull
		elseif degree == 45 then
			xNum = pushPull
			yNum = pushPull
		elseif degree == 90 then
			yNum = pushPull
		elseif degree == 135 then
			xNum = -pushPull
			yNum = pushPull			
		elseif degree == 180 then
			xNum = -pushPull
		elseif degree == 225 then
			xNum = -pushPull
			yNum = -pushPull			
		elseif degree == 270 then
			yNum = -pushPull
		elseif degree == 315 then
			xNum = pushPull
			yNum = -pushPull	
		end
		local otx, oty = rx, ry
		for i = 1, cnt do
			local tx, ty = rx + i * xNum, ry + i * yNum
			if not BASECHAR.IsBlockPoint(tx, ty) and BASECHAR.CanMoveBySlope(otx, oty, tx, ty) then --CanMoveBySlope已经判断是否可以跳上去
				otx, oty = tx, ty
			end				
		end
		local rtx, rty = lmapdata.torealpos(MAP_NO, otx, oty)
		return rtx, rty, otx, oty
	else
		local dir = BASECHAR.GetDirByPos(x, y, dx, dy)
		
		local otx, oty = rx, ry
		for i = 1, cnt do
			local tx, ty = rx + i * MOVE_DIR[dir][1], ry + i * MOVE_DIR[dir][2]
			if not BASECHAR.IsBlockPoint(tx, ty) and BASECHAR.CanMoveBySlope(otx, oty, tx, ty) then --CanMoveBySlope已经判断是否可以跳上去
				otx, oty = tx, ty
			end				
		end
		local rtx, rty = lmapdata.torealpos(MAP_NO, otx, oty)
		return rtx, rty, otx, oty
		
--		degree = math.atan(disY/disX) * 180 / math.pi
--		if degree < 0 then
--			degree = degree + 180
--		end		
--		if disY < 0 then		--只需要调整disY<0的情况
--			if disX > 0 then
--				degree = degree + 180
--			else
--				degree = degree + 180
--			end
--		end
--
--		local xNum, yNum = math.cos(math.pi/180*degree) * pushPull, math.sin(math.pi/180*degree) * pushPull
--				
--		local otx, oty = rx, ry
--		local otxreal, otyreal = lmapdata.torealpos(MAP_NO, otx, oty)
--		for i = 1, cnt do
--			local tx, ty = rx + i * xNum, ry + i * yNum
--			local txI, tyI = mfloor(tx + 0.5), mfloor(ty + 0.5)
--			if not BASECHAR.IsBlockPoint(txI, tyI) and BASECHAR.CanMoveBySlope(otx, oty, txI, tyI) then --CanMoveBySlope已经判断是否可以跳上去
--				otx, oty = txI, tyI
--			end	
--		end
--		otxreal, otyreal = lmapdata.torealpos(MAP_NO, otx, oty)
--		return otxreal, otyreal, otx, oty
	end
end

--返回类型
local function IsMiss(AttObj, TarObj, skillData, notMainTar)
	if TarObj:IsPlayer() then
		local flydata = TarObj:GetFlyData()
		if flydata.mtype then		--跳跃中
			local flyDodge = TarObj:GetFlyDodge()
			if flyDodge > 0 then
				TarObj:SubFlyDodge(1)
				return 2
			end
		end
	end

	if AttObj:GetFightValue(mFIGHT_HitOk) then return end			--必定命中
	if AttObj:GetFightValue(mFIGHT_NotHitOk) then return 1 end 		--必定不命中
	local attKind = skillData.AttKind
	if AttKind == 1 or AttKind == 3 or AttKind == 4 then return end	--自己人也必定命中
	
	if TarObj:IsNpc() and TarObj:GetBossType() == BOSS_TYPE_BOX then	--宝箱必中
		return
	end
	
	--初始95/100命中
	local hit, dodge = AttObj:GetFightValue(mFIGHT_HitRate) or 0, TarObj:GetFightValue(mFIGHT_Dodge) or 0
	hit = hit + NormalHit
	dodge = dodge + NormalDodge
	if hit < 0 then hit = 0 end 
	
	if notMainTar then
		if AttObj:IsPlayer() and TarObj:IsPlayer() then
			hit = hit * 0.4
		end
	end
	if dodge < 0 then dodge = 0 end 
	
	if hit >= dodge then return end
	if hit < dodge then
		local odds = (dodge - hit + 500)
		if AttObj:IsPlayer() and TarObj:IsPlayer() then
			odds = odds * 0.5
		end
		
		if odds >= 7000 then
			odds = 7000
		end

		return math.random(RANDOM_MAX) <= odds and 1 or nil
	end
end

--不能修改里面的函数
function GetXYBattleCharObjs(mapLayer, x, y)
	local mapChar = MAP_LAYER_CHAR[mapLayer]
	if not mapChar then return end
	local tXCharTbl = mapChar[x]
	if tXCharTbl then
		return tXCharTbl[y]
	end	
end

local function GetPvpRate(grade)
	if grade <= 60 then
		return 0.3
	elseif grade <= 80 then
		return (grade - 60) * 0.01 + 0.3
	else
		return 0.5
	end
end

local PVP_HURT_RATE = 0.5
local PVP_DHURT_RATE = 0.3

local function CalOne(AttObj, TarObj, skillData, oldTars, tarChars, tarCharEngine, notMainTar)
	local tid = TarObj:GetId()
	if oldTars[tid] then return end
	oldTars[tid] = true
	if TarObj:IsDie() then return end
	local skillType = skillData.AtkType
	local statusType = skillData.Status
	local mType = skillData.Mtype
	local skillId = skillData.ID
	TarObj:SetTmp("LastHitSkillId", skillId)
	if statusType == 100 then
		local missType = IsMiss(AttObj, TarObj, skillData, notMainTar)
		if TarObj:IsPlayer() then
			TarObj:CheckBreakDaZuo()
		end
		if not missType then
			FIGHT_EVENT.ProcessMessage(CMD_DO_HIT, AttObj, TarObj, skillData.ID)
			if not notMainTar then
				if mType == SKILL_MTYPE_MAGIC then
					FIGHT_EVENT.ProcessPassMessage(PASS_SHIT, AttObj, TarObj, skillData.ID)
				else
					FIGHT_EVENT.ProcessPassMessage(PASS_HIT, AttObj, TarObj, skillData.ID)
				end		
				FIGHT_EVENT.ProcessPassMessage(PASS_BEHIT, TarObj, AttObj, skillData.ID)
			end
			
			local hurtHp        = 0
			local hurtType      = HURT_TYPE_00	
			local Lv			= AttObj:GetFightValue(mFIGHT_Grade) or 0
			local Ap, Dp		= nil, nil
			local absHurt 		= AttObj:GetFightValue(mFIGHT_AbsHurt) or 0
			local hurtProp		= AttObj:GetFightValue(mFIGHT_Hurt) or 0
			local reHurtProp	= TarObj:GetFightValue(mFIGHT_ReHurt) or 0
			local aDoubleOtherHurt, tDoubleOtherHurt = 0, 0
			
			if skillType == FIGHT_PHYSIC_ATTACK then		--物理攻击
				Ap = AttObj:GetFightValue(mFIGHT_Ap) or 0
				Dp = TarObj:GetFightValue(mFIGHT_Dp) or 0
			else
				Ap = AttObj:GetFightValue(mFIGHT_Ma) or 0
				Dp = TarObj:GetFightValue(mFIGHT_Mr) or 0
			end
			
			local SkillRate	= AttObj:GetFightValue(mFIGHT_SkillRate) or 1
			if SkillRate <= 0 then SkillRate = 1 end	
			local DobleRate		= AttObj:GetFightValue(mFIGHT_Double) or 0		--暴击
			local TenacityRate	= TarObj:GetFightValue(mFIGHT_Tenacity) or 0	--防暴击
			
			local isPvpCal = false
			if AttObj:IsPlayer() and TarObj:IsPlayer() then
				isPvpCal = true
			end
			
			if notMainTar then
				if isPvpCal then
					Ap = Ap * 0.2
					DobleRate = DobleRate * 0.2
				end
			end
			if DobleRate <= TenacityRate then		--必定不暴击
				DobleRate = 0
			else
				DobleRate = (DobleRate - TenacityRate + 200)
			end

--			1.5.1目标为怪物时
--			当A>D时
--				伤害 = [(A-D)*(技能伤害系数+(己方伤害加成-敌方伤害减免)/10000)+8%*A]*(己方暴击伤害-敌方暴伤减免)/10000*L+固定伤害
--			当A<=D时
--				伤害 = A*5%*(己方暴击伤害-敌方暴伤减免)/10000*L+固定伤害
--		
--			1.5.2目标为玩家时（技能主目标）
--			当A>D时
--				伤害 = [(A-D)*(技能伤害系数+(己方伤害加成-敌方伤害减免)/10000)+8%*A]*(己方暴击伤害-敌方暴伤减免)/10000*L*0.6*(1-最终伤害减免)+固定伤害
--			当A<=D时
--				伤害 = A*5%*(己方暴击伤害-敌方暴伤减免)/10000*L*0.6+固定伤害

--			公式中红色标注的部分，仅为触发暴击时才加入计算，未触发时按1取值。				
--			(技能伤害系数+己方伤害加成-敌方伤害减免)>=20%									 
--			(1-最终伤害减免)>=0%															
--			(己方暴击伤害-敌方暴伤减免)>=125%												

--			灵宠攻击伤害公式：
--					（A*5%+灵宠面板伤害*50%）*（1+灵宠伤害加成）*L + 50
--	
--			法器攻击伤害公式：
--					（A*3.5%+法器面板伤害*50%）*（1+法器伤害加成）*L+400

			local isDouble = false
			
			if isPvpCal then
				DobleRate = DobleRate * 0.6
				if DobleRate > 3300 then
					DobleRate = 3300
				end
			end
			
			if math.random(RANDOM_MAX) <= DobleRate then
				isDouble = true
			end
			if AttObj:GetFightValue(mFIGHT_AbsDouble) then
				isDouble = true
			end
			local L = math.random(95, 105) / 100
			if AttObj:IsPartner() then
				local pAp = AttObj:GetFightValue(mFIGHT_PartnerAp) or 0
				local pHurt = (AttObj:GetFightValue(mFIGHT_PartnerHurt) or 0) / 10000
				local pExtraHurt = AttObj:GetFightValue(mFIGHT_PartnerExtraHurt) or 0
				
				local bHurt = (Ap * 0.05 + pAp * 0.5) * (1 + pHurt) * L
				if isDouble then
					local aDouHurt = AttObj:GetFightValue(mFIGHT_DoubleHurt) or 0
					local tDouDerate = TarObj:GetFightValue(mFIGHT_ReDoubleHurt) or 0
					local douHurtRate = aDouHurt - tDouDerate
					if TarObj:IsPlayer() then
						douHurtRate = douHurtRate * PVP_DHURT_RATE
					end
					if douHurtRate < 12500 then		--暴击伤害(己方暴击伤害-敌方暴伤减免)>=125%
						douHurtRate = 12500
					end
					bHurt = bHurt * douHurtRate / 10000
				end
				local ownerObj = AttObj:GetOwner()
				local grade = 1
				if ownerObj then
					grade = ownerObj:GetGrade() or 1
				end
				local pvpRate = GetPvpRate(grade)
				
				if TarObj:IsPlayer() then
					bHurt = bHurt * pvpRate
				end
				hurtHp = bHurt + 50 + pExtraHurt
				
			elseif AttObj:IsMagic() then
				local mAp = AttObj:GetFightValue(mFIGHT_MagicAp) or 0
				local mHurt = (AttObj:GetFightValue(mFIGHT_MagicHurt) or 0) / 10000
				
				local bHurt = (Ap * 0.035 + mAp * 0.5) * (1 + mHurt) * L
				if isDouble then
					local aDouHurt = AttObj:GetFightValue(mFIGHT_DoubleHurt) or 0
					local tDouDerate = TarObj:GetFightValue(mFIGHT_ReDoubleHurt) or 0
					local douHurtRate = aDouHurt - tDouDerate
					if TarObj:IsPlayer() then
						douHurtRate = douHurtRate * PVP_DHURT_RATE
					end
					if douHurtRate < 12500 then		--暴击伤害(己方暴击伤害-敌方暴伤减免)>=125%
						douHurtRate = 12500
					end
					bHurt = bHurt * douHurtRate / 10000
				end			
				local ownerObj = AttObj:GetOwner()
				local grade = 1
				if ownerObj then
					grade = ownerObj:GetGrade() or 1
				end
				local pvpRate = GetPvpRate(grade)
				
				if TarObj:IsPlayer() then
					bHurt = bHurt * pvpRate
				end				
				hurtHp = bHurt + 400
			else
				if Ap > Dp then	
					--(技能伤害系数+己方伤害加成-敌方伤害减免)>=20%      
					local hurtRate = nil
					if isPvpCal then
						hurtRate = (hurtProp - reHurtProp) / 10000 * PVP_HURT_RATE + SkillRate
					else
						hurtRate = (hurtProp - reHurtProp) / 10000 + SkillRate
					end
					if TarObj:IsPlayer() and IsServer() and TarObj:GetIsYunBiao() then
						if hurtRate < 0 then
							hurtRate = 0
						end
					else
						if hurtRate < 0.2 then
							hurtRate = 0.2
						end
					end
					
					local bHurt = ((Ap - Dp) * hurtRate + 0.08 * Ap) * L	
					if isDouble then
						aDoubleOtherHurt = AttObj:GetFightValue(mFIGHT_DoubleOtherHurt) or 0
						tDoubleOtherHurt = TarObj:GetFightValue(mFIGHT_DoubleOtherHurt) or 0
						
						local aDouHurt = AttObj:GetFightValue(mFIGHT_DoubleHurt) or 0
						local tDouDerate = TarObj:GetFightValue(mFIGHT_ReDoubleHurt) or 0
						local douHurtRate = aDouHurt - tDouDerate
						
						if isPvpCal then
							douHurtRate = (douHurtRate - 12500) * PVP_DHURT_RATE + 12500
						end
						
						if douHurtRate < 12500 then		--暴击伤害(己方暴击伤害-敌方暴伤减免)>=125%
							douHurtRate = 12500
						end
						bHurt = bHurt * douHurtRate / 10000
					end
					
					if isPvpCal then
						local grade = AttObj:GetGrade() or 1
						local pvpRate = GetPvpRate(grade)
						
						bHurt = bHurt * pvpRate 			--todo 添加 *(1-最终伤害减免)      >= 0%
					end
					
					--+固定伤害
					hurtHp = bHurt + absHurt
				else
					local bHurt = Ap * 0.05 * L	
					if isDouble then
						local aDouHurt = AttObj:GetFightValue(mFIGHT_DoubleHurt) or 0
						local tDouDerate = TarObj:GetFightValue(mFIGHT_ReDoubleHurt) or 0
						local douHurtRate = aDouHurt - tDouDerate
						if isPvpCal then
							douHurtRate = douHurtRate * PVP_DHURT_RATE
						end
						
						if douHurtRate < 12500 then		--暴击伤害(己方暴击伤害-敌方暴伤减免)>=125%
							douHurtRate = 12500
						end
						bHurt = bHurt * douHurtRate / 10000
					end
					if isPvpCal then
						local grade = AttObj:GetGrade() or 1
						local pvpRate = GetPvpRate(grade)
						
						bHurt = bHurt * pvpRate
					end
					
					--+固定伤害
					hurtHp = bHurt + absHurt
				end
			end
			
			if isDouble then
				hurtType = HURT_TYPE_01
				hurtHp = hurtHp + aDoubleOtherHurt + tDoubleOtherHurt
			end
			
			if AttObj:IsPartner() then			--受到灵宠攻击时候，伤害减免
				local partnerReHurt = TarObj:GetFightValue(mFIGHT_PartnerReHurt) or 0
				if partnerReHurt > 0 then
					local reHurtRate = 1 - (partnerReHurt / 10000)
					if reHurtRate < 0 then
						reHurtRate = 0
					end
					hurtHp = hurtHp * reHurtRate
				end
			end
			
			local addHurt = AttObj:GetFightValue(mFIGHT_AddOtherHurt) or 0
			local subHurt = AttObj:GetFightValue(mFIGHT_SubOtherHurt) or 0
			addHurt = addHurt + (TarObj:GetFightValue(mFIGHT_AddSelfHurt) or 0)
			subHurt = subHurt + (TarObj:GetFightValue(mFIGHT_SubSelfHurt) or 0)
			
			local addHurtRate = AttObj:GetFightValue(mFIGHT_AddOtherHurtRate) or 0
			local subHurtRate = AttObj:GetFightValue(mFIGHT_SubOtherHurtRate) or 0
			addHurtRate = addHurtRate + (TarObj:GetFightValue(mFIGHT_AddSelfHurtRate) or 0)
			subHurtRate = subHurtRate + (TarObj:GetFightValue(mFIGHT_SubSelfHurtRate) or 0)	
			
			local limitHpHurt = TarObj:GetTmp("LimitHpHurt") or TarObj:GetFightValue(mFIGHT_FixedHpHurt)
			if limitHpHurt then		--npc扣血限制
				local fixedAddHurtRate = 0
				local fixedSubHurtRate = 0
				
				if not TarObj:GetFightValue(mFIGHT_FixedHurtRateAvoid) then
					fixedAddHurtRate = AttObj:GetFightValue(mFIGHT_FixedAddOtherHurtRate) or 0
					fixedSubHurtRate = AttObj:GetFightValue(mFIGHT_FixedSubOtherHurtRate) or 0
					fixedAddHurtRate = fixedAddHurtRate + (TarObj:GetFightValue(mFIGHT_FixedAddSelfHurtRate) or 0)
					fixedSubHurtRate = fixedSubHurtRate + (TarObj:GetFightValue(mFIGHT_FixedSubSelfHurtRate) or 0)	
				end
				
				hurtHp = mfloor(limitHpHurt * (1 + (fixedAddHurtRate - fixedSubHurtRate) / 10000))
			else
				local frameRate = (AttObj:GetFightValue(mFIGHT_FrameRate) or 10000) / 10000
				local oRate = 1
				if AttObj:IsMagic() then
					oRate = 1.275
				elseif AttObj:IsPartner() then
					oRate = 1.7
				end
				hurtHp = mfloor((hurtHp + addHurt - subHurt) * frameRate * oRate * (1 + (addHurtRate - subHurtRate) / 10000))
			end
			if hurtHp < 1 then hurtHp = 1 end
			local shieldHp, fixHurtHp = TarObj:FixedShieldBuff(hurtHp, AttObj)
			if shieldHp <= 0 then
				hurtHp = 0
				hurtType = HURT_TYPE_11
				SyncAllFixedShield(TarObj, fixHurtHp)
			else
				shieldHp = TarObj:ShieldBuff(hurtHp)
			end
			
			if shieldHp > 0 then
				hurtHp = mfloor(hurtHp - shieldHp)
				if hurtHp < 0 then
					hurtHp = 0
				end
			end
			
			--3v3状态下 天地人互克
			if IS_K3V3MAP and IsKuaFuServer then 
				local AttState,TarState = 0,0		
				if AttObj:IsPartner() then 
					local OwnerObj = AttObj:GetOwner()
					if OwnerObj then
						AttState = OwnerObj:GetK3v3State() or 0
					end
				elseif AttObj:IsMagic() then
					local OwnerObj = AttObj:GetOwner()
					if OwnerObj then
						AttState = OwnerObj:GetK3v3State() or 0
					end
				elseif AttObj:IsPlayer() then
					AttState = AttObj:GetK3v3State() or 0								
				end
				if TarObj:IsPartner() then 
					local OwnerObj = TarObj:GetOwner()
					if OwnerObj then
						TarState = OwnerObj:GetK3v3State() or 0
					end
				elseif TarObj:IsMagic() then 
					local OwnerObj = TarObj:GetOwner()
					if OwnerObj then
						TarState = OwnerObj:GetK3v3State() or 0
					end
				elseif TarObj:IsPlayer() then
					TarState = TarObj:GetK3v3State() or 0
				end
				local hurtRate3v3 = K3V3_STATE_CONST[AttState] and K3V3_STATE_CONST[AttState][TarState] or 1
				hurtHp = mfloor(hurtHp*hurtRate3v3)
			end 

			local absorbHpRateByHurt = AttObj:GetFightValue(mFIGHT_AbsorbHpRateByHurt) or 0
			if absorbHpRateByHurt > 0 then											--吸血
				local addHp = hurtHp * (absorbHpRateByHurt / 10000)
				addHp = mfloor(addHp)
				if addHp > 0 then
					AttObj:AddHp(addHp, AttObj:GetId(), nil, HURT_TYPE_00)
				end
			end

			local absorbHpRateByHpMax = AttObj:GetFightValue(mFIGHT_AbsorbHpRateByHpMax) or 0
			if absorbHpRateByHpMax > 0 then											--恢复吸血
				local addHp = AttObj:GetFightValue(mFIGHT_HpMax) * (absorbHpRateByHpMax / 10000)
				addHp = mfloor(addHp)
				if addHp > 0 then
					AttObj:AddHp(addHp, AttObj:GetId(), nil, HURT_TYPE_00)
				end
			end
			
			local backHurtRate = TarObj:GetFightValue(mFIGHT_BackHurtRate) or 0
			if backHurtRate > 0 then												--返还伤害,不让AttObj死亡
				local backHurt = hurtHp * (backHurtRate / 10000)
				backHurt = mfloor(backHurt)
				if backHurt > 0 then
					local tarMapHp = TarObj:GetFightValue(mFIGHT_HpMax)
					if backHurt > tarMapHp then
						backHurt = tarMapHp
					end
					local atHp = AttObj:GetHp() or 0
					if atHp > 0 then
						if backHurt > atHp then
							backHurt = atHp - 1
						end
						if backHurt > 0 then
							AttObj:SubHp(backHurt, TarObj:GetId(), nil, HURT_TYPE_00)
						end
					end
				end
			end

			if TarObj:IsInvisible() then
				hurtHp = 0
				hurtType = HURT_TYPE_12
			elseif TarObj:IsInvincible() then
				hurtHp = 0
				hurtType = HURT_TYPE_05
			else
				TarObj:SubHp(hurtHp, AttObj:GetId(), true)
			end
			
			local sNHp = TarObj:GetHp() or 0
			local sHpstamp = nil 
			if hurtType == HURT_TYPE_11 then
				sNHp = (TarObj:FixedShieldBuffSyncHp() or 0)
				sHpstamp = TarObj:GetHpstamp()
			end
			
			local oneTarChars = {
				tar_fid = TarObj:GetFId(),
				hp = hurtType == HURT_TYPE_11 and fixHurtHp or (0 - hurtHp),	
				nhp = sNHp,	
				hpmax = TarObj:GetFightValue(mFIGHT_HpMax) or 0,
				type = hurtType ~= 0 and hurtType or nil,
				isdie = TarObj:IsDie() and 1 or nil,
				shpstamp = sHpstamp,
			}
			local movePush = TarObj:GetFightValue(mFIGHT_MovePush)			--推
			local movePull = TarObj:GetFightValue(mFIGHT_MovePull)			--拉

			if movePush or movePull then
				local pushPull = 1
				local moveNum = movePush or movePull
				local ax, ay = AttObj:GetX(), AttObj:GetY()
				local tx, ty = TarObj:GetX(), TarObj:GetY()
				if movePull then
					pushPull = -1
				end
				if ax == tx and ay == ty then 
					tx, ty = ax + 1, ay + 1
				end
				local nx, ny, nxI, nyI = GetPushPullPos(ax, ay, tx, ty, tx, ty, moveNum, pushPull)
				if nx and ny and nxI and nyI and (nxI ~= tx or nyI ~= ty) then
					local nz = lmapdata.getz(MAP_NO, nxI, nyI)
					if nz then
						if TarObj:IsNpc() then
							local ox, oy, oz = TarObj:GetX(), TarObj:GetY(), TarObj:GetZ()		
							local retnum, apTbl, dpTbl, mpTbl = laoi.map_moveobj(TarObj:GetMapObj(), TarObj:GetEngineObj(), nxI, nyI)
							if retnum >= 0 then		
								oneTarChars.x = nx
								oneTarChars.y = nz
								oneTarChars.z = ny
								TarObj:SetX(nxI)
								TarObj:SetY(nyI)
								TarObj:SetZ(nz)
								
								TarObj:SendPushPullPos(ox, oy, oz, retnum, apTbl, dpTbl)
								
								TarObj:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
								if TarObj:IsPlayer() and IS_RET_MOVE then
									lretmap.usermove(TarObj:GetVfd(), MAP_ID, TarObj:GetMapLayer(), nxI, nyI, nz)
								end
							end
						end
					end		
				end
			end
			
--			TarObj:AddSp(skillData.AddTarSp)
			FIGHT_EVENT.ClearFightEff(AttObj, CMD_DO_HIT)
			FIGHT_EVENT.ClearFightEff(TarObj, CMD_DO_HIT)	
			FIGHT_EVENT.ClearFightEff(AttObj, PASS_HIT)
			FIGHT_EVENT.ClearFightEff(TarObj, PASS_HIT)	
			FIGHT_EVENT.ClearFightEff(AttObj, PASS_SHIT)
			FIGHT_EVENT.ClearFightEff(TarObj, PASS_SHIT)	
			FIGHT_EVENT.ClearFightEff(AttObj, PASS_BEHIT)
			FIGHT_EVENT.ClearFightEff(TarObj, PASS_BEHIT)
			
			if hurtHp >= 0 and not TarObj:IsDie() then
				local tarAiObj = TarObj:GetAI()					--设置被攻击ai
				if tarAiObj then
					if AttObj:IsPartner() or AttObj:IsMagic() then
						tarAiObj:OnEvent({
							eventType = EVENT_BEATTACK,
							eventAttackCharId = AttObj:GetOwnerId(),
							attX = AttObj:GetX(),
							attY = AttObj:GetY(),
						})
					else
						tarAiObj:OnEvent({
							eventType = EVENT_BEATTACK,
							eventAttackCharId = AttObj:GetId(),
							attX = AttObj:GetX(),
							attY = AttObj:GetY(),
						})
					end
				end
			end
			
			tinsert(tarChars, oneTarChars)	
			
			if TarObj:IsDie() and skillData.IsSubSp ~= COST_SP_TYPE then
				AttObj:AddSp(skillData.AddAttSp or 0)
			end
		else
--			_RUNTIME("---------------------------------------miss")
			local oneTarChars = {
				tar_fid = TarObj:GetFId(),
				hp = 0,	
				nhp = TarObj:GetHp() or 0,
				hpmax = TarObj:GetFightValue(mFIGHT_HpMax) or 0,
				type = missType == 2 and HURT_TYPE_08 or HURT_TYPE_04,				--miss	
				isdie = TarObj:IsDie() and 1 or nil,
			}
			tinsert(tarChars, oneTarChars)	
		end	
	elseif statusType == 101 then						--非攻击类型
		TarObj:SetTmp(FIGHT_SHOW_HP, nil)
		TarObj:SetTmp(FIGHT_SHOW_ADDBUFF, nil)
		FIGHT_EVENT.ProcessMessage(CMD_DO_HIT, AttObj, TarObj, skillData.ID)

		if not notMainTar then
			if mType == SKILL_MTYPE_MAGIC then
				FIGHT_EVENT.ProcessPassMessage(PASS_SHIT, AttObj, TarObj, skillData.ID)
			else
				FIGHT_EVENT.ProcessPassMessage(PASS_HIT, AttObj, TarObj, skillData.ID)
			end	
		end
		
--		TarObj:AddSp(skillData.AddTarSp)

		local oneTarChars = nil
		if TarObj:GetTmp(FIGHT_SHOW_HP) then
			oneTarChars = {
				tar_fid = TarObj:GetFId(),
				hp = 0,							--扣血加血不在这里处理的,所以为0
				nhp = TarObj:GetHp() or 0,
				hpmax = TarObj:GetFightValue(mFIGHT_HpMax) or 0,
				type = HURT_TYPE_06,
				isdie = TarObj:IsDie() and 1 or nil,
			}
			tinsert(tarChars, oneTarChars)	
		elseif TarObj:GetTmp(FIGHT_SHOW_ADDBUFF) then
			oneTarChars = {
				tar_fid = TarObj:GetFId(),
				hp = 0,							--扣血加血不在这里处理的,所以为0
				nhp = TarObj:GetHp() or 0,
				hpmax = TarObj:GetFightValue(mFIGHT_HpMax) or 0,
				type = HURT_TYPE_07,
				isdie = TarObj:IsDie() and 1 or nil,
			}
			tinsert(tarChars, oneTarChars)				
		end
		local movePush = TarObj:GetFightValue(mFIGHT_MovePush)			--推
		local movePull = TarObj:GetFightValue(mFIGHT_MovePull)			--拉

		if movePush or movePull then
			local pushPull = 1
			local moveNum = movePush or movePull
			local ax, ay = AttObj:GetX(), AttObj:GetY()
			local tx, ty = TarObj:GetX(), TarObj:GetY()
			if movePull then
				pushPull = -1
			end
			if ax == tx and ay == ty then 
				tx, ty = ax + 1, ay + 1
			end
			local nx, ny, nxI, nyI = GetPushPullPos(ax, ay, tx, ty, tx, ty, moveNum, pushPull)
			if nx and ny and nxI and nyI and (nxI ~= tx or nyI ~= ty) then
				local nz = lmapdata.getz(MAP_NO, nxI, nyI)
				if nz then
					if TarObj:IsNpc() then
						local ox, oy, oz = TarObj:GetX(), TarObj:GetY(), TarObj:GetZ()	
						local retnum, apTbl, dpTbl, mpTbl = laoi.map_moveobj(TarObj:GetMapObj(), TarObj:GetEngineObj(), nxI, nyI)
						if retnum >= 0 then		
							if not oneTarChars then
								oneTarChars = {
									tar_fid = TarObj:GetFId(),
									hp = 0,							--扣血加血不在这里处理的,所以为0
									nhp = TarObj:GetHp() or 0,
									hpmax = TarObj:GetFightValue(mFIGHT_HpMax) or 0,
									type = HURT_TYPE_00,	
									isdie = TarObj:IsDie() and 1 or nil,						
								}
								tinsert(tarChars, oneTarChars)	
							end
											
							oneTarChars.x = nx
							oneTarChars.y = nz
							oneTarChars.z = ny
							TarObj:SetX(nxI)
							TarObj:SetY(nyI)
							TarObj:SetZ(nz)
							
							TarObj:SendPushPullPos(ox, oy, oz, retnum, apTbl, dpTbl)
							
							TarObj:MoveChangeMapPos(CHANGE_MAPPOS_MOVE, ox, oy, oz)
							if TarObj:IsPlayer() and IS_RET_MOVE then
								lretmap.usermove(TarObj:GetVfd(), MAP_ID, TarObj:GetMapLayer(), nxI, nyI, nz)
							end
						end
					end
				end		
			end
		end
		
		FIGHT_EVENT.ClearFightEff(AttObj, CMD_DO_HIT)
		FIGHT_EVENT.ClearFightEff(TarObj, CMD_DO_HIT)		
		FIGHT_EVENT.ClearFightEff(AttObj, PASS_HIT)
		FIGHT_EVENT.ClearFightEff(TarObj, PASS_HIT)	
		FIGHT_EVENT.ClearFightEff(AttObj, PASS_SHIT)
		FIGHT_EVENT.ClearFightEff(TarObj, PASS_SHIT)	
		
		TarObj:SetTmp(FIGHT_SHOW_HP, nil)
	end
	TarObj:SetTmp("LastHitSkillId", nil)
	if tarCharEngine then			--群攻用
		tinsert(tarCharEngine, TarObj:GetEngineObj())
	end
end

function UseSkillAct(AttObj, TarObj, skillId, mxCenter, myCenter, axyz, timestamp)
	if AttObj:IsDie() then return end
	local skillData = SKILL_DATA.GetMartialSkill(skillId)
	if not skillData then
		error("not skillId:" .. skillId) 
		return 
	end
	if not AttObj:CanUserSkillByBuff(skillId) then return end
	local attKind = skillData.AttKind
	if not CheckTarget(AttObj, TarObj, attKind) then 
		return 
	end
	local tarType = skillData.TargetType
	if tarType == SINGLE_TARGET_TYPE and not TarObj then 
		error(string.format("is SINGLE_TARGET_TYPE but not TarObj! att:%s, skillId:%s", AttObj:GetName(), skillId))
		return 
	end	
	local effTime = skillData.BeforeHurtTime
	local effTimeNo = 0
	if effTime then
		effTimeNo = mfloor(effTime / (lua_time_sec * 1000))
		if skillData.AttRange and skillData.FlySpeed and skillData.FlySpeed > 0 then
			effTimeNo = effTimeNo + mceil(skillData.AttRange / skillData.FlySpeed / lua_time_sec)
		end
	end		
	if skillData.IsSubSp == COST_SP_TYPE then				--主角或者普通攻击的不用怒气的
		AttObj:SubSp(SP_MAX)
	end	
	
	local uSkillData = AttObj:GetOneSkillData(skillId)
	if not uSkillData or not uSkillData.AidSkill then
		AttObj:SetFrontSkillId(skillId)
	end
	
	if effTimeNo <= 0 and (AttObj:IsNpc() or AttObj:IsPartner() or AttObj:IsMagic()) then	
		--直接发攻击,攻击协议中要包含isbegin = 1
		UseSkillHit(AttObj, TarObj, skillId, mxCenter, myCenter, axyz, timestamp, true)
	else
		local postype = 1
		local sendX = AttObj:GetX()
		local sendY = AttObj:GetY()
		local sendZ = AttObj:GetZ()
		
		local protoMsg = {
			skill_id = skillId,
			att_fid = AttObj:GetFId(),
			postype = postype,
			x = sendX,
			y = sendZ,
			z = sendY,
			tar_fid = 0,
			tx = 0,
			ty = 0,
			timestamp = timestamp,
			htype = AttObj:GetHitType(),
		}
		if AttObj:IsPartner() or AttObj:IsMagic() then
			timestamp = mrandom(1, 1000)
			protoMsg.timestamp = timestamp
		end
		if TarObj then
			protoMsg.tar_fid = TarObj:GetFId()
		else
			assert(mxCenter and myCenter, string.format("not mxCenter and myCenter attName:%s, skillId:%s", AttObj:GetName(), skillId))
			protoMsg.tx = mxCenter
			protoMsg.ty = myCenter
		end
		local isOk, playerTbl = laoi.map_region9player(AttObj:GetMapObj(), AttObj:GetEngineObj())
		if AttObj:IsPlayer() then --如果是人
			if not playerTbl then
				playerTbl = {
					AttObj:GetId(),
				}
			else
				tinsert(playerTbl, AttObj:GetId())
			end
		elseif AttObj:IsPartner() or AttObj:IsMagic() then
			local ownerObj = AttObj:GetOwner() 
			if ownerObj then
				if not playerTbl then
					playerTbl = {
						ownerObj:GetId(),
					}
				else
					tinsert(playerTbl, ownerObj:GetId())
				end	
			end		
		end
		--npc,则把技能添加进倒计时触发aoi_skill_hit
		if AttObj:IsNpc() or AttObj:IsPartner() or AttObj:IsMagic() or AttObj:IsMirrorPlayer() then
			local s_rid = FIGHT_EVENT.GetStartSkillRid()
			local cal = {
				att_id = AttObj:GetId(),
				cal_id = 0,
				s_rid = s_rid,
				checktime = GetNowTimeNo() + effTimeNo,
			}
			if AttObj:IsPlayer() then
				cal.cal_id = AttObj:GetId()
			elseif TarObj and TarObj:IsPlayer() then
				cal.cal_id = TarObj:GetId()
			else
				if playerTbl then
					for _, _id in pairs(playerTbl) do
						local _CharObj = CHAR_MGR.GetCharById(_id)
						if _CharObj and _CharObj:IsPlayer() then
							cal.cal_id = _id
							break
						end
					end
				end
			end
			protoMsg.cal = cal
			FIGHT_EVENT.AddStartSkill(AttObj, TarObj, skillId, mxCenter, myCenter, effTimeNo, s_rid, axyz, timestamp)
		end
		if isOk and playerTbl then
			local vfds = {}
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
				end
			end
			if #vfds > 0 then
				pbc_send_msg(vfds, "S2c_aoi_skill_act", protoMsg)
			end				
		end	
	end
	return true
end

local function CanAttackNpcCostSp(tarObj)
	if tarObj:IsPlayer() then
		return false
	elseif tarObj:IsNpc() then
		local spNotHurt = tarObj:GetTmp("SpNotHurt")
		if spNotHurt == 1 then
			return false
		end
	end
	return true
end

function UseSkillHit(AttObj, TarObj, skillId, mxCenter, myCenter, axyz, timestamp, isBegin)
	if AttObj:IsDie() then return end
	local skillData = SKILL_DATA.GetMartialSkill(skillId)
	if not skillData then
		return 
	end
	if not AttObj:CanUserSkillByBuff(skillId) then return end
	local attKind = skillData.AttKind
	if not CheckTarget(AttObj, TarObj, attKind) then 
		return 
	end
	local mType = skillData.Mtype
	local attRange = skillData.AttRange
	local tarType = skillData.TargetType
	local addAttSp = skillData.AddAttSp
	local notPlayerBoss = nil
	if skillData.IsSubSp == COST_SP_TYPE then				--怒气不能攻击玩家和boss
		notPlayerBoss = true
	end
	
	if tarType == SINGLE_TARGET_TYPE and not TarObj then 
		error(string.format("is SINGLE_TARGET_TYPE but not TarObj! att:%s, skillId:%s", AttObj:GetName(), skillId))
		return 
	end
	local tarChars = {}
	local postype = 1
	local sendX = AttObj:GetX()
	local sendY = AttObj:GetY()
	local sendZ = AttObj:GetZ()
--	local skillPosInfo = AttObj:GetSkillPosCheck()
--	if skillPosInfo and skillPosInfo.skillId == skillId then
--		if sendX == skillPosInfo.gx and sendY == skillPosInfo.gy then
--			postype = 2
--			sendX = skillPosInfo.x
--			sendY = skillPosInfo.y
--		end
--	end
	
	local beHitMax = skillData.BeHitMax
	if beHitMax and beHitMax <= 0 then beHitMax = nil end
	
	local protoMsg = {
		skill_id = skillId,
		att_fid = AttObj:GetFId(),
		postype = postype,
		x = sendX,
		y = sendZ,
		z = sendY,
		tar_chars = tarChars,
		tx = AttObj:GetX(),
		ty = AttObj:GetY(),
		axyz = axyz, 
		timestamp = timestamp,
		isbegin = isBegin and 1 or 0,
		htype = AttObj:GetHitType(),
	}
	
	if not protoMsg.axyz then
		if TarObj then
			local ax, az, ay = lmapdata.torealpos(MAP_NO, TarObj:GetX(), TarObj:GetY())
			protoMsg.axyz = string.format("%s,%s,%s", ax, az, ay)
		end
	end
	local normalTarRx, normalTarRy = nil, nil
	local oldTars = {}
	if tarType == SINGLE_TARGET_TYPE then		--单体攻击
		if notPlayerBoss then
			if not CanAttackNpcCostSp(TarObj) then
--				error(string.format("attName:%s skillId:%s error target!", AttObj:GetName(), skillId))
				return
			end
		end
		
		AttObj:SetTmp("LastHitSkillId", skillId)
		TarObj:SetTmp("LastHitSkillId", skillId)
		FIGHT_EVENT.ProcessMessage(CMD_DO, AttObj, TarObj, skillId)
		if mType == SKILL_MTYPE_MAGIC then
			FIGHT_EVENT.ProcessPassMessage(PASS_SDO, AttObj, TarObj, skillId)
		else
			FIGHT_EVENT.ProcessPassMessage(PASS_DO, AttObj, TarObj, skillId)
		end			

		if AttObj:CanAttackState(TarObj) and AttObj:SecurityAreaCheckAttack(TarObj) then
			CalOne(AttObj, TarObj, skillData, oldTars, tarChars)
		end
		FIGHT_EVENT.ClearFightEff(AttObj, CMD_DO)
		FIGHT_EVENT.ClearFightEff(TarObj, CMD_DO)
		FIGHT_EVENT.ClearFightEff(AttObj, PASS_DO)
		FIGHT_EVENT.ClearFightEff(TarObj, PASS_DO)
		FIGHT_EVENT.ClearFightEff(AttObj, PASS_SDO)
		FIGHT_EVENT.ClearFightEff(TarObj, PASS_SDO)
		protoMsg.tx, protoMsg.ty = TarObj:GetX(), TarObj:GetY()
		
		normalTarRx, normalTarRy = TarObj:GetX(), TarObj:GetY()
		local isOk, playerTbl = laoi.map_region9player(AttObj:GetMapObj(), TarObj:GetEngineObj())
		if TarObj:IsPlayer() then --如果是人
			if not playerTbl then
				playerTbl = {
					TarObj:GetId(),
				}
			else
				tinsert(playerTbl, TarObj:GetId())
			end
		end 
		if isOk and playerTbl then
			local vfds = {}
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
				end
			end
			if #vfds > 0 then
				pbc_send_msg(vfds, "S2c_aoi_skill_hit", protoMsg)
			end				
		end
		AttObj:SetTmp("LastHitSkillId", nil)
		TarObj:SetTmp("LastHitSkillId", nil)
		
	elseif tarType == MULTI_TARGET_TYPE then	--多体攻击
		local areaCenter = skillData.AttAreaCenter
		assert(areaCenter == 1 or areaCenter == 2, string.format("areaCenter error! att:%s, areaCenter:%s", AttObj:GetName(), areaCenter or "nil"))
		local martialLevel = AttObj:GetMartialLevelBySkillId(skillId) or 1
		local area = skillData.AttArea
		if not area then
			error(string.format("not area! att:%s, skillId:%s", AttObj:GetName(), skillId))
		end		
		area = area(martialLevel)
		if not TarObj then
			assert(mxCenter and myCenter, string.format("not mxCenter and myCenter! att:%s, skillId:%s", AttObj:GetName(), skillId))
			normalTarRx, normalTarRy = mxCenter, myCenter
		else
			normalTarRx, normalTarRy = TarObj:GetX(), TarObj:GetY()
		end
		
		local atX, atY = AttObj:GetX(), AttObj:GetY()
		local atxy2 = (atX - normalTarRx) ^ 2 + (atY - normalTarRy) * 2
		if atxy2 > attRange ^ 2 then			--目标太远，超出范围
			if AttObj:IsPlayer() and TarObj and TarObj:IsPlayer() then
				pbc_send_msg(AttObj:GetVfd(), "S2c_aoi_skillerror", {
					id = TarObj:GetId(),
					is_exist = 1,
					pos = {
						TarObj:GetX(),
						TarObj:GetY(),
					},
					y = TarObj:GetZ(),
				})
			end	
			return
		end
		
--		if areaCenter ~= 1 then		--自己为中心
			protoMsg.tx, protoMsg.ty = normalTarRx, normalTarRy
--		end
		
		local doProcessOk = nil
		if TarObj then
			AttObj:SetTmp("LastHitSkillId", skillId)
			TarObj:SetTmp("LastHitSkillId", skillId)
			
			FIGHT_EVENT.ProcessMessage(CMD_DO, AttObj, TarObj, skillId)
			if mType == SKILL_MTYPE_MAGIC then
				FIGHT_EVENT.ProcessPassMessage(PASS_SDO, AttObj, TarObj, skillId)
			else
				FIGHT_EVENT.ProcessPassMessage(PASS_DO, AttObj, TarObj, skillId)
			end	
			doProcessOk = true
		end
		local isCmdDo = false
		local tarCharEngine = {}
		for _, _oneAreaInfo in ipairs(area) do
			local shape = _oneAreaInfo.shape
			local hitTarList = {}
			if shape == SKILL_SHAPE_CIRCLE then
				local r = _oneAreaInfo.r
				assert(r, string.format("not r! att:%s, skillId:%s", AttObj:GetName(), skillId))
				local tr = r > SKILL_ATTAREA_MAX and SKILL_ATTAREA_MAX or r
				local rx, ry = nil, nil
				if areaCenter == 1 then		--自己为中心
					rx, ry = AttObj:GetX(), AttObj:GetY()	
				else						--敌人为中心
					rx, ry = normalTarRx, normalTarRy
				end
				local dtr = tr ^ 2
				for i = rx - tr, rx + tr do
					for j = ry - tr, ry + tr do
						if not BASECHAR.IsBlockPoint(i, j) then
							if (i - rx) ^ 2 + (j - ry) ^ 2 <= dtr then
								local tCharTbl = GetXYBattleCharObjs(AttObj:GetMapLayer(), i, j)
								if tCharTbl then
									for _id, _CharObj in pairs(tCharTbl) do
										local canAtt = true
										if notPlayerBoss then
											if not CanAttackNpcCostSp(_CharObj) then
												canAtt = false
											end
										end
										
										if canAtt and not _CharObj:IsDie() and CheckTarget(AttObj, _CharObj, attKind) then		--测试是否需要攻击
											if beHitMax and _CharObj ~= TarObj then
												tinsert(hitTarList, _CharObj)
											else
												if beHitMax then
													beHitMax = beHitMax - 1
												end
												if not doProcessOk then
													AttObj:SetTmp("LastHitSkillId", skillId)
													_CharObj:SetTmp("LastHitSkillId", skillId)
													FIGHT_EVENT.ProcessMessage(CMD_DO, AttObj, _CharObj, skillId)
													isCmdDo = true
													if mType == SKILL_MTYPE_MAGIC then
														FIGHT_EVENT.ProcessPassMessage(PASS_SDO, AttObj, _CharObj, skillId)
													else
														FIGHT_EVENT.ProcessPassMessage(PASS_DO, AttObj, _CharObj, skillId)
													end	
													doProcessOk = true
													if not TarObj then
														TarObj = _CharObj
													end												
												end
												if AttObj:CanAttackState(_CharObj) and AttObj:SecurityAreaCheckAttack(_CharObj) then
													CalOne(AttObj, _CharObj, skillData, oldTars, tarChars, tarCharEngine, _CharObj ~= TarObj)
												end
												FIGHT_EVENT.ClearFightEff(_CharObj, CMD_DO)
												FIGHT_EVENT.ClearFightEff(_CharObj, PASS_DO)
												FIGHT_EVENT.ClearFightEff(_CharObj, PASS_SDO)
											end
										end
									end
								end
							end
						end
					end
				end
				if beHitMax and beHitMax > 0 then
					local tHitList = table.random_values(hitTarList, beHitMax)
					for _, _CharObj in pairs(tHitList) do
						if not doProcessOk then
							AttObj:SetTmp("LastHitSkillId", skillId)
							_CharObj:SetTmp("LastHitSkillId", skillId)
							FIGHT_EVENT.ProcessMessage(CMD_DO, AttObj, _CharObj, skillId)
							isCmdDo = true
							if mType == SKILL_MTYPE_MAGIC then
								FIGHT_EVENT.ProcessPassMessage(PASS_SDO, AttObj, _CharObj, skillId)
							else
								FIGHT_EVENT.ProcessPassMessage(PASS_DO, AttObj, _CharObj, skillId)
							end	
							doProcessOk = true	
							if not TarObj then
								TarObj = _CharObj
							end												
						end
						if AttObj:CanAttackState(_CharObj) and AttObj:SecurityAreaCheckAttack(_CharObj) then
							CalOne(AttObj, _CharObj, skillData, oldTars, tarChars, tarCharEngine, _CharObj ~= TarObj)
						end
						FIGHT_EVENT.ClearFightEff(_CharObj, CMD_DO)
						FIGHT_EVENT.ClearFightEff(_CharObj, PASS_DO)
						FIGHT_EVENT.ClearFightEff(_CharObj, PASS_SDO)						
					end
				end
			elseif shape == SKILL_SHAPE_SECTOR then
--				{shape=3,r=10,degreeleft=135,degreeright=45}
				local r = _oneAreaInfo.r
				local dLeft = _oneAreaInfo.degreeleft
				local dRight = _oneAreaInfo.degreeright
				assert(dLeft and dRight and r, string.format("not enought ele! att:%s, skillId:%s", AttObj:GetName(), skillId))
				local tr = r > SKILL_ATTAREA_MAX and SKILL_ATTAREA_MAX or r
				local ax, ay = AttObj:GetX(), AttObj:GetY()	
				local rx, ry = nil, nil
				if areaCenter == 1 then		--自己为中心
					rx, ry = ax, ay
				else						--敌人为中心
					rx, ry = normalTarRx, normalTarRy
				end
				local dx, dy = nil, nil
				if TarObj then
					dx, dy = TarObj:GetX(), TarObj:GetY()
				else
					dx, dy = normalTarRx, normalTarRy
				end
				if not dx or not dy then
					dx, dy = ax + 1, ay + 1
				end
				if dx == ax and dy == ay then
					dx, dy = ax + 1, ay + 1
				end
				local func = GetSectorFunc(dLeft, dRight, ax, ay, dx, dy, rx, ry, tr)
				local dtr = tr ^ 2
				for i = rx - tr, rx + tr do
					for j = ry - tr, ry + tr do
						if not BASECHAR.IsBlockPoint(i, j) then
							if func(i, j) then
								local tCharTbl = GetXYBattleCharObjs(AttObj:GetMapLayer(), i, j)
								if tCharTbl then
									for _id, _CharObj in pairs(tCharTbl) do
										local canAtt = true
										if notPlayerBoss then
											if not CanAttackNpcCostSp(_CharObj) then
												canAtt = false
											end
										end
										
										if canAtt and not _CharObj:IsDie() and CheckTarget(AttObj, _CharObj, attKind) then		--测试是否需要攻击
											if beHitMax and _CharObj ~= TarObj then
												tinsert(hitTarList, _CharObj)
											else
												if beHitMax then
													beHitMax = beHitMax - 1
												end
												if not doProcessOk then
													AttObj:SetTmp("LastHitSkillId", skillId)
													_CharObj:SetTmp("LastHitSkillId", skillId)
													FIGHT_EVENT.ProcessMessage(CMD_DO, AttObj, _CharObj, skillId)
													isCmdDo = true
													if mType == SKILL_MTYPE_MAGIC then
														FIGHT_EVENT.ProcessPassMessage(PASS_SDO, AttObj, _CharObj, skillId)
													else
														FIGHT_EVENT.ProcessPassMessage(PASS_DO, AttObj, _CharObj, skillId)
													end	
													doProcessOk = true	
													if not TarObj then
														TarObj = _CharObj
													end													
												end
												if AttObj:CanAttackState(_CharObj) and AttObj:SecurityAreaCheckAttack(_CharObj) then
													CalOne(AttObj, _CharObj, skillData, oldTars, tarChars, tarCharEngine, _CharObj ~= TarObj)
												end
												FIGHT_EVENT.ClearFightEff(_CharObj, CMD_DO)
												FIGHT_EVENT.ClearFightEff(_CharObj, PASS_DO)
												FIGHT_EVENT.ClearFightEff(_CharObj, PASS_SDO)
											end
										end
									end
								end
							end
						end
					end
				end
				if beHitMax and beHitMax > 0 then
					local tHitList = table.random_values(hitTarList, beHitMax)
					for _, _CharObj in pairs(tHitList) do
						if not doProcessOk then
							AttObj:SetTmp("LastHitSkillId", skillId)
							_CharObj:SetTmp("LastHitSkillId", skillId)
							FIGHT_EVENT.ProcessMessage(CMD_DO, AttObj, _CharObj, skillId)
							isCmdDo = true
							if mType == SKILL_MTYPE_MAGIC then
								FIGHT_EVENT.ProcessPassMessage(PASS_SDO, AttObj, _CharObj, skillId)
							else
								FIGHT_EVENT.ProcessPassMessage(PASS_DO, AttObj, _CharObj, skillId)
							end	
							doProcessOk = true	
							if not TarObj then
								TarObj = _CharObj
							end													
						end
						if AttObj:CanAttackState(_CharObj) and AttObj:SecurityAreaCheckAttack(_CharObj) then
							CalOne(AttObj, _CharObj, skillData, oldTars, tarChars, tarCharEngine, _CharObj ~= TarObj)
						end
						FIGHT_EVENT.ClearFightEff(_CharObj, CMD_DO)
						FIGHT_EVENT.ClearFightEff(_CharObj, PASS_DO)
						FIGHT_EVENT.ClearFightEff(_CharObj, PASS_SDO)						
					end
				end
			elseif shape == SKILL_SHAPE_LINE then
				local len = _oneAreaInfo.len
				local len = len > SKILL_ATTAREA_MAX and SKILL_ATTAREA_MAX or len
				local accuracy = _oneAreaInfo.accuracy or 0.2
				local degreeTbl = _oneAreaInfo.degree
				assert(len and degreeTbl, string.format("not enought ele! att:%s, skillId:%s", AttObj:GetName(), skillId))
				local dlen = len ^ 2
				local ax, ay = AttObj:GetX(), AttObj:GetY()	
				local rx, ry = nil, nil
				if areaCenter == 1 then		--自己为中心
					rx, ry = ax, ay
				else						--敌人为中心
					rx, ry = normalTarRx, normalTarRy
				end
				local dx, dy = nil, nil
				if TarObj then
					dx, dy = TarObj:GetX(), TarObj:GetY()
				else
					dx, dy = normalTarRx, normalTarRy
				end
				if not dx or not dy then
					dx, dy = ax + 1, ay + 1
				end
				if dx == ax and dy == ay then
					dx, dy = ax + 1, ay + 1
				end
				
				--rx=x1 dx=x2 ry=y1 dy=y2
				local PosList = {}
				local IsInFunc = nil
				if rx==dx then		--x轴
					PosList.minx = rx-accuracy
					PosList.maxx = rx+accuracy
					PosList.miny = ry>dy and dy or ry
					PosList.maxy = PosList.miny+len
				elseif ry==dy then	--y轴
					PosList.miny = ry-accuracy
					PosList.maxy = ry+accuracy
					PosList.minx = rx>dx and dx or rx
					PosList.maxx = PosList.minx+len
				else
					PosList, IsInFunc = GetIsInFunc(rx, ry, dx, dy, accuracy, len)
				end
				
				--_RUNTIME("PosList:", sys.dump(PosList))
				for _x=math.floor(PosList.minx),math.ceil(PosList.maxx) do
					for _y=math.floor(PosList.miny),math.ceil(PosList.maxy) do
						if not BASECHAR.IsBlockPoint(_x, _y) then
							local tCharTbl = GetXYBattleCharObjs(AttObj:GetMapLayer(), _x, _y)
							if tCharTbl and (not IsInFunc or IsInFunc(_x, _y)) then
								for _id, _CharObj in pairs(tCharTbl) do
									local canAtt = true
									if notPlayerBoss then
										if not CanAttackNpcCostSp(_CharObj) then
											canAtt = false
										end
									end
									
									if canAtt and not _CharObj:IsDie() and CheckTarget(AttObj, _CharObj, attKind) then		--测试是否需要攻击
										if beHitMax and _CharObj ~= TarObj then
											tinsert(hitTarList, _CharObj)
										else
											if beHitMax then
												beHitMax = beHitMax - 1
											end
											if not doProcessOk then
												AttObj:SetTmp("LastHitSkillId", skillId)
												_CharObj:SetTmp("LastHitSkillId", skillId)
												FIGHT_EVENT.ProcessMessage(CMD_DO, AttObj, _CharObj, skillId)
												isCmdDo = true
												if mType == SKILL_MTYPE_MAGIC then
													FIGHT_EVENT.ProcessPassMessage(PASS_SDO, AttObj, _CharObj, skillId)
												else
													FIGHT_EVENT.ProcessPassMessage(PASS_DO, AttObj, _CharObj, skillId)
												end	
												doProcessOk = true	
												if not TarObj then
													TarObj = _CharObj
												end													
											end
											if AttObj:CanAttackState(_CharObj) and AttObj:SecurityAreaCheckAttack(_CharObj) then
												CalOne(AttObj, _CharObj, skillData, oldTars, tarChars, tarCharEngine, _CharObj ~= TarObj)
											end
											FIGHT_EVENT.ClearFightEff(_CharObj, CMD_DO)
											FIGHT_EVENT.ClearFightEff(_CharObj, PASS_DO)
											FIGHT_EVENT.ClearFightEff(_CharObj, PASS_SDO)
										end
									end
								end
							end
						end	
					end
				end
				if beHitMax and beHitMax > 0 then
					local tHitList = table.random_values(hitTarList, beHitMax)
					for _, _CharObj in pairs(tHitList) do
						if not doProcessOk then
							AttObj:SetTmp("LastHitSkillId", skillId)
							_CharObj:SetTmp("LastHitSkillId", skillId)
							FIGHT_EVENT.ProcessMessage(CMD_DO, AttObj, _CharObj, skillId)
							isCmdDo = true
							if mType == SKILL_MTYPE_MAGIC then
								FIGHT_EVENT.ProcessPassMessage(PASS_SDO, AttObj, _CharObj, skillId)
							else
								FIGHT_EVENT.ProcessPassMessage(PASS_DO, AttObj, _CharObj, skillId)
							end	
							doProcessOk = true	
							if not TarObj then
								TarObj = _CharObj
							end													
						end
						if AttObj:CanAttackState(_CharObj) and AttObj:SecurityAreaCheckAttack(_CharObj) then
							CalOne(AttObj, _CharObj, skillData, oldTars, tarChars, tarCharEngine, _CharObj ~= TarObj)
						end
						FIGHT_EVENT.ClearFightEff(_CharObj, CMD_DO)
						FIGHT_EVENT.ClearFightEff(_CharObj, PASS_DO)
						FIGHT_EVENT.ClearFightEff(_CharObj, PASS_SDO)	
					end
				end
			else
				FIGHT_EVENT.ClearFightEff(AttObj, CMD_DO)
				FIGHT_EVENT.ClearFightEff(AttObj, PASS_DO)
				FIGHT_EVENT.ClearFightEff(AttObj, PASS_SDO)
				error(string.format("not shape! att:%s, skillId:%s", AttObj:GetName(), skillId))
			end
		end
		if not isCmdDo then
			AttObj:SetTmp("LastHitSkillId", skillId)
			FIGHT_EVENT.ProcessMessage(CMD_DO, AttObj, AttObj, skillId)
		end
		
		FIGHT_EVENT.ClearFightEff(AttObj, CMD_DO)
		FIGHT_EVENT.ClearFightEff(AttObj, PASS_DO)
		FIGHT_EVENT.ClearFightEff(AttObj, PASS_SDO)
		if #tarCharEngine == 0 then
			tinsert(tarCharEngine, AttObj:GetEngineObj())
		end
		local playerTbl = laoi.map_regionsplayer(AttObj:GetMapObj(), tarCharEngine)
		if playerTbl then
			local vfds = {}
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
				end
			end
			if #vfds > 0 then
				-- if #protoMsg.tar_chars >= 16 then
				-- 	local msg = PBC.encode("S2c_aoi_skill_hit", protoMsg)
				-- 	_RUNTIME("____________:", #protoMsg.tar_chars, #msg)
				-- end
				pbc_send_msg(vfds, "S2c_aoi_skill_hit", protoMsg)
			end
		end		
	else
		error("not tarType:" .. tarType)
	end
	
--	AttObj:AddSp(addAttSp)

	if AttObj:IsMirrorPlayer() then
		if TarObj then
			local allPartner = AttObj:GetAllPartner()
			for _, _PartnerObj in pairs(allPartner) do
				if not TarObj:IsDie() then
					TryCall(_PartnerObj.SkillCheckOnce, _PartnerObj, TarObj)
				end
			end
			local allMagic = AttObj:GetAllMagic()
			for _, _MagicObj in pairs(allMagic) do
				if not TarObj:IsDie() then
					TryCall(_MagicObj.SkillCheckOnce, _MagicObj, TarObj)
				end
			end
		end
	end

	AttObj:SetTmp("LastHitSkillId", nil)

	return true
end




