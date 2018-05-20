using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UButtonScale : UButtonBase
{
    public float scaleXOnPressed = 0.8f;
    public float scaleYOnPressed = 0.8f;
    public float scaleDurationOnPressed = 0.3f;
    public float scaleDurationOnReleased = 0.3f;

    public float scaleOnSelected = 1.05f;
    public float scaleDurationOnSelected = 0.1f;
    public float scaleDurationOnDeselected = 0.1f;

    public override void ButtonHover(GameObject go, bool isValue)
    {

    }

    public override void ButtonDownEffect(GameObject go, BaseEventData eventData)
    {
        transform.DOScale(new Vector3(scaleXOnPressed, scaleYOnPressed, 1.0f), scaleDurationOnPressed);
    }

    public override void ButtonUpEffect(GameObject go, BaseEventData eventData)
    {
        transform.DOScale(this.OriginScale, scaleDurationOnReleased);
    }

    public override void ButtonSelectEffect()
    {
        transform.DOScale(scaleOnSelected, scaleDurationOnSelected);
    }

    public override void ButtonDeselectEffect()
    {
        transform.DOScale(1.0f, scaleDurationOnDeselected);
    }
}
