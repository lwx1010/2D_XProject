-- @Author: LiangZG
-- @Date:   2017-04-07 17:49:57
-- @Last Modified time: 2017-04-07 19:42:37
-- @Dsc : 基于MVVM架构的Data Binding (数据驱动更新)


local DataBinder = {}

--- 格式化Data binding 驱动table表的必要结构
--@param model 用于缓存数据的table
--@return table  格式化后的表
function DataBinder.bindFormat( model )
    local t = {}

    local mt = {_bind = {}}

    --查新定义查询
    mt.__index = function ( table , key )
        --print("key:" .. key .. ',Val:' .. tostring(mt[key]))
        return mt[key]
    end

    mt.__newindex = function ( table , key , value )
        local oldVal = mt[key]
        if oldVal == value then return end

        mt[key] = value
        local slots = mt._bind[key]
        --print("key:" .. key .. ',Val:' .. tostring(mt[key]) .. ",slots:" .. print_lua_table(slots))
        if slots then
            for _,func in pairs(slots) do
               func(key , value)
            end
        end
    end
    
    setmetatable(t , mt)

    for k,v in pairs(model) do
       t[k] = v
    end

    return t
end

--- 绑定对应UI下的所有数据绑定
-- @param ui LuaTable UI界面
-- @param model ModelBase(LuaTable) UI界面对应的数据模块
function DataBinder.binding( ui , model )
    
    local luaContext = ui.gameObject:GetComponent("LuaContext")
    if not luaContext then
        Debugger.LogWarning(string.format("%s cant find LuaContext Component !" , ui.gameObject.name))
        return 
    end

    luaContext.LuaModel = model
    local binderArr = luaContext.Binders    
	local len = binderArr.Length - 1
    local _bind = model._bind

	for i = 0 , len do
		local binder = binderArr[i]
        local bindKey = binder.Path   
        local v = model[bindKey]    

        if bindKey then   
            --print("binding key : " .. bindKey)             
            if asClass(v ,"Collection") then
                v.addDelegate = handler(binder , binder.OnCollectionItemAdded)
                v.removeDelegate = handler(binder , binder.OnCollectionItemRemoved)
                v.clearDelegate= handler(binder , binder.OnCollectionCleared)
                v.onValueChanged= handler(binder , binder.OnChangedFinish)     
                binder:OnObjectChanged(v)  
            else
                
                local hook = _bind[bindKey] or {}
                local handler = function ( key,value )
                    binder:OnObjectChanged(value)
                end
                handler(bindKey , model[bindKey]) -- init

                hook[binder] = handler
                _bind[bindKey] = hook
            end
        end
    end
end

--- 释放对应UI下的所有数据绑定
-- @param ui LuaTable UI界面
-- @param model ModelBase(LuaTable) UI界面对应的数据模块
function DataBinder.unbinding(ui , model)
    
    local luaContext = ui.gameObject:GetComponent("LuaContext")
    if not luaContext then
        Debugger.LogWarning(string.format("%s cant find LuaContext Component !" , ui.gameObject.name))
        return 
    end

    luaContext.LuaModel = model
    local binderArr = luaContext.Binders    
	local len = binderArr.Length - 1
    local _bind = model._bind

	for i = 0 , len do
		local binder = binderArr[i]
        local bindKey = binder.Path   
        local v = model[bindKey]    

        if bindKey then   
            if asClass(v ,"Collection") then
                v:clearDelegates()
            elseif _bind[bindKey] then                
                local hook = _bind[bindKey]                              
                hook[binder] = nil
            end
        end
    end
end

return DataBinder

