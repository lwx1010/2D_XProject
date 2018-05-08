-- @Author: LiangZG
-- @Date:   2017-05-04 09:54:04
-- @Last Modified time: 2017-05-04 10:12:47
-- @Desc:  设置模块数据处理


local SettingModel = class("SettingModel")


--检测名字是否可用
local validString = { " ", "\"", "\'", "\\", "/", "{", "}", "%[", "%]",} --"[", "]", 

local maxChars = 14

function SettingModel:checkNameValid(name)
    --字符数量
    if #name > maxChars then
        name = string.sub(name, 0, maxChars)
    end

    --非法字符
    local nameVaild = true
    for i =1, #validString do
        if string.find(name, validString[i]) then
            nameVaild = false
        end
    end

    return nameVaild
end


SettingModel.inst = SettingModel.new()

return SettingModel