using UnityEngine;

namespace DeadLab
{
    [System.Serializable]
    public class SpawnSetting
    {
        [Header("生成位置")]
        [SerializeField] public Transform[] spawnPositions;

        [Header("生成数")]
        [SerializeField] public int[] spawnCounts = { 0 };

        [Header("小型ゾンビ化フラグ")]
        [SerializeField] public bool spawnQuick = false;

        [Header("大型ゾンビ化フラグ")]
        [SerializeField] public bool spawnHeavy = false;

        [Header("必要討伐数")]
        [SerializeField] public int killCount = 0;

        [System.NonSerialized] public bool isSpawned = false;
    }

    /// <summary>
    /// ステージのウェーブ生成進行管理用クラス
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("関連コンポーネント")]
        [SerializeField] private ZombieSpawner spawner;

        [Header("生成設定一覧")]
        [SerializeField] private SpawnSetting[] spawnSettings;

        private int zombieCount = 0;

        private void Start()
        {
            if (spawner == null)
            {
                spawner = FindFirstObjectByType<ZombieSpawner>();
                if (spawner == null)
                {
                    Debug.LogError("[WaveManager] ZombieSpawner がアタッチされておらず、シーン上にも見つかりません。", this);
                    return;
                }
            }

            spawner.OnZombieKilled += AddZombieCount;

            if (spawnSettings == null) return;

            foreach (SpawnSetting s in spawnSettings)
            {
                if (s.spawnPositions == null || s.spawnCounts == null || s.spawnPositions.Length != s.spawnCounts.Length)
                {
                    Debug.LogError("[WaveManager] spawnPositions と spawnCounts の要素数が一致しません！", this);
                    s.isSpawned = true;
                }
                else if (s.killCount == 0)
                {
                    ExecuteInitialSpawn(s);
                }
            }
        }

        private void AddZombieCount(ZombieController zombie)
        {
            zombieCount++;
            if (spawnSettings == null) return;

            foreach (SpawnSetting s in spawnSettings)
            {
                if (s.isSpawned) continue;

                if (zombieCount >= s.killCount)
                {
                    s.isSpawned = true;
                    for (int i = 0; i < s.spawnPositions.Length; i++)
                    {
                        spawner.CallSpawnZombie(s.spawnPositions[i], s.spawnQuick, s.spawnHeavy, s.spawnCounts[i], true);
                    }
                }
            }
        }

        /// <summary>
        /// 初期スポーンの即時実行処理
        /// </summary>
        private void ExecuteInitialSpawn(SpawnSetting s)
        {
            s.isSpawned = true;
            for (int i = 0; i < s.spawnPositions.Length; i++)
            {
                spawner.CallSpawnZombie(s.spawnPositions[i], s.spawnQuick, s.spawnHeavy, s.spawnCounts[i]);
            }
        }

        private void OnDestroy()
        {
            if (spawner != null)
            {
                spawner.OnZombieKilled -= AddZombieCount;
            }
        }
    }
}
