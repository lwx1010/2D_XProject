using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("Art Tools/Shadow Projector")]
public class ShadowProjector : MonoBehaviour
{
    private static class MeshGen
    {

        public static Mesh CreatePlane(Vector3 up, Vector3 right, Rect uvRect, Color color)
        {
            Mesh planeMesh = new Mesh();

            Vector3[] vertices = new Vector3[] {
                (up * 0.5f - right * 0.5f),
                (up * 0.5f + right * 0.5f),
                (-up * 0.5f - right * 0.5f),
                (-up * 0.5f + right * 0.5f)
            };

            Vector2[] uvs = new Vector2[] {
                new Vector2(uvRect.x, uvRect.y + uvRect.height),
                new Vector2(uvRect.x + uvRect.width, uvRect.y + uvRect.height),
                new Vector2(uvRect.x, uvRect.y),
                new Vector2(uvRect.x + uvRect.width, uvRect.y),
            };

            Color[] colors = new Color[] {
                color,
                color,
                color,
                color
            };

            int[] indices = new int[] { 0, 1, 3, 0, 3, 2 };

            planeMesh.vertices = vertices;
            planeMesh.uv = uvs;
            planeMesh.colors = colors;
            planeMesh.SetTriangles(indices, 0);

            return planeMesh;
        }
    }

    public float ShadowSize
    {
        set
        {
            if (_ShadowSize != value)
            {
                _ShadowSize = value;
                if (_ShadowDummyMesh != null)
                {
                    OnShadowSizeChanged();
                }
            }
        }

        get
        {
            return _ShadowSize;
        }
    }

    [UnityEngine.SerializeField]
    float _ShadowSize = 1.0f;

    public Color ShadowColor
    {
        set
        {
            if (_ShadowColor != value)
            {
                _ShadowColor = value;
                if (_ShadowDummyMesh != null)
                {
                    OnShadowColorChanged();
                }
            }
        }

        get
        {
            return _ShadowColor;
        }
    }

    [UnityEngine.SerializeField]
    Color _ShadowColor = new Color(1.0f, 1.0f, 1.0f);

    public float ShadowOpacity
    {
        set
        {
            if (_ShadowOpacity != value)
            {
                _ShadowOpacity = value;
                if (_ShadowDummyMesh != null)
                {
                    OnShadowColorChanged();
                }
            }
        }

        get
        {
            return _ShadowOpacity;
        }
    }

    [UnityEngine.SerializeField]
    float _ShadowOpacity = 1.0f;

    public Material _Material;
    

    public Vector3 ShadowLocalOffset
    {
        set
        {
            _ShadowLocalOffset = value;

            if (_ShadowDummy != null)
            {
                _ShadowDummy._ShadowLocalOffset = _ShadowLocalOffset;
            }
        }

        get
        {
            return _ShadowLocalOffset;
        }
    }

    [UnityEngine.SerializeField]
    Vector3 _ShadowLocalOffset = new Vector3(0 , 0.2f , 0);

    public Quaternion RotationAngleOffset
    {
        set
        {
            _RotationAngleOffset = value;

            if (_ShadowDummy != null)
            {
                _ShadowDummy._RotationAngleOffset = _RotationAngleOffset;
            }
        }

        get
        {
            return _RotationAngleOffset;
        }
    }

    [UnityEngine.SerializeField]
    Quaternion _RotationAngleOffset;

    // Freeze X Rotation ----------------------------------------------------

    public bool FreezeXRot
    {
        set
        {
            _FreezeXRot = value;

            if (_ShadowDummy != null)
            {
                _ShadowDummy._freezeXRot = _FreezeXRot;
            }
        }

        get
        {
            return _FreezeXRot;
        }
    }

    [UnityEngine.SerializeField]
    bool _FreezeXRot = true;

    // Freeze Y Rotation ----------------------------------------------------

    public bool FreezeYRot
    {
        set
        {
            _FreezeYRot = value;

            if (_ShadowDummy != null)
            {
                _ShadowDummy._freezeYRot = _FreezeYRot;
            }
        }

        get
        {
            return _FreezeYRot;
        }
    }

    [UnityEngine.SerializeField]
    bool _FreezeYRot = false;

