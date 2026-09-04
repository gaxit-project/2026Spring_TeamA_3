using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using TMPro;

namespace DeadLab
{
    /// <summary>
    /// 最前面でのシャッター開閉・ステージ名表示制御用シングルトンUIクラス
    /// </summary>
    public class ShutterTransitionUI : MonoBehaviour
    {
        private static ShutterTransitionUI instance;
        public static ShutterTransitionUI Instance => instance;

        /// <summary>
        /// シャッター表示状態の取得
        /// </summary>
        public bool IsClosed => shutterPanel != null && shutterPanel.gameObject.activeSelf;

        /// <summary>
        /// シャッターの開閉処理状況の取得
        /// </summary>
        public bool IsTransitioning => isTransitioning;

        /// <summary>
        /// 操作ブロック状態の取得
        /// </summary>
        public bool IsInputBlocked => isInputBlocked;

        [Header("アニメーション設定")]
        [SerializeField] private float shutterCloseDuration = 0.5f;
        [SerializeField] private float stageNameDisplayDuration = 1.5f;
        [SerializeField] private float shutterOpenDuration = 0.6f;

        [Header("サウンド設定")]
        [Tooltip("シャッター移動時SE")]
        [SerializeField] private AudioClip shutterSound;
        [Tooltip("SE用AudioMixerGroup")]
        [SerializeField] private AudioMixerGroup seMixerGroup;

        [Header("UI要素参照")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform shutterPanel;
        [SerializeField] private TextMeshProUGUI stageNameText;

        private Vector2 closedPosition = Vector2.zero;
        private Vector2 topPosition;
        private Coroutine currentCoroutine;
        private AudioSource audioSource;
        private bool isTransitioning = false;
        private bool isInputBlocked = false;

        /// <summary>
        /// UIイベントシステムのナビゲーション有効化切替
        /// </summary>
        private void SetInputEnabled(bool enable)
        {
            if (EventSystem.current != null)
            {
                // EventSystem 自体を無効化するとマウス入力やシーンロード時の状態が壊れるため、
                // コントローラー・キーボードのナビゲーション（選択移動・Submit）のみを制御する
                EventSystem.current.sendNavigationEvents = enable;
                EventSystem.current.enabled = true;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeUI();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // シーン切り替え時は EventSystem を確実に復旧させる
            if (EventSystem.current != null)
            {
                EventSystem.current.enabled = true;
                EventSystem.current.sendNavigationEvents = true;
            }
        }

        /// <summary>
        /// UI要素未アタッチ時の自動生成・初期構築処理
        /// </summary>
        private void InitializeUI()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
                if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            if (shutterPanel == null)
            {
                GameObject shutterObj = new GameObject("ShutterPanel", typeof(RectTransform), typeof(Image));
                shutterObj.transform.SetParent(canvas.transform, false);
                shutterPanel = shutterObj.GetComponent<RectTransform>();

                shutterPanel.anchorMin = new Vector2(0, 0);
                shutterPanel.anchorMax = new Vector2(1, 1);
                shutterPanel.sizeDelta = Vector2.zero;

                Image img = shutterObj.GetComponent<Image>();
                img.color = new Color(0.08f, 0.08f, 0.1f, 1.0f);
            }

            topPosition = new Vector2(0, Screen.height * 1.5f);
            shutterPanel.anchoredPosition = topPosition;

            if (stageNameText == null)
            {
                GameObject textObj = new GameObject("StageNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(shutterPanel, false);

                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.1f, 0.4f);
                textRect.anchorMax = new Vector2(0.9f, 0.6f);
                textRect.sizeDelta = Vector2.zero;

                stageNameText = textObj.GetComponent<TextMeshProUGUI>();
                stageNameText.alignment = TextAlignmentOptions.Center;
                stageNameText.fontSize = 54;
                stageNameText.fontStyle = FontStyles.Bold;
                stageNameText.color = Color.white;
                stageNameText.text = "";
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;

            SetupAudioMixerGroup();
        }

        /// <summary>
        /// SE用AudioMixerGroupの設定
        /// </summary>
        private void SetupAudioMixerGroup()
        {
            if (audioSource == null) return;

            if (seMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = seMixerGroup;
            }
            else if (SettingsManager.SeMixerGroupInstance != null)
            {
                audioSource.outputAudioMixerGroup = SettingsManager.SeMixerGroupInstance;
            }
        }

        /// <summary>
        /// シャッターSEの再生
        /// </summary>
        private void PlayShutterSound()
        {
            SetupAudioMixerGroup();

            if (audioSource != null && shutterSound != null)
            {
                audioSource.PlayOneShot(shutterSound);
            }
        }

        /// <summary>
        /// シャッターの閉鎖処理（画面遮断）
        /// </summary>
        public void CloseShutter(Action onComplete = null)
        {
            if (isTransitioning) return;
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(CoCloseShutter(onComplete));
        }

        /// <summary>
        /// ステージ開始演出（ステージ名表示および開放）
        /// </summary>
        public void PlayStageStartSequence(string stageName, Action onStartOpening = null, Action onOpened = null)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(CoPlayStageStartSequence(stageName, onStartOpening, onOpened));
        }

        /// <summary>
        /// シャッターの開放処理
        /// </summary>
        public void OpenShutter(Action onOpened = null, float delay = 0f)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(CoOpenShutter(onOpened, delay));
        }

