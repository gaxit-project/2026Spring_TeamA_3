using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// 複数体グループ（ペア）ゾンビの復活・連動管理用クラス
    /// </summary>
    public class ZombieGroupLinker : MonoBehaviour
    {
        [Header("グループ設定")]
        [Tooltip("復活待機時間（秒）")]
        [SerializeField] private float reviveDelay = 7.0f;

        private ZombieController[] groupZombies;
        private Coroutine reviveCoroutine;
        private bool isGroupDead = false;

        public ZombieController[] GroupZombies => groupZombies;

        public event System.Action OnAnyMemberDeath;
        public event System.Action<ZombieGroupLinker> OnGroupDeath;

        private void Awake()
        {
            groupZombies = GetComponentsInChildren<ZombieController>();
        }

        private void Start()
        {
            if (groupZombies == null || groupZombies.Length == 0)
            {
                Debug.LogWarning($"[ZombieGroupLinker] {gameObject.name} にゾンビが割り当てられていません。");
                return;
            }

            foreach (var zombie in groupZombies)
            {
                if (zombie != null)
                {
                    zombie.OnDeath += HandleZombieDeath;
                }
            }
        }

        private void OnDestroy()
        {
            if (groupZombies != null)
            {
                foreach (var zombie in groupZombies)
                {
                    if (zombie != null)
                    {
                        zombie.OnDeath -= HandleZombieDeath;
                    }
                }
            }
        }

        /// <summary>
        /// グループ内ゾンビの撃破イベントハンドラ
        /// </summary>
        private void HandleZombieDeath(ZombieController killedZombie)
        {
            if (isGroupDead) return;

            OnAnyMemberDeath?.Invoke();

            bool allDead = true;
            foreach (var zombie in groupZombies)
            {
                if (zombie != null && !zombie.IsDead)
                {
                    allDead = false;
                    break;
                }
            }

            if (allDead)
            {
                if (reviveCoroutine != null)
                {
                    StopCoroutine(reviveCoroutine);
                    reviveCoroutine = null;
                }
                
                isGroupDead = true;
                Debug.Log($"[ZombieGroupLinker] {gameObject.name} グループ全滅。");
                OnGroupDeath?.Invoke(this);

                Destroy(gameObject, 5.0f);
            }
            else
            {
                if (reviveCoroutine == null)
                {
                    reviveCoroutine = StartCoroutine(ReviveCheckRoutine());
                }
            }
        }

        /// <summary>
        /// 時間経過後のグループ内ゾンビ復活ルーチン
        /// </summary>
        private IEnumerator ReviveCheckRoutine()
        {
            yield return new WaitForSeconds(reviveDelay);

            foreach (var zombie in groupZombies)
            {
                if (zombie != null && zombie.IsDead)
                {
                    zombie.Revive();
                }
            }

            reviveCoroutine = null;
        }
    }
}
