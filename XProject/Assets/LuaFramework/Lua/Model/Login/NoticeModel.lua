-- @Author: LiangZG
-- @Date:   2017-04-10 11:24:17
-- @Last Modified time: 2017-06-19 22:38:25
-- @Desc: 公告模块数据缓存

local json = require "cjson"
local NoticeModel = class("NoticeModel")

function NoticeModel:ctor( )
    -- notices
end

--获得中心服的公告
function NoticeModel:getCenterServerNotices(  )
    if User_Config.internal_sdk == 1 then
        CenterServerManager.Instance:GetNoticeInfo(function ( notices )
            if string.isEmptyOrNil(notices) then    return   end
            local decJson = json.decode(notices)

			if (not decJson) or table.nums (decJson) <= 0 then
				Debugger.Log ("Lua Json 解析失败")
				return
			end

            self.notices = decJson
            if NoticesPanel.IsNeedShow() then
                NoticesPanel.show()
            end
        end)
    else
        self.notices = {
            {["title"] = "公告" , ["headline"] = "公告" , ["announceTime"]="2017-02-09 19:50:16" , ["announceContxt"]="祝您游戏愉快！"},
            {["title"] = "公告2" , ["headline"] = "公告2" , ["announceTime"]="2017-02-17 19:50:16" , ["announceContxt"]="祝您游戏愉快！"},
        }
        if NoticesPanel.IsNeedShow() then
            NoticesPanel.show()
        end
    end
end

--公告数量
function NoticeModel:count()
    return self.notices and #self.notices or 0
end

NoticeModel.inst = NoticeModel.new()

return NoticeModel
