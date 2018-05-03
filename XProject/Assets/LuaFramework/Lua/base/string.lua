

string._htmlspecialchars_set = {}
string._htmlspecialchars_set["&"] = "&amp;"
string._htmlspecialchars_set["\""] = "&quot;"
string._htmlspecialchars_set["'"] = "&#039;"
string._htmlspecialchars_set["<"] = "&lt;"
string._htmlspecialchars_set[">"] = "&gt;"

--[[--
将特殊字符转为 HTML 转义符
~~~ lua
print(string.htmlspecialchars("<ABC>"))
-- 输出 &lt;ABC&gt;
~~~
@param string input 输入字符串
@return string 转换结果
]]
function string.htmlspecialchars(input)
    for k, v in pairs(string._htmlspecialchars_set) do
        input = string.gsub(input, k, v)
    end
    return input
end

--[[--
将 HTML 转义符还原为特殊字符，功能与 string.htmlspecialchars() 正好相反
~~~ lua
print(string.restorehtmlspecialchars("&lt;ABC&gt;"))
-- 输出 <ABC>
~~~
@param string input 输入字符串
@return string 转换结果
]]
function string.restorehtmlspecialchars(input)
    for k, v in pairs(string._htmlspecialchars_set) do
        input = string.gsub(input, v, k)
    end
    return input
end

--[[--
将字符串中的 \n 换行符转换为 HTML 标记
~~~ lua
print(string.nl2br("Hello\nWorld"))
-- 输出
-- Hello<br />World
~~~
@param string input 输入字符串
@return string 转换结果
]]
function string.nl2br(input)
    return string.gsub(input, "\n", "<br />")
end

--[[--
将字符串中的特殊字符和 \n 换行符转换为 HTML 转移符和标记
~~~ lua
print(string.nl2br("<Hello>\nWorld"))
-- 输出
-- &lt;Hello&gt;<br />World
~~~
@param string input 输入字符串
@return string 转换结果
]]
function string.text2html(input)
    input = string.gsub(input, "\t", "    ")
    input = string.htmlspecialchars(input)
    input = string.gsub(input, " ", "&nbsp;")
    input = string.nl2br(input)
    return input
end

--[[--
用指定字符或字符串分割输入字符串，返回包含分割结果的数组
~~~ lua
local input = "Hello,World"
local res = string.split(input, ",")
-- res = {"Hello", "World"}
local input = "Hello-+-World-+-Quick"
local res = string.split(input, "-+-")
-- res = {"Hello", "World", "Quick"}
~~~
@param string input 输入字符串
@param string delimiter 分割标记字符或字符串
@return array 包含分割结果的数组,起始下标index从1开始
]]
function string.split(input, delimiter)
    input = tostring(input)
    delimiter = tostring(delimiter)
    if (delimiter=='') then return false end
    local pos,arr = 0, {}
    -- for each divider found
    for st,sp in function() return string.find(input, delimiter, pos, true) end do
        table.insert(arr, string.sub(input, pos, st - 1))
        pos = sp + 1
    end
    table.insert(arr, string.sub(input, pos))
    return arr
end

--[[--
去除输入字符串头部的空白字符，返回结果
~~~ lua
local input = "  ABC"
print(string.ltrim(input))
-- 输出 ABC，输入字符串前面的两个空格被去掉了
~~~
空白字符包括：
-   空格
-   制表符 \t
-   换行符 \n
-   回到行首符 \r
@param string input 输入字符串
@return string 结果
@see string.rtrim, string.trim
]]
function string.ltrim(input)
    return string.gsub(input, "^[ \t\n\r]+", "")
end

--[[--
去除输入字符串尾部的空白字符，返回结果
~~~ lua
local input = "ABC  "
print(string.ltrim(input))
-- 输出 ABC，输入字符串最后的两个空格被去掉了
~~~
@param string input 输入字符串
@return string 结果
@see string.ltrim, string.trim
]]
function string.rtrim(input)
    return string.gsub(input, "[ \t\n\r]+$", "")
end

--[[--
去掉字符串首尾的空白字符，返回结果
@param string input 输入字符串
@return string 结果
@see string.ltrim, string.rtrim
]]
function string.trim(input)
    input = string.gsub(input, "^[ \t\n\r]+", "")
    return string.gsub(input, "[ \t\n\r]+$", "")
end

--[[--
将字符串的第一个字符转为大写，返回结果
~~~ lua
local input = "hello"
print(string.ucfirst(input))
-- 输出 Hello
~~~
@param string input 输入字符串
@return string 结果
]]
function string.ucfirst(input)
    return string.upper(string.sub(input, 1, 1)) .. string.sub(input, 2)
end

