using System.Collections;
using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// 被弾時に周囲へステータス異常（感電・凍結）を拡散させる共通ギミック用クラス
    /// </summary>
    public class GimmickObject : MonoBehaviour, IDamageable
    {
        [Header("データ設定")]
        [Tooltip("パラメータ設定用ScriptableObject")]
        [SerializeField] private StatusGimmickData gimmickData;

        [Header("エフェクトとプレハブ")]
        [Tooltip("起爆時にギミック位置へ生成するVFX")]
        [SerializeField] private GameObject explosionVFXPrefab;
        [Tooltip("効果対象ゾンビへ追従表示する状態異常VFX")]
        [SerializeField] private GameObject zombieAttachedVFXPrefab;
        [Tooltip("破壊後の残骸モデルプレハブ")]
        [SerializeField] private GameObject destroyedPrefab;

        private bool isTriggered = false;

        /// <summary>
        /// 弾丸・爆発被弾時のダメージ適用処理
        /// </summary>
        /// <param name="damageAmount">受けるダメージ量</param>
        public void TakeDamage(int damageAmount)
        {
            if (isTriggered || gimmickData == null) return;
            isTriggered = true;
            StartCoroutine(ActivateRoutine());
        }

        /// <summary>
        /// ランダム遅延後のギミック効果周囲発動ルーチン
        /// </summary>
        private IEnumerator ActivateRoutine()
        {
            float delay = Random.Range(gimmickData.TriggerDelayMin, gimmickData.TriggerDelayMax);
            yield return new WaitForSeconds(delay);

            if (destroyedPrefab != null)
            {
                Instantiate(destroyedPrefab, transform.position, transform.rotation);
            }

            if (explosionVFXPrefab != null)
            {
                GameObject explositonVFX = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
                AdjustExplosionAudio(explositonVFX);
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, gimmickData.EffectRadius);
            foreach (var hit in colliders)
            {
                var zombie = hit.GetComponent<ZombieController>();
                if (zombie == null)
                {
                    zombie = hit.GetComponentInParent<ZombieController>();
                }

                if (zombie != null && !zombie.IsDead)
                {
                    if (gimmickData.GimmickType == GimmickType.Electric)
                    {
                        zombie.ApplyElectricShock(
                            gimmickData.EffectDuration, 
                            zombieAttachedVFXPrefab,
                            gimmickData.MaxChainCount,
                            gimmickData.ChainRadius,
                            gimmickData.ChainDelay
                        );
                    }
                    else if (gimmickData.GimmickType == GimmickType.Freeze)
                    {
                        zombie.ApplyFreezing(gimmickData.EffectDuration, zombieAttachedVFXPrefab);
                    }
                }
            }

            Destroy(gameObject);
        }

        private void AdjustExplosionAudio(GameObject instance)
        {
            if (instance == null) return;

            AudioSource audioSource = instance.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = instance.GetComponentInChildren<AudioSource>();
            }

            if (audioSource != null)
            {
                audioSource.maxDistance = Mathf.Max(audioSource.maxDistance, gimmickData.EffectRadius * 1.5f);
                audioSource.minDistance = Mathf.Max(audioSource.minDistance, gimmickData.EffectRadius * 0.3f);
                audioSource.rolloffMode = AudioRolloffMode.Linear;
            }
        }

        /// <summary>
        /// 選択時のギズモ（効果適用範囲）表示
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (gimmickData == null) return;
            Gizmos.color = gimmickData.GimmickType == GimmickType.Electric ? Color.yellow : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, gimmickData.EffectRadius);
        }
    }
}
