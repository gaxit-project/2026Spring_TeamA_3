using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace DeadLab
{
    /// <summary>
    /// ステージ選択画面のUI構築および画面遷移制御用クラス
    /// </summary>
    public class StageSelectManager : MonoBehaviour
    {
        [Header("Stage Configuration")]
        [SerializeField] private StageData[] availableStages;
        [SerializeField] private StageCardView stageCardPrefab;
        [SerializeField] private Transform stageCardContainer;

        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private StageDetailView detailView;
        [SerializeField] private GameObject cardSelectPanel;
        [SerializeField] private GameObject detailPanel;

        private StageData selectedStage;

        public event Action OnBackRequested;

        private List<StageCardView> generatedCards = new List<StageCardView>();
        private bool isInitialized = false;

        private void Start()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(() => OnBackRequested?.Invoke());
            }
        }

        /// <summary>
        /// パネル表示処理（カード生成および初期フォーカス設定）
        /// </summary>
        public void Open(StageData[] stagesOverride = null)
        {
            if (stagesOverride != null && stagesOverride.Length > 0)
            {
                availableStages = stagesOverride;
            }

            GenerateCards();

            // 初期パネル状態の設定
            if (GameResultData.ShowStageDetailOnLoad && GameResultData.SelectedStageData != null)
            {
                // リザルトから「次へ」遷移時：直接詳細画面を開く
                GameResultData.ShowStageDetailOnLoad = false;

                // カード選択状態の同期
                int targetIndex = Array.IndexOf(availableStages, GameResultData.SelectedStageData);
                if (targetIndex >= 0 && targetIndex < generatedCards.Count)
                {
                    lastSelectedCardObject = generatedCards[targetIndex].CardButton.gameObject;
                }

                OnStageSelected(GameResultData.SelectedStageData);
            }
            else
            {
                // 通常起動：カード選択画面を表示
                if (cardSelectPanel != null) cardSelectPanel.SetActive(true);
                if (detailPanel != null) detailPanel.SetActive(false);

                // 最初のカードへのフォーカス設定
                if (generatedCards.Count > 0 && generatedCards[0].CardButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(generatedCards[0].CardButton.gameObject);

                    if (detailView != null)
                    {
                        bool isUnlocked = IsStageUnlocked(0);
                        detailView.Setup(generatedCards[0].StageData, isUnlocked, StartPlayGame, CloseDetailPanel);
                    }
                }
                else if (backButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(backButton.gameObject);
                }
            }
        }

        /// <summary>
        /// 指定インデックスのステージ解放状態判定（最初のステージは常に解放）
        /// </summary>
        public bool IsStageUnlocked(int index)
        {
            if (index <= 0) return true;
            if (availableStages == null || index >= availableStages.Length) return false;

            StageData prevStage = availableStages[index - 1];
            if (prevStage == null) return false;

            StageRecord record = SaveDataManager.GetRecord(prevStage.StageName);
            return record != null && record.isCleared;
        }

        private void GenerateCards()
        {
            if (isInitialized) return;

            if (availableStages == null || availableStages.Length == 0)
            {
                Debug.LogWarning("[StageSelectManager] No available stages found.");
                return;
            }

            for (int i = 0; i < availableStages.Length; i++)
            {
                StageData stage = availableStages[i];
                bool isUnlocked = IsStageUnlocked(i);

                StageCardView card = Instantiate(stageCardPrefab, stageCardContainer);
                card.Setup(stage, isUnlocked, OnStageSelected);
                generatedCards.Add(card);
            }

            SetupNavigation();
            isInitialized = true;
        }

        private void SetupNavigation()
        {
            List<StageCardView> unlockedCards = new List<StageCardView>();
            for (int i = 0; i < generatedCards.Count; i++)
            {
                if (IsStageUnlocked(i) && generatedCards[i] != null && generatedCards[i].CardButton != null)
                {
                    unlockedCards.Add(generatedCards[i]);
                }
                else if (generatedCards[i] != null && generatedCards[i].CardButton != null)
                {
                    Navigation lockNav = new Navigation { mode = Navigation.Mode.None };
                    generatedCards[i].CardButton.navigation = lockNav;
                }
            }

            for (int i = 0; i < unlockedCards.Count; i++)
            {
                Button currentBtn = unlockedCards[i].CardButton;
                Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };

                if (i > 0)
                {
                    nav.selectOnLeft = unlockedCards[i - 1].CardButton;
                }

                if (i < unlockedCards.Count - 1)
                {
                    nav.selectOnRight = unlockedCards[i + 1].CardButton;
                }

                if (backButton != null)
                {
                    nav.selectOnDown = backButton;
                }

                currentBtn.navigation = nav;
            }

            if (backButton != null && unlockedCards.Count > 0)
            {
                Navigation backNav = backButton.navigation;
                backNav.mode = Navigation.Mode.Explicit;
                backNav.selectOnUp = unlockedCards[0].CardButton;
                backButton.navigation = backNav;
            }
        }

        private float detailPanelOpenedTime = -1f;

        private void OnStageSelected(StageData stage)
        {
            if (stage == null) return;

            selectedStage = stage;

            int stageIndex = Array.IndexOf(availableStages, stage);
            bool isUnlocked = IsStageUnlocked(stageIndex);

            GameResultData.AllStages = availableStages;
            GameResultData.CurrentStageIndex = stageIndex;
            GameResultData.Reset();
            GameResultData.SelectedStageData = stage;

            if (detailView != null)
            {
                detailView.Setup(stage, isUnlocked, StartPlayGame, CloseDetailPanel);

                // 画面切替後のフォーカス移動を1フレーム遅延して確実化
                GameObject targetBtn = (detailView.StartButton != null && detailView.StartButton.interactable)
                    ? detailView.StartButton.gameObject
                    : (detailView.BackButton != null ? detailView.BackButton.gameObject : null);

                if (targetBtn != null)
                {
                    StartCoroutine(CoSetSelectedUI(targetBtn));
                }
            }

            if (cardSelectPanel != null) cardSelectPanel.SetActive(false);
            if (detailPanel != null) detailPanel.SetActive(true);

            detailPanelOpenedTime = Time.unscaledTime;
        }

        /// <summary>
        /// 画面切替時のUIフォーカス設定（1フレーム遅延処理）
        /// </summary>
        private System.Collections.IEnumerator CoSetSelectedUI(GameObject target)
        {
            yield return null;
            if (EventSystem.current != null && target != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(target);
            }
        }

        private bool isStartingGame = false;

        private void StartPlayGame()
        {
            Debug.Log($"[StageSelectManager] StartPlayGame() 実行開始 | selectedStage: {(selectedStage != null ? selectedStage.StageName : "null")} | isStartingGame: {isStartingGame}");

            if (isStartingGame)
            {
                Debug.LogWarning("[StageSelectManager] すでにゲーム開始処理が進行中のため中断しました。");
                return;
            }

            if (selectedStage == null)
            {
                Debug.LogError("[StageSelectManager] selectedStage が null のためゲームを開始できません。");
                return;
            }

            int stageIndex = Array.IndexOf(availableStages, selectedStage);
            bool isUnlocked = IsStageUnlocked(stageIndex);
            Debug.Log($"[StageSelectManager] ステージインデックス: {stageIndex} | 解放状態: {isUnlocked}");

            if (!isUnlocked)
            {
                Debug.LogWarning($"[StageSelectManager] ステージ {selectedStage.StageName} はロックされているため開始できません。");
                return;
            }

            ShutterTransitionUI shutter = ShutterTransitionUI.EnsureExists();
            if (shutter.IsTransitioning)
            {
                Debug.LogWarning("[StageSelectManager] シャッターが開放中のためゲームを開始できません。");
                return;
            }

            isStartingGame = true;
            Debug.Log($"[StageSelectManager] シャッターの閉鎖演出を開始します。(Scene: {selectedStage.SceneName})");

            shutter.CloseShutter(() =>
            {
                Debug.Log($"[StageSelectManager] シャッター閉鎖完了。シーン「{selectedStage.SceneName}」をロードします。");
                SceneManager.LoadScene(selectedStage.SceneName);
            });
        }

        private void CloseDetailPanel()
        {
            if (cardSelectPanel != null) cardSelectPanel.SetActive(true);
            if (detailPanel != null) detailPanel.SetActive(false);

            if (lastSelectedCardObject != null)
            {
                StartCoroutine(CoSetSelectedUI(lastSelectedCardObject));
            }
            else if (generatedCards.Count > 0 && generatedCards[0].CardButton != null)
            {
                StartCoroutine(CoSetSelectedUI(generatedCards[0].CardButton.gameObject));
            }
        }

        private GameObject lastSelectedCardObject;

        private void Update()
        {
            if (EventSystem.current == null) return;

            // シャッター開閉・遷移中は入力を一切受け付けない
            if (ShutterTransitionUI.Instance != null && (ShutterTransitionUI.Instance.IsTransitioning || ShutterTransitionUI.Instance.IsInputBlocked))
            {
                return;
            }

            // 詳細表示画面のダイレクト入力監視
            if (detailPanel != null && detailPanel.activeSelf)
            {
                if (Time.unscaledTime < detailPanelOpenedTime + 0.2f) return;

                bool isSubmit = false;
                bool isCancel = false;

                // New Input System の Gamepad を安全にチェック
                if (UnityEngine.InputSystem.Gamepad.current != null)
                {
                    if (UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame ||
                        UnityEngine.InputSystem.Gamepad.current.aButton.wasPressedThisFrame)
                    {
                        isSubmit = true;
                    }
                    if (UnityEngine.InputSystem.Gamepad.current.buttonEast.wasPressedThisFrame ||
                        UnityEngine.InputSystem.Gamepad.current.bButton.wasPressedThisFrame)
                    {
                        isCancel = true;
                    }
                }

                // キーボード操作の補完チェック
                if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space)) isSubmit = true;
                if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Backspace)) isCancel = true;

                if (isSubmit)
                {
                    Debug.Log("[StageSelectManager] 詳細画面で決定操作（Aボタン/Enter）を検知。ゲームを開始します。");
                    StartPlayGame();
                    return;
                }

                if (isCancel)
                {
                    Debug.Log("[StageSelectManager] 詳細画面でキャンセル操作（Bボタン/Esc）を検知。詳細画面を閉じます。");
                    CloseDetailPanel();
                    return;
                }

                return;
            }

            // カード選択画面表示時のフォーカス追従処理
            if (cardSelectPanel != null && !cardSelectPanel.activeSelf) return;
            if (detailView == null) return;

            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected != null && currentSelected != lastSelectedCardObject)
            {
                StageCardView cardView = currentSelected.GetComponentInParent<StageCardView>();
                if (cardView != null)
                {
                    lastSelectedCardObject = currentSelected;
                    int cardIndex = generatedCards.IndexOf(cardView);
                    bool isUnlocked = IsStageUnlocked(cardIndex);
                    detailView.Setup(cardView.StageData, isUnlocked, StartPlayGame, CloseDetailPanel);
                }
            }
        }
    }
}
