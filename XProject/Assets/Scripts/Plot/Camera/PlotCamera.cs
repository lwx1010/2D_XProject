using UnityEngine;
using LuaFramework;
using AL.Plot;

/// <summary>
/// 剧情相机
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlotCamera : MonoBehaviour
{
    private Camera cam;
    
    
    public bool IsMainCamera;


    private void Awake()
    {
        cam = this.GetComponent<Camera>();
        
//        Camera mainCam = Camera.main;
//        if (CopyMainCamera)
//        {
//            cam.fieldOfView = mainCam.fieldOfView;
//            cam.nearClipPlane = mainCam.nearClipPlane;
//            cam.farClipPlane = mainCam.farClipPlane;
//        }

        Util.AutoAdjustCameraRect(cam);
        cam.depth = 1;
        cam.enabled = !IsMainCamera;
    }


    public void Enable(float fadeTime)
    {
        if (fadeTime > 0)
        {
            CameraFadeEffect fadeCam = this.gameObject.AddComponent<CameraFadeEffect>();
            fadeCam.Fade(1, 0, fadeTime);
            fadeCam.AutoDesroy = true;           
        }


        cam.cullingMask = -1; //"Everything"
        CameraUtil.HideLayerName(cam, "UI");
        CameraUtil.HideLayerName(cam, "UIModel");
        CameraUtil.HideLayerName(cam, "Self");
        CameraUtil.HideLayerName(cam, "Role");
        CameraUtil.HideLayerName(cam, "Monster");
        CameraUtil.HideLayerName(cam, "Jump");
        CameraUtil.HideLayerName(cam, "Partner");
        CameraUtil.HideLayerName(cam, "Npc");
        CameraUtil.HideLayerName(cam, "TransparentBuilding");
        CameraUtil.HideLayerName(cam, "SceneEntity");
        CameraUtil.HideLayerName(cam, "RoleEffect");
        cam.enabled = true;
    }
    
    private void Start()
    {
        if(cam.enabled)
            Enable(0);
    }
}
