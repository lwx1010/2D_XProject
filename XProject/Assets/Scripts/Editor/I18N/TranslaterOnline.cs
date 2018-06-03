using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using AL.Crypto;
using UnityEngine;

namespace AL.Editor.I18N
{
    /// <summary>
    /// 在线翻译
    /// </summary>
    public class TranslaterOnline
    {
        private string translaterURL = "http://fanyi-api.baidu.com/api/trans/vip/translate";

        private string APP_ID = "20171018000089315";
        private string securityKey = "aeMGYbIWOBUeK6mMP6Zi";

        public string Translater(string world , string to)
        {
            var paramDic = buildParams(world, "zh",  to);
            string query = getUrlWithQuerystring(paramDic);
            string getResult = httpGet(translaterURL, query);

            if (string.IsNullOrEmpty(getResult)) return getResult;

            string result = string.Empty;

            Hashtable table = MiniJSON.Json.Deserialize(getResult) as Hashtable;
            if (table.Contains("error_code"))
            {
                Debug.LogError(world + ",request error:" + getResult);
                return result;
            }
            Hashtable trans_result = ((ArrayList)table["trans_result"])[0] as Hashtable;
            result = (string)trans_result["dst"];

            return result;
        }


        private Dictionary<string , string> buildParams(string query,  string from, string to)
        {
            Dictionary< string, string > paramDic = new Dictionary<string, string>();
            paramDic["q"] = query;
            paramDic["from"] = from;
            paramDic["to"] = to;

            paramDic["appid"] = APP_ID;

            // 随机数
            System.Random random = new System.Random((int)System.DateTime.Now.Ticks);
            int salt = random.Next();
            paramDic["salt"] = salt.ToString();

            // 签名
            string src = string.Concat(APP_ID , query, salt, securityKey); // 加密前的原文
            paramDic["sign"] = MD5.ComputeString(src).ToLower();

            return paramDic;
        }


        public static string getUrlWithQuerystring(Dictionary<string, string> paramDic)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (string key in paramDic.Keys)
            {
                string value = paramDic[key];
                if (value == null)
                { // 过滤空的key
                    continue;
                }

                if (i != 0)
                {
                    builder.Append('&');
                }

                builder.Append(key);
                builder.Append('=');
                builder.Append(CenterServerManager.UrlEncode(value));

                i++;
            }

            return builder.ToString();
        }

        private string httpGet(string url , string body)
        {
            string newUrl = string.Concat(url, "?", body);
//            Debug.Log(newUrl);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(newUrl));
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 5 * 1000;
            request.ReadWriteTimeout = 5 * 1000;

            string result = string.Empty;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        return result;
                    }
                    StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    result = sr.ReadToEnd().Trim();

                    sr.Close();
                    responseStream.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            return result;
        }
    }
}