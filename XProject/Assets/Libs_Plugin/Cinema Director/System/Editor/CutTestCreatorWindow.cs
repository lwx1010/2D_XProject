using CinemaDirector;
using LuaFramework;
using UnityEditor;
using UnityEngine;

public class CutTestCreatorWindow : EditorWindow
{
    #region UI Fields
    private string txtCutsceneName = "CutTest";
    private float txtDuration = 100;
    private DirectorHelper.TimeEnum timeEnum = DirectorHelper.TimeEnum.Seconds;

    private StartMethod StartMethod = StartMethod.None;
    
    #endregion
    
    #region Language

    const string TITLE = "Creator";
    
    GUIContent NameContentCutscene = new GUIContent("Name", "The name of the CutTest to be created");
    GUIContent DurationContentCutscene = new GUIContent("Duration", "The duration of the CutTest in seconds/minutes");
    
    const string MENU_ITEM = "Tools/剧情编辑器/创建测试流程";
    #endregion


    /// <summary>
    /// Sets the window title and minimum pane size
    /// </summary>
    public void Awake()
    {

#if UNITY_5 && !UNITY_5_0
        base.titleContent = new GUIContent(TITLE);
#else
        base.title = TITLE;
#endif

        this.minSize = new Vector2(250f, 150f);
    }

    [MenuItem(MENU_ITEM, false, 10)]
    static void Init()
    {
        EditorWindow.GetWindow<CutTestCreatorWindow>();
    }

    /// <summary>
    /// Draws the Director GUI
    /// </summary>
    protected void OnGUI()
    {

        txtCutsceneName = EditorGUILayout.TextField(NameContentCutscene, txtCutsceneName);

        EditorGUILayout.BeginHorizontal();
        txtDuration = EditorGUILayout.FloatField(DurationContentCutscene, txtDuration);
        timeEnum = (DirectorHelper.TimeEnum)EditorGUILayout.EnumPopup(timeEnum);
        EditorGUILayout.EndHorizontal();

        StartMethod = StartMethod.OnStart;
        
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Create CutTest"))
        {
            string cutsceneName = DirectorHelper.getCutsceneItemName(txtCutsceneName, typeof(Cutscene));

            GameObject cutsceneGO = new GameObject(cutsceneName);
            Cutscene cutscene = cutsceneGO.AddComponent<Cutscene>();
            cutsceneGO.AddComponent<CutTestStarter>();

            createNguiFlag(cutsceneGO);
            CutsceneItemFactory.CreateUTestTrackGroup(cutscene);
                
            float duration = txtDuration;
            if (timeEnum == DirectorHelper.TimeEnum.Minutes) duration *= 60;
            cutscene.Duration = duration;
            cutscene.IsSkippable = false;

            int undoIndex = Undo.GetCurrentGroup();
            Undo.RegisterCreatedObjectUndo(cutsceneGO, string.Format("Created {0}",txtCutsceneName));
            Undo.CollapseUndoOperations(undoIndex);

            Selection.activeTransform = cutsceneGO.transform;
        }

        GUILayout.Space(10);
    }


    private void createNguiFlag(GameObject cutTest)
    {
        GameObject obj = new GameObject("NguiPanel");
        UIPanel panel = obj.AddComponent<UIPanel>();
        panel.depth = 500;

        GameObject mouse = new GameObject("Mouse");
        //Util.SetParent(mouse , obj);
        UISprite sprite = mouse.AddComponent<UISprite>();
        sprite.atlas = AssetDatabase.LoadAssetAtPath<UIAtlas>("Assets/Res/Atlas/common/common.prefab");
        sprite.spriteName = "ui_liaotian_tishihd01";

        //Util.SetParent(obj , cutTest);
    }
}