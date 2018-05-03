using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Riverlake.Editor.I18N
{
    /// <summary>
    /// 翻译词映射
    /// </summary>
    public class TranslateMapper
    {
        private string filePath;

        private List<TranslatorWorld> worlds;

        public string FilePath
        {
            get { return filePath; }
        }
        /// <summary>
        /// 需要被翻译的字符
        /// </summary>
        public List<TranslatorWorld> Worlds
        {
            get { return worlds; }
        }

        public TranslateMapper(string filePath)
        {
            this.filePath = filePath;
            this.worlds = new List<TranslatorWorld>();
        }

        /// <summary>
        /// 添加文件中的原始字符
        /// </summary>
        /// <param name="world"></param>
        public void AddWorld(string world)
        {
            if(ContainWorld(world)) return;

            TranslatorWorld tw = new TranslatorWorld();
            tw.Source = world;

            worlds.Add(tw);
        }
        /// <summary>
        /// 是否包含相同的文字
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public bool ContainWorld(string world)
        {
            if (string.IsNullOrEmpty(world)) return true;

            for (int i = 0 , max = worlds.Count; i < max; i++)
            {
                if (worlds[i].Source.Equals(world))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 删除文字
        /// </summary>
        /// <param name="world"></param>
        public void RemoveWorld(string world)
        {
            if (string.IsNullOrEmpty(world)) return ;

            for (int i = 0, max = worlds.Count; i < max; i++)
            {
                if (worlds[i].Source.Equals(world))
                {
                    worlds.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// 设置翻译词汇映射
        /// </summary>
        /// <param name="srcWorld">原单词</param>
        /// <param name="translateWorld">翻译后的单词</param>
        public void SetTranslate(string srcWorld, string translateWorld)
        {
            for (int i = 0, max = worlds.Count; i < max; i++)
            {
                if (worlds[i].Source.Equals(srcWorld))
                {
                    worlds[i].Dest = translateWorld;
                    return;
                }
            }

            TranslatorWorld tWorld = new TranslatorWorld();
            tWorld.Source = srcWorld;
            tWorld.Dest = translateWorld;
            worlds.Add(tWorld);
        }

        /// <summary>
        /// 翻译转换
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public string Translate(string world)
        {
            for (int i = 0, max = worlds.Count; i < max; i++)
            {
                if (!string.IsNullOrEmpty(worlds[i].Dest) && worlds[i].Source.Equals(world))
                    return worlds[i].Dest;
            }
            Debug.LogError("无法翻译:" + world);
            return world;
        }
    }


    public class TranslatorWorld
    {
        /// <summary>
        /// 原文字
        /// </summary>
        public string Source;
        /// <summary>
        /// 翻译文字
        /// </summary>
        public string Dest;
    }
}