// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Manages the spawning and playing of sounds.
    /// </summary>
    public class AudioManagerService : MonoBehaviour, IAudioManagerService
    {
        [Header("Audio Mixer Settings")]
        [Tooltip("SE用のオーディオミキサーグループを設定します。")]
        [SerializeField]
        private AudioMixerGroup outputMixerGroup;

        /// <summary>
        /// Contains data related to playing a OneShot audio.
        /// </summary>
        private readonly struct OneShotCoroutine
        {
            /// <summary>
            /// Audio Clip.
            /// </summary>
            public AudioClip Clip { get; }
            /// <summary>
            /// Audio Settings.
            /// </summary>
            public AudioSettings Settings { get; }
            /// <summary>
            /// Delay.
            /// </summary>
            public float Delay { get; }
            
            /// <summary>
            /// Constructor.
            /// </summary>
            public OneShotCoroutine(AudioClip clip, AudioSettings settings, float delay)
            {
                //Clip.
                Clip = clip;
                //Settings
                Settings = settings;
                //Delay.
                Delay = delay;
            }
        }

        /// <summary>
        /// 再生が終了したオーディオソースを破棄します。
        /// </summary>
        private IEnumerator DestroySourceWhenFinished(AudioSource source)
        {
            // オーディオソースが再生を完了するまで待機します（シーン遷移等ですでに破棄された場合は待機を終了）
            yield return new WaitWhile(() => source != null && source.isPlaying);
            
            // オーディオオブジェクトがまだ残っていれば破棄します
            if (source != null && source.gameObject != null)
            {
                DestroyImmediate(source.gameObject);
            }
        }

        /// <summary>
        /// Waits for a certain amount of time before starting to play a one shot sound.
        /// </summary>
        private IEnumerator PlayOneShotAfterDelay(OneShotCoroutine value)
        {
            //Wait for the delay.
            yield return new WaitForSeconds(value.Delay);
            //Play.
            PlayOneShot_Internal(value.Clip, value.Settings);
        }
        
        /// <summary>
        /// Internal PlayOneShot. Basically does the whole function's name!
        /// </summary>
        private void PlayOneShot_Internal(AudioClip clip, AudioSettings settings)
        {
            //No need to do absolutely anything if the clip is null.
            if (clip == null)
                return;
            
            //Spawn a game object for the audio source.
            var newSourceObject = new GameObject($"Audio Source -> {clip.name}");
            //Add an audio source component to that object.
            var newAudioSource = newSourceObject.AddComponent<AudioSource>();

            //Set mixer group.
            if (outputMixerGroup != null)
            {
                newAudioSource.outputAudioMixerGroup = outputMixerGroup;
            }

            //Set volume.
            newAudioSource.volume = settings.Volume;
            //Set spatial blend.
            newAudioSource.spatialBlend = settings.SpatialBlend;
            
            //Play the clip!
            newAudioSource.PlayOneShot(clip);
            
            //Start a coroutine that will destroy the whole object once it is done!
            if(settings.AutomaticCleanup)
                StartCoroutine(nameof(DestroySourceWhenFinished), newAudioSource);
        }

        public void SetOutputMixerGroup(AudioMixerGroup mixerGroup)
        {
            outputMixerGroup = mixerGroup;
        }

        #region Audio Manager Service Interface

        public void PlayOneShot(AudioClip clip, AudioSettings settings = default)
        {
            //Play.
            PlayOneShot_Internal(clip, settings);
        }

        public void PlayOneShotDelayed(AudioClip clip, AudioSettings settings = default, float delay = 1.0f)
        {
            //Play.
            StartCoroutine(nameof(PlayOneShotAfterDelay), new OneShotCoroutine(clip, settings, delay));
        }

        #endregion
    }
}