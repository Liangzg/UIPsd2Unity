using UnityEngine;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using System;

namespace LuaFramework {
    public class LuaBehaviour : MonoBehaviour{
        
        #region lua模拟unity通用函数
        protected void Awake()
        {
            CallMethod(name, "Awake", gameObject);
        }

        protected void Start()
        {
            CallMethod(name, "Start");
        }

        protected void OnClick()
        {
            CallMethod(name, "OnClick");
        }

        protected void OnClickEvent(GameObject go)
        {
            CallMethod(name, "OnClick", go);
        }

        /// <summary>
        /// 初始化面板
        /// </summary>
        public void OnInit()
        {
            CallMethod(name, "OnInit");
            Debug.LogWarning("OnInit---->>>" + name);
        }

        /// <summary>
        /// 关闭界面调用(销毁和隐藏都会调用)
        /// </summary>
        public void OnClose()
        {
            CallMethod(name, "OnClose");
        }
        /// <summary>
        /// 不销毁界面切换场景时调用
        /// </summary>
        public void OnChangeSceneHide()
        {
            CallMethod(name, "OnChangeSceneHide");
        }

        protected void OnDestroy()
        {
            CallMethod(name, "OnDestroy", gameObject);
            ClearUIEvent();
           
            Debug.Log("~" + name + " was destroy!");
        }
        #endregion

        #region input输入文本框控件事件(UIInput)
        private Dictionary<UIInput, Dictionary<string, LuaFunction>> inputControls = new Dictionary<UIInput, Dictionary<string, LuaFunction>>();

