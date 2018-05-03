using System.Collections.Generic;
using UnityEngine;

public class LightFace : MonoBehaviour
{
    static LightFace _Instance;
    public static readonly string ProjectorLayer = "SceneEntity";//"ProjectorLayer"
    public enum ProjectionCulling
    {
        None,
        ProjectorBounds,
        ProjectionVolumeBounds
    }

    #region ---------------------Private Attributes-------------------------------
    private int[] _ShadowResolutions = new int[] { 128, 256, 512, 1024, 2048 };
    private ShadowTexture _Tex;
    private ProjectorCamera _ProjectorCamera;

    List<ShadowProjector> _ShadowProjectors;
    List<ShadowReceiver> _ShadowReceivers;

    Matrix4x4 _ProjectorMatrix;
    Matrix4x4 _ProjectorClipMatrix;
    Matrix4x4 _BiasMatrix;
    Matrix4x4 _ViewMatrix;
    Matrix4x4 _BPV;
    Matrix4x4 _BPVClip;
    Matrix4x4 _ModelMatrix;
    Matrix4x4 _FinalMatrix;
    Matrix4x4 _FinalClipMatrix;

    MaterialPropertyBlock _MBP;

    private const  string MatrixGlobalProjector = "_GlobalProjector";
    private const string MatrixGlobalProjectorClip = "_GlobalProjectorClip";
    private LayerMask defaultLayer;
    #endregion

    #region --------------------------Public Attributes----------------------------------
    [HideInInspector]
    public Material _ProjectorMaterialShadow;
        
    public ProjectorCamera ProCamera { get { return _ProjectorCamera; } }

    public static Vector3 GlobalProjectionDir
    {
        set
        {
            if (_Instance._GlobalProjectionDir != value)
            {
                _Instance._GlobalProjectionDir = value;
                _Instance.OnProjectionDirChange();
            }
        }

        get
        {
            return _Instance._GlobalProjectionDir;
        }
    }

    [UnityEngine.SerializeField]
    protected Vector3 _GlobalProjectionDir = new Vector3(0.0f, -1.0f, 0.0f);

    public static int GlobalShadowResolution
    {
        set
        {
            if (_Instance._GlobalShadowResolution != value)
            {
                _Instance.OnShadowResolutionChange(value);
                _Instance._GlobalShadowResolution = value;
            }
        }

        get
        {
            return _Instance._GlobalShadowResolution;
        }
    }
    [UnityEngine.SerializeField]
    protected int _GlobalShadowResolution = 2;

    public static ProjectionCulling GlobalShadowCullingMode
    {
        set
        {
            _Instance._GlobalShadowCullingMode = value;
        }

        get
        {
            return _Instance._GlobalShadowCullingMode;
        }
    }
    [UnityEngine.SerializeField]
    protected ProjectionCulling _GlobalShadowCullingMode = ProjectionCulling.ProjectorBounds;


    public static float GlobalCutOffDistance
    {
        set
        {
            _Instance._GlobalCutOffDistance = value;
        }

        get
        {
            return _Instance._GlobalCutOffDistance;
        }
    }
    [UnityEngine.SerializeField]
    protected float _GlobalCutOffDistance = 1000.0f;

    bool _renderShadows = true;

    public bool ShadowsOn
    {
        set
        {
            _renderShadows = value;
        }
        get
        {
            return _renderShadows;
        }
    }


    public bool EnableCutOff
    {
        set
        {
            if (_EnableCutOff != value)
            {
                _EnableCutOff = value;
            }
        }

        get
        {
            return _EnableCutOff;
        }
    }

    [UnityEngine.SerializeField]
    bool _EnableCutOff = false;

    public List<ShadowProjector> ShadowProjectors
    {
        get { return _ShadowProjectors; }
    }

    #endregion

    public static LightFace Get()
    {
//        if (_Instance == null)
//        {
//            if(_Instance == null)
//                _Instance = new GameObject("_LightFace").AddComponent<LightFace>();
//            _Instance.Initialize();
//        }
        return _Instance;
    }

    public static bool Exists()
    {
        return (_Instance != null);
    }

