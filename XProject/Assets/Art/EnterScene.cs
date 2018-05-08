using UnityEngine;
using System.Collections;
using LuaFramework;

public class EnterScene : MonoBehaviour {

    void Start()
    {
#if UNITY_EDITOR
        if (!AutoUpdateScene.jumped)
            return;
#endif
        // pc包屏幕适应
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            var root = GetComponent<UIRoot>();
            root.scalingStyle = UIRoot.Scaling.Flexible;
            root.minimumHeight = 1080;
        }

        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null) Destroy(rigidbody);
        Destroy(this);
    }
}
