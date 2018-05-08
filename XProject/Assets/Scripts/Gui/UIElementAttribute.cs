
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIElementAttribute : MonoBehaviour
{
    //字典序列化不了
    public Dictionary<string, GameObject> elementsDict = new Dictionary<string, GameObject>();
    public List<UIElement> allElements = new List<UIElement>();

    public List<UIElement> elements = new List<UIElement>();

    public string ParentName;


    void Awake()
    {
        Init();
    }

    private void Init()
    {
        elementsDict.Clear();
        for (int i = 0; i < elements.Count; ++i)
        {
            //表示用于代码中
            if (elements[i].m_useScript)
            {

                if (elementsDict.ContainsKey(elements[i].m_gameObject.name))
                {
                    Debug.LogError("have same key name :" + (elements[i].m_gameObject.name));
                }
                else
                {
                    elementsDict.Add(elements[i].m_gameObject.name, elements[i].m_gameObject);
                }
            }

        }
    }

    public GameObject GetElementByName(string name)
    {
        if (elementsDict != null)
        {
            if (elementsDict.ContainsKey(name))
            {
                return elementsDict[name];
            }
        }
        return null;
    }

#if UNITY_EDITOR

    public void Reset()
    {
        Debug.Log("脚本Reset");
        elementsDict.Clear();
        elementsIDDict.Clear();
        elements.Clear();

        InitElement();
     
    }


    void OnValidate()
    {
        Debug.Log("数据发生改变=" + gameObject.name);
        //elements.Clear();
        //elementsDict.Clear();
        //InitElement();
    }


    public Dictionary<int, GameObject> elementsIDDict = new Dictionary<int, GameObject>();

    public EditorState m_state = EditorState.ManualEditor;
    public bool isError = false;
    public void InitElement()
    {
        isError = false;
        Transform[] trans = gameObject.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < trans.Length; ++i)
        {
            if (m_state == EditorState.AutoEditorID)
            {
                //为了名字唯一
                if (trans[i].name.Contains("_["))
                {
                }
                else
                {
                    trans[i].name = trans[i].name + "_[" + trans[i].gameObject.GetInstanceID() + "]";
                }
                if (elementsDict.ContainsKey(trans[i].name))
                {
                    //Debug.LogError("has same name :"+ trans[i].name);
                    RefreshElement(trans[i].gameObject);
                }
                else
                {
                    if (elements.Count < trans.Length)
                    {
                        elementsDict.Add(trans[i].name, trans[i].gameObject);
                        UIElement e = TraverseToComponent(trans[i].gameObject);
                        if (e != null)
                        {
                            elements.Add(e);
                        }
                    }


                }
            }
            else if (m_state == EditorState.ManualEditor)
            {
                //取出名字ID
                
                if (trans[i].name.Contains("_["))
                {
                    string tempNmae = trans[i].name;
                    string[] arr = tempNmae.Split('[');
                    string idStr = "";
                    if (arr.Length > 1)
                    {
                        idStr = arr[1];
                    }
                    idStr = "_[" + idStr;
                    tempNmae = tempNmae.Replace(idStr, "");
                    trans[i].name = tempNmae;
                }
                if (elements.Count < trans.Length)
                {
                    if (elementsIDDict.ContainsKey(trans[i].GetInstanceID()))
                    {
                        //Debug.LogError("has same name :"+ trans[i].name);
                        RefreshElement(trans[i].gameObject);
                    }
                    else
                    {
                        elementsIDDict.Add(trans[i].GetInstanceID(), trans[i].gameObject);
                        UIElement e = TraverseToComponent(trans[i].gameObject);
                        if (e != null)
                        {
                            elements.Add(e);
                        }
                    }
                }
                else
                {
                    if (elementsIDDict.ContainsKey(trans[i].GetInstanceID()))
                    {

                        RefreshElement(trans[i].gameObject);
                    }
                }
            }






        }
        if(elements.Count != trans.Length)
        {
            isError = true;
        }
    }


    public void RefreshElementDict()
    {
        Transform[] trans = gameObject.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < trans.Length; ++i)
        {
            if (m_state == EditorState.AutoEditorID)
            {
                //为了名字唯一
                if (trans[i].name.Contains("_["))
                {
                }
                else
                {
                    trans[i].name = trans[i].name + "_[" + trans[i].gameObject.GetInstanceID() + "]";
                }
            }
            if (elementsIDDict.ContainsKey(trans[i].GetInstanceID()))
            {

            }
            else
            {
                elementsIDDict.Add(trans[i].GetInstanceID(), trans[i].gameObject);

            }

            if (elementsDict.ContainsKey(trans[i].name))
            {
                //Debug.LogError("has same name :"+ trans[i].name); 
                //RefreshElement(trans[i].gameObject);
            }
            else
            {
                elementsDict.Add(trans[i].name, trans[i].gameObject);
            }

        }
    }


    public void RefreshElement(GameObject obj)
    {
        if(elements.Count<=0)
        {
            return;
        }
        int index = -1;
        UIElement last = null;

        for (int i = 0; i < elements.Count; ++i)
        {
            if (elements[i].m_gameObject.name == obj.name)
            {
                index = i;
                last = elements[i];
                break;
            }
            else
            {
                if (elements[i].m_gameObject.GetInstanceID() == obj.GetInstanceID())
                {
                    index = i;
                    last = elements[i];
                    break;
                }

            }
        }
        UIElement currt = TraverseToComponent(obj);

        if (index != -1 && (last != null))
        {
            elements.RemoveAt(index);
            currt.m_useScript = last.m_useScript;
            elements.Add(currt);
        }
        else
        {
            Debug.LogError("can't find  :" + obj.name);

        }
    }