        /// <summary>
        /// ゲームクリア演出（文字表示および開放）
        /// </summary>
        public void PlayGameClearSequence(Action onComplete = null)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(CoPlayGameClearSequence(onComplete));
        }

        private float GetCanvasHeight()
        {
            if (canvas != null && canvas.transform is RectTransform rectTransform)
            {
                return rectTransform.rect.height > 0 ? rectTransform.rect.height : 1080f;
            }
            return 1080f;
        }

        private IEnumerator CoCloseShutter(Action onComplete)
        {
            isTransitioning = true;
            isInputBlocked = true;
            SetInputEnabled(false);

            if (shutterPanel != null) shutterPanel.gameObject.SetActive(true);
            if (stageNameText != null) stageNameText.text = "";

            PlayShutterSound();

            float canvasHeight = GetCanvasHeight();
            Vector2 startPos = new Vector2(0, canvasHeight * 1.5f);
            Vector2 endPos = Vector2.zero;
            if (shutterPanel != null) shutterPanel.anchoredPosition = startPos;
            float elapsed = 0f;

            while (elapsed < shutterCloseDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / shutterCloseDuration);
                t = 1f - Mathf.Pow(1f - t, 3);
                shutterPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            shutterPanel.anchoredPosition = endPos;
            isTransitioning = false;
            onComplete?.Invoke();
        }

