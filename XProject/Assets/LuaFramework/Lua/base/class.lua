require "base.utils"
require "base.string"
require "base.table"

--全局类型列表
ClassTypeList = {}
--基础类库

--获取一个class的父类
function Super(TmpClass)
	return TmpClass.__SuperClass
end

--判断一个class或者对象是否
function IsSub(clsOrObj, Ancestor)
	local Temp = clsOrObj
	while  1 do
		local mt = getmetatable(Temp)
		if mt then
			Temp = mt.__index
			if Temp == Ancestor then
				return true
			end
		else
			return false
		end
	end
end

--暂时没有一个比较好的方法来防止将Class的table当成一个实例来使用
--大家命名一个Class的时候一定要和其产生的实例区别开来。
clsObject = {
		--用于区别是否是一个对象 or Class or 普通table
		__ClassType = "<base class>"
	}

function clsObject:Inherit(o)
	o = o or {}

	--没有对table属性做深拷贝，如果这个类有table属性应该在init函数中初始化
	--不应该把一个table属性放到class的定义中

	if not self.__SubClass then
		self.__SubClass = {}
		setmetatable(self.__SubClass, {__mode="v"})
	end
	table.insert(self.__SubClass, o)

	--这里不能设置metatable，否则会导致UTIL.copy会导致错误！！！！，要设置metatable必须放到子类里去弄
	--setmetatable(o, {__index = self})
	for k, v in pairs(self) do
		if not o[k] then
			o[k]=v
		end
	end
	o.__SubClass = nil
	o.__SuperClass = self

	return o
end

function clsObject:AttachToClass(Obj)
	setmetatable(Obj, {__ObjectType="<base object", __index = self})
	return Obj
end

function clsObject:New(...)
	local o = {}

	--没有初始化对象的属性，对象属性应该在init函数中显示初始化
	--如果是子类，应该在自己的init函数中先调用父类的init函数

	self:AttachToClass(o)

	if o.__init__ then
		o:__init__(...)
	end
	return o
end

function clsObject:__init__()
	--nothing
end

function clsObject:IsClass()
	return true
end

function clsObject:Destroy()
	--所有对象释放的时候删除callout
	CALLOUT.RemoveAll(self)
end

function clsObject:Update( OldSelf )
	if not self.__SubClass then
		return
	end
	for _, Sub in pairs(self.__SubClass) do
		local OldSub = UTIL.Copy(Sub)
		for k, v in pairs(self) do
			if Sub[k] == OldSelf[k] then
				Sub[k] = self[k]
			end
		end
		Sub:Update(OldSub)
	end
end




