/********************************************************
*** Auth: LiangZG
********************************************************/

using System.Collections;
using UnityEngine;
using LuaFramework;

/// <summary>
/// 剧情世界观滚动文本
/// </summary>
public class ScrollTextEffect : MonoBehaviour
{

    public float SpeedRatio = 0.55f;

    public bool AutoDestroy = true;

    private UITexture[] texts;

    private int index;

    private string panelName;

    private float duration;

    private float delayTime;

    void Awake()
    {
        texts = this.GetComponentsInChildren<UITexture>();
        foreach (UITexture uiTex in texts)
        {
            uiTex.type = UIBasicSprite.Type.Filled;
            uiTex.fillDirection = UIBasicSprite.FillDirection.Horizontal;
            uiTex.invert = true;
            uiTex.fillAmount = 0;
        }
    }

	// Use this for initialization
	void Start ()
	{
	    UIPanel panel = this.GetComponentInParent<UIPanel>();
	    panelName = panel.name.Replace("Panel", "");

        this.StartCoroutine(this.UpdateTween());
    }
	
	// Update is called once per frame
	private IEnumerator UpdateTween()
	{
        yield return Yielders.GetWaitForSeconds(0.5f);

        if (texts == null) yield break;

        while (index < texts.Length)
	    {
            float amount = texts[index].fillAmount + SpeedRatio * Time.deltaTime;
            texts[index].fillAmount = Mathf.Min(amount, 1);

            if (texts[index].fillAmount >= 1.0f)
            {
                index++;
            }

	        yield return null;
	    }

        if (AutoDestroy)
        {
            this.Invoke("destroyUI", 1.5f);
            AutoDestroy = false;
        }
    }

    private void destroyUI()
    {
        PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
        panelMgr.ClosePanel(panelName);
    }
}
