using UnityEngine;
using System.Collections;
using DeadLab;

public class ExplosiveBarrelScript : MonoBehaviour {

	float randomTime;
	bool routineStarted = false;

	//Used to check if the barrel 
	//has been hit and should explode 
	public bool explode = false;

	[Header("Prefabs")]
	//The explosion prefab
	public Transform explosionPrefab;
	//The destroyed barrel prefab
	public Transform destroyedBarrelPrefab;

	[Header("Customizable Options")]
	//Minimum time before the barrel explodes
	public float minTime = 0.05f;
	//Maximum time before the barrel explodes
	public float maxTime = 0.25f;

	[Header("Explosion Options")]
	//How far the explosion will reach
	public float explosionRadius = 12.5f;
	//How powerful the explosion is
	public float explosionForce = 4000.0f;

	[Header("ScriptableObject Options")]
	[Tooltip("ドラム缶のパラメータデータ（設定すると上記の時間や爆発設定が上書きされます）")]
	public ExplosiveBarrelData barrelData;

	[Header("Sprite")]
	[Tooltip("爆発範囲を可視化するためのスプライト")]
	[SerializeField] private GameObject explosionRadiusSprite;

	private int maxDamage = 100;

	private void Start()
	{
		ApplyBarrelData();

        if (explosionRadiusSprite != null)
        {
            explosionRadiusSprite.transform.localScale = Vector3.one * explosionRadius * 2f;
        }
    }

	private void ApplyBarrelData()
	{
		if (barrelData == null) return;

		minTime = barrelData.MinTime;
		maxTime = barrelData.MaxTime;
		explosionRadius = barrelData.ExplosionRadius;
		explosionForce = barrelData.ExplosionForce;
		maxDamage = barrelData.MaxDamage;
	}
	
	private void Update () {
		//Generate random time based on min and max time values
		randomTime = Random.Range (minTime, maxTime);

		//If the barrel is hit
		if (explode == true) 
		{
			if (routineStarted == false) 
			{
				//Start the explode coroutine
				StartCoroutine(Explode());
				routineStarted = true;
			} 
		}
	}
	
	private IEnumerator Explode () {
		//Wait for set amount of time
		yield return new WaitForSeconds(randomTime);

		//Spawn the destroyed barrel prefab
		Instantiate (destroyedBarrelPrefab, transform.position, 
		             transform.rotation); 

		//Explosion force
		Vector3 explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
		foreach (Collider hit in colliders) {
			Rigidbody rb = hit.GetComponent<Rigidbody> ();
			
			//Add force to nearby rigidbodies
			if (rb != null)
				rb.AddExplosionForce (explosionForce * 50, explosionPos, explosionRadius);

			// IDamageable（ゾンビ・プレイヤーなど）にダメージを適用
			var damageable = hit.GetComponent<IDamageable>();
			if (damageable == null)
			{
				damageable = hit.GetComponentInParent<IDamageable>();
			}
			if (damageable != null)
			{
				float distance = Vector3.Distance(explosionPos, hit.transform.position);
				float damageFactor = 1.0f - Mathf.Clamp01(distance / explosionRadius);
				int damage = Mathf.RoundToInt(maxDamage * damageFactor);
				if (damage > 0)
				{
					damageable.TakeDamage(damage);
				}
			}

			//If the barrel explosion hits other barrels with the tag "ExplosiveBarrel"
			if (hit.transform.tag == "ExplosiveBarrel") 
			{
				//Toggle the explode bool on the explosive barrel object
				hit.transform.gameObject.GetComponent<ExplosiveBarrelScript>().explode = true;
			}
				
			//If the explosion hit the tag "Target"
			if (hit.transform.tag == "Target") 
			{
				//Toggle the isHit bool on the target object
				hit.transform.gameObject.GetComponent<TargetScript>().isHit = true;
			}

			//If the explosion hit the tag "GasTank"
			if (hit.GetComponent<Collider>().tag == "GasTank") 
			{
				//If gas tank is within radius, explode it
				hit.gameObject.GetComponent<GasTankScript> ().isHit = true;
				hit.gameObject.GetComponent<GasTankScript> ().explosionTimer = 0.05f;
			}
		}

		//Raycast downwards to check the ground tag
		RaycastHit checkGround;
		if (Physics.Raycast(transform.position, Vector3.down, out checkGround, 50))
		{
			//Instantiate explosion prefab at hit position
			Transform explosionInstance = Instantiate (explosionPrefab, checkGround.point, 
				Quaternion.FromToRotation (Vector3.forward, checkGround.normal)); 

			// 爆発範囲のプレイヤーまで音が届くように調整
			AdjustExplosionAudio(explosionInstance.gameObject);
		}

		//Destroy the current barrel object
		Destroy (gameObject);
	}

	/// <summary>
	/// 爆発半径に応じて、爆発エフェクト内のAudioSource（3Dサウンド）の到達範囲を動的調整します
	/// </summary>
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
			// 音が届く最大距離を爆発範囲の1.5倍に引き上げる
			audioSource.maxDistance = Mathf.Max(audioSource.maxDistance, explosionRadius * 1.5f);
			// 距離による減衰の開始点も広げ、近くでは安定した大音量で聞こえるようにする
			audioSource.minDistance = Mathf.Max(audioSource.minDistance, explosionRadius * 0.3f);
			// 線形減衰（Linear）に変更して、距離に応じて滑らかに聞こえるようにする
			audioSource.rolloffMode = AudioRolloffMode.Linear;
		}
    }

    /// <summary>
    /// エディタ上で効果の適用範囲を視覚的に表示
    /// </summary>
    private void OnDrawGizmosSelected()
    {
		Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}