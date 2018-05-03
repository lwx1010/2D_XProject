local string=string
local table=table
local debug=debug
local pairs=pairs
local tostring=tostring
local tonumber=tonumber
local math=math
local MAP_ID = MAP_ID
local MAP_NO = MAP_NO
local tinsert = table.insert

clsItem = BASECHAR.clsBaseChar:Inherit({__ClassType = "ITEM"})

function clsItem:__init__(mlCharId, x, y, z, syncData, ociData, mapLayer)
	assert(mlCharId, "not mlCharId in clsItem")
	Super(clsItem).__init__(self, x, y, z, ITEM_TYPE, syncData, ociData, mapLayer)
	
	lretmap.itemadd(self:GetId(), MAP_ID, self:GetMapLayer(), self:GetX(), self:GetY(), self:GetZ())
	self:SetComp(0)			--设置到引擎
end

function clsItem:IsItem()
	return true
end

function clsItem:AddMap()			--添加进入地图后的处理,必须重载,不然报错
	local isOk, playerTbl = laoi.map_addobj(self:GetMapObj(), self:GetEngineObj())
	assert(isOk, "item add map error: " .. self:GetName() .. " x,y" .. self:GetX()..","..self:GetY())
	if isOk then
		self:MoveChangeMapPos(CHANGE_MAPPOS_ADD)
		if playerTbl then
			local vfds = {}
			for _, pCharId in pairs(playerTbl) do
				local pCharObj = CHAR_MGR.GetCharById(pCharId)
				if pCharObj then
					tinsert(vfds, pCharObj:GetVfd())
				end
			end
			if #vfds > 0 then
--				pbc_send_msg(vfds, "S2c_aoi_add", {													--测试，暂时没有物品
--					rid = self:GetId(),
--					type = ITEM_TYPE,
--					x = self:GetX(),
--					y = self:GetY(),
--				})		
			end		
		end
	end
end

function clsItem:Move()				--添加移动后的处理,必须重载,不然报错(虽然物品没有移动)
--	_RUNTIME("clsItem:Move")
end

function clsItem:MoveTo()			--添加移动后的处理,必须重载,不然报错(虽然物品没有移动)
--	_RUNTIME("clsItem:MoveTo")
end

function clsItem:JumpTo()			--添加移动后的处理,必须重载,不然报错(虽然物品没有移动)
--	_RUNTIME("clsItem:JumpTo")
end

function clsItem:LeaveMap()			--添加离开地图后的处理,必须重载,不然报错
	local isOk, playerTbl = laoi.map_removeobj(self:GetMapObj(), self:GetEngineObj())	
	assert(isOk, "clsItem:LeaveMap error: " .. self:GetName())
	
	if playerTbl then
		local vfds = {}
		for _, pCharId in pairs(playerTbl) do
			local pCharObj = CHAR_MGR.GetCharById(pCharId)
			if pCharObj then
				tinsert(vfds, pCharObj:GetVfd())
			end
		end
		if #vfds > 0 then
			pbc_send_msg(vfds, "S2c_aoi_leave", {													--测试，没有写完
				map_no = MAP_NO,
				map_id = MAP_ID,
				id = self:GetId(),
			})		
		end		
	end	
	
	lretmap.itemleave(self:GetId(), MAP_ID, self:GetMapLayer())
	self:MoveChangeMapPos(CHANGE_MAPPOS_LEAVE)
end