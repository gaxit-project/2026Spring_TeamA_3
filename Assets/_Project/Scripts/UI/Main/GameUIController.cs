using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DeadLab
{
    /// <summary>
    /// ゲームプレイ中HUD（HP、時間、討伐数、演出パネル）の表示制御クラス
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [Header("HUD UI Elements")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI killCountText;
        [SerializeField] private Image damageOverlay;

        [Header("Game Over UI Elements")]
        [SerializeField] private Image gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;

        [Header("Game Clear UI Elements")]
        [SerializeField] private Image gameClearPanel;
        [SerializeField] private TextMeshProUGUI gameClearText;

        private GameManager gameManager;
        private PlayerHealth playerHealth;
        private Coroutine damageFlashCoroutine;
        private float lastHp = -1f;

        private void Start()
        {
            // GameManagerおよびPlayerHealthの参照取得
            gameManager = GameManager.Instance ?? FindFirstObjectByType<GameManager>();
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }

            // 初期表示設定
            if (timerText != null) timerText.text = "残り時間: --:--";
            if (killCountText != null) killCountText.text = "目標: 0 / 0";
            if (damageOverlay != null)
            {
                damageOverlay.color = new Color(1f, 0f, 0f, 0f);
                damageOverlay.gameObject.SetActive(false);
            }
            if (gameOverPanel != null) gameOverPanel.gameObject.SetActive(false);
            if (gameOverText != null) gameOverText.gameObject.SetActive(false);
            if (gameClearPanel != null) gameClearPanel.gameObject.SetActive(false);
            if (gameClearText != null) gameClearText.gameObject.SetActive(false);

            // イベントの購読
            if (gameManager != null)
            {
                gameManager.OnTimerUpdated += UpdateTimerUI;
                gameManager.OnKillCountUpdated += UpdateKillCountUI;
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthUI;
                // 初期値の反映
                UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            // イベント購読の解除
            if (gameManager != null)
            {
                gameManager.OnTimerUpdated -= UpdateTimerUI;
                gameManager.OnKillCountUpdated -= UpdateKillCountUI;
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealthUI;
            }
        }

        #region UI更新メソッド

        private void UpdateHealthUI(int current, int max)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = max;
                hpSlider.value = current;
            }
            if (hpText != null)
            {
                hpText.text = $"HP: {current} / {max}";
            }

            // ダメージ検知時の演出開始（初期化時を除くHP減少時のみ）
            if (lastHp != -1f && current < lastHp && current > 0)
            {
                TriggerDamageFlash();
            }

            lastHp = current;
        }

        private void UpdateTimerUI(float time)
        {
            if (timerText != null)
            {
                // 分:秒 フォーマット表示
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                timerText.text = $"残り時間: {minutes}:{seconds:00}";

                // 残り10秒以下での赤字警告演出
                timerText.color = time <= 10f ? Color.red : Color.white;
            }
        }

        private void UpdateKillCountUI(int current, int target)
        {
            if (killCountText != null)
            {
                killCountText.text = $"目標: {current} / {target}";
            }
        }

        #endregion

        #region ダメージ演出

        /// <summary>
        /// 被ダメージ演出（画面赤フラッシュ）の開始
        /// </summary>
        private void TriggerDamageFlash()
        {
            if (damageOverlay == null) return;

            if (damageFlashCoroutine != null)
            {
                StopCoroutine(damageFlashCoroutine);
            }
            damageFlashCoroutine = StartCoroutine(DamageFlashSequence());
        }

        /// <summary>
        /// 被ダメージ時の赤画面フェードアウト処理
        /// </summary>
        private System.Collections.IEnumerator DamageFlashSequence()
        {
            if (damageOverlay == null) yield break;

            damageOverlay.gameObject.SetActive(true);

            float elapsed = 0f;
            float duration = 0.5f;

            damageOverlay.color = new Color(0.8f, 0f, 0f, 0.65f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float ratio = elapsed / duration;
                float currentAlpha = Mathf.Lerp(0.65f, 0f, ratio);
                damageOverlay.color = new Color(0.8f, 0f, 0f, currentAlpha);
                yield return null;
            }

            damageOverlay.color = new Color(0.8f, 0f, 0f, 0f);
            damageOverlay.gameObject.SetActive(false);
            damageFlashCoroutine = null;
        }

        /// <summary>
        /// ゲームオーバー時のフェードイン演出開始
        /// </summary>
        public System.Collections.IEnumerator PlayGameOverSequence(float duration)
        {
            if (gameOverPanel == null || gameOverText == null)
            {
                Debug.LogError("[GameUIController] gameOverPanel または gameOverText がアタッチされていません。", this);
                yield break;
            }

            gameOverPanel.gameObject.SetActive(true);
            gameOverText.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                gameOverPanel.color = new Color(0f, 0f, 0f, alpha);
                gameOverText.color = new Color(1f, 0f, 0f, alpha);
                yield return null;
            }

            gameOverPanel.color = new Color(0f, 0f, 0f, 1f);
            gameOverText.color = new Color(1f, 0f, 0f, 1f);
        }

        /// <summary>
        /// ゲームクリア時のホワイトアウトフェードイン演出開始
        /// </summary>
        public System.Collections.IEnumerator PlayGameClearSequence(float duration)
        {
            if (gameClearPanel == null || gameClearText == null)
            {
                Debug.LogError("[GameUIController] gameClearPanel または gameClearText がアタッチされていません。", this);
                yield break;
            }

            gameClearPanel.gameObject.SetActive(true);
            gameClearText.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                gameClearPanel.color = new Color(1f, 1f, 1f, alpha);
                gameClearText.color = new Color(1f, 0.85f, 0f, alpha);
                yield return null;
            }

            gameClearPanel.color = new Color(1f, 1f, 1f, 1f);
            gameClearText.color = new Color(1f, 0.85f, 0f, 1f);
        }

        #endregion
    }
}