#endif


    public UIElement TraverseToComponent(GameObject obj)
    {
        if (obj == null)
        {
            return null;
        }
        UIElement element = new UIElement();
        //其实说透彻就是 Text，Image

        Text text = obj.GetComponent<Text>();
        Image image = obj.GetComponent<Image>();
        RawImage rawimage = obj.GetComponent<RawImage>();
        element.m_gameObject = obj;

        element.m_type = UGUIElementType.Image;
        Image tiImage = obj.GetComponent<Image>();

        if (image != null)
        {
            if (image.sprite == null)
            {
                element.m_imageName = UIElement.None;
            }
            else
            {
                element.m_imageName = image.sprite.name;
            }
            if (image.material == null)
            {
                element.m_imageName = UIElement.None;
            }
            else
            {
                element.m_material = image.material.name;
            }
            //面板类型
            if (obj.name.Contains("Panel") || obj.name.Contains("panel"))
            {
                element.m_type = UGUIElementType.Panel;

                //if (image.type == Image.Type.Sliced)
                //{
                //    element.m_type = UGUIElementType.Panel;

                //}
            }
        }
        if (text != null)
        {
            element.m_type = UGUIElementType.Text;

            element.m_textContent = text.text;
            element.m_font = text.font.name;
            if (text.material == null)
            {
                element.m_imageName = UIElement.None;
            }
            else
            {
                element.m_material = text.material.name;
            }
        }
        if (rawimage != null)
        {
            element.m_type = UGUIElementType.RawImage;

            if (rawimage.texture == null)
            {
                element.m_imageName = UIElement.None; ;
            }
            else
            {
                element.m_imageTexture = rawimage.texture.name;

            }
            if (rawimage.material == null)
            {
                element.m_imageName = UIElement.None;
            }
            else
            {
                element.m_material = rawimage.material.name;
            }
        }


        if (obj.GetComponent<Button>() != null)
        {
            element.m_type = UGUIElementType.Button;

        }
        else if (obj.GetComponent<Toggle>() != null)
        {
            element.m_type = UGUIElementType.Toggle;

        }
        else if (obj.GetComponent<Slider>() != null)
        {
            element.m_type = UGUIElementType.Slider;

        }
        else if (obj.GetComponent<Scrollbar>() != null)
        {
            element.m_type = UGUIElementType.Scrollbar;

        }
        else if (obj.GetComponent<Dropdown>() != null)
        {
            element.m_type = UGUIElementType.Dropdown;

        }
        else if (obj.GetComponent<InputField>() != null)
        {
            element.m_type = UGUIElementType.InputField;

        }
        else if (obj.GetComponent<Canvas>() != null)
        {
            element.m_type = UGUIElementType.Canvas;

        }
        else if (obj.GetComponent<ScrollRect>() != null)
        {
            element.m_type = UGUIElementType.ScrollView;

        }
        else if (obj.name.Contains(UGUIElementType.DynamicComponent.ToString()))
        {
            element.m_type = UGUIElementType.DynamicComponent;
        }
        else if (obj.name.Contains(UGUIElementType.ScrollPanel.ToString()))
        {
            element.m_type = UGUIElementType.ScrollPanel;
        }
        else if (obj.name.Contains(UGUIElementType.TabButton.ToString()))
        {
            element.m_type = UGUIElementType.TabButton;
        }
        else if (obj.name.Contains(UGUIElementType.TabButtonGroup.ToString()))
        {
            element.m_type = UGUIElementType.TabButtonGroup;
        }
        else if (obj.name.Contains(UGUIElementType.TabPanel.ToString()))
        {
            element.m_type = UGUIElementType.TabPanel;
        }
        return element;

    }


}




[Serializable]
public class GameElement
{
    public GameObject m_gameObject;

}

[Serializable]
public class UIElement : GameElement
{
    public const string None = "None";

    public bool m_useScript;                        //该UI元素是否用于代码中

    public UGUIElementType m_type;                  //UI元素类型

    public string m_atalsName = None;        //UI元素图集名

    public string m_imageName = None;       //UI元素图片名
    public string m_imageTexture = None;  //UI元素贴图名

    public string m_textContent = None;     //UI元素文字内容
    public string m_material = None;     //UI元素材质球

    public string m_font = None;

    public UIElement()
    {
        m_atalsName = None;
        m_imageName = None;
        m_textContent = None;
        m_font = None;
    }

}



//UI元素类型枚举
[Serializable]
public enum UGUIElementType
{
    Text,                   //文字
    Image,
    RawImage,
    Button,
    Toggle,
    Slider,
    Scrollbar,
    Dropdown,
    InputField,
    Canvas,
    Panel,

    //复杂控件
    ScrollView,
    DynamicComponent,
    ScrollPanel,
    TabButtonGroup,
    TabButton,
    TabPanel
}
[Serializable]
public enum UIType
{

    Panel,                      //普通面板  
    DynamicComponent,           //动态控件
    ScrollPanel,                //滚动面板
    DynamicScrollPanel,         //动态滚动面板
}
public enum EditorState
{
    AutoEditorID,                  //自动添加id "_[id]"
    ManualEditor,                   //手动编辑
}