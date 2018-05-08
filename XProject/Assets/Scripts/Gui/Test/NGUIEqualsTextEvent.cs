using System;
using UnityEngine;
using CinemaDirector;

[CutsceneItemAttribute("UTest", "Equals Text", CutsceneItemGenre.ActorItem)]
public class NGUIEqualsTextEvent : CinemaActorEvent
{
    [SerializeField]
    private string inputText;

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
        if(!label.text.Trim().Equals(inputText.Trim()))
            throw new Exception(string.Format("测试结果不一致！Target:{0} , Current :{1}" , inputText , label.text));

        Debug.Log(string.Format("<color=#2fd95b>Text比较测试成功!{0}=={1}</color>", inputText, label.text));
    }
}
