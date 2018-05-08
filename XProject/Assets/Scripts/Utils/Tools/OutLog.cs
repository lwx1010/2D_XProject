using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class OutLog : MonoBehaviour
{
    //static List<string> mLines = new List<string>();
    //static List<string> mWriteTxt = new List<string>();
    //private string outpath;

    void Awake()
    {
        DontDestroyOnLoad(this);
        //outpath = Application.persistentDataPath + "/outLog.txt";
        ////每次启动客户端删除之前保存的Log
        //if (System.IO.File.Exists(outpath))
        //{
        //    File.Delete(outpath);
        //}
        //在这里做一个Log的监听
        BuglyAgent.RegisterLogCallback(HandleLog);
    }

//    void Update()
//    {
//#if UNITY_EDITOR
//        //因为写入文件的操作必须在主线程中完成，所以在Update中写入文件。
//        if (mWriteTxt.Count > 0)
//        {
//            string[] temp = mWriteTxt.ToArray();
//            for (int i = 0; i < temp.Length; ++i)
//            {
//                FileStream fs = null;
//                try
//                {
//                    fs = new FileStream(outpath, FileMode.OpenOrCreate);
//                    using (StreamWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8))
//                    {
//                        writer.WriteLine(temp[i]);
//                    }
//                    mWriteTxt.Remove(temp[i]);
//                }
//                finally
//                {
//                    if (fs != null)
//                        fs.Dispose();
//                }
//            }
//        }
//#endif
//    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Log || type == LogType.Warning)
        {
            //mWriteTxt.Add(logString);
            FormatString2DebugConsole(logString);
        }
        if (type == LogType.Error || type == LogType.Exception)
        {
            //mWriteTxt.Add(logString);
            //mWriteTxt.Add(stackTrace);
            FormatString2DebugConsole(logString);
            FormatString2DebugConsole(stackTrace);
        }
    }

    void FormatString2DebugConsole(string msg)
    {
        string timeStr = string.Format("{0}.{1}-{2}:", DateTime.Now.ToString("H:m:s"),
                DateTime.Now.Millisecond, Time.frameCount % 999);
        if (!msg.Contains(timeStr))
        {
            DebugConsole.Log(string.Format("{0} {1}", timeStr, msg));
        }
        else
        {
            DebugConsole.Log(msg);
        }
    }

    //把错误的信息保存起来，用来输出在手机屏幕上
    static public void Log(params object[] objs)
    {
        string text = "";
        for (int i = 0; i < objs.Length; ++i)
        {
            if (i == 0)
            {
                text = string.Format("{0}{1}", text, objs[i]);
            }
            else
            {
                text = string.Format("{0}, {1}", text, objs[i]);
            }
        }
        //if (Application.isPlaying)
        //{
        //    if (mLines.Count > 5)
        //    {
        //        mLines.RemoveAt(0);
        //    }
        //    mLines.Add(text);
        //}
    }

    //void OnGUI()
    //{
    //    if (Config.User_Config.dev_mode)
    //    {
    //        GUI.color = Color.red;
    //        GUI.skin.label.fontSize = 15;

    //        for (int i = 0, imax = mLines.Count; i < imax; ++i)
    //        {
    //            GUILayout.Label(mLines[i]);
    //        }
    //    }
    //}
}