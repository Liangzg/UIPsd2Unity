TestView = {};
local this = TestView

local gameObject
local transform

--由LuaBehaviour自动调用
function TestView.Awake(obj)
	gameObject = obj;
	transform = obj.transform;

    this.OnInit()
end

--由LuaBehaviour自动调用
function TestView.Start()

    print("Transform:" .. this.Zu5.gameObject.name)
    print("button tweenTarget:" .. this.item_player.tweenTarget.name)
    this.item_player:luaButton()

    print("Label text:" .. this.itemLab.text)
    print("Label gameObject:" .. this.itemLab.gameObject.name)

    print("Sprite width :" .. this.iconRun.width .. ",iconName:" .. this.iconRun.spriteName)
    print("UIPanel gameObject name :" .. this.leftMapPanel.gameObject.name .. ", clipping:" .. tostring(this.leftMapPanel.clipping))

    print("UITable childCount:" .. this.monsterTable:GetChildList().Count)
    print("UIGrid childCount:" .. this.titleGrid:GetChildList().Count)
end

--由LuaBehaviour自动调用
function TestView.OnInit()
    this.widgets = {
       {field="item_player",path="DiTu.item_player",src=LuaButton, onClick = function ()  print("Lua Button OnClick")  end },
	   {field="Zu5",path="DiTu.Zu5",src = LuaTransform},
	   {field="itemLab",path="DiTu.Zu5.JiangHuHunHun（87Ji）",src=LuaText},
	-- {field="btnClose",path="DiTu.btnClose",src=LuaButton, onclick = function (btn)  --[===[todo click]===]  end },
	   {field="curScene",path="DiTu.rightTab.currentScene",src=LuaToggle , onChange = function () this.onToggleChange() end},
	-- {field="worldMapBtn",path="DiTu.rightTab.worldMap",src=LuaToggle , onChange = function (toggel) --[===[todo toggle.onchange]===] end},

    }

    LuaUIHelper.bind(gameObject , TestView)
end


function TestView.onToggleChange(  )
    print("Listener toggel onChange")
end

function TestView.onInputChange()
    print("------>onInputChange: ".. this.shareMapInput.value)
end

function TestView.onInputSubmit( )
    print("-------------->onInputSubmit:" .. this.shareMapInput.value)

end

--由LuaBehaviour自动调用
function TestView.OnClose()

end

--单击事件--
function TestView.OnDestroy()

end