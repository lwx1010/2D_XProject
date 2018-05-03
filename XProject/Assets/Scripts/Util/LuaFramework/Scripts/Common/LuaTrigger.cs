using UnityEngine;
using System.Collections;
using LuaFramework;
using System.Collections.Generic;
using LuaInterface;

public class LuaTrigger : Base
{
    public string LuaName;
    #region lua模拟unity通用函数
    protected void Awake()
    {
        Util.CallMethod(LuaName, "Awake", gameObject);
    }

    protected void Start()
    {
        Util.CallMethod(LuaName, "Start");
    }

    protected void OnEnable()
    {
        Util.CallMethod(LuaName, "OnEnable");
    }

    protected void OnDisable()
    {
        Util.CallMethod(LuaName, "OnDisable");
    }

    protected void OnClick()
    {
        Util.CallMethod(LuaName, "OnClick");
    }

    protected void OnClickEvent(GameObject go)
    {
        Util.CallMethod(LuaName, "OnClick", go);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Util.CallMethod(LuaName, "OnTriggerEnter", other, gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        Util.CallMethod(LuaName, "OnTriggerExit", other, gameObject);
    }

    protected void OnDestroy()
    {
        Util.CallMethod(LuaName, "OnDestroy", gameObject);
        //InputUIEventClear();
        Debugger.Log("~{0} was destroy!", LuaName);
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
#endif
}
