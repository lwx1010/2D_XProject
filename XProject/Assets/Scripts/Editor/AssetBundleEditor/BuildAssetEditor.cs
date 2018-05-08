using UnityEngine;
using System.Collections;
using UnityEditor;
using BehaviorDesigner.Runtime;

public class BuildAssetEditor : EditorWindow
{
    [MenuItem("[Build]/Build Assets")]
    public static void OpenBuildAsset()
    {
        GetWindow(typeof(BuildAssetEditor));
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath("Assets/Res/SkillLibrary/player/" + "1004" + ".asset", typeof(UnityEngine.Object));
        ExternalBehaviorTree skillTree = asset as ExternalBehaviorTree;
        if (skillTree != null)
        {

            skillTree.SetVariableValue("StopBehaviors", false);

            skillTree.SetVariableValue("SkillCD", 5f);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();


        }
    }

    public BuildAssetEditor()
    {
        this.titleContent = new GUIContent("Build Assets");
        base.position = new Rect(200f, 200f, 1200f, 1000f);
        base.maxSize = new Vector2(1200f, 1000f);
        base.minSize = new Vector2(500f, 700f);

    }

    public void OnEnable()
    {
        InitInfo();
    }

    private void InitInfo()
    {
        platformType = BuildUtil.GetPlatformType(EditorUserBuildSettings.activeBuildTarget);
        RightScrollViewWidth = position.width - (letfScrollViewWidth + 5);
    }

    public BuildUtil.PlatformType platformType;

    public bool isBuildAPP;                         //是否发布应用程序
    public bool BuildDebug;                         //开启开发模式调试
    public bool ScriptDebug;                         //开启脚本调试
    public bool AutoConnectProfile;                  //自动链接Profile调试

    public string assetVersion ="1804241626";       //资源版本号
    public string AppVersion ="35";                       //应用程序版本号


    private Vector2 scrollPos = new Vector2(0, 20);
    private Vector2 scrollPos2 = new Vector2(220, 20);

    private float letfScrollViewWidth = 220;
    private float RightScrollViewWidth = 220;

    private float topToolBarHeight = 20f;
    private float rightHeightOffset = 20f;

    public void OnGUI()
    {

        DrawToolBar();
        GUILayout.BeginHorizontal();

        LeftScrollView();



        RightScrollView();

        GUILayout.EndHorizontal();
    }

    private void DrawToolBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUILayout.Label("Build Set", EditorStyles.toolbarButton, GUILayout.MaxWidth(62));
        GUILayout.Label("Platform:", EditorStyles.toolbarButton, GUILayout.MaxWidth(60));
        string[] displayedOptions = System.Enum.GetNames(typeof(BuildUtil.PlatformType));
        int newType= EditorGUILayout.Popup((int)platformType, displayedOptions,EditorStyles.toolbarPopup, GUILayout.MaxWidth(90));
        if(newType != (int)(platformType))
        {
            platformType = (BuildUtil.PlatformType)newType;
        }
        GUILayout.Label("Asset", EditorStyles.toolbarButton, GUILayout.MaxWidth(80));
        GUILayout.Label("资源版本号：", EditorStyles.toolbarButton, GUILayout.MaxWidth(80));
        GUILayout.Button(assetVersion, EditorStyles.toolbarButton, GUILayout.MaxWidth(80));

        GUILayout.Label("APP版本号：", EditorStyles.toolbarButton, GUILayout.MaxWidth(80));

