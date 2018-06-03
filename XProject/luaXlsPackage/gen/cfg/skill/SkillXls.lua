local function Process(XlsInst)
	XlsInst:SetVarFunction("DivideDamageTime" , [[%s]])
	XlsInst:SetVarFunction("DivideDamageNum" , [[%s]])
end


local function postProcess(Data)
	for k, v in pairs(Data) do
		v.AttAreaShow = loadstring("return " .. v.AttAreaShow)()
	end
	
	return Data
end

cfg =
{
	xls="skill/skill.xls",
	sheet=2,
	key="ID",
	columns={"ID","Name","AttKind","HitNumType","AttRange","AttAreaCenter","CanBreak","AttAreaShow","FrontSkill","BeforeHurtTime","SkillTime","CD","ConAtt","MoveLength","ConMoveLength","CanMove", "DivideDamageTime", "DivideDamageNum", "SkillResPath", "SkillIcon"},
	outputName="Skill/SkillXls",
	postProcess=postProcess,
	beforeProcess=Process,
	needJson = true,
	desc="技能表格",
}

classcfg = 
{
	className = "SkillXls",
	props={"ID","Name","AttKind","HitNumType","AttRange","CanBreak","FrontSkill","BeforeHurtTime","SkillTime","CD","ConAtt","MoveLength","ConMoveLength","CanMove", "SkillResPath"},
	propTypes={"int", "string", "int", "int", "float", "int", "int", "int", "int", "int", "List<int>", "float", "List<float>", "int", "string"},
}