TestView = {};
local this = TestView;
--local #SCRIPTCTRL# = require("Controller/#SCRIPTCTRL#")
local gameObject;
local transform;

--由LuaBehaviour自动调用
function TestView.Awake(obj)
	gameObject = obj;
	transform = obj.transform;

	--test
	this.OnInit()
end

--由LuaBehaviour自动调用
function TestView.Start()
    print("Transform:" .. this.Zu5.gameObject.name)
end

--由LuaBehaviour自动调用
function TestView.OnInit()
    this.widgets = {
		{field="item_player",path="DiTu.item_player",src=LuaButton, onClick = function ()  --[===[todo click]===]  end },
		{field="Zu5",path="DiTu.Zu5",src="Transform",},
		{field="monsterTable",path="DiTu.Zu5.Table",src="UITable",},
		{field="btnClose",path="DiTu.btnClose",src=LuaButton, onClick = function ()  --[===[todo click]===]  end },
		{field="leftMapPanel",path="DiTu.leftMap",src=LuaPanel},
		{field="titleGrid",path="DiTu.leftMap.BiaoTi",src="UIGrid",},
		{field="shareMapInput",path="DiTu.leftMap.BiaoTiInput",src=LuaInput, onChange = function () --[===[todo input change]===]  end , onSubmit = function () --[===[todo input onSubmit]===]  end},
		{field="curScene",path="DiTu.rightTab.currentScene",src=LuaToggle , onChange = function () --[===[todo toggle.onchange]===] end},
		{field="worldMapBtn",path="DiTu.rightTab.worldMap",src=LuaToggle , onChange = function () --[===[todo toggle.onchange]===] end},

    }
	LuaUIHelper.bind(gameObject , TestView)
end

--由LuaBehaviour自动调用
function TestView.OnClose()

end

--单击事件--
function TestView.OnDestroy()

end