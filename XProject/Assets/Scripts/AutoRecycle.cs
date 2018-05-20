using UnityEngine;
using System.Collections;
using Riverlake;

public sealed class AutoRecycle : MonoBehaviour
{
    float delay;

    public void Play()
    {
        ParticleSystem[] ps = gameObject.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < ps.Length; ++i)
        {
            if (ps[i].duration + ps[i].startDelay > delay)
                delay = ps[i].duration + ps[i].startDelay;
            ps[i].Play();
        }
        Invoke("Recycle", delay);
        //StartCoroutine(Recycle());
    }

    void Recycle()
    {
        //yield return Yielders.GetWaitForSeconds(delay);
        //gameObject.RecycleDontDestroy();
    }
}
