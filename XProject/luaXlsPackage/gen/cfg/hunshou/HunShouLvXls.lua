local function postProcess(RawData)
	local Data = {}
	for _,v in ipairs(RawData)  do
		if not Data[v.HunShouNo] then
			Data[v.HunShouNo] = {}
		end
		Data[v.HunShouNo][v.Step] = v
	end
	return Data
end

cfg =
{
	xls="hunshou/hunshou.xls",
	sheet=2,
	columns={"HunShouNo","Step","StepProp","BreakCost","BressLimit"},
	outputName="Hunshou/HunShouLvXls",
	postProcess=postProcess,
	desc="hunshou",
}