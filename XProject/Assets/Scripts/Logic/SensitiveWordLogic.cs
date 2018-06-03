using System;
using System.Collections;
using AL;
using UnityEngine;
using System.Collections.Generic;
using LuaInterface;
using LuaFramework;

/// <summary>
///  敏感字
/// </summary>
public class SensitiveWordLogic : Singleton<SensitiveWordLogic>
{
    Dictionary<string, bool> SensitiveWordDic = new Dictionary<string, bool>();
    List<string> SensitiveWord;
    //List<string> unSensitiveWors = new List<string>();

    public static SensitiveWordLogic GetInstance()
    {
        return SensitiveWordLogic.Instance;
    }

    void Awake()
    {
        StartCoroutine("LoadSensitiveWord");
    }

    IEnumerator LoadSensitiveWord()
    {
        var rr = Resources.LoadAsync("sensitivewordtable", typeof(TextAsset));
        yield return rr;
        TextAsset textAsset = rr.asset as TextAsset;

        //Util.CallMethod("COMMONCTRL", "DealWithSensitiveWord");
        if (!( textAsset == null ||textAsset.text.Equals("") || textAsset.text.Equals(null)))
        {
            //Debug.Log(textAsset.text);
            //将读取到的字符串进行分割后存储到定义好的数组中
            var words = textAsset.text.Split(',');
            //this.CreateSensitiveWordMap(words);
            string element;
            for (int i = 0; i < words.Length; ++i)
            {
                element = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(words[i]));
                //SensitiveWord.Add(element);
                if (!SensitiveWordDic.ContainsKey(element))
                    SensitiveWordDic.Add(element, true);
            }
            //Debug.Log(SensitiveWord.Count);
        }

