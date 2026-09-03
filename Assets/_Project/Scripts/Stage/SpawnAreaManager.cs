using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// エリア進入時のゾンビスポーン起動制御用クラス
    /// </summary>
    public class SpawnAreaManager : MonoBehaviour
    {
        [Header("関連コンポーネント")]
        [SerializeField] private ZombieSpawner spawner;

        [Header("生成設定")]
        [Tooltip("生成位置一覧")]
        [SerializeField] private Transform[] spawnPositions;
        [Tooltip("生成数一覧")]
        [SerializeField] private int[] spawnCounts = { 5 };

        [Header("ゾンビ種類指定")]
        [Tooltip("小型ゾンビ固定化フラグ")]
        [SerializeField] private bool spawnQuick = false;
        [Tooltip("大型ゾンビ固定化フラグ")]
        [SerializeField] private bool spawnHeavy = false;

        private bool isSpawned = false;

        private void Start()
        {
            if (spawner == null)
            {
                spawner = FindFirstObjectByType<ZombieSpawner>();
            }

            if (spawnPositions == null || spawnCounts == null || spawnPositions.Length != spawnCounts.Length)
            {
                Debug.LogError("[SpawnAreaManager] spawnPositions と spawnCounts の要素数が一致しません！", this);
                isSpawned = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isSpawned) return;

            if (other.gameObject.CompareTag("Player"))
            {
                if (spawner == null)
                {
                    spawner = FindFirstObjectByType<ZombieSpawner>();
                }

                if (spawner != null && spawnPositions != null)
                {
                    isSpawned = true;
                    for (int i = 0; i < spawnPositions.Length; i++)
                    {
                        spawner.CallSpawnZombie(spawnPositions[i], spawnQuick, spawnHeavy, spawnCounts[i]);
                    }
                }
                else
                {
                    Debug.LogError("[SpawnAreaManager] ZombieSpawner が存在しないためゾンビを生成できませんでした。", this);
                }
            }
        }

        /// <summary>
        /// 選択時のギズモ（判定エリア）表示
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Collider col = GetComponent<Collider>();

            if (col != null)
            {
                Bounds bounds = col.bounds;
                Gizmos.color = new Color32(178, 178, 178, 127);
                Gizmos.DrawCube(bounds.center, bounds.size);
            }
        }
    }
}
