-- @Author: LiangZG
-- @Date:   2017-08-12 16:49:14
-- @Last Modified time: 2017-08-12 10:36:05
-- @Desc 数据Model基础父类

local ModelCell = class("ModelCell")

function ModelCell:ctor(viewComponent)
    self.view = viewComponent.new()
    self.view.dataValue = self
end


function ModelCell:onObjectChanged(go)
    if not self.view then
       Debugger.LogWarning("ViewComponent is nil !")
       return
    end

    self.view:Awake(go)
end

return ModelCell
