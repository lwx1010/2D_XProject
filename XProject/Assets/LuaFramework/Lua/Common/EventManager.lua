--[[
	 Author： LiangZG
	Email :  game.liangzg@foxmail.com
    Desc:全局事件管理器
]]
EventManager = {}
local this = EventManager
--事件池
local eventPool = {}

--添加事件模块
-- @params eventModule string   模块名
-- @params luatable  table   初注入event的table
-- @return EventPool  事件池
function EventManager.AddEvent(eventModule , luatable)
    if eventPool[eventModule] then
        Debugger.LogWarning("EventManager have same event Module ! eventModule name is " .. eventModule)
        return eventPool[eventModule]
    end

    luatable.event = EventManager.EventPool.new()
    eventPool[eventModule] = luatable.event
    return luatable.event
end

--全局广播事件
function EventManager.SendEvent(eventName , ...)

    local args = {...}
    for k , v in pairs(eventPool) do
        if v:Brocast(eventName , unpack(args))  then
            return
        end
    end

end

--清理一份LuaTable内的事件
function EventManager.ClearEvent(eventModule)
    if not eventPool[eventModule] then
      Debugger.LogWarning("EventManager Cant find event Module ! eventModule name is " .. eventModule)
      return
    end

    local subEvents = eventPool[eventModule]
    subEvents:Clear()

    eventPool[eventModule] = nil
end

------------------------具体模块的事件池------------------------------------

local EventLib = require "eventlib"

EventManager.EventPool = class("EventManager_EventPool")

function EventManager.EventPool:ctor()
    self.events = {}
end

function EventManager.EventPool:AddListener(event,handler)
    if not event or type(event) ~= "string" then
        error("event parameter in addlistener function has to be string, " .. type(event) .. " not right.")
    end
    if not handler or type(handler) ~= "function" then
        error("handler parameter in addlistener function has to be function, " .. type(handler) .. " not right")
    end

    if not self.events[event] then
        --create the Event with name
        self.events[event] = EventLib:new(event)
    end

    --conn this handler
    self.events[event]:connect(handler)
end

--广播事件
function EventManager.EventPool:Brocast(event,...)
    if not self.events[event] then
        --error("brocast " .. event .. " has no event.")
        return false
    else
        self.events[event]:fire(...)
    end

    return true
end

--删除指定指定
function EventManager.EventPool:RemoveListener(event,handler)
    if not self.events[event] then
        --error("remove " .. event .. " has no event.")
        return
    else
        self.events[event]:disconnect(handler)
    end
end

---是否存在指定事件的监听
function EventManager.EventPool:Exist(event)
    return self.events[event] or false
end

--清空事件列表
function EventManager.EventPool:Clear()
    for k , v in pairs(self.events) do
        v:DisconnectAll()
    end
end

