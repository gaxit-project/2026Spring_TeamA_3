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

        private static AudioSource sharedAudioSource;
        private static int lastPlayedFrame = -1;
        private static AudioClip lastPlayedClip;

        private void Start()
        {
            // Buttonコンポーネントがある場合、クリックイベントに自動登録
            var button = GetComponent<Button>();
            if (button != null)
            {
                // インスペクター側で既に登録済みかチェック
                bool alreadyRegistered = false;
                int persistentCount = button.onClick.GetPersistentEventCount();
                for (int i = 0; i < persistentCount; i++)
                {
                    if (button.onClick.GetPersistentTarget(i) == this &&
                        button.onClick.GetPersistentMethodName(i) == nameof(PlaySE))
                    {
                        alreadyRegistered = true;
                        break;
                    }
                }

                if (!alreadyRegistered)
                {
                    button.onClick.AddListener(PlaySE);
                }
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

            // 同一フレーム内で同じSEが重複して呼ばれた場合は二重再生を防止
            if (Time.frameCount == lastPlayedFrame && lastPlayedClip == seClip)
            {
                return;
            }

            lastPlayedFrame = Time.frameCount;
            lastPlayedClip = seClip;

            // ボタンや親パネルが直後に非アクティブ化されても音が途切れないよう、常に常駐AudioSourceで再生
            var targetSource = GetSharedAudioSource();
            if (targetSource != null)
            {
                targetSource.PlayOneShot(seClip);
            }
        }
    }
}
