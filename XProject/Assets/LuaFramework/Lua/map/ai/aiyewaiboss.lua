local assert = assert
local pairs = pairs
local mrandom = math.random
local mabs = math.abs
local tinsert = table.insert
local mceil = math.ceil

clsAIYewaiBoss = AI_WALKAROUNDATTACK.clsAIWalkAroundAttack:Inherit({__ClassType = "<yewaiboss>"})
local AllNpcData = NPC_BATTLE_DATA.GetAllNpcData()
function clsAIYewaiBoss:OnEvent(eventTbl)
	local resetAiTime = false
	local AtkList = self.charObj:GetTmp("AtkList") or {}
	local BelongId = AtkList[1] and AtkList[1].id or nil
	
--	if self.charObj:GetCharNo()==20200601 then
--		_RUNTIME(self.activeAINo)
--		_RUNTIME(sys.dump(eventTbl))
--		_RUNTIME(BelongId)
--	end
	
	if ((self.activeAINo == 1 and (eventTbl.eventType == EVENT_BEATTACK or eventTbl.eventType == EVENT_TOATTACK))
	or (self.activeAINo == 2 and eventTbl.eventType == EVENT_BEATTACK))
	and not self.urgentAI then
		if eventTbl.eventAttackCharId then
			local atkcharId = self.aAI[2]:GetTarCharId()	
			if self.activeAINo==2 and (eventTbl.eventAttackCharId==atkcharId or eventTbl.eventAttackCharId~=BelongId) then 
				return AI_CONTINUE
			end
			
			local tarObj = CHAR_MGR.GetCharById(eventTbl.eventAttackCharId)
			if tarObj then
				if not tarObj:CheckCharType(self.searchCharType) then return end
			end
			
			self.aAI[2]:SetTarCharId(eventTbl.eventAttackCharId)	
			if eventTbl.eventType == EVENT_BEATTACK then
				local x, y = eventTbl.attX, eventTbl.attY
				assert(x and y, "not clsAIWalkAroundAttack:OnEvent attX, attY")
				local sx, sy = mabs(x - self.charObj:GetX()), mabs(y - self.charObj:GetY())
				local radius = sx > sy and sx or sy
				self:SetRadius(radius)	
			end		
			
			if IsClient() then
				if eventTbl.eventType == EVENT_BEATTACK or eventTbl.eventType == EVENT_TOATTACK	then
					local oneData = AllNpcData[self.charObj:GetCharNo()]
					if oneData then
						self:SetRadius(oneData.AITrackRange)
					end
				end
			end	
				
			resetAiTime = true	
		else
			return AI_CONTINUE
		end
	end
	Super(AI_WALKAROUNDATTACK.clsAIWalkAroundAttack).OnEvent(self, eventTbl)
	if resetAiTime and self.activeAINo == 2 then
		--如果是被攻击导致追踪,需要把ai时间设置下	
		DelCharObjToAITbl(self.charObj)
		AddCharObjToAITbl(self.charObj, 10)	
	end
end