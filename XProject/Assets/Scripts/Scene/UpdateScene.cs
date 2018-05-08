using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LuaFramework;

public class UpdateScene : MonoBehaviour {

    public Image bgImg;

    private StartUpController controller;

    private void Awake()
    {
        bgImg.sprite = Resources.Load<Sprite>("Other/loading/login");
        bgImg.SetNativeSize();
    }

    // Use this for initialization
    void Start () {
        controller = this.gameObject.AddComponent<StartUpController>();
        controller.startUpEvent = OnStartUp;
    }

    void OnStartUp()
    {
        var luaMgr = AppFacade.Instance.GetManager<LuaManager>();
        luaMgr.luaBinder = new CustomBinder();
        luaMgr.delCreator = new DelegateCreator();
    }

    public void OnBackgroundClicked()
    {
        if (controller != null)
            controller.OnBackgroundClicked();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
