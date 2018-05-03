using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using Config;
using RSG;
using Riverlake.Resources;

namespace LuaFramework {
    public sealed class SoundManager : Manager {
        private LFUCache<string, AudioData> sounds = new LFUCache<string, AudioData>(20);
        /// <summary>
        /// 游戏bgm，一个场景只有一个bgm
        /// </summary>
        private AudioData bgm = null;

        private bool _closeSound = false;

        public enum SoundType
        {
            None    = 0,
            Evenything = -1 ,
            BGM       = 1 << 0,
            SKILL     = 1 << 1,
            TALK      = 1 << 2,
            UI        = 1 << 3,
            EFFECT    = 1 << 4,
            Story     = 1 << 5,
        }

        private int soundLayer = (int)SoundType.Evenything;
        /// <summary>
        /// 音频的互斥容器
        /// </summary>
        private Dictionary<string , AudioCompatible> audioCompDic = new Dictionary<string, AudioCompatible>();

        GameObject SoundParent
        {
            get
            {
                if (_soundParent == null)
                {
                    _soundParent = new GameObject("_SoundParent");
                    GameObject.DontDestroyOnLoad(_soundParent);
                    _soundParent.transform.SetAsFirstSibling();
                }
                    
                _soundParent.transform.localPosition = Vector3.zero;
                _soundParent.transform.localScale = Vector3.one;
                return _soundParent;
            }
        }

        GameObject _soundParent = null;

        private const float SoundScaleValue = 0.6f;

        public int SoundLayer
        {
            get { return soundLayer; }
        }

        class AudioData
        {
            public GameObject go { get; private set; }
            public AudioSource audio { get; private set; }
            public SoundType type { get; private set; }
            public string path { get; private set; }
            private float minPitch { get; set; }
            private float maxPitch { get; set; }
            private float defaultVol { get; set; }

            public AudioData(string path, SoundType type, bool loop, Transform parent, Vector3 position, int priority, float minPitch, float maxPitch, float volume, float maxDistance = 25, float spatialBlend = 0)
            {
                try
                {
                    go = new GameObject(string.Format("Audio_{0}_{1}", type.ToString(), path));
                    go.transform.SetParent(parent);
                    go.transform.position = position;
                    this.path = path;
                    this.type = type;
                    audio = go.AddComponent<AudioSource>();
                    audio.playOnAwake = false;
                    audio.bypassEffects = true;
                    audio.loop = loop;
                    audio.priority = priority;
                    audio.rolloffMode = AudioRolloffMode.Linear;
                    audio.dopplerLevel = 0;
                    audio.maxDistance = maxDistance;
                    audio.volume = volume;
                    audio.spatialBlend = spatialBlend;
                    this.defaultVol = volume;
                    this.minPitch = minPitch;
                    this.maxPitch = maxPitch;
                }
                catch (Exception e)
                {
                    Debug.LogError("Create audio data error");
                    Debug.LogException(e);
                }
            }

            public void Play(float delay)
            {
                if (audio == null) return;

                try
                {
                    if (type == SoundType.SKILL)
                    {
                        float pitch = UnityEngine.Random.Range(minPitch, maxPitch);
                        audio.pitch = pitch;
                    }
                    audio.PlayDelayed(delay);
                }
                catch (Exception e)
                {
                    Debug.LogError("Play audio data error");
                    Debug.LogException(e);
                }
            }

            public void Stop()
            {
                if (audio == null) return;
                audio.Stop();
            }

            public void ChangeVolume(float volume)
            {
                if (audio == null) return;
                if (type == SoundType.SKILL)
                {
                    volume = defaultVol * volume;
                }
                audio.volume = volume;
            }

            public void Destory()
            {
                if (audio != null) audio.clip = null;
                if (go != null)
                    GameObject.Destroy(go);
            }
        }
        /// <summary>
        /// 音频的兼容数据
        /// </summary>
        private class AudioCompatible
        {
            /// <summary>
            /// 主音频名称
            /// </summary>
            public string AudioName;
            /// <summary>
            /// 权重值,在同时播放时,优先
            /// </summary>
            public int Weight;
            /// <summary>
            /// 互斥列表
            /// </summary>
            public HashSet<string> Mutexs;
        }

        void Start() {
            sounds.DestroyAction = delegate(Node<string, AudioData> node)
            {
                node.Value.Stop();
                node.Value.Destory();
                node.Value = null;
            };
        }

        /// <summary>
        /// 添加音频的互斥信息
        /// </summary>
        /// <param name="audioName">音频名称</param>
        /// <param name="weight">权重</param>
        /// <param name="mutexs">互斥集合</param>
        public void AddAudioCompatible(string audioName, int weight, string[] mutexs)
        {
            if (audioCompDic.ContainsKey(audioName)) return;

            AudioCompatible audioComp = new AudioCompatible();
            audioComp.AudioName = audioName;
            audioComp.Weight = weight;
            audioComp.Mutexs = new HashSet<string>(mutexs);

            audioCompDic[audioName] = audioComp;
        }

