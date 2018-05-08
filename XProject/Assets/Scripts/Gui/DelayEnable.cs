using UnityEngine;
using System.Collections;

public class DelayEnable : MonoBehaviour {

    public float delay;

    BoxCollider bc;

    Vector3 defaultSize;

	// Use this for initialization
	void Start ()
    {
        bc = GetComponent<BoxCollider>();
        defaultSize = bc.size;
        bc.size = Vector3.one;
        Invoke("Delay", delay);
        //StartCoroutine(Delay());
	}

    void Delay()
    {
        //yield return Yielders.GetWaitForSeconds(delay);
        bc.size = defaultSize;
        Destroy(this);
    }
}
