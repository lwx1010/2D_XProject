using UnityEngine;
using System.Collections;
using LuaFramework;
using LuaInterface;

public sealed class LoginScene : MonoBehaviour {

    public Camera bgCamera;

    void Awake ()
    {
        NetworkManager.isKickOut = false;

        Util.AutoAdjustCameraRect(bgCamera);
        NetworkManager.inGame = false;
    }
}
