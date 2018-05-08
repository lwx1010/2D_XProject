using System;
using UnityEngine;
using System.Collections;
using CinemaDirector;
using CinemaDirector.Helpers;

[CutsceneItemAttribute("UTest", "Touch Position", CutsceneItemGenre.ActorItem)]
public class NGUIMouseEvent : CinemaActorEvent
{

    [SerializeField]
    private Vector3 inputPosition;
    // Options for reverting in editor.
    
    public override void Trigger(GameObject Actor)
    {
        if (Actor == null) return;

        UIWidget widget = Actor.GetComponent<UIWidget>();
        widget.cachedTransform.localPosition = inputPosition;

        UICamera curUICam = UICamera.list[0];
        Vector3 screenPos = curUICam.cachedCamera.WorldToScreenPoint(Actor.transform.position);
        this.StartCoroutine(curUICam.ProcessScreenPostion(screenPos));
        
    }
    
}
