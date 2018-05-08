using UnityEngine;
using System.Collections;

public class RandomMove : MonoBehaviour {

    public float duration;

    public float speedMax;

    private Vector3 defaultPosition;

    private Vector3 target;

    private Vector3 direction;

    private float lastTime = 0;

    private Transform selTrans;

	// Use this for initialization
	void Start ()
    {
        defaultPosition = transform.position;
        direction = Vector3.up;
        lastTime = duration;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (selTrans == null) selTrans = transform;
        lastTime -= Time.deltaTime;
        if (lastTime <= 0)
        {
            direction = direction == Vector3.up ? Vector3.down : Vector3.up;
            lastTime = duration;
        }
        var speed = Random.Range(0, speedMax);
        var pos = Vector3.MoveTowards(selTrans.position, selTrans.position + direction * speedMax, speed * Time.deltaTime);
        if (pos.y < defaultPosition.y) pos.y = defaultPosition.y;
        selTrans.position = pos;
	}
}