    // Freeze Z Rotation ----------------------------------------------------

    public bool FreezeZRot
    {
        set
        {
            _FreezeZRot = value;

            if (_ShadowDummy != null)
            {
                _ShadowDummy._freezeZRot = _FreezeZRot;
            }
        }

        get
        {
            return _FreezeZRot;
        }
    }

    [UnityEngine.SerializeField]
    bool _FreezeZRot = true;


    // UV RECT ----------------------------------------------------

    public Rect UVRect
    {
        set
        {
            _UVRect = value;
            if (_ShadowDummy != null)
            {
                OnUVRectChanged();
            }
        }

        get
        {
            return _UVRect;
        }
    }

    [UnityEngine.SerializeField]
    Rect _UVRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

    // Auto Size/Opacity ----------------------------------------------------

    public bool AutoSizeOpacity
    {
        set
        {
            _AutoSizeOpacity = value;
        }

        get
        {
            return _AutoSizeOpacity;
        }
    }

    [UnityEngine.SerializeField]
    bool _AutoSizeOpacity = false;

    // Auto Size/Opacity CutOff Distance -------------------------------------------------

    public float AutoSOCutOffDistance
    {
        set
        {
            _AutoSOCutOffDistance = value;
        }

        get
        {
            return _AutoSOCutOffDistance;
        }
    }

    [UnityEngine.SerializeField]
    float _AutoSOCutOffDistance = 10.0f;

    // Auto Size/Opacity Ray Origin Offset -------------------------------------------------

    public float AutoSORayOriginOffset
    {
        set
        {
            _AutoSORayOriginOffset = value;
        }

        get
        {
            return _AutoSORayOriginOffset;
        }
    }

    [UnityEngine.SerializeField]
    float _AutoSORayOriginOffset = 0.0f;


    // Auto Size/Opacity Max Scale Multiplier -------------------------------------------------
    public float AutoSOMaxScaleMultiplier
    {
        set
        {
            _AutoSOMaxScaleMultiplier = value;
        }

        get
        {
            return _AutoSOMaxScaleMultiplier;
        }
    }

    [UnityEngine.SerializeField]
    float _AutoSOMaxScaleMultiplier = 2.0f;

    // Auto Size/Opacity Layer -------------------------------------------------
    public int AutoSORaycastLayer
    {
        set
        {
            _AutoSORaycastLayer = value;
        }

        get
        {
            return _AutoSORaycastLayer;
        }
    }

    [UnityEngine.SerializeField]
    int _AutoSORaycastLayer = 0;

    MeshRenderer _Renderer;
    MeshFilter _MeshFilter;
    Mesh _ShadowDummyMesh;

    ShadowDummy _ShadowDummy;

    float _initialSize;
    float _initialOpacity;

    private bool _enableProjector = true;

    public Bounds ProjectorBounds;

    public Transform CacheTrans { get; private set; }
    /// <summary>
    /// Center中心的偏移量
    /// </summary>
    public Vector3 BoundCenterOffset { get; set; }
    /// <summary>
    /// 是否被激活
    /// </summary>
    public bool EnableProjector
    {
        get { return _enableProjector && this.gameObject.activeInHierarchy; }
        set { _enableProjector = value; }
    }

    void Awake()
    {
        _ShadowDummyMesh = MeshGen.CreatePlane(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 0.0f), _UVRect,
                                               new Color(_ShadowColor.r, _ShadowColor.g, _ShadowColor.b, _ShadowOpacity));

        Transform parent = transform;
        CacheTrans = parent;

        _ShadowDummy = new GameObject("shadowDummy").AddComponent<ShadowDummy>();
        _ShadowDummy.transform.parent = parent;
        _ShadowDummy.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        _ShadowDummy.transform.localRotation = Quaternion.identity * parent.localRotation;

        _ShadowDummy.gameObject.layer = LayerMask.NameToLayer(LightFace.ProjectorLayer);

        _ShadowDummy._ShadowLocalOffset = _ShadowLocalOffset;
        _ShadowDummy._RotationAngleOffset = _RotationAngleOffset;

