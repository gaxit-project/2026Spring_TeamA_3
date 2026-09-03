using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// ゾンビの各パラメータ設定アセット
    /// </summary>
    [CreateAssetMenu(fileName = "NewZombieData", menuName = "DeadLab/Zombie Data", order = 1)]
    public class ZombieData : ScriptableObject
    {
        [Header("通常ゾンビ設定")]
        [Tooltip("通常ゾンビの最大体力")]
        [SerializeField] private int maxHealth = 50;
        [Tooltip("通常ゾンビの移動速度")]
        [SerializeField] private float walkSpeed = 3.0f;
        [Tooltip("プレイヤーに与える攻撃ダメージ量")]
        [SerializeField] private int attackDamage = 10;
        [Tooltip("通常ゾンビの攻撃頻度・間隔（秒）")]
        [SerializeField] private float attackRate = 1.5f;

        [Header("小型ゾンビ設定")]
        [Tooltip("小型ゾンビの最大体力")]
        [SerializeField] private int smallMaxHealth = 30;
        [Tooltip("小型ゾンビの移動速度")]
        [SerializeField] private float smallWalkSpeed = 6.0f;
        [Tooltip("小型ゾンビの攻撃ダメージ量")]
        [SerializeField] private int smallAttackDamage = 5;
        [Tooltip("小型ゾンビの攻撃頻度・間隔（秒）")]
        [SerializeField] private float smallAttackRate = 1.0f;
        [Tooltip("小型ゾンビのスケール倍率")]
        [SerializeField] private float smallScaleMultiplier = 0.75f;

        [Header("大型ゾンビ設定")]
        [Tooltip("大型ゾンビの最大体力")]
        [SerializeField] private int heavyMaxHealth = 150;
        [Tooltip("大型ゾンビの移動速度")]
        [SerializeField] private float heavyWalkSpeed = 2.0f;
        [Tooltip("大型ゾンビの攻撃ダメージ量")]
        [SerializeField] private int heavyAttackDamage = 20;
        [Tooltip("大型ゾンビの攻撃頻度・間隔（秒）")]
        [SerializeField] private float heavyAttackRate = 2.5f;
        [Tooltip("大型ゾンビのスケール倍率")]
        [SerializeField] private float heavyScaleMultiplier = 1.5f;

        [Header("共通設定")]
        [Tooltip("ゾンビの声")]
        [SerializeField] private AudioClip zombieVoice;

        [Header("共通AI設定")]
        [Tooltip("プレイヤーを感知する範囲（m）")]
        [SerializeField] private float detectRange = 25.0f;
        [Tooltip("攻撃を開始する距離（m）")]
        [SerializeField] private float attackRange = 1.6f;

        [Header("ダメージリアクション設定")]
        [Tooltip("被弾したときのヒットスタン時間（秒）")]
        [SerializeField] private float hitStunDuration = 0.5f;
        [Tooltip("復活（起き上がり）にかかる時間（秒）")]
        [SerializeField] private float reviveDuration = 2.0f;

        public int MaxHealth => maxHealth;
        public float WalkSpeed => walkSpeed;
        public int AttackDamage => attackDamage;
        public float AttackRate => attackRate;

        public int SmallMaxHealth => smallMaxHealth;
        public float SmallWalkSpeed => smallWalkSpeed;
        public int SmallAttackDamage => smallAttackDamage;
        public float SmallAttackRate => smallAttackRate;
        public float SmallScaleMultiplier => smallScaleMultiplier;

        public int HeavyMaxHealth => heavyMaxHealth;
        public float HeavyWalkSpeed => heavyWalkSpeed;
        public int HeavyAttackDamage => heavyAttackDamage;
        public float HeavyAttackRate => heavyAttackRate;
        public float HeavyScaleMultiplier => heavyScaleMultiplier;

        public AudioClip ZombieVoice => zombieVoice;

        public float DetectRange => detectRange;
        public float AttackRange => attackRange;

        public float HitStunDuration => hitStunDuration;
        public float ReviveDuration => reviveDuration;
    }
}
