using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomRedBag : MonoBehaviour {

    public GameObject prefab;


    public int Counts = 20;

    public static Vector3 InitPos = new Vector3(0,1000,0);

    public List<GameObject> CacheGoPool;

    public List<RandomParticle> CacheRPPool;

    public Transform parent;

    private void Awake()
    {
        prefab = transform.Find("RbItem").gameObject;
        parent = this.transform.Find("Mask");
        for (int i = 0; i < Counts; i++)
        {
            GameObject go = Instantiate(prefab);
            //go.transform.position = InitPos;
            go.name = i.ToString();
            go.transform.localPosition = Vector3.zero;
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;
            //go.SetActive(true);
            CacheGoPool.Add(go);
            CacheRPPool.Add(go.GetComponent<RandomParticle>());
        }

    }

    private void Update() 
    {
        
        if(RandomParticle.NeedReset && RandomParticle.WaitTime < 0.005f)
        {
            InitRain();
        }
        /*
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartRain();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ResetRain();
        }
        */
    }



    public void StartRain()
    {
        for (int i = 0; i < CacheGoPool.Count; i++)
        {
            if (!CacheGoPool[i].activeSelf)
            {
                CacheGoPool[i].SetActive(true);
            }
            //CacheRPPool[i].ToRandom();
            //CacheRPPool[i].StartAnimForward();
            //Debug.Log(i + "/" + GetCounts());
            if (i <= 8)
            {
                CacheRPPool[i].PosTween.delay = Random.Range(0.25f, 0.6f) * i;
            }
            else if (i <= 16)
            {
                CacheRPPool[i].PosTween.delay = Random.Range(0.15f, 0.75f) * i + 2;
            }
            else if (i <= 24)
            {
                CacheRPPool[i].PosTween.delay = Random.Range(0.1f, 0.85f) * i + 4;
            }
            CacheRPPool[i].ResetAnim();
        }
    }


    public void ResetRain()
    {
        //Debug.Log(GetCounts());
        RandomParticle.Flag = false;
        RandomParticle.SelectedCounts = 0;
        RandomParticle.WaitTime = 0;
        for (int i = 0; i < GetCounts(); i++)
        {
            //Debug.Log(i + "/" + GetCounts());
            if(i<=8)
            {
                CacheRPPool[i].PosTween.delay = Random.Range(0.25f, 0.6f) * i;
            }
            else if (i <= 16)
            {
                CacheRPPool[i].PosTween.delay = Random.Range(0.15f, 0.75f) * i + 2;
            }
            else if (i <= 24)
            {
                CacheRPPool[i].PosTween.delay = Random.Range(0.1f, 0.85f) * i + 4;
            }
            CacheRPPool[i].Selected = 0;
            CacheRPPool[i].ResetAnim();
        }


    }

    public void Add(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(prefab);
            go.name = (this.CacheGoPool.Count).ToString();
            go.transform.localPosition = Vector3.zero;
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;
            go.SetActive(true);
            CacheGoPool.Add(go);
            RandomParticle rp = go.GetComponent<RandomParticle>();
            rp.ResetAnim();
            CacheRPPool.Add(rp);
            
        }

    }

    public void Remove(int count)
    {

        for (int i = 0; i < count; i++)
        {
            if (CacheGoPool.Count - 1 >= 0)
            {
                Destroy(CacheGoPool[CacheGoPool.Count - 1]);
                CacheGoPool.RemoveAt(CacheGoPool.Count - 1);
                CacheRPPool.RemoveAt(CacheRPPool.Count - 1);
                
            }
        }

    }

    public int GetCounts()
    {
        return this.CacheGoPool.Count;
    }

    public void InitRain() 
    {
        //Debug.Log(RandomParticle.WaitTime);
        RandomParticle.NeedReset = false;
        RandomParticle.Flag = false;
        RandomParticle.SelectedCounts = 0;
        //RandomParticle.WaitTime = 0;
        for (int i = 0; i < GetCounts(); i++)
        {
            //Debug.Log(i + "/" + GetCounts());
            CacheRPPool[i].Selected = 0;
            CacheRPPool[i].StopAnim();
            CacheRPPool[i].StopNow();
            //Debug.Log(i + "/" + GetCounts()+CacheRPPool[i].RotTween.enabled + CacheRPPool[i].PosTween.enabled);
        }
    }

    

}
