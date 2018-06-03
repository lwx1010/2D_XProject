using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using AL.Crypto;
using System.Xml;
using AL;

public class IOSGenerateHelper
{
    public static string IOS_RANDOM_CONFIG = Path.GetFullPath(Path.Combine(Application.dataPath, "../version/ios_random.xml")).Replace("\\", "/");

    public static string IOS_RES_PATH = Application.streamingAssetsPath + "/" + LuaConst.osDir;

    private static Dictionary<string, string> randomKeyDict = new Dictionary<string, string>();
    /// <summary>
    /// 唯一订单号生成
    /// </summary>
    /// <returns></returns>
    private static string GenerateOrderNumber()
    {
        string strDateTimeNumber = DateTime.Now.ToString("yyyyMMddHHmmssms");
        string strRandomResult = NextRandom(1000, 1).ToString();
        return strDateTimeNumber + strRandomResult;
    }
    /// <summary>
    /// 参考：msdn上的RNGCryptoServiceProvider例子
    /// </summary>
    /// <param name="numSeeds"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    private static int NextRandom(int numSeeds, int length)
    {
        // Create a byte array to hold the random value.  
        byte[] randomNumber = new byte[length];
        // Create a new instance of the RNGCryptoServiceProvider.  
        System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        // Fill the array with a random value.  
        rng.GetBytes(randomNumber);
        // Convert the byte to an uint value to make the modulus operation easier.  
        uint randomResult = 0x0;
        for (int i = 0; i < length; i++)
        {
            randomResult |= ((uint)randomNumber[i] << ((length - 1 - i) * 8));
        }
        return (int)(randomResult % numSeeds) + 1;
    }

    private static string CreateRandomCode(string packageName)
    {
        string code = string.Empty;
        if (File.Exists(IOS_RANDOM_CONFIG))
        {
            LoadConfig();
            if (!randomKeyDict.TryGetValue(packageName, out code))
            {
                code = GenerateOrderNumber();
                randomKeyDict.Add(packageName, code);
                SaveConfig();
            }
        }
        else
        {
            code = GenerateOrderNumber();
            randomKeyDict.Add(packageName, code);
            SaveConfig();
        }
        return code;
    }

    public static void GenerateNewCrypto(string packageName)
    {
        string path = Application.dataPath + "/Resources/crypto.txt";
        string key = CreateRandomCode(packageName);
        string iv = "jianghu505+iv@hw";
        string finalStr = key + "|" + iv;
        if (File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, finalStr);
        AssetDatabase.Refresh();
    }

    static void LoadConfig()
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(IOS_RANDOM_CONFIG);
        var root = doc.SelectSingleNode("ios_confuse");
        var nodes = root.SelectNodes("data");
        randomKeyDict.Clear();
        for (int i = 0; i < nodes.Count; ++i)
        {
            var key = nodes[i].Attributes["key"].Value;
            var code = nodes[i].Attributes["code"].Value;
            randomKeyDict.Add(key, code);
        }
    }

    static void SaveConfig()
    {
        XmlDocument doc = new XmlDocument();
        if (File.Exists(IOS_RANDOM_CONFIG)) File.Delete(IOS_RANDOM_CONFIG);
        XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
        doc.AppendChild(dec);
        var attr = doc.CreateElement("ios_confuse");
        var node = doc.AppendChild(attr);
        foreach (var pair in randomKeyDict)
        {
            var data = node.OwnerDocument.CreateElement("data");
            data.SetAttribute("key", pair.Key);
            data.SetAttribute("code", pair.Value);
            node.AppendChild(data);
        }
        doc.Save(IOS_RANDOM_CONFIG);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 打包完毕后搜索所有的bin文件并重命名进行混淆(IOS)
    /// </summary>
    public static string RenameResFileWithRandomCode(string fileName)
    {
        string newStr = Convert.ToBase64String(Crypto.Encode(Encoding.GetBytes(fileName)));
        if (newStr.Contains("/"))
            newStr = newStr.Replace("/", "");
        return newStr;
    }

    /// <summary>
    /// ios平台资源混淆
    /// </summary>
    public static void IOSConfusing()
    {
        RenameLuaFiles();
        InsertRandomFile();
    }

    private static void RenameLuaFiles()
    {
        string rootPath = IOS_RES_PATH + "/lua/";
        string[] files = Directory.GetFiles(rootPath, "*.unity3d", SearchOption.AllDirectories);
        ABPackHelper.ShowProgress("rename lua file...", 0);
        for (int i = 0; i < files.Length; ++i)
        {
            var file = files[i];
            var fileName = Path.GetFileName(file);
            var bytes = File.ReadAllBytes(file);
            File.Delete(file);
            var newName = RenameResFileWithRandomCode(fileName) + ".unity3d";
            File.WriteAllBytes(rootPath + newName, bytes);
            ABPackHelper.ShowProgress("rename lua file: " + fileName, (float)i / (float)files.Length);
        }
        ABPackHelper.ShowProgress("Finished...", 1);
        AssetDatabase.Refresh();
    }

    public static void InsertRandomFile()
    {
        string[] files = Directory.GetFiles(IOS_RES_PATH, "*.*", SearchOption.AllDirectories);
        Hashtable tbl = new Hashtable();
        ABPackHelper.ShowProgress("insert random file...", 0);
        for (int i = 0; i < files.Length; ++i)
        {
            var path = Path.GetDirectoryName(files[i]);
            if (tbl.ContainsKey(path)) continue;
            tbl.Add(path, "path");
            int randomNum = UnityEngine.Random.Range(30, 200);
            for (int j = 0; j < randomNum; ++j)
            {
                try
                {
                    string str = GetRandomString(30);
                    var save_path = Path.Combine(path, str);
                    if (File.Exists(save_path)) continue;
                    File.WriteAllBytes(save_path, Encoding.GetBytes(str));
                }
                catch
                {

                }
            }
            ABPackHelper.ShowProgress("insert random file...", (float)i / (float)files.Length);
        }
        ABPackHelper.ShowProgress("insert random file...", 1);
        AssetDatabase.Refresh();
    }

    ///<summary>
    ///生成随机字符串 
    ///</summary>
    ///<param name="length">目标字符串的长度</param>
    ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
    ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
    ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
    ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
    ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
    ///<returns>指定长度的随机字符串</returns>
    public static string GetRandomString(int length, bool useNum = true, bool useLow = true, bool useUpp = true, string custom = "")
    {
        byte[] b = new byte[4];
        new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
        System.Random r = new System.Random(BitConverter.ToInt32(b, 0));
        string s = null, str = custom;
        if (useNum == true) { str += "0123456789"; }
        if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
        if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
        for (int i = 0; i < length; i++)
        {
            s += str.Substring(r.Next(0, str.Length - 1), 1);
        }
        return s;
    }
}
