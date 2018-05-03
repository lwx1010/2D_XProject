//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the widget's size.
/// </summary>

[RequireComponent(typeof(UIBasicSprite))]
[AddComponentMenu("NGUI/Tween/Tween FillAmount")]
public class TweenFillAmount : UITweener
{
    [Range(0f, 1f)]
    public float from = 0f;
    [Range(0f, 1f)]
    public float to = 1f;

    UIBasicSprite mBaseSprite;

	public UIBasicSprite CachedBaseSprite { get { if (mBaseSprite == null) mBaseSprite = GetComponent<UIBasicSprite>(); return mBaseSprite; } }

	/// <summary>
	/// Tween's current value.
	/// </summary>

	public float value { get { return CachedBaseSprite.fillAmount; } set { CachedBaseSprite.fillAmount = value; } }

	/// <summary>
	/// Tween the value.
	/// </summary>

	protected override void OnUpdate (float factor, bool isFinished)
	{
        value = from * (1f - factor) + to * factor;
    }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenFillAmount Begin (UIBasicSprite widget, float duration, int fillAmount)
	{
        TweenFillAmount comp = UITweener.Begin<TweenFillAmount>(widget.gameObject, duration);
		comp.from = widget.fillAmount;
		comp.to = fillAmount;

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}

	[ContextMenu("Set 'From' to current value")]
	public override void SetStartToCurrentValue () { from = value; }

	[ContextMenu("Set 'To' to current value")]
	public override void SetEndToCurrentValue () { to = value; }

	[ContextMenu("Assume value of 'From'")]
	void SetCurrentValueToStart () { value = from; }

	[ContextMenu("Assume value of 'To'")]
	void SetCurrentValueToEnd () { value = to; }
}
