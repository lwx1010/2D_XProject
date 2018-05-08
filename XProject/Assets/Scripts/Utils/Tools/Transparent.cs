using UnityEngine;
using System.Collections;
using DG;

public class Transparent : MonoBehaviour
{
    private string defaultShaderName;

	// Use this for initialization
	void Start () 
    {
        
	}
	
	// Update is called once per frame
	void OnDestroy () 
    {
        CancelTransparent();
	}

    public void InvokeTranparent()
    {
        Renderer render = gameObject.GetComponent<Renderer>();
        Shader shader = render.material.shader;
        defaultShaderName = shader.name;
        if (!shader.name.Equals("Custom/Transparent/Cutout/Soft Edge Unlit"))
        {
            render.material.shader = Shader.Find("Custom/Transparent/Cutout/Soft Edge Unlit");
            Color color = render.material.color;
            color.a = 0.2f;
            if (render.material.GetColor("_Color").a > 0.3f)
                render.material.SetColor("_Color", color);
        }
    }

    public void CancelTransparent()
    {
        Renderer render = gameObject.GetComponent<Renderer>();
        Shader shader = render.material.shader;
        if (shader.name.Equals("Custom/Transparent/Cutout/Soft Edge Unlit"))
        {
            render.material.shader = Shader.Find(defaultShaderName);
            Color color = render.material.color;
            color.a = 1.0f;
            render.material.SetColor("_Color", color);
        }
    }

    void Update()
    {

    }
}
