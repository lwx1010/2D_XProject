using System.Collections;
using UnityEngine;
/// <summary>
/// UIMask保持遮挡唯一
/// </summary>
public class UIMask : MonoBehaviour
{

    private UIMask lastMask;
    private static UIMask curMask;
    [HideInInspector]
    public UISprite mask;

    private static Color aplha = new Color(1 ,1, 1 , 0.7f);
    void Awake()
    {
        mask = this.GetComponent<UISprite>();
    }

    private void OnEnable()
    {
        if (mask == null)
            return;
        if (this != curMask)
        {
            lastMask = curMask;
            this.StartCoroutine(hideLastMask()); 
//            if (lastMask != null && lastMask.mask != null)
//                lastMask.mask.color = aplha;
        }

        curMask = this;
        if (mask != null) mask.color = lastMask != null ? aplha : Color.white;
    }


    private IEnumerator hideLastMask()
    {
        yield return Yielders.EndOfFrame;
        if (lastMask != null && lastMask.mask != null)
            lastMask.mask.color = aplha;
    }

    private void OnDisable()
    {
        if (mask == null)
            return;
        if (lastMask != null && lastMask.mask != null)
            lastMask.mask.color = Color.white;
        curMask = lastMask;
    }


}
