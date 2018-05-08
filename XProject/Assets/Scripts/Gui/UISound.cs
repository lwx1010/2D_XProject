//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using LuaFramework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays the specified sound.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Sound")]
public class UISound : MonoBehaviour
{
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
		Custom,
		OnEnable,
		OnDisable,
        None,
    }
    
    public string audioName;
    
	public Trigger trigger = Trigger.OnClick;

	bool mIsOver = false;
    private SoundManager mSoundMgr;

    bool canPlay
	{
		get
		{
			if (!enabled) return false;
#if NGUI
            UIButton btn = GetComponent<UIButton>();
            return (btn == null || btn.isEnabled);
#else
            Button btn = GetComponent<Button>();
			return (btn == null || btn.IsActive());
#endif
        }
	}


    string AudioName
    {
        get
        {
            if (string.IsNullOrEmpty(audioName))
                audioName = AppConst.UISoundConfig[0];
            return audioName;
        }
    }

    private SoundManager soundMgr
    {
        get
        {
            if(mSoundMgr == null)
                mSoundMgr = AppFacade.Instance.GetManager<SoundManager>();
            return mSoundMgr;
        }
    }

	void OnEnable ()
	{
		if (trigger == Trigger.OnEnable)
            Play();
	}

	void OnDisable ()
	{
		if (trigger == Trigger.OnDisable)
            Play();
    }


	void OnPress (bool isPressed)
	{
		if (trigger == Trigger.OnPress)
		{
			if (mIsOver == isPressed) return;
			mIsOver = isPressed;
		}

		if (canPlay && ((isPressed && trigger == Trigger.OnPress) || (!isPressed && trigger == Trigger.OnRelease)))
            Play();
    }

	void OnClick ()
	{
		if (trigger == Trigger.OnClick) //canPlay && 
            Play();
    }


	public void Play ()
	{
	    SoundManager _soundMgr = soundMgr;
	    if (_soundMgr == null) return;
        _soundMgr.PlayUISound(AudioName);
	}
}
