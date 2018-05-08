using UnityEngine;
using System.Collections;

public class MarryBanquetEffect : MonoBehaviour {

    public ParticleSystem particl;
	
	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (particl == null)
        {
            particl = this.transform.Find("effect/xiyancaiji/guang").GetComponent<ParticleSystem>();
        }
	}
}
