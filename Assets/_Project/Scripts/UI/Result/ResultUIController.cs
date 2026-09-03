using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

namespace DeadLab
{
    /// <summary>
    /// リザルト画面（ResultScene）における結果データ反映および画面遷移制御用クラス
    /// </summary>
    public class ResultUIController : MonoBehaviour
    {
        [Header("Texts (Values Only)")]
        [Tooltip("ミッション成否のステータスヘッダーテキスト (例: MISSION CLEAR / MISSION FAILED)")]
        [SerializeField] private TextMeshProUGUI headerStatusText;
        [Tooltip("ステージ名の値表示用テキスト")]
        [SerializeField] private TextMeshProUGUI stageNameText;
        [Tooltip("ランク評価の値表示用テキスト")]
        [SerializeField] private TextMeshProUGUI rankText;
        [Tooltip("結果項目の見出しラベルテキスト (クリア時: CLEAR TIME / 失敗時: KILLS)")]
        [SerializeField] private TextMeshProUGUI resultLabelText;
        [Tooltip("結果項目の数値表示用テキスト (クリア時: クリアタイム / 失敗時: 撃破数)")]
        [SerializeField] private TextMeshProUGUI resultValueText;
        [Tooltip("記録更新時に表示する自己ベスト表示用テキスト")]
        [SerializeField] private TextMeshProUGUI newRecordText;

        [Header("Buttons")]
        [Tooltip("リトライボタン")]
        [SerializeField] private Button retryButton;
        [Tooltip("ネクストステージボタン")]
        [SerializeField] private Button nextButton;
        [Tooltip("タイトルへ戻るボタン")]
        [SerializeField] private Button titleButton;

        [Header("Background Settings")]
        [Tooltip("リザルト画面の背景画像/パネル")]
        [SerializeField] private Image backgroundImage;
        [Tooltip("クリア時の背景色")]
        [SerializeField] private Color clearBackgroundColor = new Color(0.08f, 0.18f, 0.36f, 1f);
        [Tooltip("ゲームオーバー時の背景色")]
        [SerializeField] private Color failedBackgroundColor = new Color(0.1f, 0.1f, 0.12f, 1f);

        [Header("BGM Settings")]
        [Tooltip("リザルト画面BGM（クリア時）")]
        [SerializeField] private AudioClip clearResultBgm;
        [Tooltip("リザルト画面BGM（ゲームオーバー時）")]
        [SerializeField] private AudioClip failedResultBgm;
        [Tooltip("BGM再生用AudioMixerGroup")]
        [SerializeField] private AudioMixerGroup bgmMixerGroup;

        [Header("Selectable Target (Controller Support)")]
        [Tooltip("初期フォーカスボタン")]
        [SerializeField] private Button firstSelectedButton;

        private AudioSource bgmAudioSource;

        private void Start()
        {
            // マウスカーソル可視化およびロック解除
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // BGM用AudioSourceの初期化
            InitAudioSource();

            // UIデータ反映
            UpdateResultUI();

            // ボタンイベント登録
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RetryGame);
            }
            if (nextButton != null)
            {
                if (!GameResultData.IsClear)
                {
                    nextButton.gameObject.SetActive(false);
                }
                else
                {
                    nextButton.onClick.AddListener(NextStage);
                }
            }
            if (titleButton != null)
            {
                titleButton.onClick.AddListener(LoadTitle);
            }

            // 初期選択ボタンのフォーカス設定
            if (firstSelectedButton != null && UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
            }

