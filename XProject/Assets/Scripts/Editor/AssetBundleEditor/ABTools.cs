using UnityEngine;
using System.Collections;
using UnityEditor;

public class ABTools
{
    static public Texture2D blankTexture
    {
        get
        {
            return EditorGUIUtility.whiteTexture;
        }
    }

    static public void DrawSeparator()
    {
        GUILayout.Space(12f);

        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = blankTexture;
            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.color = new Color(0f, 0f, 0f, 0.25f);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
            GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
            GUI.color = Color.white;
        }
    }

    static public bool DrawPrefixButton(string text)
    {
        return GUILayout.Button(text, "DropDown", GUILayout.Width(76f));
    }

    static public bool DrawPrefixButton(string text, params GUILayoutOption[] options)
    {
        return GUILayout.Button(text, "DropDown", options);
    }

    static public int DrawPrefixList(int index, string[] list, params GUILayoutOption[] options)
    {
        return EditorGUILayout.Popup(index, list, "DropDown", options);
    }

    static public int DrawPrefixList(string text, int index, string[] list, params GUILayoutOption[] options)
    {
        return EditorGUILayout.Popup(text, index, list, "DropDown", options);
    }
}
