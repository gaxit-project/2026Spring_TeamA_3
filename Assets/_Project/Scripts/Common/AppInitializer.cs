using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace DeadLab
{
    /// <summary>
    /// ゲーム起動時（InitializeScene）に動作環境、音量（AudioMixer）、
    /// 感度、各種静的データを一括で初期化し、タイトル画面へ安全に遷移させる初期化クラス
    /// </summary>
    public class AppInitializer : MonoBehaviour
    {
        [Header("Audio Configuration")]
        [Tooltip("音量制御用AudioMixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Transition Settings")]
        [Tooltip("初期化完了後に読み込むシーン名")]
        [SerializeField] private string nextSceneName = "TitleScene";

        [Tooltip("将来のスプラッシュ画面演出用ウェイト時間（秒）")]
        [SerializeField] private float splashWaitDuration = 0f;

        private IEnumerator Start()
        {
            // 1. 動作環境・システム初期化
            InitializeEnvironment();

            // 2. 静的ゲームデータのクリーンリセット
            InitializeData();

            // 3. 音量設定のAudioMixer反映
            // UnityのAudioMixerはシーン起動直後（1フレーム目）に内部初期化が走るため、
            // 確実にパラメータを受け付けられるよう1フレーム待機
            yield return null;
            ApplyAudioSettings();

            // 4. スプラッシュ表示時間（現在は0秒、将来演出を入れる場合は秒数を指定可能）
            if (splashWaitDuration > 0f)
            {
                yield return new WaitForSeconds(splashWaitDuration);
            }

            // 5. タイトル画面へ遷移
            SceneManager.LoadScene(nextSceneName);
        }

        /// <summary>
        /// フレームレート、解像度、カーソル、時間スケール等の環境設定
        /// </summary>
        private void InitializeEnvironment()
        {
            // タイムスケールの正常化
            Time.timeScale = 1.0f;

            // スリープ無効化
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // 60FPS固定
            Application.targetFrameRate = 60;

            // マウスカーソルの非表示・ロック（コントローラー操作を基本とし誤クリック防止）
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

#if !UNITY_EDITOR
            // ディスプレイのネイティブ解像度を取得してアスペクト比を判別
            Resolution currentRes = Screen.currentResolution;
            float aspectRatio = (float)currentRes.width / currentRes.height;

            int targetWidth;
            int targetHeight;

            if (aspectRatio >= 1.7f)
            {
                // 16:9 ディスプレイの場合（1920 x 1080）
                targetWidth = 1920;
                targetHeight = 1080;
            }
            else if (aspectRatio >= 1.55f && aspectRatio < 1.7f)
            {
                // 16:10 ディスプレイの場合（1920 x 1200）
                targetWidth = 1920;
                targetHeight = 1200;
            }
            else
            {
                // その他の比率（高さ1080を基準に比率維持）
                targetHeight = 1080;
                targetWidth = Mathf.RoundToInt(1080f * aspectRatio);
            }

            // モニター解像度がターゲットより高解像度（4K, WQHD等）の場合のみ負荷軽減スケーリング
            if (currentRes.width > targetWidth || currentRes.height > targetHeight)
            {
                Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.FullScreenWindow);
            }
#endif
        }

        /// <summary>
        /// 各種静的データおよび設定値（感度など）の初期化
        /// </summary>
        private void InitializeData()
        {
            // リザルトデータの初期化
            GameResultData.Reset();

            // 感度の初期化・値保証（未設定なら1.0f）
            if (!PlayerPrefs.HasKey("Sensitivity"))
            {
                PlayerPrefs.SetFloat("Sensitivity", 1.0f);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// PlayerPrefsに保存された音量をAudioMixerに反映
        /// </summary>
        private void ApplyAudioSettings()
        {
            if (audioMixer == null) return;

            float savedBgm = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
            float bgmDb = Mathf.Log10(Mathf.Max(savedBgm, 0.0001f)) * 20f;
            audioMixer.SetFloat("BGMVolume", bgmDb);

            float savedSe = PlayerPrefs.GetFloat("SEVolume", 1.0f);
            float seDb = Mathf.Log10(Mathf.Max(savedSe, 0.0001f)) * 20f;
            audioMixer.SetFloat("SEVolume", seDb);
        }
    }
}
