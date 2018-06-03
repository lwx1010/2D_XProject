
namespace AL.SMB.Core{
	
	public enum MB_ObjsToCombineTypes{
		prefabOnly,
		sceneObjOnly,
		dontCare
	}
	
	public enum MB_RenderType{
		meshRenderer,
		skinnedMeshRenderer
	}
	
	public enum MB2_OutputOptions{
		bakeIntoSceneObject,
		bakeMeshAssetsInPlace,
		bakeIntoPrefab
	}
	
	public enum MB2_LightmapOptions{
		preserve_current_lightmapping,
		ignore_UV2,
		copy_UV2_unchanged,
		generate_new_UV2_layout,
        copy_UV2_unchanged_to_separate_rects,
    }
    
	public enum MBValidationLevel{
		none,
		quick,
		robust
	}
    
}
