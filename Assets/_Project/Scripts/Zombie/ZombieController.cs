using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace DeadLab
{
    /// <summary>
    /// ゾンビAI挙動（追跡・攻撃・被弾のけぞり・状態異常・死亡）制御用クラス
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(CapsuleCollider))]
    public class ZombieController : MonoBehaviour, IDamageable
    {
        private enum ZombieState
        {
            Idle,       // 待機
            Chase,      // 追跡
            Attack,     // 攻撃
            Damaged,    // 被ダメージ
            Dead,       // 死亡
            Reviving,   // 復活中
            Stunned,    // 感電スタン
            Frozen,      // 凍結
            Falling     // 落下中
        }

        [Header("HP Settings")]
        [Tooltip("最大体力")]
        [SerializeField] private int maxHealth = 50;

        [Header("AI & Movement Settings")]
        [Tooltip("プレイヤー感知距離")]
        [SerializeField] private float detectRange = 25.0f;
        [Tooltip("攻撃開始距離")]
        [SerializeField] private float attackRange = 1.6f;
        [Tooltip("通常移動速度")]
        [SerializeField] private float walkSpeed = 3.0f;

        [Header("Attack Settings")]
        [Tooltip("与ダメージ量")]
        [SerializeField] private int attackDamage = 10;
        [Tooltip("攻撃頻度（秒）")]
        [SerializeField] private float attackRate = 1.5f;

        [Header("Damage Reaction Settings")]
        [Tooltip("被弾時ヒットスタン時間（秒）")]
        [SerializeField] private float hitStunDuration = 0.5f;

        [Header("Revive Settings")]
        [Tooltip("復活（起き上がり）所要時間（秒）")]
        [SerializeField] private float reviveDuration = 0.5f;

        [Header("ScriptableObject Settings")]
        [Tooltip("ゾンビパラメータデータアセット")]
        [SerializeField] private ZombieData zombieData;

        private int currentHealth;
        private ZombieState currentState = ZombieState.Idle;
        private Transform playerTarget;
        private PlayerHealth playerHealth;

        private NavMeshAgent agent;
        private Animator animator;
        private CapsuleCollider capsuleCollider;
        private Rigidbody rb;
        private AudioSource audioSource;
        private AudioClip zombieVoice;

        private float lastAttackTime;
        private float hitStunTimer;
        private float reviveTimer;
        
        private float stunTimer;
        private float freezeTimer;
        private GameObject activeStatusVFX;

        private bool isFlanking = false;
        private Vector3 flankWaypoint = Vector3.zero;
        private bool hasReachedWaypoint = false;

        private Vector3 originalScale;
        private bool isInitialized = false;

        private Renderer[] childRenderers;
        private Color[] originalColors;

        private static readonly int HashSpeed = Animator.StringToHash("Speed");
        private static readonly int HashAttackTrigger = Animator.StringToHash("Attack");
        private static readonly int HashHitTrigger = Animator.StringToHash("Hit");
        private static readonly int HashDieTrigger = Animator.StringToHash("Die");

        /// <summary>
        /// スコア加算値の取得
        /// </summary>
        public int ScoreValue => 100;

        /// <summary>
        /// 死亡状態フラグの取得
        /// </summary>
        public bool IsDead => currentState == ZombieState.Dead;

        public event System.Action<ZombieController> OnDeath;

        public void Initialize(ZombieData data, float flankChance = 0f, float quickChance = 0f, float speedMult = 2.0f, bool? forceQuick = null, bool? forceFlank = null, float heavyChance = 0f, bool? forceHeavy = null, bool? falling = null)
        {
            zombieData = data;
            ApplyZombieData();
            
            if (originalScale == Vector3.zero)
            {
                originalScale = transform.localScale != Vector3.zero ? transform.localScale : Vector3.one;
            }
            transform.localScale = originalScale;

            bool isFlank = false;
            bool isQuick = false;
            bool isHeavy = false;
            bool isFalling = false;

            if (forceQuick.HasValue || forceFlank.HasValue || forceHeavy.HasValue || falling.HasValue)
            {
                isQuick = forceQuick.GetValueOrDefault(false);
                isFlank = forceFlank.GetValueOrDefault(false);
                isHeavy = forceHeavy.GetValueOrDefault(false);
                isFalling = falling.GetValueOrDefault(false);
            }
            else
            {
                float rand = Random.value;
                float cumulativeFlank = flankChance;
                float cumulativeQuick = flankChance + quickChance;
                float cumulativeHeavy = flankChance + quickChance + heavyChance;

                if (rand <= cumulativeFlank)
                {
                    isFlank = true;
                }
                else if (rand <= cumulativeQuick)
                {
                    isQuick = true;
                }
                else if (rand <= cumulativeHeavy)
                {
                    isHeavy = true;
                }
            }

            if (isFlank)
            {
                EnableFlanking(true);
            }
            else if (isQuick)
            {
                EnableFlanking(false);
                if (zombieData != null)
                {
                    maxHealth = zombieData.SmallMaxHealth;
                    walkSpeed = zombieData.SmallWalkSpeed;
                    attackDamage = zombieData.SmallAttackDamage;
                    attackRate = zombieData.SmallAttackRate;
                    transform.localScale = originalScale * zombieData.SmallScaleMultiplier;
                }
                else
                {
                    transform.localScale = originalScale * 0.75f;
                    walkSpeed *= speedMult;
                }
            }
            else if (isHeavy)
            {
                EnableFlanking(false);
                if (zombieData != null)
                {
                    maxHealth = zombieData.HeavyMaxHealth;
                    walkSpeed = zombieData.HeavyWalkSpeed;
                    attackDamage = zombieData.HeavyAttackDamage;
                    attackRate = zombieData.HeavyAttackRate;
                    transform.localScale = originalScale * zombieData.HeavyScaleMultiplier;
                }
                else
                {
                    maxHealth = (int)(maxHealth * 2.5f);
                    walkSpeed *= 0.7f;
                    attackDamage = (int)(attackDamage * 1.5f);
                    attackRate *= 1.5f;
                    transform.localScale = originalScale * 1.5f;
                }
            }
            else
            {
                EnableFlanking(false);
            }
            
            currentHealth = maxHealth;
            if (agent != null)
            {
                agent.speed = walkSpeed;
                agent.stoppingDistance = Mathf.Max(0.1f, attackRange - 0.3f);
            }

            if (isFalling)
            {
                currentState = ZombieState.Falling;
                animator.Play("Zombie@Fall01", 0, 0f);
            }

            isInitialized = true;
        }

        /// <summary>
        /// ZombieDataパラメータ適用処理
        /// </summary>
        private void ApplyZombieData()
        {
            if (zombieData == null) return;

            maxHealth = zombieData.MaxHealth;
            detectRange = zombieData.DetectRange;
            attackRange = zombieData.AttackRange;
            walkSpeed = zombieData.WalkSpeed;
            attackDamage = zombieData.AttackDamage;
            attackRate = zombieData.AttackRate;
            hitStunDuration = zombieData.HitStunDuration;
            reviveDuration = zombieData.ReviveDuration;
            zombieVoice = zombieData.ZombieVoice;
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            audioSource = GetComponent<AudioSource>();
            if(audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.volume = 0.5f;
                if (DeadLab.SettingsManager.SeMixerGroupInstance != null)
                {
                    audioSource.outputAudioMixerGroup = DeadLab.SettingsManager.SeMixerGroupInstance;
                }
            }
            audioSource.playOnAwake = false;

            originalScale = transform.localScale;

            childRenderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[childRenderers.Length];
            for (int i = 0; i < childRenderers.Length; i++)
            {
                if (childRenderers[i] != null && childRenderers[i].material.HasProperty("_Color"))
                {
                    originalColors[i] = childRenderers[i].material.color;
                }
            }
        }

        private void Start()
        {
            if (!isInitialized)
            {
                ApplyZombieData(); // ZombieDataが設定されていれば上書き
                currentHealth = maxHealth;
                agent.speed = walkSpeed;

                // プレイヤーの中心まで押し込まないよう、攻撃範囲の手前でエージェントを停止
                agent.stoppingDistance = Mathf.Max(0.1f, attackRange - 0.3f);
                isInitialized = true;
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();

                Collider playerCollider = playerObj.GetComponent<Collider>();
                if (playerCollider != null && capsuleCollider != null)
                {
                    Physics.IgnoreCollision(capsuleCollider, playerCollider);
                }
            }
            else
            {
                Debug.LogWarning("[ZombieController] シーン上に 'Player' タグのオブジェクトが見つかりません。");
            }
        }

        private void Update()
        {
            if (currentState == ZombieState.Dead) return;

            if (Time.timeScale <= 0f) return;

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                if (agent != null && agent.enabled && !agent.isStopped)
                {
                    agent.isStopped = true;
                }
                if (animator != null)
                {
                    animator.SetFloat(HashSpeed, 0);
                }
                return;
            }

            if (currentState == ZombieState.Stunned)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    ClearStatusEffects();
                    if (agent != null && agent.enabled)
                    {
                        agent.isStopped = false;
                    }
                    currentState = ZombieState.Chase;
                }
                return;
            }

            if (currentState == ZombieState.Frozen)
            {
                freezeTimer -= Time.deltaTime;
                if (freezeTimer <= 0)
                {
                    ClearStatusEffects();
                    if (agent != null && agent.enabled)
                    {
                        agent.isStopped = false;
                    }
                    currentState = ZombieState.Chase;
                }
                return;
            }

            if (currentState == ZombieState.Reviving)
            {
                reviveTimer -= Time.deltaTime;
                if (reviveTimer <= 0)
                {
                    currentState = ZombieState.Idle;
                    if (agent != null && agent.enabled)
                    {
                        agent.isStopped = false;
                    }
                    Debug.Log($"[ZombieController] {gameObject.name} 復活完了。");
                }
                return;
            }

            if (currentState == ZombieState.Damaged)
            {
                hitStunTimer -= Time.deltaTime;
                if (hitStunTimer <= 0)
                {
                    agent.isStopped = false;
                    currentState = ZombieState.Chase;
                }
                return;
            }

            if (playerTarget == null)
            {
                currentState = ZombieState.Idle;
                animator.SetFloat(HashSpeed, 0);
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            switch (currentState)
            {
                case ZombieState.Idle:
                    animator.SetFloat(HashSpeed, 0);
                    if (distanceToPlayer <= detectRange)
                    {
                        currentState = ZombieState.Chase;
                        PlayZombieSound(zombieVoice);
                    }
                    break;

                case ZombieState.Chase:
                    if (agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                        if (isFlanking && !hasReachedWaypoint)
                        {
                            if (flankWaypoint == Vector3.zero)
                            {
                                CalculateFlankWaypoint();
                            }

                            agent.SetDestination(flankWaypoint);

                            float distanceToWaypoint = Vector3.Distance(transform.position, flankWaypoint);
                            if (distanceToWaypoint <= 1.5f)
                            {
                                hasReachedWaypoint = true;
                            }

                            if (distanceToPlayer <= attackRange * 1.5f)
                            {
                                hasReachedWaypoint = true;
                            }
                        }
                        else
                        {
                            agent.SetDestination(playerTarget.position);
                        }
                    }
                    
                    animator.SetFloat(HashSpeed, agent.velocity.magnitude);

                    if (distanceToPlayer <= attackRange)
                    {
                        currentState = ZombieState.Attack;
                    }
                    else if (distanceToPlayer > detectRange)
                    {
                        agent.isStopped = true;
                        currentState = ZombieState.Idle;
                    }
                    break;

                case ZombieState.Attack:
                    agent.isStopped = true;
                    animator.SetFloat(HashSpeed, 0);

                    Vector3 direction = (playerTarget.position - transform.position).normalized;
                    direction.y = 0;
                    if (direction != Vector3.zero)
                    {
                        Quaternion lookRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5.0f);
                    }

                    if (Time.time - lastAttackTime >= attackRate)
                    {
                        PerformAttack();
                    }

                    if (distanceToPlayer > attackRange)
                    {
                        currentState = ZombieState.Chase;
                    }
                    break;

                case ZombieState.Falling:
                    //アニメーションが終了したらIdleへ遷移
                    if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                    {
                        currentState = ZombieState.Idle;
                        animator.Play("Zombie@Idle01", 0, 0f);
                    }
                    break;
            }
        }

        /// <summary>
        /// AudioClip再生処理
        /// </summary>
        /// <param name="clip">再生するAudioClip</param>
        private void PlayZombieSound(AudioClip clip)
        {
            if (Time.timeScale <= 0f || clip == null) return;

            audioSource.PlayOneShot(clip);
        }

        /// <summary>
        /// 攻撃実行処理
        /// </summary>
        private void PerformAttack()
        {
            lastAttackTime = Time.time;
            animator.SetTrigger(HashAttackTrigger);

            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= attackRange + 0.2f && playerHealth != null && !playerHealth.IsDead)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }

        /// <summary>
        /// 被ダメージ処理（IDamageable実装）
        /// </summary>
        public void TakeDamage(int damageAmount)
        {
            if (currentState == ZombieState.Dead) return;

            currentHealth -= damageAmount;
            Debug.Log($"[ZombieController] {gameObject.name} 被弾: {damageAmount} | 残りHP: {currentHealth}");

            if (currentHealth <= 0)
            {
                ClearStatusEffects();
                Die();
                return;
            }

            if (currentState == ZombieState.Stunned || currentState == ZombieState.Frozen || currentState == ZombieState.Falling)
            {
                return;
            }

            currentState = ZombieState.Damaged;
            agent.isStopped = true;
            hitStunTimer = hitStunDuration;
            
            animator.SetTrigger(HashHitTrigger);
        }

        /// <summary>
        /// ゾンビ死亡処理
        /// </summary>
        private void Die()
        {
            currentState = ZombieState.Dead;
            
            agent.isStopped = true;
            agent.enabled = false;
            capsuleCollider.enabled = false;

            animator.SetTrigger(HashDieTrigger);

            Debug.Log($"[ZombieController] {gameObject.name} 死亡。");

            OnDeath?.Invoke(this);
        }

        /// <summary>
        /// 感電状態適用および周囲連鎖処理
        /// </summary>
        public void ApplyElectricShock(float duration, GameObject vfxPrefab, int chainCount = 0, float chainRadius = 5.0f, float chainDelay = 0.2f)
        {
            if (currentState == ZombieState.Dead) return;
            if (currentState == ZombieState.Stunned) return;

            ClearStatusEffects();

            currentState = ZombieState.Stunned;
            stunTimer = duration;

            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
            }

            animator.SetTrigger(HashHitTrigger);

            if (vfxPrefab != null)
            {
                activeStatusVFX = Instantiate(vfxPrefab, transform.position, Quaternion.identity, transform);
            }

            if (chainCount > 0)
            {
                StartCoroutine(ChainElectricShockRoutine(duration, vfxPrefab, chainCount, chainRadius, chainDelay));
            }
        }

        /// <summary>
        /// 感電周囲連鎖処理ルーチン
        /// </summary>
        private IEnumerator ChainElectricShockRoutine(float duration, GameObject vfxPrefab, int chainCount, float chainRadius, float chainDelay)
        {
            yield return new WaitForSeconds(chainDelay);

            Collider[] colliders = Physics.OverlapSphere(transform.position, chainRadius);
            foreach (var hit in colliders)
            {
                var targetZombie = hit.GetComponent<ZombieController>();
                if (targetZombie == null)
                {
                    targetZombie = hit.GetComponentInParent<ZombieController>();
                }

                if (targetZombie != null && targetZombie != this && !targetZombie.IsDead && targetZombie.currentState != ZombieState.Stunned)
                {
                    targetZombie.ApplyElectricShock(duration, vfxPrefab, chainCount - 1, chainRadius, chainDelay);
                }
            }
        }

        /// <summary>
        /// 凍結状態適用処理
        /// </summary>
        public void ApplyFreezing(float duration, GameObject vfxPrefab)
        {
            if (currentState == ZombieState.Dead) return;

            ClearStatusEffects();

            currentState = ZombieState.Frozen;
            freezeTimer = duration;

            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
            }

            animator.speed = 0f;

            if (childRenderers != null)
            {
                foreach (var r in childRenderers)
                {
                    if (r != null && r.material.HasProperty("_Color"))
                    {
                        r.material.color = new Color(0.4f, 0.7f, 1.0f, 1.0f);
                    }
                }
            }

            if (vfxPrefab != null)
            {
                activeStatusVFX = Instantiate(vfxPrefab, transform.position, Quaternion.identity, transform);
            }
        }

        /// <summary>
        /// 状態異常（感電・凍結）解除・初期化処理
        /// </summary>
        private void ClearStatusEffects()
        {
            if (activeStatusVFX != null)
            {
                Destroy(activeStatusVFX);
                activeStatusVFX = null;
            }

            animator.speed = 1f;

            if (childRenderers != null && originalColors != null)
            {
                for (int i = 0; i < childRenderers.Length; i++)
                {
                    if (childRenderers[i] != null && childRenderers[i].material.HasProperty("_Color"))
                    {
                        childRenderers[i].material.color = originalColors[i];
                    }
                }
            }
        }

        /// <summary>
        /// ゾンビ復活処理
        /// </summary>
        public void Revive()
        {
            if (currentState != ZombieState.Dead) return;

            currentState = ZombieState.Reviving;
            currentHealth = maxHealth;
            reviveTimer = reviveDuration;

            capsuleCollider.enabled = true;
            agent.enabled = true;

            if (agent.enabled)
            {
                agent.isStopped = true;
            }

            animator.ResetTrigger(HashDieTrigger);
            animator.ResetTrigger(HashAttackTrigger);
            animator.ResetTrigger(HashHitTrigger);

            animator.Play("Zombie@Idle01", 0, 0f);

            Debug.Log($"[ZombieController] {gameObject.name} 復活開始。HP: {currentHealth}");
        }

        /// <summary>
        /// 回り込み行動有効化処理
        /// </summary>
        public void EnableFlanking(bool enable)
        {
            isFlanking = enable;
            hasReachedWaypoint = false;
            flankWaypoint = Vector3.zero;
        }

        /// <summary>
        /// プレイヤー側面回り込み中継点算出処理
        /// </summary>
        private void CalculateFlankWaypoint()
        {
            if (playerTarget == null) return;

            Vector3 myPos = transform.position;
            Vector3 playerPos = playerTarget.position;

            Vector3 toPlayer = (playerPos - myPos).normalized;
            toPlayer.y = 0;

            Vector3 midPoint = (myPos + playerPos) * 0.5f;
            Vector3 perpDir = new Vector3(-toPlayer.z, 0, toPlayer.x);

            float side = Random.value > 0.5f ? 1.0f : -1.0f;
            Vector3 targetPoint = midPoint + perpDir * (4.0f * side);

            if (NavMesh.SamplePosition(targetPoint, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
            {
                flankWaypoint = hit.position;
            }
            else
            {
                flankWaypoint = midPoint;
            }
        }
    }
}
