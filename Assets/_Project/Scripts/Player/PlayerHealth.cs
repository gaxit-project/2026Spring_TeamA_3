using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// プレイヤーの体力管理および被ダメージ処理用クラス
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("ScriptableObject Settings")]
        [Tooltip("プレイヤーのパラメータデータ")]
        [SerializeField] private PlayerData playerData;

        private int maxHealth;
        private int currentHealth;
        private bool isDead;

        /// <summary>
        /// 体力変更通知イベント (現在値, 最大値)
        /// </summary>
        public event System.Action<int, int> OnHealthChanged;

        /// <summary>
        /// 現在の体力値の取得
        /// </summary>
        public int CurrentHealth => currentHealth;

        /// <summary>
        /// 最大体力値の取得
        /// </summary>
        public int MaxHealth => maxHealth;

        /// <summary>
        /// 死亡フラグの取得
        /// </summary>
        public bool IsDead => isDead;

        private void Start()
        {
            if (playerData == null)
            {
                Debug.LogError("[PlayerHealth] PlayerData がアタッチされていません！インスペクターで設定してください。", this);
                return;
            }

            maxHealth = playerData.maxHealth;
            currentHealth = maxHealth;
            isDead = false;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// ダメージ適用処理（IDamageableインターフェース実装）
        /// </summary>
        public void TakeDamage(int damageAmount)
        {
            if (isDead) return;

            currentHealth -= damageAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            Debug.Log($"[PlayerHealth] ダメージを受けました: {damageAmount} | 残りHP: {currentHealth}");

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        private void Die()
        {
            isDead = true;
            Debug.LogWarning("[PlayerHealth] プレイヤーが死亡しました！");
        }
    }
}
