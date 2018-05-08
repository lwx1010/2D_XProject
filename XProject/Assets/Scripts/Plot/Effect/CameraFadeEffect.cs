using System;
using UnityEngine;

namespace Riverlake.Plot
{
    [RequireComponent(typeof(Camera))]
    public class CameraFadeEffect : MonoBehaviour
    {
        
        private float opacity = 1;

        public bool PlayAwake = false;

        [Range(0f, 1f)]
        public float StartOpacity = 1;
        [Range(0f, 1f)]
        public float EndOpacity = 1;

        public Color color = Color.black;

        public float duration = 0;

        public bool AutoDesroy = true;

        [HideInInspector]
        public Action OnFinish;

        private Material material;
        private float startTime = 0;

        
        private bool isFading = false;
        

        private void Awake()
        {
            material = new Material(Shader.Find("Hidden/CameraFadeShader"));
            material.color = color;
        }

        public void Fade(float start , float end = 1, float duration = 1)
        {
            this.duration = duration;
            this.startTime = Time.time;
            this.StartOpacity = start;
            this.EndOpacity = end;
            this.isFading = true;
        }


        private void Start()
        {
            if (PlayAwake)
            {
                this.startTime = Time.time;
                isFading = true;
            }
        }


        private void Update()
        {
            if (isFading)
            {
                opacity = Mathf.Lerp(StartOpacity, EndOpacity, (Time.time - startTime) / duration);
                isFading = opacity != EndOpacity;

                if (!isFading )
                {
                    this.onFadeFinish();
                }
            }
        }

        private void onFadeFinish()
        {
            if (OnFinish != null)
            {
                OnFinish.Invoke();
                OnFinish = null;
            }

            if (AutoDesroy)
            {
                GameObject.Destroy(this);
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!isFading)
            {
                Graphics.Blit(source, destination);
                return;
            }
            material.SetFloat("_Opacity", 1 - opacity);
            Graphics.Blit(source, destination, material);
        }
    }
}