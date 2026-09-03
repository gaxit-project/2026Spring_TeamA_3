using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// ステージ設定に基づく動的ゾンビスポーン管理用クラス
    /// </summary>
    public class ZombieSpawner : MonoBehaviour
    {
        [Header("プレハブ＆スポーン地点設定")]
        [Tooltip("ベースゾンビプレハブ")]
        [SerializeField] private ZombieController zombiePrefab;
        [Tooltip("小型ゾンビプレハブ")]
        [SerializeField] private ZombieController quickZombiePrefab;
        [Tooltip("大型ゾンビプレハブ")]
        [SerializeField] private ZombieController heavyZombiePrefab;
        [Tooltip("スポーン位置一覧")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("グループゾンビ設定")]
        [Tooltip("グループゾンビプレハブ一覧")]
        [SerializeField] private ZombieGroupLinker[] groupZombiePrefabs;
        [Tooltip("グループゾンビスポーン確率（0.0 〜 1.0）")]
        [SerializeField] private float groupZombieSpawnChance = 0.2f;

        private StageData currentStageData;
        private List<ZombieController> activeZombies = new List<ZombieController>();
        private Coroutine spawnCoroutine;
        private bool isSpawning = false;

        public event System.Action<ZombieController> OnZombieKilled;

        /// <summary>
        /// 指定位置・指定数でのゾンビスポーン呼び出し処理
        /// </summary>
        public void CallSpawnZombie(Transform pos, bool spawnQuick, bool spawnHeavy, int n, bool? spawnFalling = null)
        {
            for (int i = 0; i < n; i++)
            {
                SpawnZombie(pos, spawnQuick, spawnHeavy, spawnFalling.GetValueOrDefault(false));
            }
        }

        /// <summary>
        /// ステージデータセット処理
        /// </summary>
        public void Setup(StageData stageData)
        {
            currentStageData = stageData;
        }

        /// <summary>
        /// スポーン処理の開始
        /// </summary>
        public void StartSpawning()
        {
            if (currentStageData == null)
            {
                Debug.LogWarning("[ZombieSpawner] ステージデータ未設定のためスポーンを開始できません。");
                return;
            }

            if (spawnCoroutine == null)
            {
                isSpawning = true;

                int spawnCount = Mathf.Min(currentStageData.InitialSpawnCount, currentStageData.MaxSimultaneousZombies);
                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnZombie();
                }

                spawnCoroutine = StartCoroutine(SpawnLoop());
            }
        }

        /// <summary>
        /// スポーン停止およびアクティブゾンビのクリア処理
        /// </summary>
        public void StopSpawning(bool clearActiveZombies = false)
        {
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }

            if (clearActiveZombies)
            {
                foreach (var zombie in activeZombies)
                {
                    if (zombie != null)
                    {
                        var linker = zombie.GetComponentInParent<ZombieGroupLinker>();
                        if (linker != null)
                        {
                            Destroy(linker.gameObject);
                        }
                        else
                        {
                            Destroy(zombie.gameObject);
                        }
                    }
                }
                activeZombies.Clear();
            }
        }

        /// <summary>
        /// ゾンビ定期生成ループルーチン
        /// </summary>
        private IEnumerator SpawnLoop()
        {
            while (isSpawning)
            {
                yield return new WaitForSeconds(currentStageData.SpawnInterval);

                if (!isSpawning) yield break;

                for (int i = 0; i < currentStageData.SpawnCountPerInterval; i++)
                {
                    if (activeZombies.Count < currentStageData.MaxSimultaneousZombies)
                    {
                        SpawnZombie();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// ランダム位置へのゾンビ生成処理
        /// </summary>
        private void SpawnZombie()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("[ZombieSpawner] スポーン地点が設定されていません。");
                return;
            }

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnZombie(spawnPoint, false, false);
        }

        /// <summary>
        /// 位置指定でのゾンビ生成処理
        /// </summary>
        private void SpawnZombie(Transform spawnPoint, bool spawnQuick, bool spawnHeavy, bool? spawnFalling = null)
        {
            bool spawnGroup = groupZombiePrefabs != null &&
                               groupZombiePrefabs.Length > 0 &&
                               Random.value <= groupZombieSpawnChance;

            if (spawnGroup)
            {
                ZombieGroupLinker groupPrefab = groupZombiePrefabs[Random.Range(0, groupZombiePrefabs.Length)];
                ZombieGroupLinker newGroup = Instantiate(groupPrefab, spawnPoint.position, spawnPoint.rotation);

                foreach (var zombie in newGroup.GroupZombies)
                {
                    if (zombie != null)
                    {
                        zombie.Initialize(
                            currentStageData.Zombie,
                            currentStageData.FlankingChance,
                            currentStageData.QuickChance,
                            currentStageData.QuickSpeedMultiplier,
                            heavyChance: currentStageData.HeavyChance
                        );
                        activeZombies.Add(zombie);
                    }
                }

                newGroup.OnGroupDeath += HandleGroupZombieDeath;
            }
            else
            {
                ZombieController basePrefab = zombiePrefab != null ? zombiePrefab : (currentStageData != null ? currentStageData.ZombiePrefab : null);
                ZombieController qPrefab = quickZombiePrefab != null ? quickZombiePrefab : (currentStageData != null ? currentStageData.QuickZombiePrefab : null);
                ZombieController hPrefab = heavyZombiePrefab != null ? heavyZombiePrefab : (currentStageData != null ? currentStageData.HeavyZombiePrefab : null);

                if (basePrefab == null)
                {
                    Debug.LogWarning("[ZombieSpawner] 出現させるゾンビプレハブが未設定です。");
                    return;
                }

                float rand = Random.value;
                float flankChance = currentStageData != null ? currentStageData.FlankingChance : 0f;
                float quickChance = currentStageData != null ? currentStageData.QuickChance : 0f;
                float heavyChance = currentStageData != null ? currentStageData.HeavyChance : 0f;
                float speedMult = currentStageData != null ? currentStageData.QuickSpeedMultiplier : 2.0f;

                float cumulativeFlank = flankChance;
                float cumulativeQuick = flankChance + quickChance;
                float cumulativeHeavy = flankChance + quickChance + heavyChance;

                ZombieController targetPrefab = basePrefab;
                bool forceQuick = false;
                bool forceFlank = false;
                bool forceHeavy = false;
                bool falling = false;

                if (spawnQuick && !spawnHeavy)
                {
                    forceQuick = true;
                    if (qPrefab != null) targetPrefab = qPrefab;
                }
                else if (spawnHeavy && !spawnQuick)
                {
                    forceHeavy = true;
                    if (hPrefab != null) targetPrefab = hPrefab;
                }
                else if (rand <= cumulativeFlank)
                {
                    forceFlank = true;
                }
                else if (rand <= cumulativeQuick)
                {
                    forceQuick = true;
                    if (qPrefab != null) targetPrefab = qPrefab;
                }
                else if (rand <= cumulativeHeavy)
                {
                    forceHeavy = true;
                    if (hPrefab != null) targetPrefab = hPrefab;
                }

                if(spawnFalling.GetValueOrDefault(false))
                {
                    falling = true;
                }

                Vector3 pos = falling ? spawnPoint.position + Vector3.up : spawnPoint.position;

                float x = Random.Range(-2.0f, 2.0f);
                float z = Random.Range(-2.0f, 2.0f);

                ZombieController newZombie = Instantiate(targetPrefab, pos + new Vector3(x, 0f, z), spawnPoint.rotation);

                newZombie.Initialize(
                    currentStageData != null ? currentStageData.Zombie : null,
                    flankChance,
                    quickChance,
                    speedMult,
                    forceQuick: forceQuick,
                    forceFlank: forceFlank,
                    heavyChance: heavyChance,
                    forceHeavy: forceHeavy,
                    falling: falling
                );

                newZombie.OnDeath += HandleZombieDeath;
                activeZombies.Add(newZombie);
            }
        }

        /// <summary>
        /// 単体ゾンビ撃破時ハンドラ
        /// </summary>
        private void HandleZombieDeath(ZombieController zombie)
        {
            zombie.OnDeath -= HandleZombieDeath;

            if (activeZombies.Contains(zombie))
            {
                activeZombies.Remove(zombie);
            }

            OnZombieKilled?.Invoke(zombie);
            StartCoroutine(DestroyAfterDelay(zombie.gameObject, 5.0f));
        }

        /// <summary>
        /// グループゾンビ全滅時ハンドラ
        /// </summary>
        private void HandleGroupZombieDeath(ZombieGroupLinker groupLinker)
        {
            groupLinker.OnGroupDeath -= HandleGroupZombieDeath;

            foreach (var zombie in groupLinker.GroupZombies)
            {
                if (zombie != null)
                {
                    if (activeZombies.Contains(zombie))
                    {
                        activeZombies.Remove(zombie);
                    }

                    OnZombieKilled?.Invoke(zombie);
                }
            }
        }

        /// <summary>
        /// 一定時間後のオブジェクト破棄ルーチン
        /// </summary>
        private IEnumerator DestroyAfterDelay(GameObject target, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (target != null)
            {
                Destroy(target);
            }
        }
    }
}
