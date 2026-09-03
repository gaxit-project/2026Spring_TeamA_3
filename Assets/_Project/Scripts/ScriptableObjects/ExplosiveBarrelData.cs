using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// 爆発ドラム缶（ExplosiveBarrel）のパラメータ設定アセット
    /// </summary>
    [CreateAssetMenu(fileName = "NewExplosiveBarrelData", menuName = "DeadLab/Explosive Barrel Data", order = 3)]
    public class ExplosiveBarrelData : ScriptableObject
    {
        [Header("ディレイ設定")]
        [Tooltip("被弾してから爆発するまでの最小時間（秒）")]
        [SerializeField] private float minTime = 0.05f;
        [Tooltip("被弾してから爆発するまでの最大時間（秒）")]
        [SerializeField] private float maxTime = 0.25f;

        [Header("爆発設定")]
        [Tooltip("爆発が届く半径（m）")]
        [SerializeField] private float explosionRadius = 12.5f;
        [Tooltip("爆発による物理的な吹き飛ばし力")]
        [SerializeField] private float explosionForce = 4000.0f;
        [Tooltip("爆発の中心部で与える最大ダメージ量")]
        [SerializeField] private int maxDamage = 100;

        public float MinTime => minTime;
        public float MaxTime => maxTime;
        public float ExplosionRadius => explosionRadius;
        public float ExplosionForce => explosionForce;
        public int MaxDamage => maxDamage;
    }
}
