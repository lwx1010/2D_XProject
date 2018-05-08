using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 流光特效
/// </summary>
[AddComponentMenu("NGUI/Interaction/Flow Light Effect")]
public class FlowLightEffect : MonoBehaviour
{
#if USING_NGUI

    public enum FlowType
    {
        LeftToRight , RightToLeft, TopToBottom, BottomToTop
    }

    /// <summary>
    /// 自动缓存
    /// </summary>
    [SerializeField]   public bool autoCache = true;

    [SerializeField]   public Texture FlowTex;

    [SerializeField]  public FlowType Type = FlowType.LeftToRight;

    [SerializeField]   public float mUvStart = 0f;

    [SerializeField]   public float mUvSpeed = 0.02f;

    [SerializeField]   public float mUvMax_X = 1f;

    [SerializeField]   public float FlowPower = 1f;

    [SerializeField]   public float mTimeInteval = 0f;

    private float mUvAdd;
    private bool mIsPlaying;

    private static Dictionary<string , Material> cacheMatrials = new Dictionary<string, Material>();

    private static Dictionary<string , List<FlowLightEffect>> cachedWidgets = new Dictionary<string, List<FlowLightEffect>>();

    private static string FLOW_LIGHT_OFFSET = "_FlowLightOffset";
    private const string PlayFlow = "Play";

    private Material mMat;
    private Texture mFlowTex;
    private UIWidget mWidget;
    private string matKey;

    private UIWidget widget
    {
        get
        {
            if (mWidget == null)
            {
                mWidget = this.GetComponent<UIWidget>();
                if (mWidget == null)
                    mWidget = this.GetComponentInParent<UIWidget>();
            }
            return mWidget;
        }
    }
    
    void Awaker()
    {
        matKey = materialKey(widget);
        mMat = getMaterial(matKey);
        mWidget.material = mMat;
        
        makeChangeOnRenderer();
        this.Play();
    }

    private string materialKey(UIWidget widget)
    {
        UIPanel[] panels = widget.GetComponentsInParent<UIPanel>();
        StringBuilder buf = new StringBuilder();
        for (int i = 0; i < panels.Length; i++)
        {
            buf.Append(panels[i].name);
        }
        string allPanels = buf.ToString() + Type;

        UISprite sprite = widget as UISprite;
        if (sprite != null)
        {
            return sprite.atlas.name + allPanels;
        }

        UITexture tex = widget as UITexture;
        if (tex != null)
        {
            return tex.texturePath + allPanels;
        }

        return null;
    }

    private Material getMaterial(string key)
    {
        if (!autoCache)
        {
            Material insMat = Instantiate(Resources.Load<Material>("Other/FlowGuiMat"));
            UISprite sprite = mWidget as UISprite;
            if (sprite != null && sprite.atlas != null)
                insMat.mainTexture = sprite.atlas.texture;
            insMat.SetTexture("_FlowLightTex", FlowTex);
            insMat.SetFloat("_FlowLightPower", FlowPower);
            return insMat;
        }

       if (string.IsNullOrEmpty(key)) return null;

        if (!cacheMatrials.ContainsKey(key))
        {
            Material newMat = Instantiate(Resources.Load<Material>("Other/FlowGuiMat"));
            UISprite sprite = mWidget as UISprite;
            if (sprite != null && sprite.atlas != null)
                newMat.mainTexture = sprite.atlas.texture;
            newMat.SetTexture("_FlowLightTex", FlowTex);
            newMat.SetFloat("_FlowLightPower", FlowPower);

            cacheMatrials[key] = newMat;
        }

        return cacheMatrials[key];
    }

