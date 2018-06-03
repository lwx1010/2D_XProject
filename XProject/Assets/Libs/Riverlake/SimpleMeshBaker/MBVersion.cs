/**
 *	\brief Hax!  DLLs cannot interpret preprocessor directives, so this class acts as a "bridge"
 */
using System;
using UnityEngine;

namespace AL.SMB.Core{

	public static class MBVersion
    {
		public static string version(){
			return "3.19";	
		}
		
		public static int GetMajorVersion(){
            /*
            #if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
                        return 3;
            #elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
                        return 4;
            #else
                        return 5;
            #endif
            */
            string v = Application.unityVersion;
            String[] vs = v.Split(new char[] { '.' });
            return Int32.Parse(vs[0]);
        }

        public static int GetMinorVersion(){
            
            string v = Application.unityVersion;
            String[] vs = v.Split(new char[] { '.' });
            return Int32.Parse(vs[1]);
        }
		
		public static bool GetActive(GameObject go){
			return go.activeInHierarchy;
		}
		
		public static void SetActive(GameObject go, bool isActive){
			go.SetActive(isActive);
		}
		
		public static void SetActiveRecursively(GameObject go, bool isActive){
			go.SetActive(isActive);
		}
		
		public static UnityEngine.Object[] FindSceneObjectsOfType(Type t){
			return GameObject.FindObjectsOfType(t);
		}
		
		public static bool IsRunningAndMeshNotReadWriteable(Mesh m){
			return !m.isReadable;
		}

		private static Vector2 _HALF_UV = new Vector2(.5f, .5f);
		public static Vector2[] GetMeshUV1s(Mesh m, MBLogLevel LOG_LEVEL)
		{
			Vector2[] uv;
			if (LOG_LEVEL >= MBLogLevel.warn) MBLog.LogDebug("UV1 does not exist in Unity 5+");
			uv = m.uv;
			if (uv.Length == 0){
				if (LOG_LEVEL >= MBLogLevel.debug) MBLog.LogDebug("Mesh " + m + " has no uv1s. Generating");
				if (LOG_LEVEL >= MBLogLevel.warn) Debug.LogWarning("Mesh " + m + " didn't have uv1s. Generating uv1s.");			
				uv = new Vector2[m.vertexCount];
				for (int i = 0; i < uv.Length; i++){uv[i] = _HALF_UV;}
			}		
			return uv;
		}

        public static Vector2[] GetMeshUV3orUV4(Mesh m, bool get3, MBLogLevel LOG_LEVEL) {
            Vector2[] uvs;
            if (get3) uvs = m.uv3;
            else uvs = m.uv4;

            if (uvs.Length == 0) {
                if (LOG_LEVEL >= MBLogLevel.debug) MBLog.LogDebug("Mesh " + m + " has no uv" + (get3 ? "3" : "4") + ". Generating");
                uvs = new Vector2[m.vertexCount];
                for (int i = 0; i < uvs.Length; i++) { uvs[i] = _HALF_UV; }
            }
            return uvs;
        }

        public static void MeshClear(Mesh m, bool t){
			m.Clear(t);
		}

		public static void MeshAssignUV3(Mesh m, Vector2[] uv3s){
            m.uv3 = uv3s;
        }

        public static void MeshAssignUV4(Mesh m, Vector2[] uv4s) {
            m.uv4 = uv4s;
        }

        public static Vector4 GetLightmapTilingOffset(Renderer r){
			return r.lightmapScaleOffset; //r.lightmapScaleOffset ;
		}

        public static Transform[] GetBones(Renderer r)
        {
            if (r is SkinnedMeshRenderer)
            {
                Transform[] bone;
                //check if I need to deoptimize
                bool didDeoptimize = false;
                Animator anim = r.GetComponentInParent<Animator>();

                if (anim != null)
                {
                    if (anim.hasTransformHierarchy)
                    {
                        //nothing to do
                    } else if (anim.isOptimizable)
                    {
                        //Deoptimize
                        AnimatorUtility.DeoptimizeTransformHierarchy(anim.gameObject);
                        didDeoptimize = true;
                    }
                    else
                    {
                        Debug.LogError("Could not getBones. Bones optimized but could not create TransformHierarchy.");
                        return null;
                    }
                    bone = ((SkinnedMeshRenderer)r).bones;
                    //can't deoptimize here because the transforms need to exist for the combined  mesh
                } else
                {
                    //no Animator component but check to see if bones were optimized on import
                    bone = ((SkinnedMeshRenderer)r).bones;
#if UNITY_EDITOR
                    if (bone.Length == 0)
                    {
					    Mesh m = ((SkinnedMeshRenderer)r).sharedMesh;
					    if (m.bindposes.Length != bone.Length)
                            Debug.LogError("SkinnedMesh (" + r.gameObject + ") in the list of objects to combine has no bones. Check that 'optimize game object' is not checked in the 'Rig' tab of the asset importer. Mesh Baker cannot combine optimized skinned meshes because the bones are not available.");
				    }
#endif
                }
				return bone;	
            }
            else if (r is MeshRenderer)
            {
				Transform[] bone = new Transform[1];
				bone[0] = r.transform;
				return bone;
            }
            else {
                Debug.LogError("Could not getBones. Object is not a Renderer.");
				return null;
			}
		}
	}
}