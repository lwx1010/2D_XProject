using System.IO;
using System.Text.RegularExpressions;

namespace Riverlake.Editor.I18N
{
    /// <summary>
    /// Lua代码中文字符翻译
    /// </summary>
    public class LuaTranslater : ITranslater
    {

        public static string[] Filters = new[] { "--", "print", 
                                            "Debug.Log", "UnityEngine.Debug.Log" ,
                                            "Debugger.Log"  };

        private static string[] filterFolders = new[] {"xlsdata"};

        public TranslateMapper Export(string filePath)
        {
            TranslateMapper trsMap = new TranslateMapper(filePath);

            if (isFilterFolder(filePath))
                return trsMap;

            Regex rx = new Regex("[\u4e00-\u9fa5]+");
            string assetPath = filePath.Substring(filePath.IndexOf("Assets/"));
            string[] lines = File.ReadAllLines(assetPath);
            //用正则表达式把代码里面两种字符串中间的字符串提取出来。
            Regex reg = new Regex("\"[^\"]*\"");
            Regex reg2 = new Regex("'[^\']*\'"); //Lua单引号字符
            for (int i = 0; i < lines.Length; i++)
            {
                if (isFilter(lines[i])) continue;

                MatchCollection mc = reg.Matches(lines[i]);
                if (mc.Count == 0)
                {
                    mc = reg2.Matches(lines[i]);
                }
                foreach (Match m in mc)
                {
                    if (rx.IsMatch(m.Value))
                    {
                        string format = m.Value.Substring(1 , m.Value.Length - 2);
                        if (!trsMap.ContainWorld(format))
                        {
                            trsMap.AddWorld(format);
                        }
                    }
                }
            }

            return trsMap;
        }


        protected bool isFilter(string file)
        {
            string format = file.Trim();
            for (int i = 0; i < Filters.Length; i++)
            {
                if (format.StartsWith(Filters[i]))
                    return true;
            }
            return false;
        }

        private bool isFilterFolder(string filePath)
        {
            for (int i = 0; i < filterFolders.Length; i++)
            {
                if (filePath.Contains(filterFolders[i]))
                    return true;
            }
            return false;
        }

        public void Translater(TranslateMapper transMap)
        {
            if (isFilterFolder(transMap.FilePath))  return ;

            Regex rx = new Regex("[\u4e00-\u9fa5]+");
            string[] lines = File.ReadAllLines(transMap.FilePath);

            Regex reg = new Regex("\"[^\"]*\"");
            Regex reg2 = new Regex("'[^\']*\'");
            for (int i = 0; i < lines.Length; i++)
            {
                if (isFilter(lines[i])) continue;

                MatchCollection mc = reg.Matches(lines[i]);
                if (mc.Count == 0)
                    mc = reg2.Matches(lines[i]);
                
                foreach (Match m in mc)
                {
                    if (rx.IsMatch(m.Value))
                    {
                        //翻译替换
                        string format = m.Value.Substring(1, m.Value.Length - 2);
                        lines[i] = lines[i].Replace(format, transMap.Translate(format));
                    }
                }
            }

            //保存文件
            File.WriteAllLines(transMap.FilePath, lines);
        }
    }
}