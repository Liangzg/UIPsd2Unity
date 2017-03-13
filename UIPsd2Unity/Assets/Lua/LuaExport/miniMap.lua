miniMap = {};
local this = miniMap;
local MiniMapCtrl = require("Controller/MiniMapCtrl")

--由LuaBehaviour自动调用
function miniMap.Awake(obj)
	this.gameObject = obj
	this.transform = obj.transform

	--test
	this.OnInit()
end

--由LuaBehaviour自动调用
function miniMap.Start()

end

--由LuaBehaviour自动调用
function miniMap.OnInit()
    this.widgets = {
		{field="Zu5",path="DiTu.Zu5",src=LuaScript , requirePath="LuaExport.SubPanelView"},
		{field="LeftMapView",path="DiTu.leftMap",src=LuaScript , requirePath="Lua.LuaExport.LeftMapView"},
		{field="item_player",path="DiTu.item_player",src="Transform"},
		{field="btnClose",path="DiTu.btnClose",src=LuaButton, onClick = this.OnClose() },
		{field="curScene",path="DiTu.rightTab.currentScene",src=LuaToggle , onChange = "test1"},
		{field="worldMapBtn",path="DiTu.rightTab.worldMap",src=LuaToggle , onChange = function (toggle) --[===[todo toggle.onchange]===] end},
		---custom extendsion
        {field="custom add ",path="DiTu.item_player",src="Transform"},

    }

	LuaUIHelper.bind(gameObject , miniMap )

    MiniMapCtrl.view = this  --注入Controller
    MiniMapCtrl.OnInit()
end

--由LuaBehaviour自动调用
function miniMap.OnClose()

end

--单击事件--
function miniMap.OnDestroy()
    MiniMapCtrl.OnDestroy()
end