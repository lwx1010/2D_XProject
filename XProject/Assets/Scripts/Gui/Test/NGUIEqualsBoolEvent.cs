using System;
using UnityEngine;
using CinemaDirector;

[CutsceneItemAttribute("UTest", "Equals Bool", CutsceneItemGenre.ActorItem)]
public class NGUIEqualsBoolEvent : CinemaActorEvent
{
    [SerializeField]
    private bool value;

    [SerializeField]
    private Vector3 position;

    public override void Trigger(GameObject Actor)
    {
        if (Actor == null) return;

        Actor.transform.localPosition = position;

        UICamera curUICam = UICamera.list[0];
        Vector3 screenPos = curUICam.cachedCamera.WorldToScreenPoint(Actor.transform.position);
        UIToggle toggle = UICamera.Raycast<UIToggle>(screenPos);
        if (toggle == null)
            throw new Exception("无法获取对应位置的文本Label组件！");
        if(!toggle.value.Equals(value))
            throw new Exception(string.Format("测试结果不一致！Target:{0} , Current :{1}" , value, toggle.value));

        Debug.Log(string.Format("<color=#2fd95b>Bool比较测试成功!{0}=={1}</color>", value, toggle.value));
    }
}