        private string GetAudioClipPath(string name, int type)
        {
            string prefix;
            switch ((SoundType)type)
            {
                case SoundType.BGM:
                    prefix = "bgm";
                    break;
                case SoundType.SKILL:
                    prefix = "skill";
                    break;
                case SoundType.TALK:
                    prefix = "talk";
                    break;
                case SoundType.UI:
                    prefix = "ui";
                    break;
                case SoundType.EFFECT:
                    prefix = "effect";
                    break;
                case SoundType.Story:
                    prefix = "story";
                    break;
                default:
                    prefix = string.Empty;
                    break;
            }
            return string.Format("Sound/{0}/{1}", prefix, name);
        }

        /// <summary>
        /// 加载音频
        /// </summary>
        private void LoadAudioClip(string name, int type, float volume, Vector3 position, float delay, float time = 0,
                                  string extension = "ogg", int priority = 128, bool loop = false, float minPitch = 0, float maxPitch = 3, float maxDistance = 25, float spatialBlend = 0)
        {
            var path = string.Concat(GetAudioClipPath(name, type) , "." ,extension);
            LoadAudioClipPath(path , type , volume , position , delay , time ,priority ,  loop , minPitch , maxPitch, maxDistance, spatialBlend);
        }

        private void LoadAudioClipPath(string path, int type, float volume, Vector3 position, float delay, float time = 0,
                                       int priority = 128, bool loop = false, float minPitch = 0, float maxPitch = 3, float maxDistance = 25, float spatialBlend = 0)
        {
            if (string.IsNullOrEmpty(path)) return;

            if (sounds[path] != null)
            {
                var audioData = sounds[path] as AudioData;
                if (audioData != null && audioData.audio != null)
                {
                    audioData.audio.volume = _closeSound ? 0f : volume;
                    audioData.go.transform.position = position;
                    audioData.Play(delay);
                }
                else
                {
                    sounds.RemoveKey(path);
                }
            }
            else
            {
                var data = new AudioData(path, (SoundType)type, loop, SoundParent.transform, position, priority, minPitch, maxPitch, volume, maxDistance, spatialBlend);
                
                string extension = Path.GetExtension(path);
                string fileName = path.Replace(extension , "");
                extension = extension.Substring(1);

                ResourceManager.LoadAudioClipAsync(fileName, extension)
                      .Then((audioClip) =>
                      {
                          if (audioClip == null) throw new Exception(string.Format("Cant Load AudioClip! Path :{0}", path));

                          if (data.audio != null)
                          {
                              data.audio.clip = audioClip;
                              data.audio.volume = _closeSound ? 0f : volume;
                              data.audio.time = time;
                              data.Play(delay);
                              sounds.Put(path, data);
                          }
                      })
                      .Catch(e => Debug.LogException(e));
            }
        }

        private IPromise<AudioData> loadAudioClipPath(string path, int type, float volume)
        {
            if (string.IsNullOrEmpty(path)) return null;

            if (sounds[path] != null)
            {
                AudioData audioData = sounds[path];
                if (audioData != null && audioData.audio != null)
                {
                    audioData.audio.volume = _closeSound ? 0f : volume;
                }
                else
                {
                    sounds.RemoveKey(path);
                }
                return Promise<AudioData>.Resolved(audioData);
            }
            
            int priority = 128;
            bool loop = false;
            float minPitch = 0;
            float maxPitch = 3;
            float maxDistance = 25;
            float spatialBlend = 0;
            AudioData data = new AudioData(path, (SoundType)type, loop, SoundParent.transform, Vector3.zero, 
                                           priority, minPitch, maxPitch, volume, maxDistance, spatialBlend);

            string extension = Path.GetExtension(path);
            string fileName = path.Replace(extension, "");
            extension = extension.Substring(1);

            IPromise<AudioData> adPromise = new Promise<AudioData>((s, j) =>
            {
                ResourceManager.LoadAudioClipAsync(fileName, extension)
                        .Then((audioClip) =>
                        {
                            if (audioClip == null) throw new Exception(string.Format("Cant Load AudioClip! Path :{0}", path));

                            if (data.audio != null)
                            {
                                data.audio.clip = audioClip;
                                data.audio.volume = _closeSound ? 0f : volume;
                                sounds.Put(path, data);
                            }
                            s.Invoke(data);
                        })
                        .Catch(e =>
                        {
                            j.Invoke(e);
                            Debug.LogException(e);
                        });
            });
            return adPromise;
        }
        /// <summary>
        /// 是否允许播放背景音乐
        /// </summary>
        /// <returns></returns>
        public bool CanPlayBackSound()
        {
            return User_Config.isMusic == 1 && User_Config.volumn >= 0.1f;
        }

