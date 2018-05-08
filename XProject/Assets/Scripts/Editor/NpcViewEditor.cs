using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(NpcView))]
public class NpcViewEditor : Editor {

    NpcView npcView;

    private SerializedProperty npcId;

    void OnEnable()
    {
        npcView = target as NpcView;
        npcId = serializedObject.FindProperty("npcId");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(npcId, new GUIContent("npc编号"));

        if (GUILayout.Button("add npc"))
            npcView.addNpc();

        serializedObject.ApplyModifiedProperties();
    }
}
