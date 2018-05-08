//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using Riverlake.SMB.Core;

namespace Riverlake.SMB
{
    /// <summary>
    /// Component that is an endless mesh. You don't need to worry about the 65k limit when adding meshes. It is like a List of combined meshes. Internally it manages
    /// a collection of CombinedMeshes that are added and deleted as necessary. 
    /// 
    /// Note that this implementation does
    /// not attempt to split meshes. Each mesh is added to one of the internal meshes as an atomic unit.
    /// 
    /// This class is a Component. It must be added to a GameObject to use it. It is a wrapper for MB2_Multi_meshCombiner which contains the same functionality but is not a component
    /// so it can be instantiated like a normal class.
    /// </summary>
    public class MultiMeshBaker : MeshBakerCommon {
		
	    protected MultiMeshCombiner _meshCombiner = new MultiMeshCombiner();

	    public override MeshCombiner meshCombiner{
		    get {return _meshCombiner;}	
	    }		
	
	    public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource){
		    if (_meshCombiner.resultSceneObject == null){
			    _meshCombiner.resultSceneObject = new GameObject("CombinedMesh-" + name);	
		    }
		    meshCombiner.name = name + "-mesh";
		    return _meshCombiner.AddDeleteGameObjects(gos,deleteGOs,disableRendererInSource);		
	    }
	
	    public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOs, bool disableRendererInSource){
		    if (_meshCombiner.resultSceneObject == null){
			    _meshCombiner.resultSceneObject = new GameObject("CombinedMesh-" + name);	
		    }
		    meshCombiner.name = name + "-mesh";
		    return _meshCombiner.AddDeleteGameObjectsByID(gos,deleteGOs,disableRendererInSource);	
	    }
    }

}

