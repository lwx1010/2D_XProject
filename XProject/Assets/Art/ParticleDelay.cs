using UnityEngine;
using System.Collections;

public class ParticleDelay : MonoBehaviour {

    public GameObject[] goes;

    public float[] delays;


    void OnEnable()
    {
        if (goes == null) return;

        if (goes.Length != delays.Length)
        {
            Debug.LogError("animator和delay长度不一致!");
            return;
        }

        for (int i = 0; i < goes.Length; ++i)
        {
            if(goes[i]!=null)
            {
                goes[i].SetActive(false);
                StartCoroutine(StartDelay(goes[i], delays[i]));
            }

        }
    }

    IEnumerator StartDelay(GameObject go, float delay)
    {
        yield return Yielders.GetWaitForSeconds(delay);
        if(go != null)
        {
            go.SetActive(true);

        }
    }
}
