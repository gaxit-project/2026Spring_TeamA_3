using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DeadLab
{
    /// <summary>
    /// ステージ詳細情報（目標・画像・自己ベスト）表示用クラス
    /// </summary>
    public class StageDetailView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image stageImage;
        [SerializeField] private TextMeshProUGUI stageNameText;
        [SerializeField] private TextMeshProUGUI timeLimitText;
        [SerializeField] private TextMeshProUGUI targetKillText;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private TextMeshProUGUI bestRankText;

        [Header("Rank Criteria UI (Optional)")]
        [Tooltip("ランク目標一覧（S, A, B, C）まとめ表示テキスト")]
        [SerializeField] private TextMeshProUGUI rankCriteriaSummaryText;

        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;

        public Button StartButton => startButton;
        public Button BackButton => backButton;

        /// <summary>
        /// ステージ詳細情報の表示反映処理
        /// </summary>
        public void Setup(StageData data, bool isUnlocked = true, System.Action onStartClick = null, System.Action onBackClick = null)
        {
            if (data == null) return;

            if (stageImage != null)
            {
                stageImage.sprite = data.StageImage;
                stageImage.gameObject.SetActive(data.StageImage != null);
            }

            if (stageNameText != null)
            {
                stageNameText.text = data.StageName;
            }

            if (timeLimitText != null)
            {
                int minutes = Mathf.FloorToInt(data.TimeLimit / 60f);
                int seconds = Mathf.FloorToInt(data.TimeLimit % 60f);
                timeLimitText.text = $"制限時間　 {minutes}:{seconds:00}";
            }

            if (targetKillText != null)
            {
                targetKillText.text = $"目標撃破数 {data.TargetKillCount}体";
            }

            // ランク目標タイム表示
            if (rankCriteriaSummaryText != null)
            {
                rankCriteriaSummaryText.text = $"S: {FormatTimeStr(data.SRankTime)}\nA: {FormatTimeStr(data.ARankTime)}\nB: {FormatTimeStr(data.BRankTime)}\nC: クリア";
            }

            // 自己ベストデータ読み込みおよび反映
            StageRecord record = SaveDataManager.GetRecord(data.StageName);

            if (bestTimeText != null)
            {
                if (record != null && record.isCleared && record.bestClearTime < 9000f)
                {
                    int totalSeconds = Mathf.RoundToInt(record.bestClearTime);
                    int bMin = totalSeconds / 60;
                    int bSec = totalSeconds % 60;
                    bestTimeText.text = $"{bMin}:{bSec:00}";
                }
                else
                {
                    bestTimeText.text = "--:--";
                }
            }

            if (bestRankText != null)
            {
                if (record != null && record.isCleared && !string.IsNullOrEmpty(record.bestRank))
                {
                    bestRankText.text = $"{record.bestRank}";

                    if (record.bestRank == "S") bestRankText.color = Color.yellow;
                    else if (record.bestRank == "A") bestRankText.color = Color.cyan;
                    else if (record.bestRank == "B") bestRankText.color = Color.green;
                    else if (record.bestRank == "C") bestRankText.color = Color.white;
                    else bestRankText.color = Color.white;
                }
                else
                {
                    bestRankText.text = "-";
                    bestRankText.color = Color.white;
                }
            }

            if (startButton != null)
            {
                startButton.interactable = isUnlocked;
                startButton.onClick.RemoveAllListeners();
                var bse = startButton.GetComponent<ButtonSE>();
                if (bse != null) startButton.onClick.AddListener(bse.PlaySE);
                if (isUnlocked && onStartClick != null) startButton.onClick.AddListener(() => onStartClick());
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                var bse = backButton.GetComponent<ButtonSE>();
                if (bse != null) backButton.onClick.AddListener(bse.PlaySE);
                if (onBackClick != null) backButton.onClick.AddListener(() => onBackClick());
            }
        }

        private string FormatTimeStr(float totalSeconds)
        {
            int min = Mathf.FloorToInt(totalSeconds / 60f);
            int sec = Mathf.FloorToInt(totalSeconds % 60f);
            return $"{min}:{sec:00}";
        }
    }
}
