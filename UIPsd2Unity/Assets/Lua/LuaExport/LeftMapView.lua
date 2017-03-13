-- @Author:
-- @Desc :

local LeftMapView = class("LeftMapView")
--self.gameObject
--self.transform

function LeftMapView:Awake()
    self.widgets = {
		{field="titleGrid",path="BiaoTi",src="Transform"},

    }
    LuaUIHelper.bind(self.gameObject , self)
end


function LeftMapView:Start()

end



function LeftMapView:OnClose()

end


function LeftMapView:OnDestroy()

end

return LeftMapView