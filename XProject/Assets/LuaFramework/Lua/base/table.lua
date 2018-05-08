

--[[--
计算表格包含的字段数量
Lua table 的 "#" 操作只对依次排序的数值下标数组有效，table.nums() 则计算 table 中所有不为 nil 的值的个数。
@param table t 要检查的表格
@return integer
]]
function table.nums(t)
    local count = 0
    for k, v in pairs(t) do
        count = count + 1
    end
    return count
end

--[[--
返回指定表格中的所有键
~~~ lua
local hashtable = {a = 1, b = 2, c = 3}
local keys = table.keys(hashtable)
-- keys = {"a", "b", "c"}
~~~
@param table hashtable 要检查的表格
@return table
]]
function table.keys(hashtable)
    local keys = {}
    for k, v in pairs(hashtable) do
        keys[#keys + 1] = k
    end
    return keys
end

--[[--
返回指定表格中的所有值
~~~ lua
local hashtable = {a = 1, b = 2, c = 3}
local values = table.values(hashtable)
-- values = {1, 2, 3}
~~~
@param table hashtable 要检查的表格
@return table
]]
function table.values(hashtable)
    local values = {}
    for k, v in pairs(hashtable) do
        values[#values + 1] = v
    end
    return values
end

--[[--
将来源表格中所有键及其值复制到目标表格对象中，如果存在同名键，则覆盖其值
~~~ lua
local dest = {a = 1, b = 2}
local src  = {c = 3, d = 4}
table.merge(dest, src)
-- dest = {a = 1, b = 2, c = 3, d = 4}
~~~
@param table dest 目标表格
@param table src 来源表格
]]
function table.merge(dest, src)
    for k, v in pairs(src) do
        dest[k] = v
    end
end

--[[--
在目标表格的指定位置插入来源表格，如果没有指定位置则连接两个表格
~~~ lua
local dest = {1, 2, 3}
local src  = {4, 5, 6}
table.insertto(dest, src)
-- dest = {1, 2, 3, 4, 5, 6}
dest = {1, 2, 3}
table.insertto(dest, src, 5)
-- dest = {1, 2, 3, nil, 4, 5, 6}
~~~
@param table dest 目标表格
@param table src 来源表格
@param [integer begin] 插入位置
]]
function table.insertto(dest, src, begin)
	begin = checkint(begin)
	if begin <= 0 then
		begin = #dest + 1
	end

	local len = #src
	for i = 0, len - 1 do
		dest[i + begin] = src[i + 1]
	end
end

--[[
从表格中查找指定值，返回其索引，如果没找到返回 false
~~~ lua
local array = {"a", "b", "c"}
print(table.indexof(array, "b")) -- 输出 2
~~~
@param table array 表格
@param mixed value 要查找的值
@param [integer begin] 起始索引值
@return integer
]]
function table.indexof(array, value, begin)
    for i = begin or 1, #array do
        if array[i] == value then return i end
    end
	return false
end

--[[--
从表格中查找指定值，返回其 key，如果没找到返回 nil
~~~ lua
local hashtable = {name = "dualface", comp = "chukong"}
print(table.keyof(hashtable, "chukong")) -- 输出 comp
~~~
@param table hashtable 表格
@param mixed value 要查找的值
@return string 该值对应的 key
]]
function table.keyof(hashtable, value)
    for k, v in pairs(hashtable) do
        if v == value then return k end
    end
    return nil
end

--[[--
从表格中删除指定值，返回删除的值的个数
~~~ lua
local array = {"a", "b", "c", "c"}
print(table.removebyvalue(array, "c", true)) -- 输出 2
~~~
@param table array 表格
@param mixed value 要删除的值
@param [boolean removeall] 是否删除所有相同的值
@return integer
]]
function table.removebyvalue(array, value, removeall)
    local c, i, max = 0, 1, #array
    while i <= max do
        if array[i] == value then
            table.remove(array, i)
            c = c + 1
            i = i - 1
            max = max - 1
            if not removeall then break end
        end
        i = i + 1
    end
    return c
end

--[[--
对表格中每一个值执行一次指定的函数，并用函数返回值更新表格内容
~~~ lua
local t = {name = "dualface", comp = "chukong"}
table.map(t, function(v, k)
    -- 在每一个值前后添加括号
    return "[" .. v .. "]"
end)
-- 输出修改后的表格内容
for k, v in pairs(t) do
    print(k, v)
end
-- 输出
-- name [dualface]
-- comp [chukong]
~~~
fn 参数指定的函数具有两个参数，并且返回一个值。原型如下：
~~~ lua
function map_function(value, key)
    return value
end
~~~
@param table t 表格
@param function fn 函数
]]
function table.map(t, fn)
    for k, v in pairs(t) do
        t[k] = fn(v, k)
    end
end

--[[--
对表格中每一个值执行一次指定的函数，但不改变表格内容
~~~ lua
local t = {name = "dualface", comp = "chukong"}
table.walk(t, function(v, k)
    -- 输出每一个值
    print(v)
end)
~~~
fn 参数指定的函数具有两个参数，没有返回值。原型如下：
~~~ lua
function map_function(value, key)
end
~~~
@param table t 表格
@param function fn 函数
]]
function table.walk(t, fn)
    for k,v in pairs(t) do
        fn(v, k)
    end
end

--[[--
对表格中每一个值执行一次指定的函数，如果该函数返回 false，则对应的值会从表格中删除
~~~ lua
local t = {name = "dualface", comp = "chukong"}
table.filter(t, function(v, k)
    return v ~= "dualface" -- 当值等于 dualface 时过滤掉该值
end)
-- 输出修改后的表格内容
for k, v in pairs(t) do
    print(k, v)
end
-- 输出
-- comp chukong
~~~
fn 参数指定的函数具有两个参数，并且返回一个 boolean 值。原型如下：
~~~ lua
function map_function(value, key)
    return true or false
end
~~~
@param table t 表格
@param function fn 函数
]]
function table.filter(t, fn)
    for k, v in pairs(t) do
        if not fn(v, k) then t[k] = nil end
    end
end

--[[--
遍历表格，确保其中的值唯一
~~~ lua
local t = {"a", "a", "b", "c"} -- 重复的 a 会被过滤掉
local n = table.unique(t)
for k, v in pairs(n) do
    print(v)
end
-- 输出
-- a
-- b
-- c
~~~
@param table t 表格
@return table 包含所有唯一值的新表格
]]
function table.unique(t)
    local check = {}
    local n = {}
    for k, v in pairs(t) do
        if not check[v] then
            n[k] = v
            check[v] = true
        end
    end
    return n
end

function table.deepcopy(src)
    if type(src) ~= "table" then
        return src
    end
    local cache = {}
    local function clone_table(t, level)
        if not level then
            level = 0
        end

        if level > 100 then
            return t
        end

        local k, v
        local rel = {}
        for k, v in pairs(t) do
            if type(v) == "table" then
                if cache[v] then
                    rel[k] = cache[v]
                else
                    rel[k] = clone_table(v, level+1)
                    cache[v] = rel[k]
                end
            else
                rel[k] = v
            end
        end
        setmetatable(rel, getmetatable(t))
        return rel
    end
    return clone_table(src)
end