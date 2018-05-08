using UnityEngine;
using System.Collections;
using LuaFramework;
using Riverlake.Resources;

public class RandomParticle : MonoBehaviour {

    public static int MaxSelectedCounts = 3;

    public static int SelectedCounts = 0;

    public static bool NeedReset = false;

    public static float LineSpeed = 200;

    public static string EffectStr = "Prefab/UIEffect/hongbaoshanguagntexiao";

    // 全局选中 是否有对象被选中
    public static bool Flag = false;

    public static int UpMaxY = 1000;
    public static int DownMinY = -1000;
    public static int RightMaxX = 1500;
    public static int LeftMinX = -1500;

    public GameObject EffectGo;
    public UIParticles EffectPart;

    public static bool CanSelected = true;
    public static float WaitTime = 0;

    // 动画终止系数
    public static float StopFactor = 0.75f;

    // 位置动画
    public TweenPosition PosTween;
    // 旋转动画
    public TweenRotation RotTween;

    public UISprite tar;

    public static string SelectedName ;

    // 动画方向 1-上下，2-下上，3左右，4右左，5上左下右，6上右下左，7下右上左，8下左上右
    public int Direction = 1;

    // 坐标缩放比例
    public float factor = 1f;

    // 当前状态 0 初始状态 1被选中 2选中后动画
    public int Selected = 0;

    public RandomRedBag manager = null;

    private void Awake()
    {
        if (manager == null)
        {
            manager = this.transform.parent.GetComponent<RandomRedBag>();
            //Debug.Log(manager);
            //Debug.Log(this.transform.parent.name);
        }
        // 获取组件
        this.PosTween = gameObject.GetComponent<TweenPosition>();
        this.RotTween = gameObject.GetComponent<TweenRotation>();
        this.tar = transform.GetComponent<UISprite>();
        GameObject eg = ResourceManager.LoadPrefab(EffectStr) as GameObject;
        this.EffectGo = Instantiate(eg);
        this.EffectGo.transform.parent = this.transform;
        this.EffectGo.transform.localScale = Vector3.one;
        this.EffectGo.transform.localPosition = Vector3.zero;
        if (this.EffectGo)
        {
            this.EffectPart = EffectGo.transform.GetComponent<UIParticles>();
        }
    }

    private void Start()
    {
        // 添加动画回调，实现循环播放。
        this.PosTween.AddOnFinished(this.ResetAnim);
        //Debug.Log(this.PosTween.onFinished.Count);
        //ResetAnim();
    }

    private void Update()
    {
        // 被选中,则停止动画
        if (this.Selected == 1)
        {
            StopNow();
            this.Selected = 2;
            GoToCenter();
            //Debug.Log(RandomParticle.WaitTime + "  //  " + RandomParticle.CanSelected + "  //  " + RandomParticle.SelectedCounts);
            Invoke("RevertState", RandomParticle.WaitTime+0.05f);
        }

        if (RandomParticle.Flag && this.Selected < 1)
        {
            JudgeState();
        }

    }

    public void OnClick()
    {
        //Debug.Log(RandomParticle.WaitTime + " //  " + RandomParticle.CanSelected);
        // 只能选中一个
        if (RandomParticle.CanSelected && this.Selected == 0 && !RandomParticle.Flag)
        {
            //Debug.Log("..");
            this.Selected = 1;
            RandomParticle.SelectedName = this.gameObject.name;
            RandomParticle.SelectedCounts++;

            RandomParticle.CanSelected = false;
            if (RandomParticle.SelectedCounts >= RandomParticle.MaxSelectedCounts)
            {
                RandomParticle.Flag = true;
            }
            this.EffectPart.Play();
            this.tar.depth = 20;
        }
    }

    // 重置动画
    public void ResetAnim()
    {
        this.tar.depth = 11;
        StopAnim();
        if (!RandomParticle.Flag)
        {
            
            this.PosTween.onFinished.RemoveRange(0, this.PosTween.onFinished.Count);
            this.PosTween.AddOnFinished(this.ResetAnim);
            ToRandom();
            StartAnimForward();
        }
        
    }

