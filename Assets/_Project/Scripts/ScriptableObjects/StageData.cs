using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// ステージのルールや出現エネミーのパラメータ設定アセット
    /// </summary>
    [CreateAssetMenu(fileName = "NewStageData", menuName = "DeadLab/Stage Data", order = 2)]
    public class StageData : ScriptableObject
    {
        [Header("ステージ基本設定")]
        [Tooltip("表示用ステージ名")]
        [SerializeField] private string stageName = "Stage 1";
        [Tooltip("ロード対象シーン名")]
        [SerializeField] private string sceneName = "Scene 1";
        [Tooltip("制限時間（秒）")]
        [SerializeField] private float timeLimit = 60.0f;
        [Tooltip("クリア目標討伐数")]
        [SerializeField] private int targetKillCount = 10;
        [Tooltip("ステージ表示用画像")]
        [SerializeField] private Sprite stageImage;

        [Header("ランク評価タイム基準（クリア所要時間・秒）")]
        [Tooltip("Sランク獲得に必要なクリアタイムの上限（秒）")]
        [SerializeField] private float sRankTime = 30.0f;
        [Tooltip("Aランク獲得に必要なクリアタイムの上限（秒）")]
        [SerializeField] private float aRankTime = 45.0f;
        [Tooltip("Bランク獲得に必要なクリアタイムの上限（秒）")]
        [SerializeField] private float bRankTime = 60.0f;

        [Header("スポーン設定")]
        [Tooltip("このステージに出現するゾンビのパラメータデータ")]
        [SerializeField] private ZombieData zombieData;
        [Tooltip("通常ゾンビのプレハブ")]
        [SerializeField] private ZombieController zombiePrefab;
        [Tooltip("小型ゾンビのプレハブ")]
        [SerializeField] private ZombieController quickZombiePrefab;
        [Tooltip("大型ゾンビのプレハブ")]
        [SerializeField] private ZombieController heavyZombiePrefab;
        [Tooltip("ゾンビの出現間隔（秒）")]
        [SerializeField] private float spawnInterval = 3.0f;
        [Tooltip("同時に存在できるゾンビの最大数")]
        [SerializeField] private int maxSimultaneousZombies = 20;
        [Tooltip("ゲーム開始時に即座に出現させるゾンビの数")]
        [SerializeField] private int initialSpawnCount = 10;
        [Tooltip("1回の出現間隔ごとに追加するゾンビの数")]
        [SerializeField] private int spawnCountPerInterval = 1;

        [Header("ゾンビ出現比率設定")]
        [Tooltip("回り込みゾンビが出現する確率（0.0 〜 1.0）")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float flankingChance = 0.2f;

        [Tooltip("小型ゾンビが出現する確率（0.0 〜 1.0）")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float quickChance = 0.1f;

        [Tooltip("大型ゾンビが出現する確率（0.0 〜 1.0）")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float heavyChance = 0.1f;

        [Tooltip("小型ゾンビの移動速度倍率")]
        [SerializeField] private float quickSpeedMultiplier = 2.0f;

        public string StageName => stageName;
        public string SceneName => sceneName;
        public float TimeLimit => timeLimit;
        public int TargetKillCount => targetKillCount;
        public Sprite StageImage => stageImage;
        public float SRankTime => sRankTime;
        public float ARankTime => aRankTime;
        public float BRankTime => bRankTime;
        public ZombieData Zombie => zombieData;
        public ZombieController ZombiePrefab => zombiePrefab;
        public ZombieController QuickZombiePrefab => quickZombiePrefab;
        public ZombieController HeavyZombiePrefab => heavyZombiePrefab;
        public float SpawnInterval => spawnInterval;
        public int MaxSimultaneousZombies => maxSimultaneousZombies;
        public int InitialSpawnCount => initialSpawnCount;
        public int SpawnCountPerInterval => spawnCountPerInterval;
        public float FlankingChance => flankingChance;
        public float QuickChance => quickChance;
        public float HeavyChance => heavyChance;
        public float QuickSpeedMultiplier => quickSpeedMultiplier;
    }
}
