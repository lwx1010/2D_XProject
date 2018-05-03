using System;
using UnityEngine;
using CinemaDirector;

[CutsceneItemAttribute("UTest", "Input", CutsceneItemGenre.ActorItem)]
public class NGUIInputEvent : CinemaActorEvent
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
        UIInput input = UICamera.Raycast<UIInput>(screenPos);
        if (input == null)
            throw new Exception("无法获取对应位置的Input组件！");
        
        input.value = inputText;
    }
    
}
