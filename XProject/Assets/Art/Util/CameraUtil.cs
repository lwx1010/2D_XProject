/********************************************************************************
** Author： LiangZG
** Email :  game.liangzg@foxmail.com
*********************************************************************************/
using UnityEngine;
/// <summary>
/// Camera 常用方法集
/// </summary>
public sealed class CameraUtil
{
    #region --------------- LayerMask cullmask 图层操作

    public static void ShowLayerIndex(Camera cam, int layer)
    {
        cam.cullingMask |= 1 << layer;
    }


    public static void ShowLayerName(Camera cam, params string[] layerNames)
    {
        foreach (string arg in layerNames)
            cam.cullingMask |= 1 << LayerMask.NameToLayer(arg);
    }

    public static void HideLayerIndex(Camera cam, int layer)
    {
        cam.cullingMask &= ~(1 << layer);
    }

    public static void HideLayerName(Camera cam, params string[] layerNames)
    {
        foreach (string arg in layerNames)
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer(arg));
    }

    public static void ToggleLayerIndex(Camera cam, int layerIndex)
    {
        cam.cullingMask ^= 1 << layerIndex;
    }

    public static void ToggleLayerName(Camera cam, string layerName)
    {
        cam.cullingMask ^= 1 << LayerMask.NameToLayer(layerName);
    }
    /// <summary>
    /// 指定Layer是否被激活
    /// </summary>
    /// <param name="cam"></param>
    /// <param name="layerIndex"></param>
    /// <returns>true表示激活</returns>
    public static bool IsLayerIndex(Camera cam , int layerIndex)
    {
        return (cam.cullingMask & (1 << layerIndex)) > 0;
    }
    /// <summary>
    /// 指定Layer是否被激活
    /// </summary>
    /// <param name="cam">指定的Camera</param>
    /// <param name="layerName">Layer名称</param>
    /// <returns>true表示激活</returns>
    public static bool IsLayerName(Camera cam, string layerName)
    {
        return IsLayerIndex(cam , LayerMask.NameToLayer(layerName));
    }
    #region ---------------------场景主相机操作----------------------------

    public static void ShowMainCameraLayer(string layerName)
    {
        Camera mainCam = Camera.main;   if (mainCam == null) return;
        ShowLayerName(mainCam , layerName);
    }

    public static void HideMainCameraLayer(string layerName)
    {
        Camera mainCam = Camera.main; if (mainCam == null) return;
        HideLayerName(mainCam, layerName);
    }

    public static bool IsMainCameraLayer(string layerName)
    {
        Camera mainCam = Camera.main; if (mainCam == null) return false;
        return IsLayerName(mainCam, layerName);
    }

    #endregion

    #endregion
}
