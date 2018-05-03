using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UButtonBase : MonoBehaviour
{
    public delegate void OnButtonClick(GameObject go);

    public OnButtonClick OnClick, OnSelected;
    public RectTransform m_Transform;
    protected Vector3 OriginScale;

    void Awake()
    {
        if (m_Transform == null)
        {
            m_Transform = transform as RectTransform;
        }

    }

    void Start()
    {
        EventTriggerListener.Get(gameObject).onDown = ButtonDownEffect;
        EventTriggerListener.Get(gameObject).onUp = ButtonUpEffect;
        OriginScale = m_Transform.localScale;
    }

    public virtual void ButtonHover(GameObject go, bool isValue)
    {
    }

    public virtual void ButtonDownEffect(GameObject go, BaseEventData eventData)
    {
    }

    public virtual void ButtonUpEffect(GameObject go, BaseEventData eventData)
    {
    }

    public virtual void ButtonEnterEffect()
    {
    }

    public virtual void ButtonExitEffect()
    {
    }

    public virtual void ButtonSelectEffect()
    {
    }

    public virtual void ButtonDeselectEffect()
    {
    }
}