        SensitiveWord = new List<string>(SensitiveWordDic.Keys);
        SensitiveWordDic.Clear();
        //print("=-------LoadSensitiveWord------------");
        Util.CallMethod("COMMONCTRL", "DealWithSensitiveWord");
    }

    /// 替换敏感字
    /// </summary>
    /// <param name="src"></param>
    /// <param name="replace"></param>
    /// <returns></returns>
    public string filter(string src, string replace = "*")
    {
        try
        {
            if (SensitiveWord == null)
                return src;

            int len = SensitiveWord.Count;
            for (int i = 0; i < len; ++i)
            {
                if (SensitiveWord[i].Length == 0)
                    continue;

                var bytes = System.Text.Encoding.UTF8.GetBytes(src);
                src = System.Text.Encoding.UTF8.GetString(bytes);
                src = src.Replace(SensitiveWord[i], replace);
            }
        }
        catch (Exception)
        {

        }

        return src;
    }

    /// <summary>
    /// 动态删除屏蔽字
    /// </summary>
    /// <param name="srcWords"></param>
    public void RemoveSensitiveWord(LuaTable srcWords)
    {
        string element;
        if (srcWords != null)
        {
            List<string> removeSensitiveWords = new List<string>();
            int len = srcWords.Length;
            for (int i = 1; i <= srcWords.Length; ++i)
            {
                element = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(srcWords[i].ToString()));
                removeSensitiveWords.Add(element);
            }

            List<string> removeList = new List<string>();
            len = SensitiveWord.Count;
            for (int i = 0; i < len; ++i)
            {
                if (removeSensitiveWords.IndexOf(SensitiveWord[i]) >= 0)
                    removeList.Add(SensitiveWord[i]);
            }

            for (int i = 0; i < removeList.Count; i++)
            {
                SensitiveWord.Remove(removeList[i]);
            }

            removeSensitiveWords = null;
            removeList = null;
        }
    }

    /// <summary>
    /// 动态添加屏蔽字
    /// </summary>
    /// <param name="srcWords"></param>
    public void AddSensitiveWord(LuaTable srcWords)
    {
        string element;
        if (srcWords != null)
        {
            for (int i = 1; i <= srcWords.Length; ++i)
            {
                element = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(srcWords[i].ToString()));

                if (SensitiveWord.IndexOf(element) < 0)
                    SensitiveWord.Add(element);
            }
        }
    }

    /// <summary>
    /// 是否包含敏感字
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public bool isSensitive(string src)
    {
        int len = SensitiveWord.Count;
        var bytes = System.Text.Encoding.UTF8.GetBytes(src);
        src = System.Text.Encoding.UTF8.GetString(bytes);
        for (int i = 0; i < len; i++)
        {
            if (SensitiveWord[i].Length > 0 && src.Contains(SensitiveWord[i]))
            {
                return true;
            }
        }

        return false;
    }

    ///////////////////////////////////////////////////////////////////////////////////
    //DFA屏蔽字处理
    //public class SensitiveWordNode
    //{
    //    public Int32 flag;
    //    public Dictionary<char, SensitiveWordNode> nodeChildren;

    //    public SensitiveWordNode(Int32 f, Dictionary<char, SensitiveWordNode> nodes = null)
    //    {
    //        this.flag = f;
    //        this.nodeChildren = nodes;
    //        if (nodes == null)
    //            this.nodeChildren = new Dictionary<char, SensitiveWordNode>();
    //    }
    //}

    //SensitiveWordNode rootNode = new SensitiveWordNode(0);

    //private void CreateSensitiveWordMap(string[] words)
    //{
    //    string element;
    //    for (int i = 0; i < words.Length; ++i)
    //    {
    //        element = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(words[i]));
    //        if (this.unSensitiveWors.IndexOf(element) >= 0)
    //            continue;

    //        if (element.Length > 0)
    //            this.AddSensitiveWordNode(this.rootNode, element, 0);
    //    }
    //}

    //public void AddSensitiveWordNode(SensitiveWordNode node, string word, Int32 index)
    //{
    //    SensitiveWordNode curNode = this.FindSensitiveWordNode(node, word[index]);
    //    if (curNode == null)
    //    {
    //        curNode = new SensitiveWordNode(0);
    //        node.nodeChildren.Add(word[index], curNode);
    //    }

    //    if (index == (word.Length - 1))
    //        curNode.flag = 1;

    //    index = index + 1;
    //    if (index < word.Length)
    //        this.AddSensitiveWordNode(curNode, word, index);
    //}

    //private SensitiveWordNode FindSensitiveWordNode(SensitiveWordNode node, char c)
    //{
    //    if (node.nodeChildren.ContainsKey(c))
    //        return node.nodeChildren[c];

    //    return null;
    //}

    //public bool isSensitive(string inputStr)
    //{
    //    SensitiveWordNode node = this.rootNode;
    //    Int32 wordCnt = 0;

    //    for (int i = 0; i < inputStr.Length; i++)
    //    {
    //        if (!inputStr[i].Equals(' '))
    //            node = this.FindSensitiveWordNode(node, inputStr[i]);

    //        if (node == null)
    //        {
    //            i = i - wordCnt;
    //            node = this.rootNode;
    //            wordCnt = 0;
    //        }
    //        else if (node.flag == 1)
    //            return true;
    //        else
    //            wordCnt = wordCnt + 1;
    //    }

    //    return false;
    //}

    //public string filter(string inputStr, string replace = "*")
    //{
    //    char[] srcArr = inputStr.ToCharArray();
    //    SensitiveWordNode node = this.rootNode;
    //    Int32 wordCnt = 0;

    //    for (int i = 0; i < srcArr.Length; i++)
    //    {
    //        node = this.FindSensitiveWordNode(node, srcArr[i]);

    //        if (node == null)
    //        {
    //            i = i - wordCnt;
    //            node = this.rootNode;
    //            wordCnt = 0;
    //        }
    //        else if (node.flag == 1)
    //            for (int k = 0; k <= wordCnt; k++)
    //            {
    //                srcArr[i - k] = replace[0];
    //            } 
    //        else
    //            wordCnt = wordCnt + 1;
    //    }

    //    return new string(srcArr);
    //}

    //public void RemoveSensitiveWord(LuaTable srcWords)
    //{
    //    string element;
    //    if (srcWords != null)
    //    {
    //        List<string> removeSensitiveWords = new List<string>();
    //        int len = srcWords.Length;
    //        for (int i = 1; i <= srcWords.Length; ++i)
    //        {
    //            element = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(srcWords[i].ToString()));
    //            this.unSensitiveWors.Add(element);
    //        }
    //    }
    //}

    //public void AddSensitiveWord(LuaTable srcWords)
    //{
    //    string element;
    //    if (srcWords != null)
    //    {
    //        for (int i = 1; i <= srcWords.Length; ++i)
    //        {
    //            element = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(srcWords[i].ToString()));
    //            if (this.unSensitiveWors.IndexOf(element) >= 0)
    //                continue;

    //            if (element.Length > 0)
    //                this.AddSensitiveWordNode(this.rootNode, element, 0);
    //        }
    //    }
    //}
}