        private IEnumerator CoPlayStageStartSequence(string stageName, Action onStartOpening, Action onOpened)
        {
            isTransitioning = true;
            isInputBlocked = true;
            SetInputEnabled(false);

            shutterPanel.anchoredPosition = Vector2.zero;

            if (stageNameText != null)
            {
                stageNameText.text = stageName;
                stageNameText.alpha = 0f;

                float fadeInTime = 0.3f;
                float elapsedFade = 0f;
                while (elapsedFade < fadeInTime)
                {
                    elapsedFade += Time.unscaledDeltaTime;
                    stageNameText.alpha = Mathf.Clamp01(elapsedFade / fadeInTime);
                    yield return null;
                }
                stageNameText.alpha = 1f;
            }

            yield return new WaitForSecondsRealtime(stageNameDisplayDuration);

            if (stageNameText != null)
            {
                float fadeOutTime = 0.2f;
                float elapsedFade = 0f;
                while (elapsedFade < fadeOutTime)
                {
                    elapsedFade += Time.unscaledDeltaTime;
                    stageNameText.alpha = 1f - Mathf.Clamp01(elapsedFade / fadeOutTime);
                    yield return null;
                }
                stageNameText.alpha = 0f;
                stageNameText.text = "";
            }

            PlayShutterSound();

            float canvasHeight = GetCanvasHeight();
            Vector2 startPos = Vector2.zero;
            Vector2 endPos = new Vector2(0, canvasHeight * 1.5f);
            float elapsedOpen = 0f;
            bool unblocked = false;

            while (elapsedOpen < shutterOpenDuration)
            {
                elapsedOpen += Time.unscaledDeltaTime;
                float ratio = Mathf.Clamp01(elapsedOpen / shutterOpenDuration);

                // 画面が約70%見えた段階で先行して操作解禁（操作のキビキビ感・手触り向上）
                if (!unblocked && ratio >= 0.7f)
                {
                    unblocked = true;
                    isInputBlocked = false;
                    SetInputEnabled(true);
                    onStartOpening?.Invoke();
                }

                float t = ratio * ratio * ratio;
                shutterPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            shutterPanel.anchoredPosition = endPos;
            shutterPanel.gameObject.SetActive(false);
            isTransitioning = false;
            if (!unblocked)
            {
                isInputBlocked = false;
                SetInputEnabled(true);
                onStartOpening?.Invoke();
            }
            onOpened?.Invoke();
        }

        private IEnumerator CoOpenShutter(Action onOpened, float delay = 0f)
        {
            isTransitioning = true;
            isInputBlocked = true;
            SetInputEnabled(false);

            if (shutterPanel != null)
            {
                shutterPanel.gameObject.SetActive(true);
                shutterPanel.anchoredPosition = Vector2.zero;
            }
            if (stageNameText != null)
            {
                stageNameText.text = "";
                stageNameText.alpha = 0f;
            }

            if (delay > 0f)
            {
                yield return new WaitForSecondsRealtime(delay);
            }

            PlayShutterSound();

            float canvasHeight = GetCanvasHeight();
            Vector2 startPos = Vector2.zero;
            Vector2 endPos = new Vector2(0, canvasHeight * 1.5f);
            float elapsedOpen = 0f;
            bool unblocked = false;

            while (elapsedOpen < shutterOpenDuration)
            {
                elapsedOpen += Time.unscaledDeltaTime;
                float ratio = Mathf.Clamp01(elapsedOpen / shutterOpenDuration);

                // 画面が約70%見えた段階で先行して操作解禁
                if (!unblocked && ratio >= 0.7f)
                {
                    unblocked = true;
                    isInputBlocked = false;
                    SetInputEnabled(true);
                }

                float t = ratio * ratio * ratio;
                shutterPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            shutterPanel.anchoredPosition = endPos;
            shutterPanel.gameObject.SetActive(false);
            isTransitioning = false;
            if (!unblocked)
            {
                isInputBlocked = false;
                SetInputEnabled(true);
            }
            onOpened?.Invoke();
        }

        private IEnumerator CoPlayGameClearSequence(Action onComplete)
        {
            yield return CoCloseShutter(null);

            if (stageNameText != null)
            {
                stageNameText.text = "ゲームクリア";
                stageNameText.color = new Color(1f, 0.85f, 0f);
                stageNameText.alpha = 0f;

                float fadeInTime = 0.4f;
                float elapsedFade = 0f;
                while (elapsedFade < fadeInTime)
                {
                    elapsedFade += Time.unscaledDeltaTime;
                    stageNameText.alpha = Mathf.Clamp01(elapsedFade / fadeInTime);
                    yield return null;
                }
                stageNameText.alpha = 1f;
            }

            yield return new WaitForSecondsRealtime(stageNameDisplayDuration);

            if (stageNameText != null)
            {
                float fadeOutTime = 0.3f;
                float elapsedFade = 0f;
                while (elapsedFade < fadeOutTime)
                {
                    elapsedFade += Time.unscaledDeltaTime;
                    stageNameText.alpha = 1f - Mathf.Clamp01(elapsedFade / fadeOutTime);
                    yield return null;
                }
                stageNameText.alpha = 0f;
                stageNameText.text = "";
                stageNameText.color = Color.white;
            }

            onComplete?.Invoke();
        }

        /// <summary>
        /// シーン上の ShutterTransitionUI インスタンス確保処理
        /// </summary>
        public static ShutterTransitionUI EnsureExists()
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("ShutterTransitionUI");
                instance = obj.AddComponent<ShutterTransitionUI>();
            }
            return instance;
        }
    }
}
