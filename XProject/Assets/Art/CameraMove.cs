using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {

    public float speed = 15;

    private bool bUpState = false;
    private bool bDownState = false;
    private bool bLeftState = false;
    private bool bRightState = false;

    private Transform cameraTrans;
    private Transform selTrans;

    void Awake()
    {
        cameraTrans = Camera.main.transform;
        selTrans = transform;
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        MoveByKeyboard();
    }

    public void MoveByKeyboard()
    {
        bUpState = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bDownState = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bLeftState = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bRightState = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        if (!bUpState && !bDownState && !bLeftState && !bRightState)
        {
            return;
        }

        Vector3 vecDir = cameraTrans.rotation * Vector3.forward;
        vecDir.y = 0.0f;
        vecDir.Normalize();

        float fAngle = 0.0f;
        if (bUpState)
        {
            if (bLeftState)
                fAngle = -180 / 4.0f;
            else if (bRightState)
                fAngle = 180 / 4.0f;
        }
        else if (bDownState)
        {
            if (bLeftState)
                fAngle = -180 * 3.0f / 4.0f;
            else if (bRightState)
                fAngle = 180 * 3.0f / 4.0f;
            else
                fAngle = 180;
        }
        else
        {
            if (bLeftState)
                fAngle = -180 / 2.0f;
            else if (bRightState)
                fAngle = 180 / 2.0f;
        }

        Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, fAngle, 0)), Vector3.one);
        WalkOnDirection(mat.MultiplyVector(vecDir));
    }

    public void WalkOnDirection(Vector3 vecDir)
    {
        if (vecDir.Equals(Vector3.zero)) return;

        vecDir.Normalize();

        var step = speed * Time.deltaTime;

        var targetPosition = selTrans.position + vecDir;

        var pos = Vector3.MoveTowards(selTrans.position, targetPosition, step);
        selTrans.position = pos;
    }
}
