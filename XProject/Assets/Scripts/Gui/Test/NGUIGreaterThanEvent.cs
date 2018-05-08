using System;
using UnityEngine;
using CinemaDirector;

[CutsceneItemAttribute("UTest", "Greater Than", CutsceneItemGenre.ActorItem)]
public class NGUIGreaterThanEvent : CinemaActorEvent
{
    [SerializeField]
    private float value;

    [SerializeField]
    private Vector3 position;

    public override void Trigger(GameObject Actor)
    {
        if (Actor == null) return;

        Actor.transform.localPosition = position;

        UICamera curUICam = UICamera.list[0];
        Vector3 screenPos = curUICam.cachedCamera.WorldToScreenPoint(Actor.transform.position);
        UIWidget widget = UICamera.Raycast<UIWidget>(screenPos);
        UILabel label = UTestHelper.FindWidget<UILabel>(widget.panel, Actor.transform.position);

        if (label == null)
            throw new Exception("无法获取对应位置的文本Label组件！");
        float curValue = Convert.ToSingle(label.text);
        if (curValue <= value)
            throw new Exception(string.Format("数值测试结果不成功！Target:{0} , Current :{1}" , value, curValue));

        Debug.Log(string.Format("<color=#2fd95b>数值大于测试成功!{0}>{1}</color>", curValue , value));
    }
}
