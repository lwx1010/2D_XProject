using UnityEngine;
using System.Collections;
using System.Text;
using System;

namespace AL
{
    public static class SensitiveWord
    {
        private static string[] _words = {"钓鱼岛", "习大大"};

        /// <summary>
        /// 替换敏感字
        /// </summary>
        /// <param name="src"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string filter(string src, string replace = "*")
        {
           try
			{
				int len = _words.Length;
				for( int i=0; i<len; ++i )
				{
                    src = src.Replace(_words[i], replace);
					//src = src.split(_words[i]).join(replacement);
				}
			}
			catch(Exception)
			{
				
			}
			
			return src;
        }

        /// <summary>
        /// 是否包含敏感字
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static bool isSensitive(string src)
        {
            int len = _words.Length;
            for (int i = 0; i < len; i++)
            {
                if( src.IndexOf(_words[i])>=0)
                    return true;
            }

            return false;
        }
    }
}
