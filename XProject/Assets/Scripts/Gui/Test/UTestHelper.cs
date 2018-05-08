using UnityEngine;

namespace CinemaDirector
{
    public class UTestHelper
    {

        public static T FindWidget<T>(UIPanel panel , Vector3 pos) where T : Component
        {
            pos = new Vector3(pos.x , pos.y , panel.transform.position.z); //保持深度一致
            
            foreach (UIWidget w in panel.widgets)
            {
                Bounds bounds = CalculateAbsoluteWidgetBounds(w);
                if (bounds.Contains(pos))
                {
                    T comp = w as T;
                    if (comp != null) return comp;
                }
            }
            return default(T);
        }


        public static Bounds CalculateAbsoluteWidgetBounds(UIWidget widget)
        {
            Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 v;

            if (!widget.enabled) return new Bounds(Vector3.zero , Vector3.zero);

            Vector3[] corners = widget.worldCorners;

            for (int j = 0; j < 4; ++j)
            {
                v = corners[j];

                if (v.x > vMax.x) vMax.x = v.x;
                if (v.y > vMax.y) vMax.y = v.y;
                if (v.z > vMax.z) vMax.z = v.z;

                if (v.x < vMin.x) vMin.x = v.x;
                if (v.y < vMin.y) vMin.y = v.y;
                if (v.z < vMin.z) vMin.z = v.z;
            }

            Bounds bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);

            return bounds;
        }
    }
}