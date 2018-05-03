using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Reflection;
using System;
using System.Text;

public class SDKConfigCreator : EditorWindow {

    [MenuItem("AssetBundlePacker/SDK配置工具", false, 2)]
    static void AddWindow()
    {
        //创建窗口
        Rect wr = new Rect(0, 0, 500, 600);
        SDKConfigCreator window = (SDKConfigCreator)EditorWindow.GetWindowWithRect(typeof(SDKConfigCreator), wr, true, "SDK配置工具");
        window.Show();
    }

    public string file_name;

    public string editor_name;

    int last_item_cnt;
    public int sdk_item_cnt;

    private Vector2 scrollPos = Vector2.zero;
    //private Rect scrollViewRect;

    public struct SDKCFGItem
    {
        public int development;

        public int use_sdk;
        public string sdk_name;

        public string company_name;
        public string game_name;
        public string cn_name;
        public string app_icon;
        public string channel;
        public string config_path;
        public string sdk_path;
        public string upload_script;
        public string keystore;

        public void ShowPannel()
        {
            development = EditorGUILayout.IntField("    development", development);
            use_sdk = EditorGUILayout.IntField("    use_sdk", use_sdk);
            sdk_name = EditorGUILayout.TextField("    sdk_name", sdk_name);

            company_name = EditorGUILayout.TextField("    company_name", company_name);
            game_name = EditorGUILayout.TextField("    game_name", game_name);
            cn_name = EditorGUILayout.TextField("    cn_name", cn_name);
            app_icon = EditorGUILayout.TextField("    app_icon", app_icon);
            channel = EditorGUILayout.TextField("    channel", channel);
            config_path = EditorGUILayout.TextField("    config_path", config_path);
            sdk_path = EditorGUILayout.TextField("    sdk_path", sdk_path);
            upload_script = EditorGUILayout.TextField("    upload_script", upload_script);
            keystore = EditorGUILayout.TextField("    development", keystore);
        }
    }

    public SDKCFGItem[] sdk_items;

    void ShowAddSDKCFGEditor()
    {
        file_name = EditorGUILayout.TextField("config文件名:", file_name);
        editor_name = EditorGUILayout.TextField("editor_name:", editor_name);

        sdk_item_cnt = EditorGUILayout.IntField("sdk配置数量", sdk_item_cnt);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        //EditorGUILayout.BeginVertical();
        if (sdk_item_cnt != last_item_cnt)
            sdk_items = new SDKCFGItem[sdk_item_cnt];

        last_item_cnt = sdk_item_cnt;
        for (int i = 0; i < sdk_item_cnt; ++i)
        {
            EditorGUILayout.Space();
            EditorGUILayout.TextField(string.Format("  SDK配置{0}", i + 1));
            sdk_items[i].ShowPannel();
        }
        
        //scrollViewRect = GUILayoutUtility.GetLastRect();

        if (GUILayout.Button("生成Config文件", GUILayout.Width(200)))
        {
            if (!CreateConfig())
                Debug.LogError("生成Config文件失败");
        }
        EditorGUILayout.EndScrollView();
    }

    public string ToJsonString()
    {
        Hashtable tbl = new Hashtable();
        tbl.Add("editor_name", editor_name);

        Hashtable[] items = new Hashtable[sdk_item_cnt];
        for (int i = 0; i < sdk_item_cnt; ++i)
        {
            Hashtable item = new Hashtable();
            item.Add("development", sdk_items[i].development);
            item.Add("use_sdk", sdk_items[i].use_sdk);
            item.Add("sdk_name", sdk_items[i].sdk_name);
            item.Add("company_name", sdk_items[i].company_name);
            item.Add("game_name", sdk_items[i].game_name);
            item.Add("cn_name", sdk_items[i].cn_name);
            item.Add("app_icon", sdk_items[i].app_icon);
            item.Add("channel", sdk_items[i].channel);
            item.Add("config_path", sdk_items[i].config_path);
            item.Add("sdk_path", sdk_items[i].sdk_path);
            item.Add("upload_script", sdk_items[i].upload_script);
            item.Add("keystore", sdk_items[i].keystore);
            items[i] = item;
        }

        tbl.Add("items", items);
        return MiniJSON.Json.Serialize(tbl);
    }

    public bool CreateConfig()
    {
        if (string.IsNullOrEmpty(file_name))
        {
            Debug.LogError("请先输入文件保存路径!");
            return false;
        }

        if (File.Exists(file_name)) File.Delete(file_name);

        string jsonStr = ToJsonString();
        FileManager.WriteFile(file_name, jsonStr);


        Debug.Log("生成config文件成功!");
        return true;
    }
    void OnGUI()
    {
        ShowAddSDKCFGEditor();


    }
}
