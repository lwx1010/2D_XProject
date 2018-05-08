using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScaleTexcoord : MonoBehaviour
{
    private float wr;
    private float hr;
    private float offX;
    private float offY;
    private UISprite s;

    public int maskRadius = 120;

    void Awake()
    {
        s = GetComponent<UISprite>();

        wr = maskRadius * 1.0f / s.atlas.spriteMaterial.mainTexture.width;
        offX = (s.GetAtlasSprite().x + (s.GetAtlasSprite().width - maskRadius) /2) * 1.0f / s.atlas.spriteMaterial.mainTexture.width;


        hr = maskRadius * 1.0f / s.atlas.spriteMaterial.mainTexture.height;
        offY = (s.GetAtlasSprite().y + s.GetAtlasSprite().height - (s.GetAtlasSprite().height - maskRadius) / 2) * 1.0f / s.atlas.spriteMaterial.mainTexture.height;
    }

    void Start()
    {
        s.atlas.spriteMaterial.SetFloat("_WidthRate", wr);
        s.atlas.spriteMaterial.SetFloat("_HeightRate", hr);
        s.atlas.spriteMaterial.SetFloat("_XOffset", offX);
        s.atlas.spriteMaterial.SetFloat("_YOffset", offY);
    }

    public void Update()
    {
       
    }
}