local function urlencodechar(char)
    return "%" .. string.format("%02X", string.byte(char))
end

--[[--
将字符串转换为符合 URL 传递要求的格式，并返回转换结果
~~~ lua
local input = "hello world"
print(string.urlencode(input))
-- 输出
-- hello%20world
~~~
@param string input 输入字符串
@return string 转换后的结果
@see string.urldecode
]]
function string.urlencode(input)
    -- convert line endings
    input = string.gsub(tostring(input), "\n", "\r\n")
    -- escape all characters but alphanumeric, '.' and '-'
    input = string.gsub(input, "([^%w%.%- ])", urlencodechar)
    -- convert spaces to "+" symbols
    return string.gsub(input, " ", "+")
end

--[[--
将 URL 中的特殊字符还原，并返回结果
~~~ lua
local input = "hello%20world"
print(string.urldecode(input))
-- 输出
-- hello world
~~~
@param string input 输入字符串
@return string 转换后的结果
@see string.urlencode
]]
function string.urldecode(input)
    input = string.gsub (input, "+", " ")
    input = string.gsub (input, "%%(%x%x)", function(h) return string.char(checknumber(h,16)) end)
    input = string.gsub (input, "\r\n", "\n")
    return input
end

--[[--
计算 UTF8 字符串的长度，每一个中文算一个字符
~~~ lua
local input = "你好World"
print(string.utf8len(input))
-- 输出 7
~~~
@param string input 输入字符串
@return integer 长度
]]
function string.utf8len(input)
    local len  = string.len(input)
    local left = len
    local cnt  = 0
    local arr  = {0, 0xc0, 0xe0, 0xf0, 0xf8, 0xfc}
    while left ~= 0 do
        local tmp = string.byte(input, -left)
        local i   = #arr
        while arr[i] do
            if tmp >= arr[i] then
                left = left - i
                break
            end
            i = i - 1
        end
        cnt = cnt + 1
    end
    return cnt
end

--[[--
将数值格式化为包含千分位分隔符的字符串
~~~ lua
print(string.formatnumberthousands(1924235))
-- 输出 1,924,235
~~~
@param number num 数值
@return string 格式化结果
]]
function string.formatnumberthousands(num)
    local formatted = tostring(checknumber(num))
    local k
    while true do
        formatted, k = string.gsub(formatted, "^(-?%d+)(%d%d%d)", '%1,%2')
        if k == 0 then break end
    end
    return formatted
end


--返回当前字符实际占用的字符数
local function SubStringGetByteCount(str , index)
    local curByte = string.byte(str , index)
    local byteCount = 1
    if curByte == nil then
        byteCount = 0
    elseif curByte > 0 and curByte <= 127 then
        byteCount = 1
    elseif curByte >= 192 and curByte <= 223 then
        byteCount = 2
    elseif curByte >= 224 and curByte <= 239 then
        byteCount = 3
    elseif curByte >= 240 and curByte <= 247 then
        byteCount = 4
    end
    return byteCount
end

--获取中英混合UTF8 字符串的真实符数量
local function SubStringGetTotalIndex(str)
    local curIndex = 0
    local i = 1
    local lastCount = 1
    repeat
        lastCount = SubStringGetByteCount(str , i)
        i = i + lastCount
        curIndex = curIndex + 1
    until(lastCount == 0)
    return curIndex - 1
end

--获取中英混合UTF8 字符串的真实符数量
local function SubStringGetTrueIndex(str , index)
    local curIndex = 0
    local i = 1
    local lastCount = 1
    repeat
        lastCount = SubStringGetByteCount(str , i)
        i = i + lastCount
        curIndex = curIndex + 1
    until(curIndex >= index)
    return i - lastCount
end


--[[
字符串截取 索引从1开始截取
print(string.subString('字符串123截取') , 4)
--输出 123截取
print(string.subString('字符串123截取') , 4 , 7)
--输出 123截
@str  字符串
@startIndex  开始截取索引
@endIndex   结束索引 （可有可无）
]]
function string.subString(str , startIndex , endIndex)
    if startIndex < 0 then
        startIndex = SubStringGetTotalIndex(str) + startIndex + 1
    end

    if endIndex ~= nil and endIndex < 0 then
        endIndex = SubStringGetTotalIndex(str) + endIndex + 1
    end

    if endIndex == nil then
        return string.sub(str , SubStringGetTrueIndex(str , startIndex))
    else
        return string.sub(str , SubStringGetTrueIndex(str , startIndex) , SubStringGetTrueIndex(str , endIndex + 1) -1)
    end
end

--[[
字符串是否为空
]]
function string.isEmptyOrNil(str)
    if str then
        str = string.trim(str)
        return string.len(str) <= 0
    end
    return true
end

