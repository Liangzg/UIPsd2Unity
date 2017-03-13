-- @Author:
-- @Desc :

local SubPanelView = class("SubPanelView")
--self.gameObject
--self.transform

function SubPanelView:Awake()
    self.widgets = {

    }
    LuaUIHelper.bind(self.gameObject , self)
end


function SubPanelView:Start()

end



function SubPanelView:OnClose()

end


function SubPanelView:OnDestroy()

end

return SubPanelView