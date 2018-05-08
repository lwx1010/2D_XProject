using UnityEngine;
using System.Collections;

public class AnimateTexture : MonoBehaviour
{
    public string[] sequenceNames;

    public int currentFrame;
    public UISprite uiSprite;
    public bool start
    {
        get
        {
            return _start;
        }
        set
        {
            _start = value;
            if (value == false && uiSprite != null && defaultSpriteName != string.Empty && uiSprite.spriteName != defaultSpriteName)
            {
                uiSprite.spriteName = defaultSpriteName;
                uiSprite.MakePixelPerfect();
            }
        }
    }

    bool _start = false;

    float timeElipsed = 0.0f;
    float fps = 4;

    string defaultSpriteName;

    void Awake()
    {
        if (uiSprite == null)
            uiSprite = this.GetComponent<UISprite>();
        defaultSpriteName = uiSprite.spriteName;
    }

    // Update is called once per frame
    void Update()
    {
        if (start) AutoPlayTexture();
    }

    void AutoPlayTexture()
    {
        timeElipsed += Time.deltaTime;
        if (timeElipsed >= 1.0 / fps)
        {
            timeElipsed = 0;

            DynamicLoadUnload(currentFrame);
            if (currentFrame < sequenceNames.Length)
                currentFrame++;
            else
                currentFrame = 0;
        }
    }

    void DynamicLoadUnload(int curframe)
    {
        if (curframe < sequenceNames.Length)
        {
            uiSprite.spriteName = sequenceNames[curframe];
            uiSprite.MakePixelPerfect();
        }
    }

}