    void Initialize()
    {
        if (_ProjectorCamera != null) return;

        defaultLayer = LayerMask.NameToLayer("Default");
        gameObject.layer = defaultLayer;
        
//        _ProjectorMaterialShadow = new Material(Shader.Find("Fast Shadow Projector/Multiply"));
        _ProjectorMaterialShadow = Resources.Load<Material>("Other/ShadowProjecterMat");

        _ProjectorCamera = gameObject.GetComponent<ProjectorCamera>();
        if(_ProjectorCamera == null)
            _ProjectorCamera = gameObject.AddComponent<ProjectorCamera>();

        _BiasMatrix = new Matrix4x4();
        _BiasMatrix.SetRow(0, new Vector4(0.5f, 0.0f, 0.0f, 0.5f));
        _BiasMatrix.SetRow(1, new Vector4(0.0f, 0.5f, 0.0f, 0.5f));
        _BiasMatrix.SetRow(2, new Vector4(0.0f, 0.0f, 0.5f, 0.5f));
        _BiasMatrix.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

        _ProjectorMatrix = new Matrix4x4();
        _ProjectorClipMatrix = new Matrix4x4();

        _MBP = new MaterialPropertyBlock();

        _ShadowProjectors = new List<ShadowProjector>();
        _ShadowReceivers = new List<ShadowReceiver>();
            

        _ProjectorCamera.enabled = false;
    }
    #region ------------------MonoBehaviour 生命周期------------------------------------
    void Awake()
    {
        _Instance = this;

        this.Initialize();
    }

    void Start()
    {
        CreateProjectorEyeTexture();

        OnProjectionDirChange();
    }

    void Update()
    {
//        ShadowReceiver receiver;
//
//        for (int i = 0; i < _ShadowReceivers.Count; i++)
//        {
//            receiver = _ShadowReceivers[i];
//        }
    }


    void LateUpdate()
    {
        if (!_renderShadows)    return;
            
        RenderProjectors(_ProjectorCamera, _ShadowProjectors, _ProjectorMaterialShadow);
    }

    void OnPreCull()
    {
        foreach (ShadowProjector shadowProjector in _ShadowProjectors)
        {
            shadowProjector.SetVisible(true);
            shadowProjector.OnPreRenderShadowProjector(_ProjectorCamera);
        }
    }

    void OnPostRender()
    {
        if(_Tex != null)
            _Tex.GrabScreenIfNeeded();

        foreach (ShadowProjector shadowProjector in _ShadowProjectors)
        {
            shadowProjector.SetVisible(false);
        }
    }

    void OnDestroy()
    {
        _Instance = null;
    }
    #endregion
    
    void RenderProjectors(ProjectorCamera projectorCamera, List<ShadowProjector> projectors, Material material)
    {
        if (!_renderShadows)
        {
            return;
        }

        if (projectors.Count > 0 && _ShadowReceivers.Count > 0)
        {
            projectorCamera.CalculateShadowBounds(projectors);
            Camera targetCamera = projectorCamera.TargetCamera;

            float n = targetCamera.nearClipPlane;
            float f = targetCamera.farClipPlane;
            float r = targetCamera.orthographicSize;
            float t = targetCamera.orthographicSize;
            float clipN = 0.1f;
            float clipF = 100.0f;


            _ProjectorMatrix.SetRow(0, new Vector4(1 / r, 0.0f, 0.0f, 0));
            _ProjectorMatrix.SetRow(1, new Vector4(0.0f, 1 / t, 0.0f, 0));
            _ProjectorMatrix.SetRow(2, new Vector4(0.0f, 0.0f, -2 / (f - n), 0));
            _ProjectorMatrix.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

            _ProjectorClipMatrix.SetRow(0, new Vector4(clipN / r, 0.0f, 0.0f, 0));
            _ProjectorClipMatrix.SetRow(1, new Vector4(0.0f, clipN / t, 0.0f, 0));
            _ProjectorClipMatrix.SetRow(2, new Vector4(0.0f, 0.0f, -(clipF + clipN) / (clipF - clipN), -2 * clipF * clipN / (clipF - clipN)));
            _ProjectorClipMatrix.SetRow(3, new Vector4(0.0f, 0.0f, -1.0f, 0.0f));


            _ViewMatrix = projectorCamera.CacheTrans.localToWorldMatrix.inverse;

            _BPV = _BiasMatrix * _ProjectorMatrix * _ViewMatrix;
            _BPVClip = _BiasMatrix * _ProjectorClipMatrix * _ViewMatrix;

            Render(material);
        }
    }