        /// <summary>
        /// 播放一个背景音乐
        /// </summary>
        /// <param name="canPlay"></param>
        public void PlayBGM(string name, float volume)
        {
            if (!CanPlayBackSound()) return;
            var path = GetAudioClipPath(name, (int)SoundType.BGM);
            if (bgm != null && bgm.path != path)
            {
                bgm.Destory();
                bgm = null;
            }
            if (bgm == null)
            {
                bgm = new AudioData(path, SoundType.BGM, true, SoundParent.transform, Vector3.zero, 128, 1, 1, (volume > 1f ? 1f : volume));
                ResourceManager.LoadAudioClipAsync(path, "ogg")
                      .Then((audioClip) =>
                      {
                          if (audioClip == null) throw new Exception(string.Format("Cant Load AudioClip! Path :{0}", path));

                          if (bgm != null)
                          {
                              // bgm != null 为防止在异步加载时，bgm对象被外部释放
                              bgm.audio.clip = audioClip;
                              bgm.audio.volume = volume;
                              bgm.Play(0);
                          }
                      }).Catch(e => Debug.LogException(e));
            }
            else
            {
                bgm.audio.volume = volume;
                if (!bgm.audio.isPlaying) bgm.Play(0);
            }
        }

        /// <summary>
        /// 是否能播放音效
        /// </summary>
        /// <returns></returns>
        public bool CanPlaySoundEffect(SoundType soundType)
        {
            if (!IsActiveLayer(soundType)) return false;

            return User_Config.isAudio == 1 && User_Config.volumn >= 0.1f;
        }

        /// <summary>
        /// 播放一个技能音频剪辑
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="position"></param>
        public void PlaySkillSound(string name, float volume, float delay, Vector3 position, int priority, bool loop, float minPitch, float maxPitch, string extension)
        {
            if (!CanPlaySoundEffect(SoundType.SKILL)) return;
            LoadAudioClip(name, (int)SoundType.SKILL, volume * User_Config.volumn * SoundScaleValue, position, delay, 0, extension, priority, loop, minPitch, maxPitch);
        }
        /// <summary>
        /// 播放一段音频
        /// </summary>
        /// <param name="path">音频路径</param>
        /// <param name="delay">延迟播放时间</param>
        /// <param name="loop">是否循环</param>
        public void PlaySound(string path, float delay, bool loop)
        {
            PlaySound(path , delay , 0 , loop);
        }

        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="path">音频路径</param>
        /// <param name="delay">延迟播放时间</param>
        /// <param name="loop">是否循环</param>
        /// <param name="time">开始时间点</param>
        public void PlaySound(string path, float delay = 0, float time = 0, bool loop = false)
        {
            if (!CanPlaySoundEffect(SoundType.EFFECT)) return;

            LoadAudioClipPath(path, (int)SoundType.EFFECT, User_Config.volumn * SoundScaleValue, Vector3.zero, delay, time, 128, loop, 0, 3);
        }

        public void PlaySound(string path, SoundType soundType, float delay = 0, float time = 0, bool loop = false)
        {
            if (!CanPlaySoundEffect(soundType)) return;

            LoadAudioClipPath(path, (int)soundType, User_Config.volumn * SoundScaleValue, Vector3.zero, delay, time, 128, loop, 0, 3);
        }

        /// <summary>
        /// 播放一个效果音频剪辑(冲刺，打坐，移动)
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="position"></param>
        public void PlayEffectSound(string name, Vector3 position, bool loop)
        {
            if (!CanPlaySoundEffect(SoundType.EFFECT)) return;
            object[] rets = Util.CallMethod("Game", "GetSoundEffectVol", name);
            float volume = 1f;
            if (rets != null && rets.Length == 1)
                volume = Convert.ToSingle(rets[0]);

            LoadAudioClip(name, (int)SoundType.EFFECT, volume * User_Config.volumn, position, 0, 0, "ogg", 128, loop);
        }

        /// <summary>
        /// 播放一个技能音频剪辑
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="position"></param>
        public void PlayTalkSound(string name, float volume, float delay, Vector3 position)
        {
            if (!CanPlaySoundEffect(SoundType.TALK)) return;
            if (IsPlaying(name, (int)SoundType.TALK)) return;
            LoadAudioClip(name, (int)SoundType.TALK, volume * User_Config.volumn, position, delay, 0, "ogg", 123, false, 0, 3, 15, 1);
        }

        public void PlayButtonDownSound()
        {
            PlayUISound("button-down");
        }