--[[--
创建一个类
~~~ lua
-- 定义名为 Shape 的基础类
local Shape = class("Shape")
-- ctor() 是类的构造函数，在调用 Shape.new() 创建 Shape 对象实例时会自动执行
function Shape:ctor(shapeName)
    self.shapeName = shapeName
    printf("Shape:ctor(%s)", self.shapeName)
end
-- 为 Shape 定义个名为 draw() 的方法
function Shape:draw()
    printf("draw %s", self.shapeName)
end
--
-- Circle 是 Shape 的继承类
local Circle = class("Circle", Shape)
function Circle:ctor()
    -- 如果继承类覆盖了 ctor() 构造函数，那么必须手动调用父类构造函数
    -- 类名.super 可以访问指定类的父类
    Circle.super.ctor(self, "circle")
    self.radius = 100
end
function Circle:setRadius(radius)
    self.radius = radius
end
-- 覆盖父类的同名方法
function Circle:draw()
    printf("draw %s, raidus = %0.2f", self.shapeName, self.raidus)
end
--
local Rectangle = class("Rectangle", Shape)
function Rectangle:ctor()
    Rectangle.super.ctor(self, "rectangle")
end
--
local circle = Circle.new()             -- 输出: Shape:ctor(circle)
circle:setRaidus(200)
circle:draw()                           -- 输出: draw circle, radius = 200.00
local rectangle = Rectangle.new()       -- 输出: Shape:ctor(rectangle)
rectangle:draw()                        -- 输出: draw rectangle
~~~
### 高级用法
class() 除了定义纯 Lua 类之外，还可以从 C++ 对象继承类。
比如需要创建一个工具栏，并在添加按钮时自动排列已有的按钮，那么我们可以使用如下的代码：
~~~ lua
-- 从 CCNode 对象派生 Toolbar 类，该类具有 CCNode 的所有属性和行为
local Toolbar = class("Toolbar", function()
    return display.newNode() -- 返回一个 CCNode 对象
end)
-- 构造函数
function Toolbar:ctor()
    self.buttons = {} -- 用一个 table 来记录所有的按钮
end
-- 添加一个按钮，并且自动设置按钮位置
function Toolbar:addButton(button)
    -- 将按钮对象加入 table
    self.buttons[#self.buttons + 1] = button
    -- 添加按钮对象到 CCNode 中，以便显示该按钮
    -- 因为 Toolbar 是从 CCNode 继承的，所以可以使用 addChild() 方法
    self:addChild(button)
    -- 按照按钮数量，调整所有按钮的位置
    local x = 0
    for _, button in ipairs(self.buttons) do
        button:setPosition(x, 0)
        -- 依次排列按钮，每个按钮之间间隔 10 点
        x = x + button:getContentSize().width + 10
    end
end
~~~
class() 的这种用法让我们可以在 C++ 对象基础上任意扩展行为。
既然是继承，自然就可以覆盖 C++ 对象的方法：
~~~ lua
function Toolbar:setPosition(x, y)
    -- 由于在 Toolbar 继承类中覆盖了 CCNode 对象的 setPosition() 方法
    -- 所以我们要用以下形式才能调用到 CCNode 原本的 setPosition() 方法
    getmetatable(self).setPosition(self, x, y)
    printf("x = %0.2f, y = %0.2f", x, y)
end
~~~
**注意:** Lua 继承类覆盖的方法并不能从 C++ 调用到。也就是说通过 C++ 代码调用这个 CCNode 对象的 setPosition() 方法时，并不会执行我们在 Lua 中定义的 Toolbar:setPosition() 方法。
@param string classname 类名
@param [mixed super] 父类或者创建对象实例的函数
@return table

]]
function class(classname, super)
    local superType = type(super)
    local cls

    if superType ~= "function" and superType ~= "table" then
        superType = nil
        super = nil
    end

    if superType == "function" or (super and super.__ctype == 1) then
        -- inherited from native C++ Object
        cls = {}

        if superType == "table" then
            -- copy fields from super
            for k,v in pairs(super) do cls[k] = v end
            cls.__create = super.__create
            cls.super    = super
        else
            cls.__create = super
            cls.ctor = function() end
        end

        cls.__cname = classname
        cls.__ctype = 1

        function cls.new(...)
            local instance = cls.__create(...)
            -- copy fields from class to native object
            for k,v in pairs(cls) do instance[k] = v end
            instance.class = cls
            instance:ctor(...)
            return instance
        end

    else
        -- inherited from Lua Object
        if super then
            cls = {}
            setmetatable(cls, {__index = super})
            cls.super = super
        else
            cls = {ctor = function() end}
        end

        cls.__cname = classname
        cls.__ctype = 2 -- lua
        cls.__index = cls

        function cls.new(...)
            local instance = setmetatable({}, cls)
            instance.class = cls
            instance:ctor(...)
            return instance
        end
    end

    return cls
end


-- 提供假名以避免和 moonscript 发生冲突
function quick_class(classname, super)
  return class(classname, super)
end


--[[--
如果对象是指定类或其子类的实例，返回 true，否则返回 false
~~~ lua
local Animal = class("Animal")
local Duck = class("Duck", Animal)
print(iskindof(Duck.new(), "Animal")) -- 输出 true
~~~
@param mixed obj 要检查的对象
@param string classname 类名
@return boolean
]]
function iskindof(obj, classname)
    local t = type(obj)
    local mt
    if t == "table" then
        mt = getmetatable(obj)
    elseif t == "userdata" then
        mt = tolua.getpeer(obj)
    end

    while mt do
        if mt.__cname == classname then
            return true
        end
        mt = mt.super
    end

    return false
end


function asClass(obj , classname)
    return iskindof(obj , classname) and obj or nil
end

function implement( srcObj , interface )
    local function _copy(object)
        if type(object) ~= "table" then
            return object
        elseif srcObj[object] then
            return srcObj[object]
        end

        for key, value in pairs(object) do
            srcObj[_copy(key)] = _copy(value)
        end
    end
    _copy(interface)
end