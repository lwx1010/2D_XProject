using UnityEngine;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LuaFramework {
    public class LuaBehaviour : View {

        private string[] ignoreNames = new string[] { "LoginPanel", "MainPanel", "ChatPanel", "NetConnecting" };

        static bool isQuit = false;
        
        #region lua模拟unity通用函数
        protected void Awake()
        {
            Util.CallMethod(name, "Awake", gameObject);
        }

        protected void Start()
        {
            Util.CallMethod(name, "Start");
        }

        protected void OnClick()
        {
            Util.CallMethod(name, "OnClick");
        }

        protected void OnClickEvent(GameObject go)
        {
            Util.CallMethod(name, "OnClick", go);
        }

        /// <summary>
        /// 初始化面板
        /// </summary>
        public void OnInit()
        {
            Util.CallMethod(name, "OnInit");
            //Debug.LogWarning(string.Format("OnInit---->>>{0}", name));
        }

        /// <summary>
        /// 关闭界面调用(销毁和隐藏都会调用)
        /// </summary>
        public void OnClose()
        {
            if (!IgnoreSoundEffect() && !isQuit)
                LuaHelper.GetSoundManager().PlayUISound("panel-close");
            Util.CallMethod(name, "OnClose");
        }
        /// <summary>
        /// 不销毁界面切换场景时调用
        /// </summary>
        public void OnChangeSceneHide()
        {
            Util.CallMethod(name, "OnChangeSceneHide");
        }

        protected void OnDestroy()
        {
            if (!isQuit)
            {
                Util.CallMethod(name, "OnDestroy", gameObject);
            }
            Debugger.Log(string.Format("~{0} was destroy!", name));
        }

        void OnApplicationQuit()
        {
            isQuit = true;
        }

        bool IgnoreSoundEffect()
        {
            if (LuaHelper.GetSoundManager() == null) return true;

            bool ignore = false;
            for (int i = 0; i < ignoreNames.Length; ++i)
            {
                if (name.Equals(ignoreNames[i]))
                {
                    ignore = true;
                    break;
                }
            }

            LuaTable luaTab = LuaManager.mainLua.GetTable(name);
            if (luaTab != null && luaTab["_panelLayerType"] != null)
            {
                if (Convert.ToInt32(luaTab["_panelLayerType"]) == 3)
                {
                    //剧情UI界面,过滤界面默认音效
                    return true;  
                }
            }


            return ignore;
        }
        #endregion

        public void AddClick(GameObject go, LuaFunction luafunc)
        {
            EventTriggerListener.Get(go).onClick = delegate 
            {
                luafunc.Call(go);
            };
        }

        public void AddToggleChange(GameObject go, LuaFunction luafunc)
        {
            var toggle = go.GetComponent<Toggle>();
            if (toggle != null)
            {
                var changeEvent = new Toggle.ToggleEvent();
                changeEvent.AddListener((bool value) => luafunc.Call(toggle));
                toggle.onValueChanged = changeEvent;
            }
        }

        /// <summary>
        /// 文本输入框监控文本输入，变化
        /// </summary>
        /// <param name="go"></param>
        /// <param name="lucfunc"></param>
        public void AddInputFieldChanged(GameObject go, LuaFunction luafunc)
        {
            var input = go.GetComponent<InputField>();
            if(input != null)
            {
                var changeEvent = new InputField.OnChangeEvent();
                changeEvent.AddListener((string txt) => luafunc.Call(input));
                input.onValueChanged = changeEvent;
            }
        }

        /// <summary>
        /// 文本输入框输入完成回调
        /// </summary>
        /// <param name="go"></param>
        /// <param name="lucfunc"></param>
        public void AddInputFieldSubmit(GameObject go, LuaFunction luafunc)
        {
            var input = go.GetComponent<InputField>();
            if (input != null)
            {
                var changeEvent = new InputField.SubmitEvent();
                changeEvent.AddListener((string txt) => luafunc.Call(input));
                input.onEndEdit = changeEvent;
            }
        }

        /// <summary>
        /// 滑动条数值变动监控
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddSliderValueChange(GameObject go, LuaFunction luafunc)
        {
            var slider = go.GetComponent<Slider>();
            if(slider != null)
            {
                var changeEvent = new Slider.SliderEvent();
                changeEvent.AddListener((float value) => luafunc.Call(value));
                slider.onValueChanged = changeEvent;
            }
        }

        /// <summary>
        /// 滑动条数值变动监控
        /// </summary>
        /// <param name="go"></param>
        /// <param name="luafunc"></param>
        public void AddScrollderbarValueChange(GameObject go, LuaFunction luafunc)
        {
            var bar = go.GetComponent<Scrollbar>();
            if(bar != null)
            {
                var changeEvent = new Scrollbar.ScrollEvent();
                changeEvent.AddListener((float value) => luafunc.Call(value));
                bar.onValueChanged = changeEvent;
            }
        }


        #region ugui事件
        public void AddOnDown(GameObject go, LuaFunction luafunc)
        {
            EventTriggerListener.Get(go).onDown = delegate (GameObject obj, BaseEventData eventData)
            {
                luafunc.Call(obj, eventData);
            };
        }

        public void AddOnUp(GameObject go, LuaFunction luafunc)
        {
            EventTriggerListener.Get(go).onUp = delegate (GameObject obj, BaseEventData eventData)
            {
                luafunc.Call(obj, eventData);
            };
        }

        public void AddOnDrag(GameObject go, LuaFunction luafunc)
        {
            EventTriggerListener.Get(go).onDrag = delegate (GameObject obj, BaseEventData eventData)
            {
                luafunc.Call(obj, eventData);
            };
        }

        public void AddOnEndDrag(GameObject go, LuaFunction luafunc)
        {
            EventTriggerListener.Get(go).onEndDrag = delegate (GameObject obj, BaseEventData eventData)
            {
                luafunc.Call(obj, eventData);
            };
        }
        #endregion


#if USING_NGUI
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
                        {
                            controls[goname].Add(eventType, luafunc);
                        }
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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

                        ExtendEventDeal(go, "onclick");
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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

                        ExtendEventDeal(go, "ondoubleclick");
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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

                        ExtendEventDeal(go, "onpress");
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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

                        ExtendEventDeal(go, "ondragstart");
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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

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
                        if (!LuaHelper.GetPanelManager().CanOpenPanel() || PreLoadingScene.inPreloading)
                            return;

                        luafunc.Call(go, dropGo);
                    };
                }

                public void AddExitArea(GameObject go, LuaFunction luaFunc)
                {
                    var controlArea = go.transform.GetOrAddComponent<ControlArea>();
                    if (controlArea != null)
                        controlArea.EnterControlArea(go, luaFunc);
                }

                public void ReleaseExitArea(GameObject go)
                {
                    var controlArea = go.transform.GetOrAddComponent<ControlArea>();
                    if (controlArea != null)
                        controlArea.Release();
                }

                public void UpdateExitArea(GameObject go, float deltaX, float deltaY)
                {
                    var controlArea = go.GetComponent<ControlArea>();
                    if (controlArea != null)
                        controlArea.UpdateColliderPosition(go, deltaX, deltaY);
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
                        ExtendEventDeal(go, "onkey");
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

        #region ui额外事件处理
            public delegate void UiExtendEventDeal(GameObject go, string type);

            static List<UiExtendEventDeal> extendEventDeals = new List<UiExtendEventDeal>();

            public static void ExtendEventDeal(GameObject go, string type)
            {
                for (int i = 0; i < extendEventDeals.Count; i++)
                {
                    extendEventDeals[i](go, type);
                }
            }

            public static void AddExtendEvent(UiExtendEventDeal func)
            {
                for (int i = 0; i < extendEventDeals.Count; i++)
                {
                    if (extendEventDeals[i] == func)
                        return;
                }

                extendEventDeals.Add(func);
            }

            public static void RemoveExtendEvent(UiExtendEventDeal func)
            {
                for (int i = 0; i < extendEventDeals.Count; i++)
                {
                    if (extendEventDeals[i] == func)
                    {
                        extendEventDeals.RemoveAt(i);
                        return;
                    }
                }
            }
        #endregion
#endif
    }
}