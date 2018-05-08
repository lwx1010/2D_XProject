using UnityEngine;
using System.Collections;
using LuaFramework;
using UnityEngine.UI;

public sealed class PreLoginScene : MonoBehaviour {

    public static PreLoginScene Instance;

    public Text showInfo;
    public Image progress;
    public Image progressBg;
    public Image bgTex;
    public Camera bgCamera;

    void Awake ()
    {
        Util.AutoAdjustCameraRect(bgCamera);
        Instance = this;
        progress.fillAmount = 0;

        bgTex.sprite = Resources.Load<Sprite>("Other/loading/login");
        progress.sprite = Resources.Load<Sprite>("Other/loading/ui_loading_01");
        progressBg.sprite = Resources.Load<Sprite>("Other/loading/ui_loading_box");

        progress.gameObject.SetActive(false);
    }

    public void UpdateProgress(float value)
    {
        progress.gameObject.SetActive(true);
        progress.fillAmount = value;
    }

    public void SetMessage(string message)
    {
        showInfo.text = message;
        showInfo.color = new Color(1, 1, 1, 1);
    }

    void OnDestroy()
    {
        Instance = null;
    }
}