    public void OnRender(Material mat)
    {
        if (mIsPlaying)
        {
            mUvAdd += mUvSpeed * Time.deltaTime;
            switch (Type)
            {
                    case FlowType.LeftToRight:
                    mat.SetVector(FLOW_LIGHT_OFFSET, new Vector4(mUvStart + mUvAdd, 0));
                    break;
                    case FlowType.RightToLeft:
                    mat.SetVector(FLOW_LIGHT_OFFSET, new Vector4(mUvMax_X - mUvAdd, 0));
                    break;
                    case FlowType.TopToBottom:
                    mat.SetVector(FLOW_LIGHT_OFFSET, new Vector4(0, mUvStart + mUvAdd, 0));
                    break;
                    case FlowType.BottomToTop:
                    mat.SetVector(FLOW_LIGHT_OFFSET, new Vector4(0, mUvMax_X - mUvAdd, 0));
                    break;
            }
            
            if (mUvAdd >= mUvMax_X)
            {
                mIsPlaying = false;
                Invoke(PlayFlow, mTimeInteval);
            }
        }
    }

    // Use this for initialization
	void Start ()
	{
        Awaker();
        UISprite sprite = mWidget as UISprite;
        if (sprite != null && sprite.atlas != null)
	    {
	        UISpriteData sprData = sprite.GetAtlasSprite();
	        Texture atlasTex = sprite.atlas.texture;

            float widthRate = sprData.width * 1.0f /atlasTex.width;
	        float heightRate = sprData.height*1.0f/atlasTex.height;
	        float offsetX = sprData.x*1.0f/atlasTex.width;
	        float offsetY = 1.0f - (sprData.y + sprData.height)*1.0f/atlasTex.height;

            sprite.material.SetFloat("_WidthRate" , widthRate);
            sprite.material.SetFloat("_HeightRate" , heightRate);
            sprite.material.SetVector("_Offset" , new Vector4(offsetX , offsetY));
	    }

        this.OnEnable();
	}

    private void OnEnable()
    {
        if (autoCache && !string.IsNullOrEmpty(matKey))
        {
            if(!cachedWidgets.ContainsKey(matKey))
                cachedWidgets[matKey] = new List<FlowLightEffect>();
            cachedWidgets[matKey].Add(this);
        }
            
        makeChangeOnRenderer();
    }



    // Update is called once per frame
    void Update () {

	}


    public void Play()
    {
        mUvAdd = 0;
        mIsPlaying = true;
    }

    private void OnDisable()
    {
        string key = matKey;
        if (mWidget != null && mWidget.onRender != null) mWidget.onRender = null;

        if (string.IsNullOrEmpty(key) || !cachedWidgets.ContainsKey(key)) return;

        List<FlowLightEffect> widgets = cachedWidgets[key];
        widgets.Remove(this);
    }




    void OnDestroy()
    {
        string key = matKey;
        if (string.IsNullOrEmpty(key) || !cachedWidgets.ContainsKey(key)) return;

        List<FlowLightEffect> widgets = cachedWidgets[key];
        if (widgets.Count <= 0 && cacheMatrials.ContainsKey(key))
        {
            GameObject.Destroy(cacheMatrials[key]);
            cacheMatrials.Remove(key);
        }
        else
        {
            makeChangeOnRenderer();
        }
    }

    /// <summary>
    /// OnRender更新
    /// </summary>
    private void makeChangeOnRenderer()
    {
        if (!autoCache)
        {
            widget.onRender = OnRender;
            return;
        }
        
        if (string.IsNullOrEmpty(matKey) || !cachedWidgets.ContainsKey(matKey)) return;
        
        List<FlowLightEffect> widgets = cachedWidgets[matKey];
        bool isOnRendered = false;
        for (int i = 0; i < widgets.Count; i++)
        {
            if (!isOnRendered && widgets[i].gameObject.activeInHierarchy)
            {
                widgets[i].mWidget.onRender = widgets[i].OnRender;
                isOnRendered = true;
            }
            else
            {
                widgets[i].mWidget.onRender = null;
            }
        }
    }

    private void OnValidate()
    {
        if (mMat == null) return;

        mMat.SetFloat("_FlowLightPower" , FlowPower);
        if(mMat.GetTexture("_FlowLightTex") != FlowTex)
            mMat.SetTexture("_FlowLightTex" , FlowTex);
    }

#endif
}
