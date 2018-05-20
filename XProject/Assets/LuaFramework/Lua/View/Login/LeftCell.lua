-- @Author:
-- @Last Modified time: 2017-01-01 00:00:00
-- @Desc

local LeftCell = class("LeftCell")


function LeftCell:Awake(go)
    self.gameObject = go
    self.transform = go.transform
    self.widgets = {
		{field="titleNormal",path="title",src=LuaText},
		{field="titleActiveLab",path="checkmark.title",src="GameObject"},
		{field="imgNew",path="new",src="GameObject"},

    }
    LuaUIHelper.bind(self.gameObject , self)
end


function LeftCell:Copy(parentGO)
    local newGo = newObject(self.gameObject)
    Util.SetParent(newGo , parentGO)

    local copy = self.new()
    copy:Awake(newGo)
    return copy
end

function LeftCell:OnDestroy()

end

return LeftCell