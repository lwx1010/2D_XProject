using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FSG.MeshAnimator
{
    public class MeshAnimationBake
    {

        [SerializeField] private GameObject spawnedAsset;

        [SerializeField] private GameObject prefab;

        public List<SkinnedMeshRenderer> skinnedRenderers = new List<SkinnedMeshRenderer>();

        private Dictionary<string, bool> bakeAnims = new Dictionary<string, bool>();
        private Dictionary<string, int> frameSkips = new Dictionary<string, int>();

        private struct CombineInstanceMaterial
        {
            public CombineInstance combine;
            public Material material;
            public Mesh sharedMesh;
        }

        public Dictionary<string, bool> BakeAnims
        {
            get { return bakeAnims; }
        }

        public Dictionary<string, int> FrameSkips
        {
            get { return frameSkips; }
        }



        public void OnPrefabChanged(GameObject newPrefab)
        {
            prefab = newPrefab;
            if (spawnedAsset)
                GameObject.DestroyImmediate(spawnedAsset.gameObject);
            if (Application.isPlaying)
            {
                return;
            }
            if (prefab)
            {
                if (spawnedAsset == null)
                {
                    spawnedAsset = GameObject.Instantiate(prefab) as GameObject;
                    SetChildFlags(spawnedAsset.transform, HideFlags.HideAndDontSave);
                }
                bakeAnims.Clear();
                frameSkips.Clear();
                AutoPopulateFiltersAndRenderers();
            }
        }

        private void SetChildFlags(Transform t, HideFlags flags)
        {
            Queue<Transform> q = new Queue<Transform>();
            q.Enqueue(t);
            for (int i = 0; i < t.childCount; i++)
            {
                Transform c = t.GetChild(i);
                q.Enqueue(c);
                SetChildFlags(c, flags);
            }
            while (q.Count > 0)
            {
                q.Dequeue().gameObject.hideFlags = flags;
            }
        }

        private void AutoPopulateFiltersAndRenderers()
        {
            skinnedRenderers.Clear();
            MeshFilter[] filtersInPrefab = spawnedAsset.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < filtersInPrefab.Length; i++)
            {
                if (filtersInPrefab[i].GetComponent<MeshRenderer>())
                    filtersInPrefab[i].GetComponent<MeshRenderer>().enabled = false;
            }
            SkinnedMeshRenderer[] renderers = spawnedAsset.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (skinnedRenderers.Contains(renderers[i]) == false)
                    skinnedRenderers.Add(renderers[i]);
                renderers[i].enabled = false;
            }
        }

        public List<AnimationClip> GetClips(List<AnimationClip> customClips)
        {
            var clips = EditorUtility.CollectDependencies(new Object[] {prefab}).ToList();
            foreach (var obj in clips.ToArray())
                clips.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(obj)));
            if (customClips != null)
                clips.AddRange(customClips.Select(q => (Object) q));
            clips.RemoveAll(q => q is AnimationClip == false || q == null);
            foreach (AnimationClip clip in clips)
            {
                if (bakeAnims.ContainsKey(clip.name) == false)
                    bakeAnims.Add(clip.name, true);
            }
            clips.RemoveAll(q => bakeAnims.ContainsKey(q.name) == false);
            clips.RemoveAll(q => bakeAnims[q.name] == false);

            var distinctClips = clips.Select(q => (AnimationClip) q).Distinct().ToList();

            for (int i = 0; i < distinctClips.Count; i++)
            {
                if (bakeAnims.ContainsKey(distinctClips[i].name) == false)
                    bakeAnims.Add(distinctClips[i].name, true);
            }
            return distinctClips;
        }

        public void CreateSnapshots(string assetPath, List<AnimationClip> customClips, List<string> exposedTransforms)
        {
            CreateSnapshots(assetPath, customClips, exposedTransforms, 30, -1,null);
        }

        public void CreateSnapshots(string assetPath, List<AnimationClip> customClips, List<string> exposedTransforms,
                                    int fps, int smoothMeshAngle, List<KeyValuePair<int, float>> lodDistances)
        {
            try
            {
                if (string.IsNullOrEmpty(assetPath))
                {
                    EditorUtility.DisplayDialog("Mesh Animator",
                        "Unable to locate the asset path for prefab: " + prefab.name, "OK");
                    return;
                }

                HashSet<string> allAssets = new HashSet<string>();

                var clips = GetClips(customClips);
                foreach (var clip in clips)
                    allAssets.Add(AssetDatabase.GetAssetPath(clip));

                string suffix = Path.GetExtension(assetPath);
                string assetFolder = assetPath.Replace(suffix, "");
                string assetAnimFolder = assetFolder + "/Anims";

                if (!Directory.Exists(assetFolder)) Directory.CreateDirectory(assetFolder);
                if (!Directory.Exists(assetAnimFolder)) Directory.CreateDirectory(assetAnimFolder);

                var sampleGO = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                if (skinnedRenderers.Count(q => q) == 0)
                {
                    throw new System.Exception("Bake Error! No MeshFilter's or SkinnedMeshRender's found to bake!");
                }
                else
                {
                    GameObject asset = new GameObject(prefab.name);
                    MeshAnimator ma = asset.AddComponent<MeshAnimator>();
                    List<Material> mats = GatherMaterials(skinnedRenderers);
                    asset.AddComponent<MeshRenderer>().sharedMaterials = mats.ToArray();
                    List<MeshAnimation> createdAnims = new List<MeshAnimation>();
                    int animCount = 0;
                    Transform rootMotionBaker = new GameObject().transform;
                    foreach (AnimationClip animClip in clips)
                    {
                        if (bakeAnims.ContainsKey(animClip.name) && bakeAnims[animClip.name] == false) continue;
                        if (frameSkips.ContainsKey(animClip.name) == false)
                        {
                            Debug.LogWarningFormat("No animation with name {0} in frame skips", animClip.name);
                            continue;
                        }
                        string meshAnimationPath = string.Format("{0}/{1}_{2}.asset", assetAnimFolder, prefab.name,
                            FormatClipName(animClip.name));
                        MeshAnimation meshAnim =
                            AssetDatabase.LoadAssetAtPath(meshAnimationPath, typeof (MeshAnimation)) as MeshAnimation;
                        bool create = false;
                        if (meshAnim == null)
                        {
                            meshAnim = ScriptableObject.CreateInstance<MeshAnimation>();
                            create = true;
                        }
                        meshAnim.name = animClip.name;
                        meshAnim.length = animClip.length;
                        // create exposed transforms
                        int exposedCount = exposedTransforms != null ? exposedTransforms.Count : 0;
                        meshAnim.exposedTransforms = new string[exposedCount];
                        for (int i = 0; i < meshAnim.exposedTransforms.Length; i++)
                        {
                            Transform t = sampleGO.transform.Find(exposedTransforms[i]);
                            if (t)
                            {
                                string[] splitT = exposedTransforms[i].Split('/');
                                string lastName = splitT[splitT.Length - 1];
                                meshAnim.exposedTransforms[i] = lastName;
                            }
                        }
                        int bakeFrames = Mathf.CeilToInt(animClip.length*fps);
                        int frame = 0;
                        List<MeshFrameData> verts = new List<MeshFrameData>();
                        List<Vector3> meshesInFrame = new List<Vector3>();
                        float lastFrameTime = 0;
                        for (int i = 0; i <= bakeFrames; i += frameSkips[animClip.name])
                        {
                            float bakeDelta = Mathf.Clamp01(((float) i/bakeFrames));
                            EditorUtility.DisplayProgressBar("Baking Animation",
                                                            string.Format("Processing: {0} Frame: {1}", animClip.name, i), 
                                                            bakeDelta);
                            float animationTime = bakeDelta*animClip.length;
                            GameObject sampleObject = sampleGO;
                            Animation legacyAnimation = sampleObject.GetComponentInChildren<Animation>();
                            if (legacyAnimation && legacyAnimation.gameObject != sampleObject)
                                sampleObject = legacyAnimation.gameObject;
                            animClip.SampleAnimation(sampleObject, animationTime);

                            meshesInFrame.Clear();

                            Mesh m = null;
                            List<SkinnedMeshRenderer> sampleSkinnedRenderers = new List<SkinnedMeshRenderer>();
                            foreach (SkinnedMeshRenderer sr in skinnedRenderers)
                            {
                                Transform matchTrans = FindMatchingTransform(prefab.transform, sr.transform,
                                    sampleGO.transform);
                                SkinnedMeshRenderer sampleSr = matchTrans.GetComponent<SkinnedMeshRenderer>();
                                sampleSkinnedRenderers.Add(sampleSr);
                                m = new Mesh();
                                sampleSr.BakeMesh(m);
                                Vector3[] v = m.vertices;
                                for (int vIndex = 0; vIndex < v.Length; vIndex++)
                                {
                                    v[vIndex] = sampleSr.transform.TransformPoint(v[vIndex]);
                                }
                                meshesInFrame.AddRange(v);
                                GameObject.DestroyImmediate(m);
                            }
                            bool combinedMeshes = false;
                            var combinedInFrame = GenerateCombinedMesh(sampleSkinnedRenderers, out combinedMeshes);
                            if (combinedMeshes)
                            {
                                meshesInFrame = combinedInFrame.vertices.ToList();
                                GameObject.DestroyImmediate(combinedInFrame);
                            }

                            MeshFrameData data = new MeshFrameData();
                            data.SetVerts(meshesInFrame.ToArray());
                            verts.Add(data);
                            // bake exposed tranforms
                            data.exposedTransforms = new Matrix4x4[exposedCount];
                            for (int j = 0; j < data.exposedTransforms.Length; j++)
                            {
                                Transform t = sampleGO.transform.Find(exposedTransforms[j]);
                                if (t)
                                {
                                    data.exposedTransforms[j] = Matrix4x4.TRS(
                                        rootMotionBaker.InverseTransformPoint(t.position),
                                        Quaternion.Inverse(rootMotionBaker.rotation)*t.rotation,
                                        t.localScale);
                                }
                            }

                            // debug only
                            // Instantiate(sampleGO, frame * Vector3.right, Quaternion.identity);

                            frame++;
                        }
                        meshAnim.rootMotionMode = MeshAnimation.RootMotionMode.None;
                        meshAnim.smoothNormalsAngle = smoothMeshAngle;
                        meshAnim.compressedFrameData = verts.ToArray();
                        meshAnim.animationName = animClip.name;
                        meshAnim.wrapMode = animClip.isLooping ? WrapMode.Loop : animClip.wrapMode;
                        meshAnim.frameSkip = frameSkips[animClip.name];

                        if (create)
                        {
                            AssetDatabase.CreateAsset(meshAnim, meshAnimationPath);
                        }
                        AssetDatabase.SaveAssets();

                        createdAnims.Add(meshAnim);
                        animCount++;
                    }
                    GameObject.DestroyImmediate(rootMotionBaker.gameObject);

                    ma.animations = createdAnims.ToArray();
                    string meshPath = string.Format("{0}/{1}_Mesh.asset", assetFolder, prefab.name);
                    bool combined = false;
                    Mesh combinedMesh = GenerateCombinedMesh(out combined);
                    Mesh existingMesh = null;
                    if (combined)
                    {
                        existingMesh = (Mesh) AssetDatabase.LoadAssetAtPath(meshPath, typeof (Mesh));
                        if (existingMesh)
                        {
                            TransferMesh(combinedMesh, existingMesh);
                            combinedMesh = existingMesh;
                        }
                    }
                    else
                    {
                        existingMesh = new Mesh() {name = combinedMesh.name};
                        TransferMesh(combinedMesh, existingMesh);
                        combinedMesh = existingMesh;
                        AssetDatabase.CreateAsset(combinedMesh, meshPath);
                    }
                    ma.baseMesh = combinedMesh;
                    ma.meshFilter = ma.GetComponent<MeshFilter>();
                    ma.meshFilter.sharedMesh = ma.baseMesh;
                    ma.FPS = fps;
                    if (createdAnims.Count > 0)
                        ma.defaultAnimation = ma.animations[0];

                    int lodCount = lodDistances != null ? lodDistances.Count : 0;
                    ma.LODLevels = new MeshAnimator.MeshAnimatorLODLevel[lodCount];
                    for (int i = 0; i < lodCount; i++)
                    {
                        ma.LODLevels[i] = new MeshAnimator.MeshAnimatorLODLevel()
                        {
                            fps = lodDistances[i].Key,
                            distance = lodDistances[i].Value
                        };
                    }
                    // create exposed children objects
                    if (exposedTransforms != null && exposedTransforms.Count > 0)
                    {
                        for (int i = 0; i < exposedTransforms.Count; i++)
                        {
                            string[] splitT = exposedTransforms[i].Split('/');
                            string lastName = splitT[splitT.Length - 1];
                            var child = new GameObject(lastName);
                            child.transform.SetParent(asset.transform);
                            Transform t = sampleGO.transform.Find(exposedTransforms[i]);
                            if (t)
                            {
                                child.transform.position = t.position;
                            }
                            else
                            {
                                child.transform.localPosition = Vector3.zero;
                            }
                        }
                    }

                    string maPrefabPath = string.Format("{0}/{1}.prefab", assetFolder, asset.name);
                    var maPrefab = AssetDatabase.LoadAssetAtPath(maPrefabPath, typeof (GameObject));
                    if (maPrefab != null)
                    {
                        PrefabUtility.ReplacePrefab(asset, maPrefab);
                    }
                    else
                    {
                        PrefabUtility.CreatePrefab(maPrefabPath, asset);
                    }
                    GameObject.DestroyImmediate(asset);
                }
                GameObject.DestroyImmediate(sampleGO);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Mesh Animator", string.Format("Baked {0} animation{1} successfully!",
                                            clips.Count, clips.Count > 1 ? "s" : string.Empty), "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Bake Error",
                    string.Format("There was a problem baking the animations.\n\n{0}\n\nIf the problem persists.", e),
                    "OK");
            }
        }


        private string FormatClipName(string name)
        {
            string badChars = "!@#$%%^&*()=+}{[]'\";:|";
            for (int i = 0; i < badChars.Length; i++)
            {
                name = name.Replace(badChars[i], '_');
            }
            return name;
        }


        private void TransferMesh(Mesh from, Mesh to)
        {
            to.vertices = from.vertices;
            to.subMeshCount = from.subMeshCount;
            for (int i = 0; i < from.subMeshCount; i++)
            {
                to.SetTriangles(from.GetTriangles(i), i);
            }
            to.normals = from.normals;
            to.tangents = from.tangents;
            to.colors = from.colors;
            to.uv = from.uv;
            to.uv2 = from.uv2;
            to.uv3 = from.uv3;
            to.uv4 = from.uv4;
        }

        private void AutoWeld(Mesh mesh, float threshold, float bucketStep)
        {
            Vector3[] oldVertices = mesh.vertices;
            Vector3[] newVertices = new Vector3[oldVertices.Length];
            int[] old2new = new int[oldVertices.Length];
            int newSize = 0;

            // Find AABB
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < oldVertices.Length; i++)
            {
                if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
                if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
                if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
                if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
                if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
                if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
            }

            // Make cubic buckets, each with dimensions "bucketStep"
            int bucketSizeX = Mathf.FloorToInt((max.x - min.x)/bucketStep) + 1;
            int bucketSizeY = Mathf.FloorToInt((max.y - min.y)/bucketStep) + 1;
            int bucketSizeZ = Mathf.FloorToInt((max.z - min.z)/bucketStep) + 1;
            List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

            // Make new vertices
            for (int i = 0; i < oldVertices.Length; i++)
            {
                // Determine which bucket it belongs to
                int x = Mathf.FloorToInt((oldVertices[i].x - min.x)/bucketStep);
                int y = Mathf.FloorToInt((oldVertices[i].y - min.y)/bucketStep);
                int z = Mathf.FloorToInt((oldVertices[i].z - min.z)/bucketStep);

                // Check to see if it's already been added
                if (buckets[x, y, z] == null)
                    buckets[x, y, z] = new List<int>(); // Make buckets lazily

                for (int j = 0; j < buckets[x, y, z].Count; j++)
                {
                    Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                    if (Vector3.SqrMagnitude(to) < threshold)
                    {
                        old2new[i] = buckets[x, y, z][j];
                        goto skip; // Skip to next old vertex if this one is already there
                    }
                }

                // Add new vertex
                newVertices[newSize] = oldVertices[i];
                buckets[x, y, z].Add(newSize);
                old2new[i] = newSize;
                newSize++;

                skip:
                ;
            }

            // Make new triangles
            int[] oldTris = mesh.triangles;
            int[] newTris = new int[oldTris.Length];
            for (int i = 0; i < oldTris.Length; i++)
            {
                newTris[i] = old2new[oldTris[i]];
            }

            Vector3[] finalVertices = new Vector3[newSize];
            for (int i = 0; i < newSize; i++)
                finalVertices[i] = newVertices[i];

            mesh.Clear();
            mesh.vertices = finalVertices;
            mesh.triangles = newTris;
            mesh.RecalculateNormals();
        }

        private List<Material> GatherMaterials(List<SkinnedMeshRenderer> renderers)
        {
            List<Material> mats = new List<Material>();
            foreach (SkinnedMeshRenderer sm in renderers)
                if (sm) mats.AddRange(sm.sharedMaterials);
            mats.RemoveAll(q => q == null);
            mats = mats.Distinct().ToList();
            return mats;
        }


        private Mesh GenerateCombinedMesh(out bool combined)
        {
            return GenerateCombinedMesh(skinnedRenderers, out combined);
        }

        private Mesh GenerateCombinedMesh(List<SkinnedMeshRenderer> renderers, out bool combined)
        {
            int totalMeshes = renderers.Count;
            combined = false;
            if (totalMeshes == 1)
            {
                foreach (SkinnedMeshRenderer sr in renderers)
                {
                    return sr.sharedMesh;
                }
            }
            List<Mesh> tempMeshes = new List<Mesh>();
            List<CombineInstanceMaterial> combineInstances = new List<CombineInstanceMaterial>();
            foreach (SkinnedMeshRenderer sr in renderers)
            {
                Material[] materials = sr.sharedMaterials.Where(q => q != null).ToArray();

                for (int i = 0; i < sr.sharedMesh.subMeshCount; i++)
                {
                    Mesh t = new Mesh();
                    sr.BakeMesh(t);
                    tempMeshes.Add(t);
                    var m = sr.transform.localToWorldMatrix;
                    Matrix4x4 scaledMatrix = Matrix4x4.TRS(MatrixUtils.GetPosition(m), MatrixUtils.GetRotation(m),
                        Vector3.one);
                    combineInstances.Add(new CombineInstanceMaterial()
                    {
                        combine = new CombineInstance()
                        {
                            mesh = t,
                            transform = scaledMatrix,
                            subMeshIndex = i
                        },
                        material = materials.Length > i ? materials[i] : null,
                        sharedMesh = sr.sharedMesh,
                    });
                }
            }
            Dictionary<Material, Mesh> materialMeshes = new Dictionary<Material, Mesh>();
            Mesh mesh = null;
            while (combineInstances.Count > 0)
            {
                Material cMat = combineInstances[0].material;
                var combines = combineInstances.Where(q => q.material == cMat).Select(q => q.combine).ToArray();
                mesh = new Mesh();
                mesh.CombineMeshes(combines, true, true);
                materialMeshes.Add(cMat, mesh);
                tempMeshes.Add(mesh);
                combineInstances.RemoveAll(q => q.material == cMat);
            }
            CombineInstance[] finalCombines =
                materialMeshes.Select(q => new CombineInstance() {mesh = q.Value}).ToArray();
            mesh = new Mesh();
            mesh.CombineMeshes(finalCombines, false, false);
            combined = true;
            foreach (Mesh m in tempMeshes)
            {
                GameObject.DestroyImmediate(m);
            }
            return mesh;
        }

        private Transform FindMatchingTransform(Transform parent, Transform source, Transform newParent)
        {
            List<int> stepIndexing = new List<int>();
            while (source != parent && source != null)
            {
                if (source.parent == null)
                    break;
                for (int i = 0; i < source.parent.childCount; i++)
                {
                    if (source.parent.GetChild(i) == source)
                    {
                        stepIndexing.Add(i);
                        source = source.parent;
                        break;
                    }
                }
            }
            stepIndexing.Reverse();
            for (int i = 0; i < stepIndexing.Count; i++)
            {
                newParent = newParent.GetChild(stepIndexing[i]);
            }
            return newParent;
        }

        public void OnDisable()
        {
            if (spawnedAsset)
            {
                GameObject.DestroyImmediate(spawnedAsset.gameObject);
            }
        }

    }
}
