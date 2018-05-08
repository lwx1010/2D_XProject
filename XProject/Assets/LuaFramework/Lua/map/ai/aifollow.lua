local assert = assert
local pairs = pairs
local mrandom = math.random
local mabs = math.abs
local tinsert = table.insert
local c_nBlockCnt 		= c_nBlockCnt
local c_nBlockMaxCnt 	= c_nBlockMaxCnt
local c_nHaltCnt 		= c_nHaltCnt				--追随者在被追随者旁边的停留步数c_nHaltCnt后移动一下
local c_aPoint = MOVE_DIR
--AI,EVENT状态--
local AI_CONTINUE 		= AI_CONTINUE
local AI_NEXT 			= AI_NEXT
local AI_EXCEPTION 		= AI_EXCEPTION
local AI_CANCELTARGET	= AI_CANCELTARGET
local AI_NULL 			= AI_NULL

local EVENT_BEATTACK	= EVENT_BEATTACK					--被攻击
local EVENT_TOATTACK	= EVENT_TOATTACK					--指使追随者(例如宠物)攻击

local NORMAL_NPC_RADIUS = NORMAL_NPC_RADIUS

------------------追随(跟WalkToCharacter不一样,不在范围就直接飞)玩家AI类------------------------
clsAIFollow = AI_WALKTOCHAR.clsAIWalkToCharacter:Inherit()
function clsAIFollow:__init__(charObj, tarCharId, radius, fradius, wTime, aTime, walkGrid)
	Super(clsAIFollow).__init__(self, charObj, tarCharId, radius, fradius, wTime, aTime, walkGrid)
	
	self.tarCharId = tarCharId
	self.radius = radius			--跟踪半径，超过就不跟了
	self.fradius = fradius			--距离跟踪物体多少就停止跟踪
	
	self.wTime = wTime
	self.aTime = aTime
	self.walkGrid = walkGrid or 1
end
function clsAIFollow:IsFollowAI()
	return true
end

function clsAIFollow:Run()
	local rt = Super(clsAIFollow).Run(self)
	if rt == AI_EXCEPTION then
		--搜索整张地图，找玩家，然后飞到旁边
		local px, py = self.charObj:FindCharObjPosByRId(self.tarCharId) 			--整张地图搜索玩家坐标，(没有则返回nil,nil)
		if px and py then	
			for i = 0, NORMAL_NPC_RADIUS - 1 do
				for randDir = 1, 8 do			
--					local randDir = mrandom(8)
					local nx = px + c_aPoint[randDir][1]
					local ny = py + c_aPoint[randDir][2]
					
					--JumpTo函数必须有返回true,false，如果返回true代表已经移动了
					if self.charObj:JumpTo(nx, ny) then
						self:SetActiveAIInit()			--清除一下阻塞追踪数据
						return AI_CONTINUE
					end
				end
			end
		end
		return AI_CONTINUE
	elseif rt == AI_NEXT then		--停止5次就走一步
--		local haltCnt = (self.haltCnt or 0) + 1
--		if haltCnt >= c_nHaltCnt then
----			_RUNTIME("clsAIFollow:Run:", haltCnt, c_nHaltCnt)
--			local ox, oy = self.charObj:GetX(), self.charObj:GetY()
--			for i = 1, 16 do	--循环后还没移动代表无法移动					
--				local randDir
--				if i <= 8 then
--					randDir = mrandom(8)
--				else
--					randDir = i - 8
--				end
--				local nx = ox + c_aPoint[randDir][1]
--				local ny = oy + c_aPoint[randDir][2]
--				
--				--MoveTo函数必须有返回true,false，如果返回true代表已经移动了
--				if self.charObj:MoveTo(nx, ny) then
--					break
--				end
--			end		
--		end
--		self.haltCnt = haltCnt % c_nHaltCnt
		return AI_CONTINUE
	end
	return rt
end

function clsAIFollow:OnEvent(eventTbl)
	--被攻击了转到下个目标去
	if eventTbl.eventType == EVENT_BEATTACK or eventTbl.eventTbl == EVENT_TOATTACK then		
		return AI_NEXT							
	end
	return AI_CONTINUE
end