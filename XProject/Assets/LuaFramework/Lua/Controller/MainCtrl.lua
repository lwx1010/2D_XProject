local MainCtrl = {}
local this = MainCtrl

local main
local transform
local gameObject

--构建函数--
function MainCtrl.New()
	return this
end

function MainCtrl.Awake()
	createPanel('Main', this.OnCreate, false, 0, 0, 0, true)
end

--启动事件--
function MainCtrl.OnCreate(obj)
	gameObject = obj
	transform = gameObject.transform

	main = gameObject:GetComponent('LuaBehaviour')


end


return this