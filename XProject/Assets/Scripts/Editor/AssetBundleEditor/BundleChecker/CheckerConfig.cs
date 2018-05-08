using System.Collections.Generic;

namespace BundleChecker
{
    public class CheckerConfig
    {
        public const string AssetBundleSuffix = ".ab";
        //排除文件
        public static HashSet<string> ExcludeFiles = new HashSet<string>(new[] { ".cs", ".dll", ".asset", ".ttf" });
        //排除目录
        public static string[] ExcloudFolder = new[]
        {
            "Assets/T4MOBJ" , "Assets/T4M" ,"Assets/Effect/Changjingtexiao" , 
            "Assets/Models/Terrian" , "Assets/Shader" ,"Assets/Perfabs/Terrian_desgin" ,
        };

        public static bool IsExcludeFolder(string path)
        {
            for (int i = 0 , max = ExcloudFolder.Length; i < max; i++)
            {
                if (path.StartsWith(ExcloudFolder[i])) return true;
            }
            return false;
        }
    }
}