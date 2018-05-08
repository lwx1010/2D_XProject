using System;
using System.Collections.Generic;
using UnityEngine;

namespace BundleChecker.ResoucreAttribute
{
    /// <summary>
    /// 资源公共属性
    /// </summary>
    public sealed class ResourceGlobalProperty
    {
        public const string Name = "Name";
        public const string ResType = "ResType";
        public const string ABCount = "ABCount";
        public const string AssetBundles = "Bundles";
    }
    /// <summary>
    /// 资源基础
    /// </summary>
    public abstract class ABaseResource
    {
        private List<ResPropertyGUI> attributeGUI = new List<ResPropertyGUI>();

        protected ResoucresBean mainAsset;

        private static int column = 2;
        private static string[] toggleBtn = new[] {"ButtonMid", "ButtonMid" };

        private float fixTotalWidth;
        public ABaseResource(ResoucresBean res)
        {
            this.mainAsset = res;
        }

        public void AddProperty(string property, float guiWidth = 0)
        {
            ResPropertyGUI propertyGui = new ResPropertyGUI();
            propertyGui.PropertyName = property;
            propertyGui.GuiWidth = guiWidth;

            attributeGUI.Add(propertyGui);
        }


        public void SetPropertyGUI(ResPropertyGUI[] propertys)
        {
            this.attributeGUI.Clear();
            attributeGUI.AddRange(propertys);

            fixTotalWidth = 0;
            foreach (ResPropertyGUI rpg in propertys)
            {
                if(rpg.GuiWidth <= 0)   continue;
                fixTotalWidth += rpg.GuiWidth;
            }
        }


        public void OnGUI()
        {
            foreach (ResPropertyGUI rpgui in attributeGUI)
            {
                string[] propertyValueArr = this.getPropertyValue(rpgui.PropertyName);
                if (propertyValueArr == null || propertyValueArr.Length <= 0)    continue;
                float realWidth = getRealWidth(rpgui.GuiWidth);
                if(propertyValueArr.Length > 1)
                    drawPropertyArr(rpgui.PropertyName , propertyValueArr, realWidth);
                else
                    drawPropertyString(propertyValueArr[0] , realWidth);
            }
        }

        private float getRealWidth(float guiWidth)
        {
            if(guiWidth >= 0)   return guiWidth;

            float totalWidth = ABMainChecker.MainChecker.Width;
            return (totalWidth - fixTotalWidth)*Mathf.Abs(guiWidth);
        }

        protected void drawPropertyString(string propertyValue , float width)
        {
            if (width > 0)
            {
               GUILayout.Label(propertyValue, GUILayout.MinWidth(width));
            }
            else
            {
                GUILayout.Label(propertyValue);
            }
        }


        private void drawPropertyArr(string property , string[] propertyArr, float width)
        {
            GUILayout.BeginVertical();

            int endIndex = 0;

            for (int i = 0, maxCount = propertyArr.Length; i < maxCount; i++)
            {
                if (i % column == 0)
                {
                    endIndex = i + column - 1;
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                }

                GUILayout.Toggle(false , propertyArr[i], toggleBtn[i % column], GUILayout.Width(width*0.5f));
                
                if (i == endIndex)
                {
                    endIndex = 0;
                    GUILayout.Space(10);
                    GUILayout.EndHorizontal();
                }
            }
            if (endIndex != 0) GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        
        protected virtual string[] getPropertyValue(string property)
        {
            if (property == ResourceGlobalProperty.Name) return new []{this.mainAsset.Name };
            if (property == ResourceGlobalProperty.ResType) return new[] { this.mainAsset.ResourceType};
            if (property == ResourceGlobalProperty.ABCount) return new[] {Convert.ToString(mainAsset.IncludeBundles.Count)};

            if (property == ResourceGlobalProperty.AssetBundles)
            {
                List<string> bundls = new List<string>();
                for (int i = 0, maxCount = mainAsset.IncludeBundles.Count; i < maxCount; i++)
                {
                    EditorBundleBean depBundle = mainAsset.IncludeBundles[i];
                    bundls.Add(depBundle.BundleName);
                }
                return bundls.ToArray();
            }

            return null;
        }
    }
}