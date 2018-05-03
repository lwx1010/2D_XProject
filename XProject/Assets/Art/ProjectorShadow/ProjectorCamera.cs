using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ProjectorCamera : MonoBehaviour
{
         
    public Transform CacheTrans { get; private set; }

    public Camera TargetCamera { get { return _ProjectorCamera; } }

    public float boundOfffset = 20;

    private Camera _ProjectorCamera;
    private bool _cameraPlainsCalculated;
    private Plane[] _mainCameraPlains;

    Bounds _projectorBounds;

    private Transform mainCamTrans;
    private Camera mainCamera;
    
    /// <summary>
    /// The Max Orthographic size for projector camera
    /// </summary>
    private const float MaxOrthographicSize = 16f;

    public Camera MainCamera
    {
        get { return mainCamera; }
        set
        {
            mainCamera = value;
            mainCamTrans = mainCamera.transform;

        }
    }

    public Transform MainCamTrans
    {
        get { return mainCamTrans; }
    }

    void Awake()
    {
        this.CacheTrans = this.transform;
        _ProjectorCamera = this.CacheTrans.GetOrAddComponent<Camera>();

        _ProjectorCamera.clearFlags = CameraClearFlags.SolidColor;
        _ProjectorCamera.backgroundColor = new Color(1, 1, 1 ,0);
        if(_ProjectorCamera.cullingMask == 1 << LayerMask.NameToLayer("Everything"))
            _ProjectorCamera.cullingMask = 0;
        _ProjectorCamera.orthographic = true;
        _ProjectorCamera.nearClipPlane = -10;
        _ProjectorCamera.farClipPlane = 1000;
        _ProjectorCamera.aspect = 1.0f;
        _ProjectorCamera.depth = float.MinValue;

        MainCamera = Camera.main;
        _projectorBounds = new Bounds();
    }


void Start()
    {
            
    }


    public void CalculateShadowBounds(List<ShadowProjector> projectors)
    {
        Vector2 xRange = new Vector2(float.MaxValue, float.MinValue);
        Vector2 yRange = new Vector2(float.MaxValue, float.MinValue);

        Vector2 shadowCoords;
        float maxShadowSize = 0.0f;

        bool noVisibleProjectors = true;
        int projectorIndex = 0;


        ShadowProjector shadowProjector;
        LightFace lightFace = LightFace.Get();

        Vector3 mainCamPosition = mainCamTrans.position;

        _cameraPlainsCalculated = false;
        for (int n = 0; n < projectors.Count; n++)
        {
            shadowProjector = projectors[n];

            if (!shadowProjector.EnableProjector) continue;

            if (lightFace.EnableCutOff)
            {
                if ((shadowProjector.CacheTrans.position - mainCamPosition).magnitude > LightFace.GlobalCutOffDistance)
                {
                    shadowProjector.SetVisible(false);
                    continue;
                }
            }

            switch (LightFace.GlobalShadowCullingMode)
            {
                case LightFace.ProjectionCulling.ProjectorBounds:
                    {
                        Plane[] _mainCameraPlains = CheckMainCameraPlains();

                        if (!GeometryUtility.TestPlanesAABB(_mainCameraPlains, shadowProjector.GetBounds()))
                        {
                            shadowProjector.SetVisible(false);
                            continue;
                        }
                    }
                    break;
                case LightFace.ProjectionCulling.ProjectionVolumeBounds:
                    {
                        Plane[] _mainCameraPlains = CheckMainCameraPlains();

                        if (!lightFace.IsProjectionVolumeVisible(_mainCameraPlains, shadowProjector))
                        {
                            shadowProjector.SetVisible(false);
                            continue;
                        }
                    }
                    break;
                default:
                    break;
            }                
            


            noVisibleProjectors = false;
            shadowProjector.SetVisible(true);

            shadowCoords = TargetCamera.WorldToViewportPoint(shadowProjector.GetShadowPos());

            if (projectorIndex == 0)
            {
                _projectorBounds.center = shadowProjector.GetShadowPos();
                _projectorBounds.size = Vector3.zero;
            }
            else
            {
                _projectorBounds.Encapsulate(shadowProjector.GetShadowPos());
            }

            if (shadowCoords.x < xRange.x) xRange.x = shadowCoords.x;
            if (shadowCoords.x > xRange.y) xRange.y = shadowCoords.x;

            if (shadowCoords.y < yRange.x) yRange.x = shadowCoords.y;
            if (shadowCoords.y > yRange.y) yRange.y = shadowCoords.y;

            float shadowSize = shadowProjector.GetShadowWorldSize();

            if (shadowSize > maxShadowSize)
            {
                maxShadowSize = shadowSize;
            }

            projectorIndex++;
        }

        if (noVisibleProjectors)
        {
            return;
        }

        float cameraWorldSize = _ProjectorCamera.orthographicSize * 2.0f;
        float maxShadowSizeViewport = Mathf.Max(0.08f, maxShadowSize / cameraWorldSize);

        Vector3 camPos = _projectorBounds.center + _projectorBounds.extents.magnitude * -LightFace.GlobalProjectionDir.normalized;
        CacheTrans.position = camPos;

        float maxRange = Mathf.Max(xRange[1] - xRange[0] + maxShadowSizeViewport * 2.0f, yRange[1] - yRange[0] + maxShadowSizeViewport * 2.0f);
        maxRange += boundOfffset / 100;
        TargetCamera.orthographicSize = Mathf.Min(TargetCamera.orthographicSize * maxRange, MaxOrthographicSize);

    }

    public void SetTargetTexture(RenderTexture texture)
    {
        _ProjectorCamera.targetTexture = texture;
    }
    /// <summary>
    /// 渲染指定图层的动态阴影
    /// </summary>
    /// <param name="layerName"></param>
    public void ShowLayerName(string layerName)
    {
        CameraUtil.ShowLayerName(_ProjectorCamera , layerName);
        LightFace.Get().SetLayerVisableProjectors(layerName, true);
    }
    /// <summary>
    /// 隐藏指定图层阴影渲染
    /// </summary>
    /// <param name="layerName"></param>
    public void HideLayerName(string layerName)
    {
        CameraUtil.HideLayerName(_ProjectorCamera , layerName);
        LightFace.Get().SetLayerVisableProjectors(layerName , false);
    }

    public void ClearAllLayer()
    {
        _ProjectorCamera.cullingMask = 0;
        LightFace.Get().ClearVisableProjectors();
    }



    public Plane[] CheckMainCameraPlains()
    {
        if (!_cameraPlainsCalculated)
        {
            _mainCameraPlains = GeometryUtility.CalculateFrustumPlanes(mainCamera);
            _cameraPlainsCalculated = true;
        }
        return _mainCameraPlains;
    }

}
