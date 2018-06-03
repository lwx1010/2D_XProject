
local function GetData(RawData)
	local Data = {}
	for rowno,rowdata in pairs(RawData) do
		Data[rowdata.MapNo] = rowdata

		local securityAreas = rowdata.SecurityAreas
		local areasTblStr = string.split(securityAreas, ";")
		securityAreas = {}
		for _i, _areaStr in pairs(areasTblStr) do
			securityAreas[_i] = assert(loadstring("return " .. _areaStr))()
			assert(#securityAreas[_i].pos == 2)
			assert(securityAreas[_i].r)
		end
		if #securityAreas > 0 then
			rowdata.SecurityAreas = securityAreas
		else
			rowdata.SecurityAreas = nil
		end
	end
	return Data
end


cfg =
{
	xls="map/map.xls",
	sheet="Map",
	columns={"searchtips", "Enemy", "MapNo", "Name", "LoadType", "SecurityAreas", "SceneName", "MapData", "SyncInterval", "SceneType", "FightType", "CR1", "FOV1", "YOffset1", "CDistance", "XAngle", "MaxClimbHeight", "MapYOffset", "MinimapViewSize", "Dir", "BGM", "volume", "DisPlayType" , "Level", "ShowAllSkillEffect", "ShowSimpleSkillEffect", "ModelType", "SceneBlock", "DiduiScene","ClientEffects","ShowSceneName" , "CutsceneTriggers", "IsQuadScene"},
	outputName="Map/MapDataXls",
	postProcess=GetData,
	desc="地图信息表",
}
