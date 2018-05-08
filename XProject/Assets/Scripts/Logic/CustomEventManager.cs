using Riverlake;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;


/// <summary>
///  事件管理
/// </summary>
public class CustomEventManager : Singleton<CustomEventManager>
{
    private Hashtable _eventTable;

    void Awake()
    {
        _eventTable = new Hashtable();
    }

    /// <summary>
    /// 触发监听
    /// </summary>
    /// <param name="type"></param>
    /// <param name="infoList"></param>
    public void DispatchCustomEvent(int type, List<KeyValuePair<string, object>> infoList)
    {
        if (_eventTable.ContainsKey(type))
        {
            List<CustomEvent> eventList = _eventTable[type] as List<CustomEvent>;
            if (eventList == null || eventList.Count <= 0)
                return;

            int count = eventList.Count;
            CustomEvent oneEvent;
            for (int i = 0; i < count; i++)
            {
                oneEvent = eventList[i];
                oneEvent.callBack(infoList);
            }
        }
    }

    /// <summary>
    /// 添加监听
    /// </summary>
    /// <param name="aEvent"></param>
    public void AddEventListener(CustomEvent aEvent)
    {
        if (aEvent == null)
            return;

        List<CustomEvent> eventList;
        if (_eventTable.ContainsKey(aEvent.eventType))
        {
            eventList = _eventTable[aEvent.eventType] as List<CustomEvent>;
            eventList.Add(aEvent);
        }
        else
        {
            eventList = new List<CustomEvent>();
            eventList.Add(aEvent);

            _eventTable.Add(aEvent.eventType, eventList);
        }
    }

    /// <summary>
    /// 移除单个监听
    /// </summary>
    /// <param name="aEvent"></param>
    public void RemvoeCustomEventListener(CustomEvent aEvent)
    {
        if (aEvent == null || _eventTable == null )
            return;

        if (_eventTable.ContainsKey(aEvent.eventType))
        {
            List<CustomEvent> eventList = _eventTable[aEvent.eventType] as List<CustomEvent>;
            eventList.Remove(aEvent);
        }
    }

    /// <summary>
    /// 删除一类监听
    /// </summary>
    /// <param name="type"></param>
    public void DeleteEventListenerByType(int type)
    {
        if (_eventTable.ContainsKey(type))
        {
            _eventTable.Remove(type);
        }

    }

    /// <summary>
    /// 清理所有监听，断线重连时处理
    /// </summary>
    public void ClearEventListener()
    {
        _eventTable.Clear();
    }
}

/// <summary>
/// 事件类型
/// </summary>
public class CustomEventType
{
    public static int HERO_ATTR_CHANGED = 1;
}


/// <summary>
///  事件
/// </summary>
public class CustomEvent
{
    public int eventType;

    public Action<List<KeyValuePair<string, object>>> callBack;
}