        GUILayout.Button(AppVersion, EditorStyles.toolbarButton, GUILayout.MaxWidth(80));
        GUILayout.Label("启用打包配置 :", EditorStyles.toolbarButton, GUILayout.MaxWidth(80));
        GUILayout.Button("不热更包配置", EditorStyles.toolbarButton, GUILayout.MaxWidth(80));
        GUILayout.Button("设置资源目录", EditorStyles.toolbarButton, GUILayout.MaxWidth(80));
        GUILayout.Button("加载资源配置", EditorStyles.toolbarButton, GUILayout.MaxWidth(80));
        GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.MaxWidth(80));

        GUILayout.Label("", EditorStyles.toolbarButton);

        GUILayout.EndHorizontal();
    }

    private void LeftScrollView()
    {
        

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos,false,true, GUILayout.Width(letfScrollViewWidth), GUILayout.Height(position.height - topToolBarHeight));
        GUILayout.BeginVertical();
        GUILayout.Space(2);

        if (GUILayout.Button("清除标记", "LargeButton", GUILayout.MaxWidth(180)))
        {
            EditorGUIUtility.AddCursorRect(new Rect(0, 20, 180, 180), MouseCursor.Zoom);

            //BuildUtil.Reset();
            //BuildUtil.ClearAssetBundleName();
        }
        EditorGUILayout.LabelField("清除所有资源的AssetBundleName标记", EditorStyles.helpBox, GUILayout.MinHeight(20));
        GUILayout.Label("", "IN Title", GUILayout.MinWidth(letfScrollViewWidth));


        if (GUILayout.Button("标记", "LargeButton", GUILayout.MaxWidth(180)))
        {
            //BuildUtil.Reset();
            //BuildUtil.CheckDependencies(buidlePath);
            //BuildUtil.DependenciesLog();

        }
        EditorGUILayout.LabelField("标记Res路径需要打包的AssetBundleName", EditorStyles.helpBox, GUILayout.MinHeight(20));

        GUILayout.Label("", "IN Title", GUILayout.MinWidth(letfScrollViewWidth));


        if (GUILayout.Button("打包资源", "LargeButton", GUILayout.MaxWidth(180)))
        {

        }
        if (GUILayout.Button("发布更新资源", "LargeButton", GUILayout.MaxWidth(180)))
        {

        }
        if (GUILayout.Button("发布强更资源包", "LargeButton", GUILayout.MaxWidth(180)))
        {

        }


        GUILayout.Label("", "IN Title", GUILayout.MinWidth(letfScrollViewWidth));
        string text = "是否打包应用程序";
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            text = "是否打包 APK";
        }
        else if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            text = "是否打包 API";
        }
        GUILayout.Space(4);
        

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(text, this.isBuildAPP ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.MinWidth(160),GUILayout.MaxWidth(160));
        this.isBuildAPP = EditorGUILayout.Toggle(this.isBuildAPP, GUILayout.MaxWidth(30));
        GUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Development Build", this.BuildDebug ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.MinWidth(160), GUILayout.MaxWidth(160));
        this.BuildDebug = EditorGUILayout.Toggle(this.BuildDebug, GUILayout.MaxWidth(30));
        GUILayout.EndHorizontal();
        if(!this.BuildDebug)
        {
            this.ScriptDebug = false;
            this.AutoConnectProfile = false;
        }
        EditorGUI.BeginDisabledGroup(!this.BuildDebug);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Script Debugging", this.ScriptDebug ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.MinWidth(160), GUILayout.MaxWidth(160));
        this.ScriptDebug = EditorGUILayout.Toggle(this.ScriptDebug, GUILayout.MaxWidth(30));
        GUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Autoconnect Profile", this.AutoConnectProfile ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.MinWidth(160), GUILayout.MaxWidth(160));
        this.AutoConnectProfile = EditorGUILayout.Toggle(this.AutoConnectProfile, GUILayout.MaxWidth(30));
        GUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
        GUILayout.Label("", "IN Title", GUILayout.MinWidth(letfScrollViewWidth));


        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

    }
    private void RightScrollView()
    {

        //GUILayout.BeginVertical();
        //GUILayout.BeginHorizontal();
        //GUILayout.Label("资源版本号："+assetVersion, EditorStyles.boldLabel,GUILayout.MaxWidth(200));

        //GUILayout.Label("APP版本号："+AppVersion, EditorStyles.boldLabel, GUILayout.MaxWidth(200));
        //GUILayout.EndHorizontal();
        //GUILayout.Label("", "IN Title", GUILayout.MinWidth(RightScrollViewWidth));
        scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2 ,false,true, GUILayout.Width(RightScrollViewWidth), GUILayout.Height(position.height - rightHeightOffset));

        EditorGUILayout.EndScrollView();

        //GUILayout.EndVertical();
    }

}