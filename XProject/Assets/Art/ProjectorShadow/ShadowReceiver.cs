
using UnityEngine;

[AddComponentMenu("Art Tools/Shadow Receiver")]
public class ShadowReceiver : MonoBehaviour
{

    MeshFilter _meshFilter;
    Mesh _mesh;
    Mesh _meshCopy;
    MeshRenderer _meshRenderer;
    
    public int _id;

    public Transform CacheTrans { get; private set; }

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

//        if (_meshRenderer == null)
//        {
//            GameObject.Destroy(this.gameObject);
//            return;
//        }

        if ( _meshFilter != null) //!_meshRenderer.isPartOfStaticBatch &&
        {
            _mesh = _meshFilter.sharedMesh;
        }

        CacheTrans = this.transform;
        _meshCopy = null;
    }

    void Start()
    {
        AddReceiver();

        if( _meshRenderer != null && _meshRenderer.isPartOfStaticBatch)
        {
            FSPStaticMeshHolder meshHolder = FSPStaticMeshHolder.Get();
            if(meshHolder != null)
                _meshCopy = meshHolder.GetMesh(_id);
        }
    }

    public Mesh GetMesh()
    {
        if (_meshCopy != null)
        {
            return _meshCopy;
        }
        else
        {
            return _mesh;
        }
    }

    void OnEnable()
    {
        AddReceiver();
    }

    void OnDisable()
    {
        RemoveReceiver();
    }

    void OnBecameVisible()
    {
        AddReceiver();
    }

    void OnBecameInvisible()
    {
        RemoveReceiver();
    }

    void OnDestroy()
    {
        RemoveReceiver();
    }

    void AddReceiver()
    {
        if (_meshFilter != null && LightFace.Exists())
        {
            LightFace.Get().AddReceiver(this);
        }
    }

    void RemoveReceiver()
    {
        if (LightFace.Exists())
        {
            if (_meshFilter != null)
            {
                LightFace.Get().RemoveReceiver(this);
            }
        }
    }
}
