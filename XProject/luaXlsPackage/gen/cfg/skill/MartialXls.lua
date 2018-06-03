local function PostProcess(Data)
	for k, v in pairs(Data) do
		v.SkillPriority = v.SkillPriority or 0
	end
	return Data
end

cfg =
{
	xls="skill/skill.xls",
	sheet=1,
	key="ID",
	columns={"ID","Name","Type","AtkType","Mtype","SkillList","SkillDesc", "SkillPriority"},
	outputName="Skill/MartialXls",
	postProcess=PostProcess,
	needJson = true,
	desc="Œ‰—ß±Ì∏Ò",
}

classcfg = 
{
	className = "MartialXls",
	props=cfg.columns,
	propTypes={"int", "string", "int", "int", "int", "List<int>", "string", "int"},
}