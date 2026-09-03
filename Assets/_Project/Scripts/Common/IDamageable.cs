namespace DeadLab
{
    /// <summary>
    /// 被ダメージ可能オブジェクト用インターフェース
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// ダメージ適用処理
        /// </summary>
        /// <param name="damageAmount">ダメージ量</param>
        void TakeDamage(int damageAmount);
    }
}
