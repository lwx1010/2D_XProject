-- @Author: LiangZG
-- @Date:   2017-02-28 11:01:08
-- @Last Modified time: 2017-07-25 10:42:08

local LuaUIHelper = {}

function LuaUIHelper.bind( gObj , panel)
    local trans = gObj.transform

    local behaviour = gObj:GetComponentInParent(typeof(LuaFramework.LuaBehaviour))
    panel.behaviour = behaviour


    for _,widget in pairs(panel.widgets) do
        local widgetPath = string.gsub(widget.path , "%." , "/")
        local childTrans = trans:Find(widgetPath)

        if childTrans then
            local luaCom = widget.src
            if type(widget.src) ~= "table" then
                luaCom = LuaUiComponent
            end
            luaCom = luaCom.new()
            luaCom = luaCom:bind(childTrans , widget , behaviour)
            panel[widget.field] = luaCom
        else
            Debugger.Log("Cant find Child ! Path is " .. widgetPath)
        end
    end
end

--设置所有Panel的深度偏移
function LuaUIHelper.setPanelDepthOffset( root , offsetDepth )
    local panels = root:GetComponentsInChildren(typeof(UnityEngine.UI.Canvas))
    for i= panels.Length - 1,0 , -1 do
        panels[i].sortingOrder = panels[i].sortingOrder + offsetDepth
    end
end

--注册事件
function LuaUIHelper.addUIDepth(gObj , panel)
    --注册事件
    local rootEvent = EventManager.AddEvent(gObj.name , panel)
    if panel.widgets then
        for _,widget in pairs(panel.widgets) do
            if widget.src == LuaScript then
                panel[widget.field].event = rootEvent
            end
        end
    end
end

--- 刷新界面深度
-- @param gObj GameObject UI界面根结点
-- @param panel LuaTable   UI逻辑处理LuaTable
function LuaUIHelper.reflushDepth(gObj , panel)
    local panels = gObj:GetComponentsInChildren(typeof(UnityEngine.UI.Canvas) , true)
    local offsetDepth = CtrlManager.panelDepth
    local maxDepth = offsetDepth + CtrlManager.PANEL_DEPTH_OFFSET - 5 --保留5个单位的间隔

    if not panel._orginPanelDepth then
        local orginPanel = panel._orginPanelDepth or {}

        for i= panels.Length - 1,0 , -1 do
            local path = LuaUIHelper._getRelativePath(gObj , panels[i] , {})
            orginPanel[path] = orginPanel[path] or panels[i].depth

            panels[i].sortingOrder = Mathf.Min(orginPanel[path] + offsetDepth , maxDepth) --更新深度
        end

        panel._orginPanelDepth = orginPanel

        return
    end

    for i= panels.Length - 1,0 , -1 do
        local path = LuaUIHelper._getRelativePath(gObj , panels[i] , {})
        local orginePanelDepth = 0
        if not panel._orginPanelDepth[path] then
           -- 动态添加,查找父UIPanel的原始深度
           local parentPath = LuaUIHelper._getParentPath(path)

           while (not string.isEmptyOrNil(parentPath)) do
                if panel._orginPanelDepth[parentPath] then
                    orginePanelDepth = panel._orginPanelDepth[parentPath] + 5
                    break
                end
                parentPath = LuaUIHelper._getParentPath(parentPath)
           end        
        else
            orginePanelDepth = panel._orginPanelDepth[path]
        end

        panels[i].sortingOrder = Mathf.Min(orginePanelDepth + offsetDepth , maxDepth) --更新深度
    end

end

--移除事件
function LuaUIHelper.removeUIDepth( gObj )
    if IsNil(gObj) then return end

    EventManager.ClearEvent(gObj.name)  --注册事件
end

--- 获取结点的相对层次路径
function LuaUIHelper._getRelativePath( root , child , bufName)
    bufName[#bufName + 1] = child.name
    if not child.transform.parent or child.transform.parent.gameObject == root then
        return table.concat(bufName , ".")
    end
    return LuaUIHelper._getRelativePath(root , child.transform.parent.gameObject , bufName)
end


function LuaUIHelper._getParentPath(path)    
    local pathArr = string.split(path , ".")
    table.remove(pathArr , 1)
    return table.concat(pathArr , ".")
end

--- 绑定界面UI id
--@param id int 界面ID
--@param mainView LuaTable  界面显示逻辑脚本
function LuaUIHelper.uiid(id , mainView)
    local panelIdMap = CtrlManager.panelId2TabMap --界面ID映射表
    if panelIdMap[id] then
        Debugger.LogError("Have Same UIID ! ID is " .. id )
        return         
    end
    panelIdMap[id] = mainView
end

return LuaUIHelper