        /// <summary>
        /// 添加text输入事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddSubmit(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            var input = go.GetComponent<UIInput>();
            if (input == null) return;
            if (!inputControls.ContainsKey(input))
            {
                Dictionary<string, LuaFunction> temp = new Dictionary<string, LuaFunction>();
                temp.Add("onsubmit", luafunc);
                inputControls.Add(input, temp);
            }
            else
            {
                if (!inputControls[input].ContainsKey("onsubmit"))
                    inputControls[input].Add("onsubmit", luafunc);
            }
            EventDelegate.Add(input.onSubmit, delegate ()
            {
                luafunc.Call(input);
            });
        }
        /// <summary>
        /// 添加text文本改变事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddValueChange(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            var input = go.GetComponent<UIInput>();
            if (input == null) return;
            if (!inputControls.ContainsKey(input))
            {
                Dictionary<string, LuaFunction> temp = new Dictionary<string, LuaFunction>();
                temp.Add("onchange", luafunc);
                inputControls.Add(input, temp);
            }
            else
            {
                if (!inputControls[input].ContainsKey("onchange"))
                    inputControls[input].Add("onchange", luafunc);
            }
            EventDelegate.Add(input.onChange, delegate ()
            {
                luafunc.Call(input);
            });
        }
        /// <summary>
        /// 移除input控件submit事件
        /// </summary>
        /// <param name="go"></param>
        public void RemoveSubmit(GameObject go)
        {
            if (go == null) return;
            var input = go.GetComponent<UIInput>();
            RemoveUIInputEvent(input, "onsubmit");
        }
        /// <summary>
        /// 移除input控件valuechange事件
        /// </summary>
        /// <param name="go"></param>
        public void RemoveValueChange(GameObject go)
        {
            if (go == null) return;
            var input = go.GetComponent<UIInput>();
            RemoveUIInputEvent(input, "onchange");
        }

        private void RemoveUIInputEvent(UIInput input, string eventType)
        {
            if (input == null) return;
            Dictionary<string, LuaFunction> temp = null;
            if (inputControls.TryGetValue(input, out temp))
            {
                LuaFunction luafunc = null;
                if (temp.TryGetValue(eventType, out luafunc))
                {
                    luafunc.Dispose();
                    luafunc = null;
                    input.onSubmit.Clear();
                    temp.Remove(eventType);
                }
                if (temp.Count == 0)
                    inputControls.Remove(input);
            }
        }

        private void InputUIEventClear()
        {
            foreach (var temp in inputControls)
            {
                foreach (var de in temp.Value)
                {
                    if (de.Value != null)
                    {
                        de.Value.Dispose();
                    }
                }
                temp.Key.onSubmit.Clear();
                temp.Key.onChange.Clear();
            }
            inputControls.Clear();
        }
        #endregion

        #region WidgetContainer控件ValueChange事件(UIToggle, UIProgressBar, UIPopupList)
        private Dictionary<UIWidgetContainer, LuaFunction> widgetContainers = new Dictionary<UIWidgetContainer, LuaFunction>();

        public void AddWidgetContainerChangeEvent(UIWidgetContainer container, LuaFunction luafunc)
        {
            if (!widgetContainers.ContainsKey(container))
            {
                widgetContainers.Add(container, luafunc);
            }
        }

        public void AddPopupListChange(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            var popuplist = go.GetComponent<UIPopupList>();
            if (popuplist == null) return;
            AddWidgetContainerChangeEvent(popuplist, luafunc);
            EventDelegate.Add(popuplist.onChange, delegate ()
            {
                luafunc.Call(popuplist);
            });
        }

        public void AddProgressBarChange(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            var progressbar = go.GetComponent<UIProgressBar>();
            if (progressbar == null) return;
            AddWidgetContainerChangeEvent(progressbar, luafunc);
            EventDelegate.Add(progressbar.onChange, delegate ()
            {
                luafunc.Call(progressbar);
            });
        }

        public void AddToggleChange(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            var toggle = go.GetComponent<UIToggle>();
            if (toggle == null) return;
            AddWidgetContainerChangeEvent(toggle, luafunc);
            EventDelegate.Add(toggle.onChange, delegate ()
            {
                luafunc.Call(toggle);
            });
        }

        public void RemoveWidgetContainerEvent(GameObject go)
        {
            if (go == null) return;
            var container = go.GetComponent<UIWidgetContainer>();
            if (container == null) return;
            LuaFunction luafunc = null;
            if (widgetContainers.TryGetValue(container, out luafunc))
            {
                luafunc.Dispose();
                luafunc = null;
                if (container is UIPopupList)
                {
                    var popuplist = container as UIPopupList;
                    popuplist.onChange.Clear();
                }
                else if (container is UIProgressBar)
                {
                    var progressbar = container as UIProgressBar;
                    progressbar.onChange.Clear();
                }
                else if (container is UIToggle)
                {
                    var toggle = container as UIToggle;
                    toggle.onChange.Clear();
                }
                widgetContainers.Remove(container);
            }
        }

        private void WidgetContainerEventClear()
        {
            foreach (var de in widgetContainers)
            {
                if (de.Value != null)
                {
                    de.Value.Dispose();
                }
                if (de.Key is UIPopupList)
                {
                    var popuplist = de.Key as UIPopupList;
                    popuplist.onChange.Clear();
                }
                else if (de.Key is UIProgressBar)
                {
                    var progressbar = de.Key as UIProgressBar;
                    progressbar.onChange.Clear();
                }
                else if (de.Key is UIToggle)
                {
                    var toggle = de.Key as UIToggle;
                    toggle.onChange.Clear();
                }
            }
            widgetContainers.Clear();
        }
        #endregion

        #region 其他控件事件
        private Dictionary<string, Dictionary<string, LuaFunction>> controls = new Dictionary<string, Dictionary<string, LuaFunction>>();

        private void AddEventListener(string goname, string eventType, LuaFunction luafunc)
        {
            if (!controls.ContainsKey(goname))
            {
                Dictionary<string, LuaFunction> temp = new Dictionary<string, LuaFunction>();
                temp.Add(eventType, luafunc);
                controls.Add(goname, temp);
            }
            else
            {
                if (!controls[goname].ContainsKey(eventType))
                    controls[goname].Add(eventType, luafunc);
            }
        }

        /// <summary>
        /// 添加单击事件
        /// </summary>
        public void AddClick(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "onclick", luafunc);
            UIEventListener.Get(go).onClick = delegate (GameObject o)
            {
                luafunc.Call(go);
            };
        }

        /// <summary>
        /// 添加双击事件
        /// </summary>
        public void AddDoubleClick(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "ondoubleclick", luafunc);
            UIEventListener.Get(go).onDoubleClick = delegate (GameObject o)
            {
                luafunc.Call(go);
            };
        }

        /// <summary>
        /// 添加悬停事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddHovor(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "onhovor", luafunc);
            UIEventListener.Get(go).onHover = delegate (GameObject o, bool isOver)
            {
                luafunc.Call(go, isOver);
            };
        }

        /// <summary>
        /// 添加按下事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddPress(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "onpress", luafunc);
            UIEventListener.Get(go).onPress = delegate (GameObject o, bool isPressed)
            {
                luafunc.Call(go, isPressed);
            };
        }

        /// <summary>
        /// 添加选择事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddSelect(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "onselect", luafunc);
            UIEventListener.Get(go).onSelect = delegate (GameObject o, bool selected)
            {
                luafunc.Call(go, selected);
            };
        }

        /// <summary>
        /// 添加滚动事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddScroll(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "onscroll", luafunc);
            UIEventListener.Get(go).onScroll = delegate (GameObject o, float delta)
            {
                luafunc.Call(go, delta);
            };
        }

        /// <summary>
        /// 添加拖动开始事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddDragStart(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "ondragstart", luafunc);
            UIEventListener.Get(go).onDragStart = delegate (GameObject o)
            {
                luafunc.Call(go);
            };
        }

        /// <summary>
        /// 添加拖动事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddDrag(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "ondrag", luafunc);
            UIEventListener.Get(go).onDrag = delegate (GameObject o, Vector2 delta)
            {
                luafunc.Call(go, delta);
            };
        }

        /// <summary>
        /// 添加拖动over事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddDragOver(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "ondragover", luafunc);
            UIEventListener.Get(go).onDragOver = delegate (GameObject o)
            {
                luafunc.Call(go);
            };
        }

        /// <summary>
        /// 添加拖动out事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddDragOut(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "ondragout", luafunc);
            UIEventListener.Get(go).onDragOut = delegate (GameObject o)
            {
                luafunc.Call(go);
            };
        }

        /// <summary>
        /// 添加拖动结束事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddDragEnd(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "ondragend", luafunc);
            UIEventListener.Get(go).onDragEnd = delegate (GameObject o)
            {
                luafunc.Call(go);
            };
        }

        /// <summary>
        /// 添加拖动out事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddDrop(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "ondrop", luafunc);
            UIEventListener.Get(go).onDrop = delegate (GameObject o, GameObject dropGo)
            {
                luafunc.Call(go, dropGo);
            };
        }

        /// <summary>
        /// 添加键盘事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddKey(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "onkey", luafunc);
            UIEventListener.Get(go).onKey = delegate (GameObject o, KeyCode key)
            {
                luafunc.Call(go, key);
            };
        }

        /// <summary>
        /// 添加悬浮提示
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddTooltip(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            AddEventListener(go.name, "ontooltip", luafunc);
            UIEventListener.Get(go).onTooltip = delegate (GameObject o, bool show)
            {
                luafunc.Call(go, show);
            };
        }

        /// <summary>
        /// 删除单击事件
        /// </summary>
        /// <param name="go"></param>
        public void RemoveClick(GameObject go)
        {
            RemoveUIEvent(go, "onclick");
        }

        /// <summary>
        /// 删除单击事件
        /// </summary>
        /// <param name="go"></param>
        public void RemoveHovor(GameObject go)
        {
            RemoveUIEvent(go, "onhover");
        }

        public void RemoveUIEvent(GameObject go, string eventType)
        {
            if (go == null) return;
            Dictionary<string, LuaFunction> dictFunc = null;
            if (controls.TryGetValue(go.name, out dictFunc))
            {
                LuaFunction luafunc = null;
                if (dictFunc.TryGetValue(eventType, out luafunc))
                {
                    dictFunc.Remove(eventType);
                    luafunc.Dispose();
                    luafunc = null;
                }
                if (dictFunc.Count == 0)
                    controls.Remove(go.name);
            }
        }

        private void OtherUIEventClear()
        {
            foreach (var temp in controls)
            {
                foreach (var de in temp.Value)
                {
                    if (de.Value != null)
                    {
                        de.Value.Dispose();
                    }
                }
            }
            controls.Clear();
        }
        #endregion

        #region UIPlayTween onFinished事件
        Dictionary<UIPlayTween, LuaFunction> playTweens = new Dictionary<UIPlayTween, LuaFunction>();
        public void AddUIPlayTweenEvent(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            var playTween = go.GetComponent<UIPlayTween>();
            if (playTween == null) return;
            if (!playTweens.ContainsKey(playTween)) playTweens.Add(playTween, luafunc);
            EventDelegate.Add(playTween.onFinished, delegate ()
            {
                luafunc.Call(playTween);
            });
        }

        public void RemoveUIPlayTweenEvent(GameObject go)
        {
            if (go == null) return;
            var playTween = go.GetComponent<UIPlayTween>();
            if (playTween == null) return;
            LuaFunction luafunc = null;
            if (playTweens.TryGetValue(playTween, out luafunc))
            {
                luafunc.Dispose();
                luafunc = null;
                playTween.onFinished.Clear();
                playTweens.Remove(playTween);
            }
        }

        private void UIPlayTweenEventClear()
        {
            foreach (var de in playTweens)
            {
                if (de.Value != null)
                {
                    de.Value.Dispose();
                }
                de.Key.onFinished.Clear();
            }
        }
        #endregion

        #region 退出清理
        /// <summary>
        /// 清除单击事件
        /// </summary>
        public void ClearUIEvent()
        {
            WidgetContainerEventClear();
            InputUIEventClear();
            OtherUIEventClear();
            UIPlayTweenEventClear();
        }
        #endregion

        /// <summary>
        /// 执行Lua方法
        /// </summary>
        public static object[] CallMethod(string module, string func, params object[] args)
        {
            LuaClientStarter luaMgr = LuaClientStarter.instance;
            if (luaMgr == null) return null;
            return luaMgr.CallFunction(module + "." + func, args);
        }
    }
}