    // 随机动画状态
    public void ToRandom()
    {
        //this.Direction = Random.Range(1, 8);
        this.factor = Random.Range(1.2f,2f);
        //this.RotTween.from = Vector3.zero;
        //this.RotTween.to = new Vector3(0, 0, 360);
        //this.RotTween.duration = Random.Range(1.2f, 3.6f);
        //this.RotTween.style = UITweener.Style.Loop;

        this.PosTween.duration = Random.Range(5.0f, 8.0f);
        //this.PosTween.delay = Random.Range(0.05f, 2.0f) * (8 - this.PosTween.duration);
        this.PosTween.animationCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
        //this.PosTween.method = (UITweener.Method)Random.Range(0, 5);
        this.PosTween.method = UITweener.Method.Linear;
        float newX = (Random.Range(0, Screen.width) - (int)(Screen.width * 0.5)) * this.factor;
        this.PosTween.from = new Vector3(newX, UpMaxY, 0);
        this.PosTween.to = new Vector3(newX, DownMinY, 0);
        /*
        // 随机方向
        if (this.Direction == 1 || this.Direction == 5)
        {
            this.PosTween.from = new Vector3((Random.Range(0,Screen.width)- (int)(Screen.width*0.5)), UpMaxY, 0);
            this.PosTween.to = new Vector3((Random.Range(0, Screen.width) - (int)(Screen.width * 0.5)), DownMinY, 0);
        }

        if (this.Direction == 2 || this.Direction == 6)
        {
            this.PosTween.from = new Vector3((Random.Range(0, Screen.width) - (int)(Screen.width * 0.5)), DownMinY, 0);
            this.PosTween.to = new Vector3((Random.Range(0, Screen.width) - (int)(Screen.width * 0.5)), UpMaxY, 0);
        }

        if (this.Direction == 3 || this.Direction == 7)
        {
            this.PosTween.from = new Vector3(LeftMinX, (Random.Range(0, Screen.height) - (int)(Screen.width * 0.5)), 0);
            this.PosTween.to = new Vector3(RightMaxX, (Random.Range(0, Screen.height) - (int)(Screen.width * 0.5)), 0);
        }

        if (this.Direction == 4 || this.Direction == 8)
        {
            this.PosTween.from = new Vector3(RightMaxX, (Random.Range(0, Screen.width) - (int)(Screen.width * 0.5)), 0);
            this.PosTween.to = new Vector3(LeftMinX, (Random.Range(0, Screen.width) - (int)(Screen.width * 0.5)), 0);
        }

        */
        
        transform.localPosition = this.PosTween.from;

        //Debug.Log(this.Selected + " Dic： " + this.Direction + " Method: " + this.PosTween.method + " Start: " + transform.localPosition + " End: " + this.PosTween.to);

    }

    public void StartAnimForward()
    {
        this.PosTween.PlayForward();
        //this.RotTween.PlayForward();
    }

    // 停止目标
    public void StopAnim()
    {
        if (this.PosTween.gameObject)
        {
            this.PosTween.ResetToBeginning();
            this.PosTween.PlayReverse();
        }
        if (this.RotTween.gameObject)
        {
            this.RotTween.ResetToBeginning();
            this.RotTween.PlayReverse();
        }
    }

    //JudgeState 选中红包数量达到最大时,决定自身的动画状态
    public void JudgeState()
    {
        if (this.transform.localPosition.y >= UpMaxY * StopFactor || this.transform.localPosition.y <= DownMinY * StopFactor)
        {
            this.StopAnim();
        }
        else 
        {
            this.PosTween.duration = 4.4f;
        }

    }

    public void StopNow()
    {
        this.PosTween.enabled = false;
        //this.RotTween.enabled = false;
    }

    public void GoToCenter()
    {
        //this.factor = Random.Range(1f, 1.8f);

        //RandomParticle.WaitTime = Vector3.Distance(this.transform.localPosition, Vector3.zero) / LineSpeed;
        RandomParticle.WaitTime = 1f;
        this.RotTween.duration = RandomParticle.WaitTime * 0.2f;
        this.RotTween.from = new Vector3(0, 0, 10);
        this.RotTween.to = new Vector3(0, 0, -10);
        this.RotTween.style = UITweener.Style.Loop;

        //this.PosTween.duration = RandomParticle.WaitTime;
        //this.PosTween.delay = Random.Range(0f, 2.0f);
        //this.PosTween.animationCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
        //this.PosTween.method = UITweener.Method.Linear;  //(UITweener.Method)Random.Range(0, 5);
        //this.PosTween.from = this.transform.localPosition;
        //this.PosTween.style = UITweener.Style.Once;
        //this.PosTween.to = Vector3.zero;

        //this.PosTween.ResetToBeginning();
        this.RotTween.ResetToBeginning();
        this.RotTween.PlayForward();
        //this.PosTween.PlayForward();
        this.PosTween.enabled = false;
        this.RotTween.enabled = true;
        
        this.PosTween.onFinished.RemoveRange(0, this.PosTween.onFinished.Count);
        //this.PosTween.AddOnFinished(this.DropReset);
        if (RandomParticle.SelectedCounts >= RandomParticle.MaxSelectedCounts)
        {
            RandomParticle.NeedReset = true;
            Invoke("DropReset", RandomParticle.WaitTime);
        }
        else
        {
            Invoke("DropReset", RandomParticle.WaitTime);
        }

    }

    public void RevertState()
    {
        RandomParticle.CanSelected = true;
    }

    public void DropReset()
    {
        //Debug.Log("选中重置");
        this.Selected = 0;
        //RandomParticle.WaitTime = 0;
        this.RotTween.from = new Vector3(0, 0, 0);
        this.RotTween.to = new Vector3(0, 0, 0);
        this.RotTween.style = UITweener.Style.Once;
        this.RotTween.duration = 0.6f;
        this.PosTween.ResetToBeginning();
        this.RotTween.ResetToBeginning();
        this.RotTween.enabled = false;
        if (RandomParticle.SelectedCounts >= RandomParticle.MaxSelectedCounts)
        {
            Invoke("ResetTime", 2f);
        }
        else
        {
            Invoke("ResetTime", 0f);
        }
        ToRandom();
        StartAnimForward();
    }

    public void ResetTime() 
    {
        RandomParticle.WaitTime = 0f;
    }

}

