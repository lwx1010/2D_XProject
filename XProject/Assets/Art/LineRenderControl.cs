using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineRenderControl : MonoBehaviour
{
    public Vector3[] speed;
    public float[] duration;
    public float delay;

    private Vector3[] currentPosition;
    private LineRenderer lineRenderer;

    //private bool[] finished;
    private bool allFinished = false;

	// Use this for initialization
	void Start ()
    {
        lineRenderer = GetComponent<LineRenderer>();
        Debug.Assert(lineRenderer != null, "You should add a LineRenderer Component at first");
        Debug.Assert(speed.Length != 0, "Parameters' size should not be 0");
        Debug.Assert(speed.Length == duration.Length, "Parameters' size should be the same");
        if (duration.Length > 0)
        {
            currentPosition = new Vector3[duration.Length];
            //finished = new bool[duration.Length];
        }
        for (int i = 0; i < currentPosition.Length; ++i)
        {
            currentPosition[i] = Vector3.zero;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (allFinished) return;

        if (delay > 0)
            delay -= Time.deltaTime;
        if (delay <= 0)
        {
            if (lineRenderer != null)
            {
                for (int i = 0; i < speed.Length; ++i)
                {
                    var dis = speed[i] * Time.deltaTime;
                    duration[i] -= Time.deltaTime;
                    if (duration[i] >= 0)
                    {
                        currentPosition[i] += dis;
                    }
                    lineRenderer.SetPosition(i, currentPosition[i]);
                }
                allFinished = true;
                for (int i = 0; i < duration.Length; ++i)
                {
                    if (duration[i] >=0)
                    {
                        allFinished = false;
                        break;
                    } 
                }
            }
        }
	}
}
