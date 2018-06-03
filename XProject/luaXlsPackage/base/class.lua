
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