        _ShadowDummy._freezeXRot = _FreezeXRot;
        _ShadowDummy._freezeYRot = _FreezeYRot;
        _ShadowDummy._freezeZRot = _FreezeZRot;

        OnShadowSizeChanged();

        _Renderer = _ShadowDummy.gameObject.AddComponent<MeshRenderer>();
        _Renderer.receiveShadows = false;
        _Renderer.shadowCastingMode = ShadowCastingMode.Off;
        _Renderer.material = _Material;
        _Renderer.enabled = false;

        _MeshFilter = _ShadowDummy.gameObject.AddComponent<MeshFilter>();
        _MeshFilter.mesh = _ShadowDummyMesh;

        _initialSize = _ShadowSize;
        _initialOpacity = _ShadowOpacity;

        ProjectorBounds = new Bounds();
    }

    void Start()
    {
//        LightFace.Get().AddProjector(this);
//        _Renderer.enabled = true;
    }

    void OnEnable()
    {
        if (!LightFace.Exists()) return;


        LightFace.Get().AddProjector(this);

        _Renderer.enabled = true;
    }



    void OnDisable()
    {
        if (LightFace.Exists())
        {
            LightFace.Get().RemoveProjector(this);
            if (_ShadowDummy != null)
            {
                _Renderer.enabled = false;
            }
        }
    }

    void OnDestroy()
    {
        if (LightFace.Exists())
        {
            LightFace.Get().RemoveProjector(this);
        }

        if(_ShadowDummy != null)
            GameObject.Destroy(_ShadowDummy.gameObject);
    }

    public Bounds GetBounds()
    {
        ProjectorBounds.center = _Renderer.bounds.center + BoundCenterOffset;
        return ProjectorBounds;
    }

    public bool IsVisible()
    {
        return _Renderer.isVisible;
    }

    public void SetVisible(bool visible)
    {
        _Renderer.enabled = visible;
    }

    void Update()
    {
        if (_AutoSizeOpacity)
        {
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(new Ray(CacheTrans.position, LightFace.GlobalProjectionDir), out hitInfo, _AutoSOCutOffDistance, 1 << _AutoSORaycastLayer);

            if (hit)
            {
                float opacity = 1.0f - Mathf.Min(_AutoSOCutOffDistance, Mathf.Max(0, hitInfo.distance - _AutoSORayOriginOffset)) / _AutoSOCutOffDistance;
                float scaleMultiplier = Mathf.Lerp(_AutoSOMaxScaleMultiplier, 1.0f, opacity);

                ShadowSize = _initialSize * scaleMultiplier;
                ShadowOpacity = _initialOpacity * opacity;
            }
            else
            {
                ShadowOpacity = 0.0f;
            }
        }
    }

    public void OnPreRenderShadowProjector(ProjectorCamera camera)
    {
        if (_ShadowDummy != null)
        {
            _ShadowDummy.OnPreRenderShadowDummy(camera);
        }
    }

    public Matrix4x4 ShadowDummyLocalToWorldMatrix()
    {
        return _ShadowDummy.CacheTrans.localToWorldMatrix;
    }

    public float GetShadowWorldSize()
    {
        return (ShadowDummyLocalToWorldMatrix() * new Vector3(1.0f, 0.0f, 0.0f)).magnitude;
    }

    public Vector3 GetShadowPos()
    {
        return _ShadowDummy.CacheTrans.position;
    }

    void OnShadowSizeChanged()
    {
        _ShadowDummy.CacheTrans.localScale = new Vector3(_ShadowSize, _ShadowSize, _ShadowSize);
    }

    void OnUVRectChanged()
    {
        RebuildMesh();
    }

    public void OnShadowColorChanged()
    {
        Color color = new Color(_ShadowColor.r, _ShadowColor.g, _ShadowColor.b, _ShadowOpacity);
        _ShadowDummyMesh.colors = new Color[] { color, color, color, color };
    }

    void RebuildMesh()
    {
        _ShadowDummyMesh = MeshGen.CreatePlane(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 0.0f), _UVRect, new Color(_ShadowColor.r, _ShadowColor.g, _ShadowColor.b, _ShadowOpacity));
        _MeshFilter.mesh = _ShadowDummyMesh;
    }
}
