-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc

local ServerCell = class("ServerCell")


function ServerCell:Awake(go)
    self.gameObject = go
    self.transform = go.transform
    self.widgets = {
		{field="imgServerStatus",path="statusIcon",src="GameObject"},
		{field="serverNameLab",path="serverName",src="GameObject"},
		{field="imgNewServer",path="newServer",src="GameObject"},
		{field="accentIcon",path="accentIcon",src="GameObject"},
		{field="accentLvLab",path="accentIcon.lv",src="GameObject"},

    }
    LuaUIHelper.bind(self.gameObject , self)
end


function ServerCell:Copy(parentGO)
    local newGo = newObject(self.gameObject)
    Util.SetParent(newGo , parentGO)

    local copy = self.new()
    copy:Awake(newGo)
    return copy
end

function ServerCell:OnDestroy()

end

return ServerCell