    void Render(Material material)
    {
        if (!_renderShadows)
        {
            return;
        }


        ShadowReceiver receiver;
        
        for (int i = 0; i < _ShadowReceivers.Count; i++)
        {
            receiver = _ShadowReceivers[i];
            _ModelMatrix = receiver.CacheTrans.localToWorldMatrix;
            _FinalMatrix = _BPV * _ModelMatrix;
            _FinalClipMatrix = _BPVClip * _ModelMatrix;
            
            _MBP.Clear();
            _MBP.SetMatrix(MatrixGlobalProjector, _FinalMatrix);
            _MBP.SetMatrix(MatrixGlobalProjectorClip, _FinalClipMatrix);

            Mesh receiverMesh = receiver.GetMesh();
            Graphics.DrawMesh(receiverMesh, _ModelMatrix, material, defaultLayer, null, 0, _MBP);
        }
    }

    public void OnProjectionDirChange()
    {
        if (ProCamera != null)
        {
            ProCamera.CacheTrans.rotation = Quaternion.LookRotation(_GlobalProjectionDir);
        }
    }

    public void OnShadowResolutionChange(int resoulution)
    {
        if (_Tex != null && GlobalShadowResolution == resoulution) return;

        _GlobalShadowResolution = resoulution;
        CreateProjectorEyeTexture();
    }

    private void CreateProjectorEyeTexture()
    {
        if (_Tex != null)
        {
            _Tex.CleanUp();
        }

        _Tex = new ShadowTexture(_ProjectorCamera, _ShadowResolutions[_GlobalShadowResolution]);
        if(_ProjectorMaterialShadow)
            _ProjectorMaterialShadow.SetTexture("_ShadowTex", _Tex.GetTexture());
        

    }

    public bool IsProjectionVolumeVisible(Plane[] planes, ShadowProjector projector)
    {
        float boundSize = 1000000.0f;

        Vector3 center = projector.GetShadowPos() + GlobalProjectionDir.normalized * (boundSize * 0.5f);
        Vector3 size = new Vector3(Mathf.Abs(GlobalProjectionDir.normalized.x), Mathf.Abs(GlobalProjectionDir.normalized.y), Mathf.Abs(GlobalProjectionDir.normalized.z)) * boundSize;
        Bounds bounds = new Bounds(center, size);

        float shadowSize = projector.GetShadowWorldSize();

        bounds.Encapsulate(new Bounds(projector.GetShadowPos(), new Vector3(shadowSize, shadowSize, shadowSize)));

        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    #region ----------------阴影实体容器操作-----------------------
    public void AddProjector(ShadowProjector projector)
    {
        if (!_ShadowProjectors.Contains(projector))
        {
            _ShadowProjectors.Add(projector);

            if (_ProjectorCamera.enabled == false)
            {
                _ProjectorCamera.enabled = true;
            }
        }
    }

    public void RemoveProjector(ShadowProjector projector)
    {
        if (_ShadowProjectors.Contains(projector))
        {
            _ShadowProjectors.Remove(projector);

            if (_ShadowProjectors.Count == 0)
            {
                _ProjectorCamera.enabled = false;
            }
        }
           
    }

    /// <summary>
    /// 设置某一层次Layer是否显示
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <param name="visiable"></param>
    public void SetLayerVisableProjectors(int layerIndex, bool visiable)
    {
        foreach (ShadowProjector projector in _ShadowProjectors)
        {
            if (projector.gameObject.layer == layerIndex)
                projector.EnableProjector = visiable;
        }
    }

    public void SetLayerVisableProjectors(string layerName, bool visiable)
    {
        SetLayerVisableProjectors(LayerMask.NameToLayer(layerName) , visiable);
    }

    public void ClearVisableProjectors()
    {
        foreach (ShadowProjector projector in _ShadowProjectors)
            projector.EnableProjector = false;
    }

    public void AddReceiver(ShadowReceiver receiver)
    {
        if (!_ShadowReceivers.Contains(receiver))
        {
            _ShadowReceivers.Add(receiver);
        }
    }

    public void RemoveReceiver(ShadowReceiver receiver)
    {
        if (_ShadowReceivers.Contains(receiver))
        {
            _ShadowReceivers.Remove(receiver);
        }
    }
    #endregion
}
