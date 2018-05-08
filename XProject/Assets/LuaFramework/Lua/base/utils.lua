

--[[--
将 Lua 对象及其方法包装为一个匿名函数
在 Unity 中，许多功能需要传入一个 Lua 函数做参数，然后在特定事件发生时就会调用传入的函数。例如触摸事件、帧事件等等。
~~~ lua
local MyScene = class("MyScene", function()
    return display.newScene("MyScene")
end)
function MyScene:ctor()
    self.frameTimeCount = 0
    -- 注册帧事件
    self:addEventListener(cc.ENTER_FRAME_EVENT, self.onEnterFrame)
end
function MyScene:onEnterFrame(dt)
    self.frameTimeCount = self.frameTimeCount + dt
end
~~~
上述代码执行时将出错，报告"Invalid self" ，这就是因为 C++ 无法识别 Lua 对象方法。因此在调用我们传入的 self.onEnterFrame 方法时没有提供正确的参数。
要让上述的代码正常工作，就需要使用 handler() 进行一下包装：
~~~ lua
function MyScene:ctor()
    self.frameTimeCount = 0
    -- 注册帧事件
    self:addEventListener(cc.ENTER_FRAME_EVENT, handler(self, self.onEnterFrame))
end
~~~
实际上，除了 C++ 回调 Lua 函数之外，在其他所有需要回调的地方都可以使用 handler()。
@param mixed obj Lua 对象
@param function method 对象方法
@return function
]]
function handler(obj, method)
    return function(...)
        return method(obj, ...)
    end
end
