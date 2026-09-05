using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DeadLab
{
    /// <summary>
    /// ステージ選択画面カード要素の表示および入力制御用クラス
    /// </summary>
    public class StageCardView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button cardButton;
        [SerializeField] private TextMeshProUGUI stageNameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private GameObject lockPanel;

        public Button CardButton => cardButton;
        public StageData StageData { get; private set; }

        /// <summary>
        /// ステージデータのUI反映およびクリックアクション登録
        /// </summary>
        public void Setup(StageData data, bool isUnlocked, System.Action<StageData> onClickAction)
        {
            if (data == null) return;

            StageData = data;

            if (stageNameText != null) stageNameText.text = data.StageName;
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(data.TimeLimit / 60f);
                int seconds = Mathf.FloorToInt(data.TimeLimit % 60f);
                timeText.text = $"制限時間\n{minutes}:{seconds:00}";
            }
            if (targetText != null) targetText.text = $"目標撃破数\n{data.TargetKillCount}体";

            if (lockPanel != null)
            {
                lockPanel.SetActive(!isUnlocked);
            }

            if (cardButton != null)
            {
                cardButton.interactable = isUnlocked;
                cardButton.onClick.RemoveAllListeners();
                if (isUnlocked)
                {
                    var bse = cardButton.GetComponent<ButtonSE>();
                    if (bse != null)
                    {
                        cardButton.onClick.AddListener(bse.PlaySE);
                    }
                    cardButton.onClick.AddListener(() => onClickAction?.Invoke(data));
                }
            }
        }
    }
}
