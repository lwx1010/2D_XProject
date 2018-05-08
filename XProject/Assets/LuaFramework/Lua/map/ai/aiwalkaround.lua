local assert = assert
local pairs = pairs
local mrandom = math.random
local mabs = math.abs
local tinsert = table.insert
local c_nBlockCnt 		= c_nBlockCnt
local c_nBlockMaxCnt 	= c_nBlockMaxCnt
local c_nHaltCnt 		= c_nHaltCnt				--追随者在被追随者旁边的停留步数c_nHaltCnt后移动一下
local c_aPoint = MOVE_DIR
local c_nSleepCnt = c_nSleepCnt
--AI,EVENT状态--
local AI_CONTINUE 		= AI_CONTINUE
local AI_NEXT 			= AI_NEXT
local AI_EXCEPTION 		= AI_EXCEPTION
local AI_CANCELTARGET	= AI_CANCELTARGET
local AI_NULL 			= AI_NULL

local EVENT_BEATTACK	= EVENT_BEATTACK					--被攻击
local EVENT_TOATTACK	= EVENT_TOATTACK					--指使追随者(例如宠物)攻击

local lua_time_sec = lua_time_sec
local sleep_timeno = 60	

------------------周围逛AI类-----------------------------------------------------
clsAIWalkAround = AI_BASE.clsAIBase:Inherit()
function clsAIWalkAround:__init__(charObj, wTime, aTime, walkGrid)
	Super(clsAIWalkAround).__init__(self, charObj)
--	self.wTime = wTime + math.random(5)
	self.wTime = sleep_timeno + math.random(5)
	self.aTime = aTime
	self.walkGrid = walkGrid or 1
end

function clsAIWalkAround:GetTime()
	return self.wTime
end

function clsAIWalkAround:Run()
	local sCharObj = self.charObj
	if sCharObj:IsNpc() then
		if sCharObj:GetAIPatrol() == 0 then			--不巡逻
			return AI_CONTINUE
		end
	end

--	local sleepCnt = self.sleepCnt or 1
	local ox = sCharObj:GetX()	
	local oy = sCharObj:GetY()
	local speed = self.walkGrid				--npc的速度规定1一格（不是1复杂很多，要判断中间有没阻塞，终点阻塞，中间的能过）
	
--	if sleepCnt >= sleep_timeno then
		local aiRange
		local minx, miny, maxx, maxy
		if sCharObj:IsNpc() then
			local roundMax = sCharObj:GetPatrolRange()
			aiRange = sCharObj:GetAIRange()
			minx, miny, maxx, maxy = aiRange.x - roundMax, aiRange.y - roundMax, aiRange.x + roundMax, aiRange.y + roundMax
		else
			local roundMax = NORMAL_NPC_RADIUS
			aiRange = sCharObj:GetAIRange()
			minx, miny, maxx, maxy = aiRange.x - roundMax, aiRange.y - roundMax, aiRange.x + roundMax, aiRange.y + roundMax
		end
		if aiRange and ox < minx or ox > maxx or oy < miny or oy > maxy then
			local dir1, dir2, dir3, dir4 = AI_WALKTOCHAR.PublicGetPriorityDir(ox, oy, aiRange.x, aiRange.y)
			local testRandomStep = true
			local function TestOneGridMove(tox, toy, ...)
				local num_args = select("#", ...)
				local arg = {}
				for i = 1, num_args do
					arg[i] = select(i, ...)
				end
				arg['n'] = num_args
				for i = 1, num_args do
					if arg[i] then
						local tnx = tox + c_aPoint[arg[i]][1]
						local tny = toy + c_aPoint[arg[i]][2]
						
						if sCharObj:CanMoveToOneGrid(tox, toy, tnx, tny) then
							return true, tnx, tny
						end
					end
				end
			end
			local function TestDirMove(testDir)
				if testDir then
					local nx = ox + c_aPoint[testDir][1]
					local ny = oy + c_aPoint[testDir][2]				
					--如果是2格的移动,移动不能带slope
					if sCharObj:CanMoveToOneGrid(ox, oy, nx, ny) then
						if speed ~= 1 then
							local dx, dy = aiRange.x, aiRange.y
							local tdisX = mabs(nx - dx)
							local tdisY = mabs(ny - dy)		
							if tdisX ~= 0 or tdisY ~= 0 then		--0,0达到了dx,dy就不用判断移2步
								local tdir1, tdir2, tdir3, tdir4 = AI_WALKTOCHAR.PublicGetPriorityDir(nx, ny, dx, dy)
								local isOk, tnx, tny = TestOneGridMove(nx, ny, tdir1, tdir2, tdir3, tdir4)
								if isOk and tnx and tny then
									--判断如果是2格则才移动2格
									tdisX = mabs(ox - tnx)
									tdisY = mabs(oy - tny)
									local tLen = tdisX ^ 2 + tdisY ^ 2
									if tLen == 4 or tLen == 5 or tLen == 8 then
										if sCharObj:MoveTo(tnx, tny, nil, speed, true) then
											return AI_CONTINUE
										end
									end
								end
							end
						end
						sCharObj:MoveTo(nx, ny, nil, speed, true)
						return AI_CONTINUE
					end
				end
			end
			local rtTestDir = TestDirMove(dir1) or TestDirMove(dir2) or TestDirMove(dir3) or TestDirMove(dir4)
			if rtTestDir then 
				return rtTestDir 
			end
		else
			for i = 1, 16 do	--循环后还没移动代表无法移动		
				local randDir = nil
				if i <= 8 then
					randDir = mrandom(8)
				else
					randDir = i - 8
				end
				
				local dx, dy = ox, oy
				for j = 1, speed do
					local nx = dx + c_aPoint[randDir][1]
					local ny = dy + c_aPoint[randDir][2]
					
					--如果超出范围
					if nx < minx or nx > maxx or ny < miny or ny > maxy then
						break
					else
						if sCharObj:CanMoveToOneGrid(dx, dy, nx, ny) then
							dx, dy = nx, ny
						end
					end
				end
				if dx ~= ox or dy ~= oy then
					--MoveTo函数必须有返回true,false，如果返回true代表已经移动了
					if sCharObj:MoveTo(dx, dy, nil, speed, true) then
--						self.sleepCnt = (sleepCnt + 1) % c_nSleepCnt
						return AI_CONTINUE
					end				
				end
			end			
		end

--		sleepCnt = sleepCnt - sleep_timeno
--	end
	
--	if self.wTime >= sleep_timeno then
--		self.sleepCnt = sleep_timeno
--	else
--		self.sleepCnt = sleepCnt + self.wTime
--	end
	return AI_CONTINUE	
end

function clsAIWalkAround:OnEvent(eventTbl)
	if eventTbl.eventType == EVENT_BEATTACK or eventTbl.eventType == EVENT_TOATTACK then		--被攻击了转到下个目标去
		return AI_NEXT					
	end
	return AI_CONTINUE
end