            // シャッター状態に応じたBGM再生
            if (ShutterTransitionUI.Instance != null && ShutterTransitionUI.Instance.IsClosed)
            {
                ShutterTransitionUI.Instance.OpenShutter(() =>
                {
                    PlayResultBgm();
                }, delay: 0.25f);
            }
            else
            {
                // シャッターが開いている・存在しない場合はそのままBGM再生
                PlayResultBgm();
            }
        }

        /// <summary>
        /// BGM再生用AudioSource初期化
        /// </summary>
        private void InitAudioSource()
        {
            bgmAudioSource = GetComponent<AudioSource>();
            if (bgmAudioSource == null)
            {
                bgmAudioSource = gameObject.AddComponent<AudioSource>();
            }
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;

            if (bgmMixerGroup != null)
            {
                bgmAudioSource.outputAudioMixerGroup = bgmMixerGroup;
            }
        }

        /// <summary>
        /// 勝敗に応じたリザルトBGMの再生
        /// </summary>
        private void PlayResultBgm()
        {
            if (bgmAudioSource == null) return;

            AudioClip clipToPlay = GameResultData.IsClear ? clearResultBgm : (failedResultBgm != null ? failedResultBgm : clearResultBgm);
            if (clipToPlay != null)
            {
                bgmAudioSource.clip = clipToPlay;
                bgmAudioSource.volume = 1f;
                bgmAudioSource.Play();
            }
        }

        /// <summary>
        /// BGMのフェードアウト停止処理
        /// </summary>
        private System.Collections.IEnumerator CoFadeOutBgm(float duration)
        {
            if (bgmAudioSource == null || !bgmAudioSource.isPlaying) yield break;

            float startVolume = bgmAudioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            bgmAudioSource.volume = 0f;
            bgmAudioSource.Stop();
        }

        /// <summary>
        /// UIコンポーネントへのデータ表示反映
        /// </summary>
        private void UpdateResultUI()
        {
            // 背景色の設定
            if (backgroundImage != null)
            {
                backgroundImage.color = GameResultData.IsClear ? clearBackgroundColor : failedBackgroundColor;
            }
            else
            {
                Debug.LogWarning("[ResultUIController] backgroundImage がアタッチされていません。", this);
            }

            // ヘッダーテキスト設定
            if (headerStatusText != null)
            {
                if (GameResultData.IsClear)
                {
                    headerStatusText.text = "MISSION CLEAR";
                    headerStatusText.color = Color.yellow;
                }
                else
                {
                    headerStatusText.text = "MISSION FAILED";
                    headerStatusText.color = Color.red;
                }
            }

            // ステージ名表示
            if (stageNameText != null)
            {
                stageNameText.text = GameResultData.StageName;
            }

            // 評価ランク表示
            if (rankText != null)
            {
                rankText.text = GameResultData.Rank;
                if (GameResultData.Rank == "S") rankText.color = Color.yellow;
                else if (GameResultData.Rank == "A") rankText.color = Color.cyan;
                else if (GameResultData.Rank == "B") rankText.color = Color.green;
                else if (GameResultData.Rank == "C") rankText.color = Color.white;
                else rankText.color = Color.red;
            }

            // 勝敗に応じた表示項目の切り替え
            if (GameResultData.IsClear)
            {
                if (resultLabelText != null) resultLabelText.text = "クリアタイム";

                if (resultValueText != null)
                {
                    float timeVal = GameResultData.ClearTime > 0f ? GameResultData.ClearTime : GameResultData.RemainingTime;
                    int totalSeconds = Mathf.RoundToInt(timeVal);
                    int minutes = totalSeconds / 60;
                    int seconds = totalSeconds % 60;
                    resultValueText.text = $"{minutes}:{seconds:00}";
                }
            }
            else
            {
                if (resultLabelText != null) resultLabelText.text = "撃破数";

                if (resultValueText != null)
                {
                    resultValueText.text = $"{GameResultData.KillCount} / {GameResultData.TargetKillCount}";
                }
            }

            // 自己ベスト更新時の点滅表示制御
            if (newRecordText != null)
            {
                if (GameResultData.IsClear && GameResultData.IsNewRecord)
                {
                    newRecordText.gameObject.SetActive(true);
                    newRecordText.text = "NEW RECORD!";
                    StartCoroutine(CoBlinkNewRecordText());
                }
                else
                {
                    newRecordText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// NEW RECORDテキストの点滅演出処理
        /// </summary>
        private System.Collections.IEnumerator CoBlinkNewRecordText()
        {
            if (newRecordText == null) yield break;

            Color baseColor = new Color(1.0f, 0.85f, 0.0f, 1.0f);
            float blinkSpeed = 4.0f;

            while (newRecordText != null && newRecordText.gameObject.activeSelf)
            {
                float alpha = Mathf.Lerp(0.25f, 1.0f, (Mathf.Sin(Time.unscaledTime * blinkSpeed) + 1.0f) * 0.5f);
                newRecordText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                yield return null;
            }
        }

        #region ボタンアクション

        /// <summary>
        /// 直前ステージの再ロード（リトライ）処理
        /// </summary>
        private void RetryGame()
        {
            StartCoroutine(CoFadeOutBgm(0.4f));

            string sceneToLoad = string.IsNullOrEmpty(GameResultData.LastPlaySceneName) ? "TestScene" : GameResultData.LastPlaySceneName;
            Debug.Log($"[ResultUIController] {sceneToLoad} シーンをロードしてリトライします。");

            ShutterTransitionUI shutter = ShutterTransitionUI.EnsureExists();
            shutter.CloseShutter(() =>
            {
                SceneManager.LoadScene(sceneToLoad);
            });
        }

        /// <summary>
        /// 次のステージへの遷移処理
        /// </summary>
        private void NextStage()
        {
            StartCoroutine(CoFadeOutBgm(0.4f));

            ShutterTransitionUI shutter = ShutterTransitionUI.EnsureExists();
            shutter.CloseShutter(() =>
            {
                if (GameResultData.AllStages != null && GameResultData.AllStages.Length > 0)
                {
                    int nextIndex = GameResultData.CurrentStageIndex + 1;
                    if (nextIndex < GameResultData.AllStages.Length)
                    {
                        StageData nextStage = GameResultData.AllStages[nextIndex];
                        GameResultData.SelectedStageData = nextStage;
                        GameResultData.CurrentStageIndex = nextIndex;
                        GameResultData.ShowStageDetailOnLoad = true;

                        Debug.Log($"[ResultUIController] 次のステージ「{nextStage.StageName}」詳細画面へ遷移するためタイトル画面に戻ります。");
                        SceneManager.LoadScene("TitleScene");
                    }
                    else
                    {
                        Debug.Log("[ResultUIController] 全てのステージをクリアしました！タイトルに戻ります。");
                        SceneManager.LoadScene("TitleScene");
                    }
                }
                else
                {
                    Debug.LogError("[ResultUIController] ステージリストが見つかりません。");
                    SceneManager.LoadScene("TitleScene");
                }
            });
        }

        /// <summary>
        /// タイトルシーンへの遷移処理
        /// </summary>
        private void LoadTitle()
        {
            StartCoroutine(CoFadeOutBgm(0.4f));

            ShutterTransitionUI shutter = ShutterTransitionUI.EnsureExists();
            shutter.CloseShutter(() =>
            {
                Debug.Log("[ResultUIController] TitleScene をロードします。");
                SceneManager.LoadScene("TitleScene");
            });
        }

        #endregion
    }
}
