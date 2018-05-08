-- @Author: LiangZG
-- @Date:   2017-03-02 10:46:26
-- @Last Modified time: 2018-02-03 16:46:34
-- @Desc 对象缓存池

GoPool = {}

function GoPool.reset( goTab )
	-- Debugger.Log(print_lua_table(goTab))
	if goTab then
		goTab.prefab = nil
		goTab._hides = goTab._hides or {}
        goTab._shows = goTab._shows or {}

        for _,go in ipairs(goTab._shows) do
            if go and go ~= nil then
                goTab._hides[#goTab._hides + 1] = go
                go:SetActive(false)
            end
        end

        goTab._shows = {}
    end

    -- Debugger.Log("reset : " .. print_lua_table(goTab))
    return goTab or {}
end

--获取一个GameObject的实例，如果没有缓存对象就创建新实例
--@goTab  table  缓存对象
--@prefab  GameObject  实例对象
--@parent  Transform  父结点
function GoPool.swapnGameObject( goTab , prefab , parent)
	goTab.prefab = goTab.prefab or prefab

	if goTab.prefab ~= prefab then
		Debugger.LogError("缓存池Prefab不匹配!")
	end

	goTab._hides = goTab._hides or {}
    goTab._shows = goTab._shows or {}

    local child = nil
	if #goTab._hides > 0 then
		child = goTab._hides[1]
		table.remove(goTab._hides , 1)
    else
        child = newObject(prefab)
    end

    GoPool.addChild(child , parent)
    goTab._shows[#goTab._shows + 1] = child

    return child
end

--- 添加一个子结点
-- @param go  GameObject 子结点
-- @param parent Transform  父结点
function GoPool.addChild( go , parent )
    if not parent then  return   end

    go:SetActive(true)
    go.transform:SetParent(parent)

    go.transform.localPosition = Vector3.zero
    go.transform.localScale = Vector3.one
    go.transform.localRotation = Quaternion.identity;
    go.layer = parent.gameObject.layer
end


function GoPool.remove(pool , gObj)

	if not pool then  return   end

	for i,go in ipairs(pool._shows) do
        if go == gObj then
            pool._hides[#pool._hides + 1] = go
            go:SetActive(false)
            table.remove(pool._shows , i);
            return
        end
    end

end


function GoPool.getList( goTab )
    return goTab._shows
end

function GoPool.count( goTab )
    if not goTab then return 0 end

    return goTab._shows and #goTab._shows or 0
end

function GoPool.clear( goTab )
    if not goTab then   return  end

    if goTab._shows then
        for _,go in ipairs(goTab._shows) do
            if  go and go ~= nil  then
                GameObject.Destroy(go)
            end
        end
        goTab._shows = nil
    end

    if goTab._hides then
        for _,go in ipairs(goTab._hides) do
            if  go and go ~= nil  then
                GameObject.Destroy(go)
            end
        end
        goTab._hides = nil
	end

	goTab.prefab = nil
end