        /// <summary>
        /// 播放一个UI音频剪辑
        /// </summary>
        /// <param name="audioPath">相对路径</param>
        public void PlayUISound(string audioPath)
        {
            if (!CanPlaySoundEffect(SoundType.UI)) return;
            object[] rets = Util.CallMethod("Game", "GetSoundEffectVol", audioPath);
            float volume = 1f;
            if (rets != null && rets.Length == 1)
                volume = Convert.ToSingle(rets[0]);
            
            string path = string.Concat(GetAudioClipPath(audioPath, (int)SoundType.UI), ".ogg");
            this.loadAudioClipPath(path, (int) SoundType.UI, volume*User_Config.volumn)
                .Then((audioData) =>
                {
                    string audioName = Path.GetFileNameWithoutExtension(path);
                    AudioCompatible audioComp = null;
                    if (audioCompDic.TryGetValue(audioName , out audioComp))
                    {
                        int uiSoundType = (int) SoundType.UI;
                        foreach (string mutexAudio in audioComp.Mutexs)
                        {
                            if(!IsPlaying(mutexAudio , uiSoundType))    continue;

                            //互斥对象正在播放时,检查权重
                            AudioCompatible mutexComp = null;
                            if (!audioCompDic.TryGetValue(mutexAudio, out mutexComp))
                                return;

                            if (mutexComp.Weight < audioComp.Weight)
                            {
                                StopSound(mutexComp.AudioName , uiSoundType);
                            }
                        }
                    }
                   
                    if(!audioData.audio.isPlaying)
                        audioData.audio.Play();
                });
        }

        public void ChangeVolume(float volume)
        {
            var ie = sounds.GetEnumerator();
            while (ie.MoveNext())
            {
                var sound = (KeyValuePair<string, LFUNode<string, AudioData>>)ie.Current;
                sound.Value.Value.ChangeVolume(volume);
            }
            if (bgm != null) bgm.ChangeVolume(volume);
        }

        public void CloseSound(bool close)
        {
            _closeSound = close;
            ChangeVolume(close ? 0 : User_Config.volumn);
        }

        public void StopBGM()
        {
            if (bgm != null)
            {
                bgm.Stop();
                bgm.Destory();
                bgm = null;
            }
        }


        public void PauseMusic()
        {
            if (bgm != null) bgm.audio.Pause();
        }

        public void ReplayMusic()
        {
            if (bgm != null) bgm.audio.Play();
        }

        public void StopEffectSound(string name, int type)
        {
            if (name != null && name.IndexOf('.') < 0)
                name = string.Format("{0}.ogg", name);

            var key = GetAudioClipPath(name, type);
            if (sounds.ContainsKey(key))
            {
                var sound = sounds[key] as AudioData;
                sound.Stop();
            }
        }
        /// <summary>
        /// 停止指定类型的音频
        /// </summary>
        /// <param name="soundType">音频类型</param>
        public void StopSound(SoundType soundType)
        {
            var ie = sounds.GetEnumerator();
            while (ie.MoveNext())
            {
                var sound = (KeyValuePair<string, LFUNode<string, AudioData>>)ie.Current;
                AudioData soundData = sound.Value.Value;
                if(soundData.type == soundType && soundData.audio.isPlaying)
                     soundData.audio.Stop();
            }
        }

        public void StopSound(string audioName , int type)
        {
            string path = string.Concat(GetAudioClipPath(name, type), ".ogg");
            this.StopSound(path);
        }

        public void StopSound(string audioPath)
        {
            if (!sounds.ContainsKey(audioPath)) return;

            if (sounds[audioPath].audio && sounds[audioPath].audio.isPlaying)
                sounds[audioPath].audio.Stop();
        }

        public void StopEffectSound()
        {
            sounds.Destroy();
        }

        public bool IsPlaying(string name, int type, string extension = "ogg")
        {
            var path = string.Concat(GetAudioClipPath(name, type), ".", extension);
            if (sounds.ContainsKey(path) && sounds[path].audio && sounds[path].audio.isPlaying)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否已激活指定类型的音频
        /// </summary>
        /// <param name="type">音频类型</param>
        /// <returns>如果激活返回true</returns>
        public bool IsActiveLayer(SoundType type)
        {
            return (soundLayer & (int)type) > 0;
        }

        /// <summary>
        /// 设置指定类型的音频是否激活
        /// </summary>
        /// <param name="type">音频类型</param>
        /// <param name="active">true表示激活</param>
        public void SetActiveLayer(SoundType type, bool active)
        {
            SetActiveLayer((int)type , active);
        }


        public void SetActiveLayer(int layer, bool active)
        {
            if (layer <= 0)
            {
                soundLayer = layer;
                return;
            }

            if (active)
            {
                soundLayer |= layer;
            }
            else
            {
                soundLayer &= ~layer;
            }
        }

        public void Clear()
        {
            StopBGM();
            StopEffectSound();
//            Destroy(_soundParent);
//            _soundParent = null;
        }
    }
}