using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShadowProjector))] 
public class ShadowProjectorEditor : Editor {
    
	Rect UVRect;
	
	public override void OnInspectorGUI() {
		serializedObject.Update ();

        ShadowProjector shadowProj = (ShadowProjector) target;
        shadowProj.ShadowSize = EditorGUILayout.FloatField("Shadow size", shadowProj.ShadowSize);
		
		shadowProj.ShadowColor = EditorGUILayout.ColorField("Shadow color", shadowProj.ShadowColor);
		
		shadowProj.ShadowOpacity = EditorGUILayout.Slider("Shadow opacity", shadowProj.ShadowOpacity, 0.0f, 1.0f);
		
		shadowProj._Material = (Material)EditorGUILayout.ObjectField("Shadow material", (Object)shadowProj._Material, typeof(Material), false, null);
		
		EditorGUILayout.LabelField("Shadow UV Rect");

		UVRect = shadowProj.UVRect;

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("X:", GUILayout.MaxWidth(15));
		UVRect.x = EditorGUILayout.FloatField(UVRect.x, GUILayout.ExpandWidth(true));
		EditorGUILayout.LabelField("Y:", GUILayout.MaxWidth(15));
		UVRect.y = EditorGUILayout.FloatField(UVRect.y, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("W:", GUILayout.MaxWidth(15));
		UVRect.width = EditorGUILayout.FloatField(UVRect.width , GUILayout.ExpandWidth(true));
		EditorGUILayout.LabelField("H:", GUILayout.MaxWidth(15));
		UVRect.height = EditorGUILayout.FloatField(UVRect.height, GUILayout.ExpandWidth(true));
		
		EditorGUILayout.EndHorizontal();
		
		shadowProj.ShadowLocalOffset = EditorGUILayout.Vector3Field("Shadow local offset", shadowProj.ShadowLocalOffset, null);
		
	
		shadowProj.RotationAngleOffset = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation angle offsets", shadowProj.RotationAngleOffset.eulerAngles));
		
		EditorGUILayout.LabelField("Freeze rotation:");
		shadowProj.FreezeXRot = EditorGUILayout.Toggle("  X", shadowProj.FreezeXRot);
		shadowProj.FreezeYRot = EditorGUILayout.Toggle("  Y", shadowProj.FreezeYRot);
		shadowProj.FreezeZRot = EditorGUILayout.Toggle("  Z", shadowProj.FreezeZRot);
		
		if (GUILayout.Button("Open UV Editor")) {
			ShadowTextureUVEditor.Open(shadowProj);
		}
		
		shadowProj.AutoSizeOpacity = EditorGUILayout.BeginToggleGroup("Auto opacity/size:", shadowProj.AutoSizeOpacity);
		shadowProj.AutoSORaycastLayer = EditorGUILayout.LayerField("Raycast layer", shadowProj.AutoSORaycastLayer);
		shadowProj.AutoSORayOriginOffset = EditorGUILayout.FloatField("Ray origin offset", shadowProj.AutoSORayOriginOffset);
		shadowProj.AutoSOCutOffDistance = EditorGUILayout.FloatField("Cutoff distance", shadowProj.AutoSOCutOffDistance);
		shadowProj.AutoSOMaxScaleMultiplier = EditorGUILayout.FloatField("Max scale multiplier", shadowProj.AutoSOMaxScaleMultiplier);

		EditorGUILayout.EndToggleGroup();

		shadowProj.UVRect = UVRect;

		serializedObject.ApplyModifiedProperties();
        
		if (GUI.changed)
			EditorUtility.SetDirty (target);
	}



}
