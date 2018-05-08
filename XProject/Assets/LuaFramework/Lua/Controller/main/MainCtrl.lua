local MainCtrl = {}
local this = MainCtrl

this.init = false

--构建函数--
function MainCtrl.New()
	return this
end

--启动事件--
function MainCtrl.OnCreate(obj)

	print("=====================MainCtrl========================")

	-- mainRole.roleState:AddListener(CtrlNames.Main, this.OnStateChanged)
end


function MainCtrl.OnStateChanged(state, action)
	if not roleMgr.mainRole then return end
	print("****", action, "****", state)
	local roleState = roleMgr.mainRole.roleState
	if action == "addstate" then

	elseif action == "removestate" then

	end
end

return this