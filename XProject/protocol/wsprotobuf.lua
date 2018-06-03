local skynet = require "skynet"
local sharedata = require "skynet.sharedata"
local table = table
local tconcat = table.concat
local tinsert = table.insert
local string = string
local spack = string.pack
local sunpack = string.unpack
local pairs = pairs
local ipairs = ipairs
local type = type
local error = error

local M = {}
local PROTO = nil
dofile "base/extend.lua"

skynet.init(function()
    PROTO = sharedata.query("pb_data").pb_data
end)

local function _encode(protoName, protoTbl)
    if not PROTO[protoName] then
        error("encode not protoName:" .. protoName)
    end
    -- skynet.error("---:", protoName, sys.dump(protoTbl))
    local tbl = {}
    for _no, _data in ipairs(PROTO[protoName]) do
        if _data.sType == "required" then
        	local d = protoTbl[_data.attrName]
        	if not d then
        		local tips = string.format("encode protocal:%s attr:%s empty error, attr is required", protoName, _data.attrName)
        		error(tips)
        	end
            if _data.aType == "int32" then
                tinsert(tbl, spack(">i4", d))
            elseif _data.aType == "int64" then
                tinsert(tbl, spack(">i8", d))
            elseif _data.aType == "string" then
                tinsert(tbl, spack(">s2", d))
            elseif _data.aType == "float" then
                tinsert(tbl, spack(">f", d))
            else
                tinsert(tbl, _encode(_data.aType, d))
            end
        elseif _data.sType == "optional" then
            local d = protoTbl[_data.attrName]
            if d then
                if _data.aType == "int32" then
                    tinsert(tbl, spack(">i4", protoTbl[_data.attrName]))
                elseif _data.aType == "int64" then
                    tinsert(tbl, spack(">i8", protoTbl[_data.attrName]))
                elseif _data.aType == "string" then
                    tinsert(tbl, spack(">s2", protoTbl[_data.attrName]))
                elseif _data.aType == "float" then
                    tinsert(tbl, spack(">f", protoTbl[_data.attrName]))
                else
                    tinsert(tbl, _encode(_data.aType, protoTbl[_data.attrName]))
                end
            else
                if _data.aType == "int32" then
                    tinsert(tbl, spack(">i4", 0))
                elseif _data.aType == "int64" then
                    tinsert(tbl, spack(">i8", 0))
                elseif _data.aType == "string" then
                    tinsert(tbl, spack(">s2", ""))
                elseif _data.aType == "float" then
                    tinsert(tbl, spack(">f", 0))
                else
                    error("optional can not be message struct")
                end             
            end
        elseif _data.sType == "repeated" then
        	local d = protoTbl[_data.attrName]
        	if not d then
        		local tips = string.format("encode protocal:%s attr:%s empty error, attr is repeated", protoName, _data.attrName)
        		error(tips)        		
        	end
            local list = {}
            tinsert(list, spack(">i2", #d))
            if _data.aType == "int32" then
                for _i, _d in ipairs(d) do
                    tinsert(list, spack(">i4", _d))
                end
                tinsert(tbl, tconcat(list))
            elseif _data.aType == "int64" then
                for _i, _d in ipairs(d) do
                    tinsert(list, spack(">i8", _d))
                end
            elseif _data.aType == "string" then
                for _i, _d in ipairs(d) do
                    tinsert(list, spack(">s2", _d))
                end
            elseif _data.aType == "float" then
                for _i, _d in ipairs(d) do
                    tinsert(list, spack(">f", _d))
                end
            else
                for _i, _d in ipairs(d) do
                    tinsert(list, _encode(_data.aType, _d))
                end
            end
            tinsert(tbl, tconcat(list))
        else
            error("not sType:" .. _data.sType)
        end
    end
    local enS = tconcat(tbl)
    -- local dumpStr = ""
    -- for i = 1, #enS do
    -- 	dumpStr = dumpStr .. string.byte(enS, i) .. " "
    -- end
    -- skynet.error("encode protocol:", protoName, dumpStr)
    return enS
end

-- 如果有回调函数则使用回调函数
-- func(encodeString, ...)
function M.encode(protoName, protoTbl, func, ...)
    if func then
        if type(func) ~= "function" then
            error("third param not function")
        end
        local enS = _encode(protoName, protoTbl)
        return func(enS, ...)
    else
        return _encode(protoName, protoTbl)
    end
end

local function _decode(protoName, encodeData, sPos)
    if not PROTO[protoName] then
        error("decode not protoName:" .. protoName)
    end 

    local tbl = {}
    local d = nil
    for _no, _data in ipairs(PROTO[protoName]) do
        if _data.sType == "required" or _data.sType == "optional" then
            if _data.aType == "int32" then
                d, sPos = sunpack(">i4", encodeData, sPos)
            elseif _data.aType == "int64" then
                d, sPos = sunpack(">i8", encodeData, sPos)
            elseif _data.aType == "string" then
                d, sPos = sunpack(">s2", encodeData, sPos)
            elseif _data.aType == "float" then
                d, sPos = sunpack(">f", encodeData, sPos)
            else
                d, sPos = _decode(_data.aType, encodeData, sPos)
            end
            tbl[_data.attrName] = d
        elseif _data.sType == "repeated" then
            local list = {}
            local cnt = nil
            cnt, sPos = sunpack(">i2", encodeData, sPos)
            if _data.aType == "int32" then
                for i = 1, cnt do
                    d, sPos = sunpack(">i4", encodeData, sPos)
                    tinsert(list, d)
                end
            elseif _data.aType == "int64" then
                for i = 1, cnt do
                    d, sPos = sunpack(">i8", encodeData, sPos)
                    tinsert(list, d)
                end
            elseif _data.aType == "string" then
                for i = 1, cnt do
                    d, sPos = sunpack(">s2", encodeData, sPos)
                    tinsert(list, d)
                end
            elseif _data.aType == "float" then
                for i = 1, cnt do
                    d, sPos = sunpack(">f", encodeData, sPos)
                    tinsert(list, d)
                end
            else
                for i = 1, cnt do
                    d, sPos = _decode(_data.aType, encodeData, sPos)
                    tinsert(list, d)
                end
            end
            tbl[_data.attrName] = list
        else
            error("not sType:" .. _data.sType)
        end
    end
    return tbl, sPos
end

-- 分2种情况, 一个是字符串, 一个是指针+sz
function M.decode(protoName, encodeData, size)
    if type(encodeData) == "userdata" then
        encodeData = skynet.tostring(encodeData, size)
    end
    local tbl, sPos = _decode(protoName, encodeData, 1)
    return tbl
end

return M