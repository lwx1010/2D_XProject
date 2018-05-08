using UnityEngine;
using System.Collections;

/// <summary>  
/// This is a container to deal with the particles render by control the render queue.  
/// </summary>  
[ExecuteInEditMode]
public sealed class UIParticles : MonoBehaviour
{
    private const float UPDATE_RENDER_TIME = 0.2f;
    private float lastTime = 0f;
    private Renderer[] rendererArray = null;
    private bool isWidgetOK = false;
    private bool isRendererArrayOK = false;
    private Renderer tempMeshRenderer = null;

    //public bool isExplicit = false;
    public int RenderQueue = 3000;
    public UIWidget parentWidget = null;
    public bool IsForward = true;
    private ParticleSystem ps;
    private float rosx, rosy, rosz = 0;
    private bool IsSetRotation = false;

    void OnDestroy()
    {
        rendererArray = null;
        parentWidget = null;
    }

    void Start()
    {
        ps = transform.GetComponentInChildren<ParticleSystem>();
        ps.Play();
        if (IsSetRotation)
        {
            SetParticleRotation(rosx, rosy, rosz);
        }     
    }

    void LateUpdate()
    {
        lastTime += Time.deltaTime;
        if (lastTime < UPDATE_RENDER_TIME)
            return;
        lastTime = -Random.Range(0, UPDATE_RENDER_TIME);

        if (parentWidget == null)
        {
            parentWidget = NGUITools.FindInParents<UIWidget>(this.gameObject);
        }

        if (rendererArray == null || rendererArray.Length == 0)
        {
            rendererArray = this.GetComponentsInChildren<Renderer>(true);
        }

        isWidgetOK = parentWidget != null && parentWidget.drawCall != null;
        isRendererArrayOK = rendererArray != null && rendererArray.Length > 0;

        if (isWidgetOK  && isRendererArrayOK)
        {
            OnChangeRenderQueue();
        }
    }

    void OnChangeRenderQueue()
    {
        int curRenderQueue = !isWidgetOK ? RenderQueue : parentWidget.drawCall.finalRenderQueue;

        if (curRenderQueue != RenderQueue)
        {
            if (IsForward)
                curRenderQueue += 1;
            else
                curRenderQueue -= 1;
            RenderQueue = curRenderQueue;
            for (int i = 0; i != rendererArray.Length; ++i)
            {
                tempMeshRenderer = rendererArray[i];
                if (tempMeshRenderer != null)
                {
#if UNITY_EDITOR
                    tempMeshRenderer.material.renderQueue = RenderQueue;
#else
                    tempMeshRenderer.material.renderQueue = RenderQueue;  
#endif
                }
            }
        }
    }

    public void Play()
    {
        if (ps == null)
            return;
        ps.Play();
    }

    public void Pause()
    {
        if (ps == null)
            return;
        ps.Pause();
    }

    public void Stop()
    {
        if (ps == null)
            return;
        ps.Stop();
    }

    /// <summary>
    /// 设置旋转方向
    /// </summary>
    /// <param name="x">世界坐标x</param>
    /// <param name="y">世界坐标y</param>
    /// <param name="z">世界坐标z</param>
    public void SetParticleRotation(float x, float y, float z)
    {
        if (ps == null)
        {
            IsSetRotation = true;
            rosx = x;
            rosy = y;
            rosz = z;
            return;
        }
        ps.startRotation3D = new Vector3(x, y, z);
    }
}