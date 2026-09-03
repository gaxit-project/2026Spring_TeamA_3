using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// ガスボンベ（GasTank）のパラメータ設定アセット
    /// </summary>
    [CreateAssetMenu(fileName = "NewGasTankData", menuName = "DeadLab/Gas Tank Data", order = 4)]
    public class GasTankData : ScriptableObject
    {
        [Header("挙動設定")]
        [Tooltip("被弾してから大爆発するまでの飛び回る時間（秒）")]
        [SerializeField] private float explosionTimer = 2.0f;
        [Tooltip("飛び回る際の直進スピード")]
        [SerializeField] private float moveSpeed = 15.0f;
        [Tooltip("回転の加速スピード")]
        [SerializeField] private float rotationSpeed = 10.0f;
        [Tooltip("最大回転スピード")]
        [SerializeField] private float maxRotationSpeed = 50.0f;

        [Header("爆発設定")]
        [Tooltip("爆発が届く半径（m）")]
        [SerializeField] private float explosionRadius = 12.5f;
        [Tooltip("爆発による物理的な吹き飛ばし力")]
        [SerializeField] private float explosionForce = 4000.0f;
        [Tooltip("爆発の中心部で与える最大ダメージ量")]
        [SerializeField] private int maxDamage = 100;

        public float ExplosionTimer => explosionTimer;
        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float MaxRotationSpeed => maxRotationSpeed;
        public float ExplosionRadius => explosionRadius;
        public float ExplosionForce => explosionForce;
        public int MaxDamage => maxDamage;
    }
}
