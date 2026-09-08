using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DeadLab
{
    /// <summary>
    /// タイトル画面、ステージ選択、設定画面のUI表示および画面遷移制御用クラス
    /// </summary>
    public class TitleUIController : MonoBehaviour
    {
        [Header("Stage Configuration")]
        [Tooltip("ステージ選択画面用ステージデータアセットリスト")]
        [SerializeField] private StageData[] availableStages;

        [Header("Panels")]
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private GameObject stageSelectPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private GameObject creditPanel;

        [Header("Managers")]
        [SerializeField] private StageSelectManager stageManager;
        [SerializeField] private SettingsManager settingsManager;

        [Header("Title Buttons")]
        [SerializeField] private Button startBtn;
        [SerializeField] private Button settingsBtn;
        [SerializeField] private Button exitBtn;
        [SerializeField] private Button tutorialBtn;
        [SerializeField] private Button tutorialBackBtn;
        [SerializeField] private Button creditBtn;
        [SerializeField] private Button creditBackBtn;

        [Header("Default Selected UI")]
        [SerializeField] private GameObject titleDefaultButton;
        [SerializeField] private GameObject settingsDefaultSlider;

        private void Awake()
        {
            // タイトル画面ボタンアクションの登録
            if (startBtn != null) startBtn.onClick.AddListener(() => ShowPanel(stageSelectPanel));
            if (settingsBtn != null) settingsBtn.onClick.AddListener(() => ShowPanel(settingsPanel));
            if (tutorialBtn != null) tutorialBtn.onClick.AddListener(() => ShowPanel(tutorialPanel));
            if (tutorialBackBtn != null) tutorialBackBtn.onClick.AddListener(() => ShowPanel(titlePanel));
            if (creditBtn != null) creditBtn.onClick.AddListener(() => ShowPanel(creditPanel));
            if (creditBackBtn != null) creditBackBtn.onClick.AddListener(() => ShowPanel(titlePanel));
            if (exitBtn != null) exitBtn.onClick.AddListener(QuitGame);

            // 各Managerからの戻るイベントの購読
            if (stageManager != null) stageManager.OnBackRequested += () => ShowPanel(titlePanel);
            if (settingsManager != null)
            {
                // 設定パネルが非アクティブでもSE用MixerGroupを即座に初期化
                settingsManager.InitializeSettings();
                settingsManager.OnBackRequested += () => ShowPanel(titlePanel);
            }
        }

        private void Start()
        {
            // シャッター全閉時の開放処理（シーン描画安定のため少し溜め(0.25s)を作ってから開放）
            if (ShutterTransitionUI.Instance != null && ShutterTransitionUI.Instance.IsClosed)
            {
                ShutterTransitionUI.Instance.OpenShutter(delay: 0.25f);
            }

            // リザルト経由遷移時のパネル表示分岐
            if (GameResultData.ShowStageDetailOnLoad)
            {
                ShowPanel(stageSelectPanel);
            }
            else
            {
                ShowPanel(titlePanel);
            }
        }

        /// <summary>
        /// 対象パネルのアクティブ化切替
        /// </summary>
        private void ShowPanel(GameObject targetPanel)
        {
            if (titlePanel != null) titlePanel.SetActive(titlePanel == targetPanel);
            if (stageSelectPanel != null) stageSelectPanel.SetActive(stageSelectPanel == targetPanel);
            if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == targetPanel);
            if (tutorialPanel != null) tutorialPanel.SetActive(tutorialPanel == targetPanel);
            if(creditPanel != null) creditPanel.SetActive(creditPanel == targetPanel);

            // フォーカスリセット
            EventSystem.current.SetSelectedGameObject(null);

            // パネルに応じた初期フォーカス対象の設定
            if (targetPanel == titlePanel && titleDefaultButton != null)
            {
                EventSystem.current.SetSelectedGameObject(titleDefaultButton);
            }
            else if (targetPanel == stageSelectPanel)
            {
                if (stageManager != null)
                {
                    stageManager.Open(availableStages);
                }
            }
            else if (targetPanel == settingsPanel && settingsDefaultSlider != null)
            {
                EventSystem.current.SetSelectedGameObject(settingsDefaultSlider);
            }
            else if (targetPanel == tutorialPanel && tutorialBackBtn != null)
            {
                EventSystem.current.SetSelectedGameObject(tutorialBackBtn.gameObject);
            }
            else if(targetPanel == creditPanel && creditBackBtn != null)
            {
                EventSystem.current.SetSelectedGameObject(creditBackBtn.gameObject);
            }
        }

        /// <summary>
        /// アプリケーション終了処理
        /// </summary>
        private void QuitGame()
        {
            Debug.Log("[TitleUIController] ゲームを終了します。");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
