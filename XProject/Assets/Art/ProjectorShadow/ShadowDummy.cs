using UnityEngine;

public class ShadowDummy : MonoBehaviour
{

    public Vector3 _ShadowLocalOffset;
    public Quaternion _RotationAngleOffset;

    public bool _freezeXRot;
    public bool _freezeYRot;
    public bool _freezeZRot;

    Quaternion _AngleOffset;

    public Transform CacheTrans { get; private set; }

    void Awake()
    {
        CacheTrans = this.transform;
    }

    public void OnPreRenderShadowDummy(ProjectorCamera camera)
    {

        Vector3 offsetEuler = _RotationAngleOffset.eulerAngles;

        CacheTrans.rotation = Quaternion.identity;

        if (!_freezeXRot)
        {
            CacheTrans.rotation *= Quaternion.AngleAxis(CacheTrans.parent.rotation.eulerAngles.x + offsetEuler.x, camera.MainCamTrans.right);
        }

        if (!_freezeYRot)
        {
            CacheTrans.rotation *= Quaternion.AngleAxis(CacheTrans.parent.rotation.eulerAngles.y + offsetEuler.y, -camera.MainCamTrans.forward);
        }

        if (!_freezeZRot)
        {
            CacheTrans.rotation *= Quaternion.AngleAxis(CacheTrans.parent.rotation.eulerAngles.z + offsetEuler.z, camera.MainCamTrans.up);
        }

        CacheTrans.rotation *= Quaternion.LookRotation(camera.MainCamTrans.up, camera.MainCamTrans.forward);

        CacheTrans.localPosition = _ShadowLocalOffset;
    }
}
