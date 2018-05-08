using UnityEngine;
using System.Collections;

public class SelfRotate : MonoBehaviour
{
    /// <summary>
    /// 旋转速度
    /// </summary>
    public float xSpeed = 0f;
    /// <summary>
    /// 旋转速度
    /// </summary>
    public float ySpeed = 0f;
    /// <summary>
    /// 旋转速度
    /// </summary>
    public float zSpeed = 0f;
    /// <summary>
    /// 是否顺时针
    /// </summary>
    public bool headClockwise = true;

    private Transform transCache;

    void Start()
    {
        transCache = this.transform;
    }

    // Update is called once per frame
    void Update ()
    {
        if (transCache != null)
        {
            if (headClockwise)
            {
                transCache.Rotate(xSpeed, ySpeed, zSpeed);
            }
            else
            {
                transCache.Rotate(-xSpeed, -ySpeed, -zSpeed);
            }
        }
	}
}
