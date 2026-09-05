using UnityEngine;
using UnityEngine.UI;

namespace DeadLab
{
    /// <summary>
    /// UIボタンのSE再生用クラス
    /// </summary>
    public class ButtonSE : MonoBehaviour
    {
        [SerializeField] public AudioClip seClip;

        private AudioSource localAudioSource;
        private static AudioSource sharedAudioSource;

        private void Awake()
        {
            localAudioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            // Buttonコンポーネントがある場合、クリックイベントに自動登録
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(PlaySE);
            }
        }

        private static AudioSource GetSharedAudioSource()
        {
            if (sharedAudioSource == null)
            {
                var go = new GameObject("[ButtonSE_SharedAudioSource]");
                DontDestroyOnLoad(go);
                sharedAudioSource = go.AddComponent<AudioSource>();
                sharedAudioSource.playOnAwake = false;
            }

            if (sharedAudioSource.outputAudioMixerGroup == null && SettingsManager.SeMixerGroupInstance != null)
            {
                sharedAudioSource.outputAudioMixerGroup = SettingsManager.SeMixerGroupInstance;
            }

            return sharedAudioSource;
        }

        /// <summary>
        /// ボタンSE再生処理
        /// </summary>
        public void PlaySE()
        {
            if (seClip == null) return;

            // 自身または親が非アクティブ化された場合は、常駐の sharedAudioSource で再生
            AudioSource targetSource = null;
            if (localAudioSource != null && localAudioSource.isActiveAndEnabled)
            {
                if (localAudioSource.outputAudioMixerGroup == null && SettingsManager.SeMixerGroupInstance != null)
                {
                    localAudioSource.outputAudioMixerGroup = SettingsManager.SeMixerGroupInstance;
                }
                targetSource = localAudioSource;
            }
            else
            {
                targetSource = GetSharedAudioSource();
            }

            if (targetSource != null)
            {
                targetSource.PlayOneShot(seClip);
            }
        }
    }
}
