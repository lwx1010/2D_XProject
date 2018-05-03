-- @Author: LiangZG
-- @Date:   2017-08-12 16:49:14
-- @Last Modified time: 2017-08-12 10:36:05

local Collection = class("Collection")

function Collection:ctor()
    self._arr = {}
    self._length= 0
end


--- 添加item
-- @param item 
-- @param finish 添加完成，如果存在回调
function Collection:add(item)
    
    self._length = self._length + 1
    self._arr[self._length] = item

    if self.addDelegate then
        self.addDelegate(item)
    else
        print("add delegate is nil")
    end
end


function Collection:insert(item , index)
    
    self._length = self._length + 1
    table.insert(self._arr , index + 1 , item)

    if self.addDelegate then
        self.addDelegate(item)
    end
end

function Collection:remove(item)
    for i , v in ipairs(self._arr) do
        if v == item then
            table.remove(self._arr  , i)
            self._length = self._length - 1

            if self.removeDelegate then
                self.removeDelegate(v)
            end
            return 
        end
    end

end

function Collection:removeAt(index)
    if index < 0 or index >= self._length then return end
    
    local item = self._arr[index + 1]
    table.remove(self._arr , index + 1) 
    self._length = self._length - 1
    
    if self.removeDelegate then
        self.removeDelegate(item)
    end
end


function Collection:clear()
    self._arr = {}
    self._length = 0

    if self.clearDelegate then
        self.clearDelegate()
    end
end

--- 清除所有外部委托
function Collection:clearDelegates()
    self.addDelegate = nil
    self.removeDelegate = nil
    self.clearDelegate = nil
    self.onValueChanged = nil
end

function Collection:count()
    return self._length
end

--- 获取指定索引的元素，下标从[0 , Count - 1]
function Collection:get(index)
    if index >= self._length then return nil end
    
    return self._arr[index + 1]
end

--- 获取容器内的所有元素
function Collection:values()
    return self._arr
end

--- 刷新GUI
function Collection:reflush()
    if not self.onValueChanged then return end

    self.onValueChanged()
end

return Collection
