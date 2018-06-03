//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using AL.SMB.Core;

namespace AL.SMB
{
    /// <summary>
    /// Component that manages a single combined mesh. 
    /// 
    /// This class is a Component. It must be added to a GameObject to use it. It is a wrapper for MB2_MeshCombiner which contains the same functionality but is not a component
    /// so it can be instantiated like a normal class.
    /// </summary>
    public class MeshBaker : MeshBakerCommon {	  
	
	    protected MeshCombinerSingle _meshCombiner = new MeshCombinerSingle();
	
	    public override MeshCombiner meshCombiner{
		    get{return _meshCombiner;}	
	    }
	
	    public void BuildSceneMeshObject(){
		    _meshCombiner.BuildSceneMeshObject();
	    }

	    public virtual bool ShowHide(GameObject[] gos, GameObject[] deleteGOs){
		    return _meshCombiner.ShowHideGameObjects(gos, deleteGOs);
	    }

	    public virtual void ApplyShowHide(){
		    _meshCombiner.ApplyShowHide();		
	    }
	
	    public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource){
    //		if ((_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoSceneObject || (_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoPrefab && _meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer) )) BuildSceneMeshObject();
		    _meshCombiner.name = name + "-mesh";
		    return _meshCombiner.AddDeleteGameObjects(gos,deleteGOs,disableRendererInSource);		
	    }
	
	    public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource){
    //		if ((_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoSceneObject || (_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoPrefab && _meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer) )) BuildSceneMeshObject();
		    _meshCombiner.name = name + "-mesh";
		    return _meshCombiner.AddDeleteGameObjectsByID(gos,deleteGOinstanceIDs,disableRendererInSource);
	    }
    }


}
