using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeadLab
{
    /// <summary>
    /// ゲーム全体ステート（進行、時間カウント、クリア・失敗判定、シーン遷移）制御用マネージャー
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            Init,
            Playing,
            GameClear,
            GameOver
        }

        [Header("ステージ設定")]
        [Tooltip("ロード対象ステージデータ")]
        [SerializeField] private StageData stageData;

        [Header("演出設定")]
        [Tooltip("クリア判定後のリザルト遷移遅延時間（秒）")]
        [SerializeField] private float clearDelay = 2.0f;
        [Tooltip("ゲームオーバー演出後のリザルト遷移遅延時間（秒）")]
        [SerializeField] private float gameOverDelay = 3.0f;

        [Header("関連コンポーネント")]
        [SerializeField] private ZombieSpawner spawner;

        private GameState currentState = GameState.Init;
        private float remainingTime;
        private int currentKillCount = 0;
        private int currentScore = 0;

        private PlayerHealth playerHealth;

        public event System.Action<float> OnTimerUpdated;
        public event System.Action<int, int> OnKillCountUpdated;
        public event System.Action<int> OnScoreUpdated;

        public GameState CurrentState => currentState;
        public float RemainingTime => remainingTime;
        public int CurrentKillCount => currentKillCount;
        public int CurrentScore => currentScore;
        public StageData CurrentStageData => stageData;

        private static GameManager instance;
        public static GameManager Instance => instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            GameResultData.LastPlaySceneName = SceneManager.GetActiveScene().name;

            if (spawner == null) spawner = FindFirstObjectByType<ZombieSpawner>();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }

            StartGame();
        }

        /// <summary>
        /// ゲームセッション開始処理
        /// </summary>
        private void StartGame()
        {
            if (GameResultData.SelectedStageData != null)
            {
                stageData = GameResultData.SelectedStageData;
            }

            if (stageData == null)
            {
                Debug.LogError("[GameManager] StageDataがアタッチされていません。");
                return;
            }

            remainingTime = stageData.TimeLimit;
            currentKillCount = 0;
            currentScore = 0;

            OnTimerUpdated?.Invoke(remainingTime);
            OnKillCountUpdated?.Invoke(currentKillCount, stageData.TargetKillCount);
            OnScoreUpdated?.Invoke(currentScore);

            if (spawner != null)
            {
                spawner.Setup(stageData);
                spawner.StartSpawning();
                spawner.OnZombieKilled += HandleZombieKilled;
            }

            ShutterTransitionUI shutter = ShutterTransitionUI.EnsureExists();
            string stageNameToShow = stageData != null ? stageData.StageName : "STAGE 1";

            shutter.PlayStageStartSequence(stageNameToShow, onStartOpening: () =>
            {
                currentState = GameState.Playing;
            });
        }

        private void Update()
        {
            if (currentState != GameState.Playing) return;

            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                if (remainingTime <= 0)
                {
                    remainingTime = 0;
                    TimeUp();
                }
                OnTimerUpdated?.Invoke(remainingTime);
            }

            if (playerHealth != null && playerHealth.IsDead)
            {
                PlayerDied();
            }
        }

        /// <summary>
        /// ゾンビ撃破イベント受領ハンドラ
        /// </summary>
        private void HandleZombieKilled(ZombieController zombie)
        {
            if (currentState != GameState.Playing) return;

            float multiplier = 1.0f;
            int addedScore = Mathf.RoundToInt(zombie.ScoreValue * multiplier);
            currentScore += addedScore;

            currentKillCount++;

            OnScoreUpdated?.Invoke(currentScore);
            OnKillCountUpdated?.Invoke(currentKillCount, stageData.TargetKillCount);

            if (currentKillCount >= stageData.TargetKillCount)
            {
                MissionClear();
            }
        }

        /// <summary>
        /// ミッションクリア確定処理
        /// </summary>
        private void MissionClear()
        {
            currentState = GameState.GameClear;
            StopGameSession();

            StartCoroutine(LoadResultSceneWithDelay());
        }

        /// <summary>
        /// 遅延後のリザルトシーン遷移ルーチン
        /// </summary>
        private System.Collections.IEnumerator LoadResultSceneWithDelay()
        {
            yield return new WaitForSeconds(1.5f);

            GameResultData.Reset();
            GameResultData.IsClear = true;
            GameResultData.KillCount = currentKillCount;
            GameResultData.TargetKillCount = stageData != null ? stageData.TargetKillCount : 0;
            GameResultData.Score = currentScore;
            GameResultData.StageName = stageData != null ? stageData.StageName : "";
            GameResultData.RemainingTime = remainingTime;
            float rawClearTime = stageData != null ? Mathf.Max(0f, stageData.TimeLimit - remainingTime) : 0f;
            GameResultData.ClearTime = Mathf.Round(rawClearTime);

            float clearTime = GameResultData.ClearTime;
            if (stageData != null)
            {
                if (clearTime <= stageData.SRankTime) GameResultData.Rank = "S";
                else if (clearTime <= stageData.ARankTime) GameResultData.Rank = "A";
                else if (clearTime <= stageData.BRankTime) GameResultData.Rank = "B";
                else GameResultData.Rank = "C";
            }
            else
            {
                GameResultData.Rank = "C";
            }

            if (stageData != null)
            {
                bool isNew = SaveDataManager.SaveClearRecord(stageData.StageName, GameResultData.ClearTime, GameResultData.Rank);
                GameResultData.IsNewRecord = isNew;
            }

            ShutterTransitionUI shutter = ShutterTransitionUI.EnsureExists();
            bool isCompleted = false;

            shutter.PlayGameClearSequence(() =>
            {
                Debug.Log("[GameManager] ミッションクリア！リザルトシーンへ遷移します。");
                SceneManager.LoadScene("ResultScene");
                isCompleted = true;
            });

            while (!isCompleted)
            {
                yield return null;
            }
        }

        /// <summary>
        /// プレイヤー死亡時失敗処理
        /// </summary>
        private void PlayerDied()
        {
            currentState = GameState.GameOver;
            StopGameSession();

            StartCoroutine(LoadResultSceneOnGameOver());
        }

        /// <summary>
        /// タイムアップ時失敗処理
        /// </summary>
        private void TimeUp()
        {
            currentState = GameState.GameOver;
            StopGameSession();

            StartCoroutine(LoadResultSceneOnGameOver());
        }

        /// <summary>
        /// ゲームオーバー演出後のリザルトシーン遷移ルーチン
        /// </summary>
        private System.Collections.IEnumerator LoadResultSceneOnGameOver()
        {
            GameUIController uiController = FindFirstObjectByType<GameUIController>();
            if (uiController != null)
            {
                float fadeDuration = gameOverDelay * 0.5f;
                yield return uiController.StartCoroutine(uiController.PlayGameOverSequence(fadeDuration));
                
                yield return new WaitForSeconds(gameOverDelay - fadeDuration);
            }
            else
            {
                yield return new WaitForSeconds(gameOverDelay);
            }

            GameResultData.Reset();
            GameResultData.IsClear = false;
            GameResultData.KillCount = currentKillCount;
            GameResultData.TargetKillCount = stageData != null ? stageData.TargetKillCount : 0;
            GameResultData.StageName = stageData != null ? stageData.StageName : "";
            GameResultData.RemainingTime = remainingTime;
            GameResultData.Rank = "D";

            Debug.Log("[GameManager] ゲームオーバー！リザルトシーンへ遷移します。");
            SceneManager.LoadScene("ResultScene");
        }

        /// <summary>
        /// セッション終了時リスナー解除処理
        /// </summary>
        private void StopGameSession()
        {
            if (spawner != null)
            {
                spawner.OnZombieKilled -= HandleZombieKilled;
                spawner.StopSpawning(false);
            }
        }
    }
}
