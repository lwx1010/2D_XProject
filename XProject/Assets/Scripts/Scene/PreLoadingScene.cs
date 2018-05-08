using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using LuaFramework;
using System.IO;
using Riverlake;
using Riverlake.LuaFramework.Controller.Transition;
using Riverlake.Resources;
using UnityEngine.UI;

public sealed class PreLoadingScene : ASceneLoadingTransition
{
    public static bool inPreloading = false;
    public static string next_scene;
    public static GameObject loadingPanel;
    public static AudioListener al;
    public GameObject loadingCanvas;
    public Image backImage;
    public Image progressBar;
    public Image progressBarBg;

    [HideInInspector]
    public Action<float> processAction;

    private AssetBundleCreateRequest abcr;

    public static int curIndex = 1;

    protected override void Awake()
    {
        base.Awake();
        backImage.sprite = ResourceManager.LoadSpriteAssets(string.Format("Other/loading/loading{0}.jpg", curIndex++));
        curIndex = curIndex > 4 ? 1 : curIndex;

        progressBar.sprite = ResourceManager.LoadSpriteAssets("Other/loading/ui_loading_01.png");
        progressBar.SetNativeSize();
        progressBar.type = Image.Type.Filled;
        progressBar.fillMethod = Image.FillMethod.Horizontal;
        progressBar.fillAmount = 0;

        progressBarBg.sprite = ResourceManager.LoadSpriteAssets("Other/loading/ui_loading_box.png");

        ProcessAction = OnProgressChanged;
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
         
        NetworkManager.isKickOut = false;

        Util.AutoAdjustCameraRect(Camera.main);
        al = GetComponent<AudioListener>();
        StartCoroutine(PreloadScene());

        DontDestroyOnLoad(loadingCanvas);
        //if (Config.User_Config.internal_sdk == 1)
        //{
        //    CenterServerManager.Instance.StepLogRequest(5);
        //}
    }

    IEnumerator PreloadScene()
    {
        yield return Yielders.EndOfFrame;
        AssetBundleManager.Instance.Clear();
        yield return Yielders.GetWaitForSeconds(0.1f);

        yield return this.StartCoroutine(loadSceneBundle());

        yield return this.StartCoroutine(OnLoading());
    }


    private IEnumerator loadSceneBundle()
    {
        if(!AppConst.AssetBundleMode)   yield break;

        string scenePath = string.Format("{0}scenes/{1}.ab", Util.DataPath, next_scene);
        abcr = AssetBundle.LoadFromFileAsync(scenePath);
        yield return abcr;
    }

    void OnProgressChanged(float value)
    {
        if (progressBar != null)
            progressBar.fillAmount = value;
    }

    protected override void OnDestroy()
    {
        if (abcr != null)
        {
            abcr.assetBundle.Unload(false);
            abcr = null;
        } 
    }

    public static void DestroySelf()
    {
        inPreloading = false;
        next_scene = string.Empty;
        GameObject go = GameObject.Find("PreLoading");
        if (loadingPanel != null) Destroy(loadingPanel);
        loadingPanel = null;
        al = null;
        Destroy(go);
    }
}
