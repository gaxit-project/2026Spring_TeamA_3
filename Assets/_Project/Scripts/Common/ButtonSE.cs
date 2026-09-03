using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// UIボタンのSE再生用クラス
    /// </summary>
    public class ButtonSE : MonoBehaviour
    {
        private AudioSource audioSource;
        [SerializeField] public AudioClip seClip;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// ボタンSE再生処理
        /// </summary>
        public void PlaySE()
        {
            if (seClip != null)
            {
                Vector3 cameraPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(seClip, cameraPos);
            }
        }
    }
}
