using UnityEngine;
using System.Collections;

public class ParticleLoop : MonoBehaviour
{
    public float loopTime;

	// Use this for initialization
	void Start ()
    {
        Invoke("StartLoop", loopTime);
        //StartCoroutine(StartLoop());
    }

    void StartLoop()
    {
        //yield return Yielders.GetWaitForSeconds(loopTime);
        foreach (var particle in GetComponentsInChildren<ParticleSystem>())
        {
            particle.Play();
        }
        Invoke("StartLoop", loopTime);
        //StartCoroutine(StartLoop());
    }
}
