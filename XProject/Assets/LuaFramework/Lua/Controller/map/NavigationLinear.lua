-- @Author: LiangZG
-- @Date:   2017-02-28 11:01:08
-- @Last Modified time: 2017-06-10 12:19:12
-- @Desc    寻路线路导航

local NavigationLinear = class("NavigationLinear")

function NavigationLinear:ctor( )
    self.cachedItems = {}
end

    -- 估算路线
function NavigationLinear:calLinearPath(corners , showDest)

    if not corners or corners.Length <= 0 then  return  end

     self.isMoveing = false
     self:popAllLinear()

     self.roleTrans = roleMgr.mainRole.transform
     self.mainRole = roleMgr.mainRole

     self.sceneMap.destPoint.gameObject:SetActive(showDest)
     --Debugger.Log("---------Corners length:" .. corners.Length)
     local sample = 2 --采样
    self.destWorldPos = corners[corners.Length - 1]
    local cornerCount = corners.Length - 2
    if cornerCount > sample then
        local newCorners = {}
        newCorners[#newCorners + 1] = corners[0]

        cornerCount = Mathf.ToInt(cornerCount / sample)
        for i=1,cornerCount do
            newCorners[#newCorners + 1] = corners[i * sample]
        end

        newCorners[#newCorners + 1] = corners[corners.Length - 1]
        corners = newCorners
        cornerCount = #corners - 1
    end

    self.items = {}
    for i = 1 , cornerCount  do

        local startPoint = corners[i]
        local localStartPoint = self.sceneMap:worldToMapPoint(startPoint)
        local endPoint = corners[i + 1]
        local localEndPoint = self.sceneMap:worldToMapPoint(endPoint);

        local dir = localEndPoint - localStartPoint;
        local distance = Vector3.Distance(localStartPoint, localEndPoint)

        local item = self:swapnItemGameObject();
        local angle = Mathf.Acos(Vector3.Dot(Vector3.right, dir.normalized)) * Mathf.Rad2Deg;

        local newPos = localStartPoint + (localEndPoint - localStartPoint) * 0.5
        item.transform.localPosition = newPos
        item.transform.localRotation = Quaternion.AngleAxis(dir.y < 0 and angle * -1 or angle, Vector3.forward);
        item.transform.localScale = Vector3.one;

        local sprite = item:GetComponent(typeof(UISprite))
        sprite.width = Mathf.ToInt(distance / 9) * 9
        sprite.height = 9
        sprite.pivot = UIWidget.Pivot.Right;

        local linear = NavigationLinear.Linear.new(item)
        linear.Dir = dir;
        linear.Length = distance
        linear.StartPoint = localStartPoint;
        linear.EndPoint = localEndPoint;

        self.items[#self.items + 1] = linear
    end

    self.curLinear = self:popLinear();
    self.isMoveing = true
end



function NavigationLinear:calPathLength( corners )
    local length = 0
    local cornerCount = corners.Length - 2
    for i=0,cornerCount do
        local dis = Vector3.Distance(corners[i] , corners[i + 1])
        length = length + dis
    end
    return length
end



function NavigationLinear:swapnItemGameObject()
    local gObj = nil
    if #self.cachedItems > 0 then

        gObj = self.cachedItems[1].mainSprite.cachedGameObject;
        table.remove(self.cachedItems , 1)
    else
        gObj = GameObject.Instantiate(self.sceneMap.linearItem.gameObject)
        gObj.transform:SetParent(self.root.parent);
    end

    gObj:SetActive(true);
    return gObj
end


function NavigationLinear:update( )


    local curPlayPos = self.sceneMap:worldToMapPoint(self.roleTrans.position)
    self.DobberTrans.localPosition = curPlayPos

    --屏蔽
    --self.sceneMap:lookatPlayer(self.DobberTrans)

    if not self.mainRole.move or not self.mainRole.move:InMoving() then   return  end

    if self.curLinear then
        self.curLinear:Offset(curPlayPos)

        if self.curLinear:IsOver() then
            self.curLinear = self:popLinear();
        end
    end
end

function NavigationLinear:popLinear()

    if self.curLinear then
        self.curLinear.mainSprite.pivot = UIWidget.Pivot.Center;
        self.curLinear.mainSprite.cachedGameObject:SetActive(false)
        table.insert(self.cachedItems , curLinear)
    end

    if #self.items <= 0 then
        -- Debugger.Log("---------NavigationLinear is Over !!!!")
        return nil
    end

    local linear = self.items[1];
    table.remove(self.items , 1)


    -- self.DobberTrans.localPosition = Vector3.new(linear.StartPoint.x , linear.StartPoint.y);
    -- self.DobberTrans.localRotation = linear.mainSprite.transform.localRotation;

    return linear;
end

function NavigationLinear:popAllLinear( )
    if self.items then
        -- print("items:" .. print_lua_table(self.items , 0 , 2 , true))
        for _,linear in pairs(self.items) do
            table.insert(self.cachedItems , linear)
            linear.mainSprite.gameObject:SetActive(false)
        end

        if self.curLinear and not self.curLinear:IsOver() then
            self.curLinear.mainSprite.gameObject:SetActive(false)
            table.insert(self.cachedItems , self.curLinear)
        end
    end
    self.items = {}
end

function NavigationLinear:intrrupt(  )
    self.isMoveing = false
    self:popAllLinear()
end


function NavigationLinear:setActive( active )
    for _,linear in pairs(self.items) do
        linear.mainSprite.gameObject:SetActive(active)
    end
end

NavigationLinear.Linear = class("NavigationLinear_Linear")


function NavigationLinear.Linear:ctor(mainObj)

    self.mainSprite = mainObj:GetComponent(typeof(UISprite));
    self.lastLength = self.mainSprite.width;
    self.isOver = false
end

function NavigationLinear.Linear:Offset(curMapPos)

    if self.isOver then return  end

    local dis = Vector3.Distance(curMapPos , self.EndPoint)
    local disStart = Vector3.Distance(curMapPos , self.StartPoint)
    self.mainSprite.width = Mathf.ToInt(dis)
    self.isOver = disStart > dis and disStart >= self.Length
    -- Debugger.Log("Length:" .. self.Length .. ",disStart:" .. disStart .. ",dis:" .. dis .. ", curMapPos:" .. tostring(curMapPos))

    if self.isOver then  self.mainSprite.cachedGameObject:SetActive(false) end
end

function NavigationLinear.Linear:IsOver()
     return self.isOver
end

return NavigationLinear
