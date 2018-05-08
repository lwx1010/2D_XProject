using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIElementAttribute))]
[CanEditMultipleObjects]

public class UIElementInspector : Editor
{


    public UIElementAttribute ElementAttribute;
    public SerializedObject ElementSerialized;

    public override void OnInspectorGUI()
    {
        if ((ElementAttribute == null))
        {
            return;
        }
        if (ElementAttribute.elements == null)
        {
            return;
        }

            if (!Application.isPlaying)
            {
                ElementAttribute.InitElement();
            }
            DrawInspector();
        
    }
    public void OnEnable()
    {
        ElementAttribute = target as UIElementAttribute;
        if ((ElementAttribute == null))
        {
            return;
        }
        if (ElementAttribute.elements == null)
        {
            return;
        }
        if (!Application.isPlaying)
        {
            ElementAttribute.InitElement();
        }
        ElementSerialized = new SerializedObject(ElementAttribute);
    }
    public void OnDisable()
    {
        if (target == null)
        {
            return;
        }
    }
    bool useScripte;
    bool isError;
    private void DrawInspector()
    {
        if ((ElementAttribute == null))
        {
            return;
        }
        if (ElementAttribute.elements == null)
        {
            return;
        }


        EditorGUILayout.BeginVertical();

        if(isError != ElementAttribute.isError)
        {
            isError = ElementAttribute.isError;
        }
        if (isError)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset", GUILayout.Width(60), GUILayout.Height(40), GUILayout.MaxWidth(60)))
            {
                ElementAttribute.Reset();
            }
            EditorGUILayout.LabelField("", GUILayout.MaxWidth(52));
            EditorGUILayout.HelpBox("配 置 有 问 题，请 Reset 一 下", MessageType.Error,true);

            EditorGUILayout.EndHorizontal();
        }
        //空行
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("UI Elements ", GUILayout.MaxWidth(116));
        EditorGUILayout.TextField("", ElementAttribute.elements.Count.ToString(), GUILayout.MaxWidth(60));
        EditorGUILayout.LabelField("Elements Dict", GUILayout.MaxWidth(100));
        EditorGUILayout.TextField("", ElementAttribute.elementsDict.Count.ToString(), GUILayout.MaxWidth(60));
        EditorGUILayout.LabelField("Element ID Dict ", GUILayout.MaxWidth(100));
        EditorGUILayout.TextField("", ElementAttribute.elementsIDDict.Count.ToString(), GUILayout.MaxWidth(60));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Parent Name:", GUILayout.MaxWidth(115));

        ElementAttribute.ParentName =EditorGUILayout.TextField("", ElementAttribute.ParentName, GUILayout.MaxWidth(160));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Editor State", GUILayout.MaxWidth(115));


        EditorState state = (EditorState)EditorGUILayout.EnumPopup(ElementAttribute.m_state);
        if (ElementAttribute.m_state != state)
        {
            ElementAttribute.m_state = state;
            EditorUtility.SetDirty(ElementAttribute);
        }
        int nameWidth = 120;
        if(state == EditorState.AutoEditorID)
        {
            nameWidth = 200;
        }
        EditorGUILayout.EndHorizontal();



        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("T", GUILayout.MaxWidth(20));
        EditorGUILayout.LabelField("控件名称", GUILayout.MinWidth(80), GUILayout.MaxWidth(nameWidth));
        EditorGUILayout.LabelField("UI类型", GUILayout.MaxWidth(65));
        EditorGUILayout.LabelField("图片名称", GUILayout.MaxWidth(120));
        EditorGUILayout.LabelField("贴图名称", GUILayout.MaxWidth(80));
        EditorGUILayout.LabelField("字体名称", GUILayout.MaxWidth(80));
        EditorGUILayout.LabelField("图集名称", GUILayout.MaxWidth(80));
        EditorGUILayout.LabelField("材质名称", GUILayout.MaxWidth(80));
        EditorGUILayout.LabelField("文本内容", GUILayout.MaxWidth(80));

        EditorGUILayout.EndHorizontal();
        //空行
        EditorGUILayout.Space();

        foreach (var ElementItem in ElementAttribute.elements)
        {
            EditorGUILayout.BeginHorizontal();
            //UIElement uiElement = ElementItem.Value;
            UIElement uiElement = ElementItem;
            useScripte = EditorGUILayout.Toggle(uiElement.m_useScript, GUILayout.MaxWidth(20));
            if (useScripte != uiElement.m_useScript)
            {
                uiElement.m_useScript = useScripte;
                EditorUtility.SetDirty(ElementAttribute);

            }
            EditorGUILayout.ObjectField(uiElement.m_gameObject, typeof(GameObject), true, GUILayout.Width(80));

            //Disabled掉，是表示不可编辑
            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.LabelField(uiElement.m_gameObject.name, GUILayout.MinWidth(100) ,GUILayout.MaxWidth(nameWidth));
            EditorGUILayout.TextField("", uiElement.m_type.ToString(), GUILayout.MaxWidth(65));

            EditorGUILayout.TextField("", uiElement.m_imageName, GUILayout.MaxWidth(120));
            EditorGUILayout.TextField("", uiElement.m_imageTexture, GUILayout.MaxWidth(80));
            EditorGUILayout.TextField("", uiElement.m_font, GUILayout.MaxWidth(80));
            EditorGUILayout.TextField("", uiElement.m_atalsName, GUILayout.MaxWidth(80));
            EditorGUILayout.TextField("", uiElement.m_material, GUILayout.MaxWidth(80));

            EditorGUILayout.TextField("", uiElement.m_textContent, GUILayout.MaxWidth(80));

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }
        if (ElementAttribute.elementsDict.Count <= 0)
        {
            ElementAttribute.RefreshElementDict();
        }


        EditorGUILayout.EndVertical();
    }
}