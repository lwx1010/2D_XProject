using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Riverlake.Editor.I18N
{
    /// <summary>
    /// Prefab资源内UILabel文本提取及翻译
    /// </summary>
    public class PrefabTranslater : ITranslater
    {
        public TranslateMapper Export(string filePath)
        {
            Regex rx = new Regex("[\u4e00-\u9fa5]+");
            TranslateMapper trsMap = new TranslateMapper(filePath);

            string assetPath = filePath.Substring(filePath.IndexOf("Assets/"));

            GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
            GameObject instance = GameObject.Instantiate(prefab) as GameObject;
#if NGUI
            UILabel[] labels = instance.GetComponentsInChildren<UILabel>(true);
            for (int q = 0; q < labels.Length; q++)
            {
                string labText = labels[q].text.Trim();
                if (string.IsNullOrEmpty(labText)) continue;

                labText = labText.Replace("\n", @"\n").Replace("\r" , "");
                if (rx.IsMatch(labText) && !trsMap.ContainWorld(labText))
                    trsMap.AddWorld(labText);
            }

            // UIInput中的默认文本
            UIInput[] inputs = instance.GetComponentsInChildren<UIInput>(true);
            for (int q = 0; q < inputs.Length; q++)
            {
                string text = inputs[q].value.Trim();
                if (string.IsNullOrEmpty(text)) continue;

                text = text.Replace("\n", @"\n").Replace("\r", "");
                if (rx.IsMatch(text) && !trsMap.ContainWorld(text))
                      trsMap.AddWorld(text);
            }
#endif
            GameObject.DestroyImmediate(instance);

            return trsMap;
        }

        public void Translater(TranslateMapper transMap)
        {
            Regex rx = new Regex("[\u4e00-\u9fa5]+");

            GameObject prefab = AssetDatabase.LoadAssetAtPath(transMap.FilePath, typeof(GameObject)) as GameObject;
            GameObject instance = GameObject.Instantiate(prefab) as GameObject;
#if NGUI
            UILabel[] labels = instance.GetComponentsInChildren<UILabel>(true);
            for (int q = 0; q < labels.Length; q++)
            {
                string labText = labels[q].text.Trim();
                if (string.IsNullOrEmpty(labText)) continue;

                labText = labText.Replace("\n", @"\n").Replace("\r", "");
                if (rx.IsMatch(labText))    //翻译替换
                   labels[q].text = transMap.Translate(labText);
             }

            // UIInput中的默认文本
            UIInput[] inputs = instance.GetComponentsInChildren<UIInput>(true);
            for (int q = 0; q < inputs.Length; q++)
            {
                string text = inputs[q].value.Trim();
                if (string.IsNullOrEmpty(text)) continue;

                text = text.Replace("\n", @"\n").Replace("\r", "");
                if (rx.IsMatch(text))
                    inputs[q].value = transMap.Translate(text);
            }

            PrefabUtility.ReplacePrefab(instance, prefab);
#endif
            GameObject.DestroyImmediate(instance);
        }
    }
}