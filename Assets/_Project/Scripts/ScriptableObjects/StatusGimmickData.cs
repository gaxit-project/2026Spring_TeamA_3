using UnityEngine;

namespace DeadLab
{
    public enum GimmickType
    {
        Electric, // 感電
        Freeze    // 凍結
    }

    /// <summary>
    /// ステータス効果ギミック（感電・凍結など）のパラメータ設定アセット
    /// </summary>
    [CreateAssetMenu(fileName = "NewStatusGimmickData", menuName = "DeadLab/Status Gimmick Data", order = 5)]
    public class StatusGimmickData : ScriptableObject
    {
        [Header("ギミック設定")]
        [Tooltip("ギミックの属性（感電 / 凍結）")]
        [SerializeField] private GimmickType gimmickType = GimmickType.Electric;
        
        [Tooltip("被弾してから効果が発生するまでの最小時間（秒）")]
        [SerializeField] private float triggerDelayMin = 0.05f;
        [Tooltip("被弾してから効果が発生するまでの最大時間（秒）")]
        [SerializeField] private float triggerDelayMax = 0.25f;

        [Tooltip("効果が届く半径（m）")]
        [SerializeField] private float effectRadius = 8.0f;
        [Tooltip("効果の持続時間（秒）")]
        [SerializeField] private float effectDuration = 4.0f;

        [Header("感電連鎖設定")]
        [Tooltip("感電が連鎖する最大回数（バッテリー専用）")]
        [SerializeField] private int maxChainCount = 3;
        [Tooltip("感電したゾンビから次のゾンビに連鎖する検知半径（m）")]
        [SerializeField] private float chainRadius = 5.0f;
        [Tooltip("次のゾンビへ連鎖するまでの遅延時間（秒）")]
        [SerializeField] private float chainDelay = 0.2f;

        public GimmickType GimmickType => gimmickType;
        public float TriggerDelayMin => triggerDelayMin;
        public float TriggerDelayMax => triggerDelayMax;
        public float EffectRadius => effectRadius;
        public float EffectDuration => effectDuration;
        public int MaxChainCount => maxChainCount;
        public float ChainRadius => chainRadius;
        public float ChainDelay => chainDelay;
    }
}
