require "base.class"

_require = require

require = function (file)
    if file == "Logic/Network" then
        -- error("can't require Network")
        return
    end
    return _require(file)
end

local function FileInfo()
    --对严重错误日志记录文件及line信息
    local dinfo = debug.getinfo(3, 'Sl')
    local CallFile = dinfo.short_src
    local CurLine = dinfo.currentline
    return CallFile.." line:"..CurLine
end

--输出日志--
function log(str)
    Util.Log(str);
end

--错误日志--
function logError(str)
    print(debug.traceback())
    Util.LogError(str);
end

_print = print

function print(...)
    _print(FileInfo(),...)
end

function error( errmsg )
    logError(errmsg or "")
end

--警告日志--
function logWarn(str)
    --print(debug.traceback())
	Util.LogWarning(str);
end

--查找对象--
function find(str)
	return GameObject.Find(str);
end

function destroy(obj)
    if obj == nil then 
        return
    end
	GameObject.Destroy(obj);
end

function newObject(prefab, depth)
    local go = GameObject.Instantiate(prefab)
    if depth and depth >= 0 then
        local baseDepth = 0
        local widget = go.transform:GetComponent('UIWidget')
        if widget then
            baseDepth = widget.depth
            widget.depth = depth
        end
        local count = go.transform.childCount
        for i=1, count do
            widget = go.transform:GetChild(i-1):GetComponent('UIWidget')
            if widget then
                if baseDepth == 0 then
                    baseDepth = widget.depth
                end
                widget.depth = widget.depth + depth - baseDepth
            end
        end
    end
	return go;
end

--创建面板--
function createPanel(path, weight, callback)
	return panelMgr:CreatePanel(path, weight, callback)
end

function child(str)
	return transform:Find(str);
end

function subGet(childNode, typeName)
	return child(childNode):GetComponent(typeName);
end

function findPanel(str)
	local obj = find(str);
	if obj == nil then
		error(str.." is null");
		return nil;
	end
	return obj:GetComponent("BaseLua");
end

function MultiString(s,n)
    local r=""
    for i=1,n do
      r=r..s
    end
    return r
end
--o ,obj;b use [];n \n;t num \t;
-- function TableToString(o,n,b,t)
--     if type(b) ~= "boolean" and b ~= nil then
--         print("expected third argument %s is a boolean", tostring(b))
--     end
--     if(b==nil)then b=true end
--     t=t or 1
--     local s=""
--     if type(o) == "number" or
--         type(o) == "function" or
--         type(o) == "boolean" or
--         type(o) == "nil" then
--         s = s..tostring(o)
--     elseif type(o) == "string" then
--         s = s..string.format("%q",o)
--     elseif type(o) == "table" then
--         s = s.."{"
--         if(n)then
--             s = s.."\n"..MultiString("  ",t)
--         end
--         for k,v in pairs(o) do
--             if b then
--                 s = s.."["
--             end
--             s = s..TableToString(k,n, b,t+1)
--             if b then
--                 s = s.."]"
--             end
--             s = s.." = "
--             s = s..TableToString(v,n, b,t+1)
--             s = s..","
--             if(n)then
--                 s=s.."\n"..MultiString("  ",t)
--             end
--         end
--         s = s.."}"
--     end
--     return s;
-- end

