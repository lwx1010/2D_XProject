-- @Author: LiangZG
-- @Date:   2017-03-01 21:30:56
-- @Last Modified time: 2017-03-02 17:21:05

ConfigUtils = {}

--检索数据配置，根据唯一属性字段查找，查找后的数据将在内部生成Key缓存记录
--再次查找相同数据时，就可以通过记录直接获取数据
function ConfigUtils.index( src , propertyName , matchData )

    local _cacheTab = src[propertyName] or {}

    local function _indexData(propertyName , matchData)

        if _cacheTab[matchData] then
            local key = _cacheTab[matchData]
            return src[key]
        end

        for key,data in pairs(src) do
            if type(data) == "table" and data[propertyName] == matchData then
                _cacheTab[matchData] = key
                return data
            end
        end

        return nil
    end


    src[propertyName] = _cacheTab

    return _indexData(propertyName , matchData)
end


function ConfigUtils.toVector3( vecStr )
    local vecArr = string.split(vecStr , ",")
    return Vector3.New(tonumber(vecArr[1]) , tonumber(vecArr[2]) , tonumber(vecArr[3]))
end