--[[
@value [table] 必须参数  
@desciption [string] [非必要参数] 描述标记 
@nesting [number] [非必要参数] table最大嵌套 若不设置 默认全打印
]]
function TableToString(value, desciption, nesting)
    local table_format = string.format
    local string_len = string.len
    local string_rep = string.rep
    local string_sub = string.sub
    local string_gsub = string.gsub
    local string_find = string.find
    local table_insert = table.insert
    -- local debug_traceback = debug.traceback
    local lookup = {}
    local result = {}

    local function trim(input)
        input = string_gsub(input, "^[ \t\n\r]+", "")
        return string_gsub(input, "[ \t\n\r]+$", "")
    end
    local function split(input, delimiter)
        input = tostring(input)
        delimiter = tostring(delimiter)
        if (delimiter=='') then return false end
        local pos,arr = 0, {}
        for st,sp in function() return string_find(input, delimiter, pos, true) end do
            table_insert(arr, string_sub(input, pos, st - 1))
            pos = sp + 1
        end
        table_insert(arr, string_sub(input, pos))
        return arr
    end
    -- local traceback = split(debug_traceback("", 2), "\n")
    -- local logStr = "dump from: " .. trim(traceback[3].."\n")
    local logStr = ""
    local function _dump_value(v)
        if type(v) == "string" then
            v = "\"" .. v .. "\""
        end
        return tostring(v)
    end

    local function _dump(value, desciption, indent, nest, keylen)
        desciption = desciption or "<log>"
        local spc = ""
        if type(keylen) == "number" then
            spc = string_rep(" ", keylen - string_len(_dump_value(desciption)))
        end
        if type(value) ~= "table" then
            result[#result +1 ] = table_format("%s%s%s = %s", indent, _dump_value(desciption), spc, _dump_value(value))
        elseif lookup[tostring(value)] then
            -- result[#result +1 ] = table_format("%s%s%s = *REF*", indent, _dump_value(desciption), spc)
            result[#result +1 ] = table_format("%s%s%s = *REF*===%s", indent, _dump_value(desciption), spc,_dump_value(value))
        else
            lookup[tostring(value)] = true
            if nesting and nest > nesting then
                result[#result +1 ] = table_format("%s%s = *MAX NESTING*", indent, _dump_value(desciption))
            else
                result[#result +1 ] = table_format("%s%s = {", indent, _dump_value(desciption))
                local indent2 = indent.."    "
                local keys = {}
                local keylen = 0
                local values = {}
                for k, v in pairs(value) do
                    keys[#keys + 1] = k
                    local vk = _dump_value(k)
                    local vkl = string_len(vk)
                    if vkl > keylen then keylen = vkl end
                    values[k] = v
                end
                table.sort(keys, function(a, b)
                    if type(a) == "number" and type(b) == "number" then
                        return a < b
                    else
                        return tostring(a) < tostring(b)
                    end
                end)
                for i, k in ipairs(keys) do
                    _dump(values[k], k, indent2, nest + 1, keylen)
                end
                result[#result +1] = table_format("%s}", indent)
            end
        end
    end
    _dump(value, desciption, "- ", 1)
    
    for i, line in ipairs(result) do
        logStr = logStr .."\n" .. line
    end

    return logStr
end

function internale(lua_table,indent,depth,isLimit)
  local str = {}
  local prefix = string.rep("    ", indent)
  table.insert(str,"\n"..prefix.."{\n")
  for k, v in pairs(lua_table) do
      if type(k) == "string" then
          k = string.format("%q", k)
      else
          k = string.format("%s", tostring(k))
      end

      local szSuffix = ""

      if type(v) == "string" then
          szSuffix = string.format("%q", v)
      elseif type(v) == "number" or type(v) == "userdata" then
          szSuffix = tostring(v)
      elseif type(v) == "table" then
          if isLimit then
              if (depth > 0) then
                  szSuffix = internale(v,indent + 1,depth - 1,isLimit)
              else
                  szSuffix = tostring(v)
              end
          else

              szSuffix = print_lua_table(v,indent + 1,depth - 1)
          end
      else
         szSuffix = tostring(v)
      end

      local szPrefix = string.rep("    ", indent+1)
      table.insert(str,szPrefix.."["..k.."]".." = "..szSuffix..",\n")
   end

   table.insert(str,prefix.."}\n")
   return table.concat(str, '')
end

--輸出信息，缩进，遍历深度,不设置上限
function print_lua_table(lua_table, indent,depth,isLimit)
    indent = indent or 0
    depth = depth or 1
    isLimit = isLimit or false
    local str = ""

    if lua_table == nil then
        str = tostring(lua_table)
    end

    if type(lua_table) == "string" then
        str = string.format("%q", lua_table)
    end

    if type(lua_table) == "userdata" or type(lua_table) == "number" or type(lua_table) == "function" or type(lua_table) == "boolean" then
        str = tostring(lua_table)
    end

    if type(lua_table) == "table" then
        str = internale(lua_table,indent,depth,isLimit)
    end

    return str
end

--将一个str以del分割为若干个table中的元素
--n为分割次数
function string.split( line, sep, maxsplit )
    if not line or string.len(line) == 0 then
        return {}
    end

    sep = sep or ' '
    maxsplit = maxsplit or 0
    local retval = {}
    local pos = 1
    local step = 0
    while true do
        local from, to = string.find(line, sep, pos, true)
        step = step + 1
        if (maxsplit ~= 0 and step > maxsplit) or from == nil then
            local item = string.sub(line, pos)
            table.insert( retval, item )
            break
        else
            local item = string.sub(line, pos, from-1)
            table.insert( retval, item )
            pos = to + 1
        end
    end
    return retval
end

--返回12:12:21
function timeToString(time)
    time = math.ceil(time)
    local h = math.floor(time/3600)
    local m = math.fmod(math.floor(time/60), 60)
    local s = math.fmod(time, 60)
    return string.format("%02d:%02d:%02d", h,m,s)
end

--返回 12:12(分秒)
function timeToString1(time)
    time = math.floor(time)
    local h = math.floor(time/3600)
    local m = math.fmod(math.floor(time/60), 60)
    local s = math.fmod(time, 60)
    return string.format("%02d:%02d", m,s)
end
--返回 12:12(时分)
function timeToString2(time)
    time = math.ceil(time)
    local h = math.floor(time/3600)
    local m = math.fmod(math.floor(time/60), 60)
    return string.format("%02d小时%02d分", h,m)
end

--返回--%d天
function timeToString3(time)
    time = math.ceil(time)
    local day = math.floor(time/86400)
    return string.format("%d天%s", day, timeToString(time - day*86400))
end

--返回 12(时) or 60(分)
function timeToString4(time)
    time = math.ceil(time)
    local h = math.floor(time/3600)
    local m = math.fmod(math.floor(time/60), 60)
    if h ~= 0 then
        return string.format("%d小时", h)
    else
        return string.format("%d分钟", m)
    end
end

--返回 12:12(时分)
function timeToString5(time)
    time = math.ceil(time)
    local h = math.floor(time/3600)
    local m = math.fmod(math.floor(time/60), 60)
    if h ~= 0 then
        return string.format("%d小时%d分", h,m)
    else
        return string.format("%d分钟", m)
    end
end

--返回 1分20秒or10秒
function timeToString6(time)
    time = math.ceil(time)
    local m = math.fmod(math.floor(time/60), 60)
    local s = math.fmod(time, 60)
    if m == 0 then
        return string.format("%d%s", s, LANGUAGE_TIP.second)
    elseif s == 0 then
        return string.format("%d%s", m, LANGUAGE_TIP.minute)
    else
        return string.format("%d%s%d%s", m, LANGUAGE_TIP.minute, s, LANGUAGE_TIP.second)
    end
end

function isInTable(tbl, item )
    for k, v in pairs(tbl) do
        if v == item then
            return k
        end
    end

    return nil
end

function numberConvert( num )
    if num >= 10000 then
        return string.format("%d%s", math.floor(num/10000), LANGUAGE_TIP.wan)
    end

    return num
end

function numberConvert1( num )
    if num < 100000 then
        return tostring(num)
    elseif num < 1000000000 then
        return string.format("%d%s", math.floor(num/10000), LANGUAGE_TIP.wan)
    else
        return string.format("%d%s", math.floor(num/100000000), LANGUAGE_TIP.yi)
    end

    return num
end

function numberConvert2( num )
    if num >= 1000000 then
        return string.format("%d%s", math.floor(num/10000), LANGUAGE_TIP.wan)
    end

    return num
end
--xx天xx小时xx分
function ConvertToEndTimeString(seconds)
   local d = math.floor(seconds / (24*3600))
    local h = d > 0 and  math.floor((seconds - (d*24*3600)) / 3600) or  math.floor(seconds / 3600)
    local m = d > 0 and  math.floor((seconds - (d*24*3600)) / 60 % 60) or math.floor(seconds / 60 % 60)
    m = m == 0 and 1 or m
    local str = d == 0 and "" or d..LANGUAGE_TIP.day
    return string.format("%s%s%s" , str , h..LANGUAGE_TIP.hour , m..LANGUAGE_TIP.minute)
end

function ConvertToBeforeTimeString(seconds)
    if seconds/3600/24 > 1 then
        return string.format("%s%s%s", math.floor(seconds/3600/24) > 7 and 7 or math.floor(seconds/3600/24), LANGUAGE_TIP.day, LANGUAGE_TIP.before)
    elseif seconds/3600 > 1 then
        return string.format("%s%s%s", math.floor(seconds/3600), LANGUAGE_TIP.hour, LANGUAGE_TIP.before)
    elseif seconds/60 > 1 then
        return string.format("%s%s%s", math.floor(seconds/60), LANGUAGE_TIP.minute, LANGUAGE_TIP.before)
    else
        return string.format("%s%s%s", math.floor(seconds), LANGUAGE_TIP.second, LANGUAGE_TIP.before)
    end
end

function ConvertToTimeString(seconds)
    local str = ''
    if seconds/3600 > 1 then
        str = string.format("%s%s", math.floor(seconds/3600), LANGUAGE_TIP.hour)
    elseif seconds/60 > 1 then
        str = str ..  string.format("%s%s", math.floor(seconds/60), LANGUAGE_TIP.minute)
    else
        str = str .. string.format("%s%s", math.floor(seconds), LANGUAGE_TIP.second)
    end
    return str
end

function ConvertToTimeString2(seconds)
    local str = ''
    if seconds/3600 > 1 then
        str = string.format("%s%s", math.floor(seconds/3600), LANGUAGE_TIP.hour2)
    end

    local minSeconds = seconds - math.floor(seconds/3600)*3600
    if minSeconds/60 > 1 then
        str = str ..  string.format("%s%s", math.floor(minSeconds/60), LANGUAGE_TIP.min)
    end

    local sceSeconds = minSeconds - math.floor(minSeconds/60)*60
    str = str .. string.format("%s%s", math.floor(sceSeconds), LANGUAGE_TIP.second)
    return str
end


function GetStringWordNum(str)
    local fontSize = 20
    local str = string.gsub(str, "%[%-%]", "")
    local i, j = string.find(str, "%[url=")
    if i and j then
        local k = string.find(str, "%]", j+1)
        local tmpstr = string.sub(str, 1, i-1) .. string.sub(str, k+1)
        str = tmpstr
    end
    str = string.gsub(str, "%[/url%]", "")
    local lenInByte = #str
    local count = 0
    i = 1
    while true do
        local curByte = string.byte(str, i)
        if i > lenInByte then
            break
        end
        local byteCount = 1
        if curByte > 0 and curByte < 128 then
            byteCount = 1
        elseif curByte>=128 and curByte<224 then
            byteCount = 2
        elseif curByte>=224 and curByte<240 then
            byteCount = 3
        elseif curByte>=240 and curByte<=247 then
            byteCount = 4
        else
            break
        end
        -- local char = string.sub(str, i, i+byteCount-1)
        i = i + byteCount
        count = count + 1
    end
    return count
end

--unity 对象判断为空, 如果你有些对象是在c#删掉了，lua 不知道
--判断这种对象为空时可以用下面这个函数。
function IsNil(uobj)
    return uobj == nil or uobj:Equals(nil)
end

--xx天xx小时xx分xx秒
function ConvertToEndTimeString1(time)
    local d = math.floor(time / (24*3600))
    local h = d > 0 and  math.floor((time - (d*24*3600)) / 3600) or  math.floor(time / 3600)
    local m = d > 0 and  math.floor((time - (d*24*3600)) / 60 % 60) or math.floor(time / 60 % 60)
    m = m == 0 and 1 or m
    local s = math.floor(math.fmod(time, 60))
    local str = d == 0 and "" or d..LANGUAGE_TIP.day
    return string.format("%s%s%s%s" , str , h..LANGUAGE_TIP.hour , m..LANGUAGE_TIP.minute , s..LANGUAGE_TIP.second)
end

--格式化配置
local itemFormat = {
    --item=道具no=数量=是否绑定=限时=男女
    [1] = "id",
    [2] = "count",
    [3] = "bind",
    [4] = "timeLimit",
    [5] = "sex",
}

function XlsSplitToTable(xlsStr)
    
    local data = {}
    local tmp = string.split(xlsStr,"=")
    local format = nil
    if tmp[1] == "item" then
        format = itemFormat
        table.remove(tmp,1)
    end
    if format then
        for k,v in ipairs(tmp) do 
            data[format[k]] = v
        end
    end

    return data
end