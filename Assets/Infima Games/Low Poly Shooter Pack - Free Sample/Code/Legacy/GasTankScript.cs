using UnityEngine;
using System.Collections;
using DeadLab;

public class GasTankScript : MonoBehaviour {

	float randomRotationValue;
	float randomValue;

	bool routineStarted = false;

	//Used to check if the gas tank 
	//has been hit
	public bool isHit = false;

	[Header("Prefabs")]
	//Explosion prefab
	public Transform explosionPrefab;
	//The destroyed gas tank prefab
	public Transform destroyedGasTankPrefab;

	[Header("Customizable Options")]
	//Time before the gas tank explodes, 
	//after being hit
	public float explosionTimer;
	//How fast the gas tank rotates
	public float rotationSpeed;
	//The maximum rotation speed of the
	//gast tank
	public float maxRotationSpeed;
	//How fast the gast tank moves
	public float moveSpeed;
	//How fast the audio pitch should increase
	public float audioPitchIncrease = 0.5f;

	[Header("Explosion Options")]
	//How far the explosion will reach
	public float explosionRadius = 12.5f;
	//How powerful the explosion is
	public float explosionForce = 4000.0f;

	[Header("ScriptableObject Options")]
	[Tooltip("ガスボンベのパラメータデータ（設定すると上記の挙動設定や爆発設定が上書きされます）")]
	public GasTankData gasTankData;

	private int maxDamage = 100;

	[Header("Light")]
	public Light lightObject;

	[Header("Particle Systems")]
	public ParticleSystem flameParticles;
	public ParticleSystem smokeParticles;

	[Header("Audio")]
	public AudioSource flameSound;
	public AudioSource impactSound;
	//Used to check if the audio has played
	bool audioHasPlayed = false;
	
	private void Start () {
		ApplyGasTankData();

		// 自動適用
		if (flameSound != null && DeadLab.SettingsManager.SeMixerGroupInstance != null)
			flameSound.outputAudioMixerGroup = DeadLab.SettingsManager.SeMixerGroupInstance;
		if (impactSound != null && DeadLab.SettingsManager.SeMixerGroupInstance != null)
			impactSound.outputAudioMixerGroup = DeadLab.SettingsManager.SeMixerGroupInstance;

		//Make sure the light is off at start
		lightObject.intensity = 0;
		//Get a random value for the rotation
		randomValue = Random.Range (-50, 50);
	}

	private void ApplyGasTankData()
	{
		if (gasTankData == null) return;

		explosionTimer = gasTankData.ExplosionTimer;
		moveSpeed = gasTankData.MoveSpeed;
		rotationSpeed = gasTankData.RotationSpeed;
		maxRotationSpeed = gasTankData.MaxRotationSpeed;
		explosionRadius = gasTankData.ExplosionRadius;
		explosionForce = gasTankData.ExplosionForce;
		maxDamage = gasTankData.MaxDamage;
	}

	private void Update () {
		//If the gas tank is hit
		if (isHit == true) 
		{
			// 物理演算を有効化し、全ての制限（Constraints）を解除して飛んでいくようにする
			var rb = GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.isKinematic = false;
				rb.constraints = RigidbodyConstraints.None;
			}

			//Start increasing the rotation speed over time
			randomRotationValue += 1.0f * Time.deltaTime;

			//If the random rotation is higher than the maximum rotation, 
			//set it to the max rotation value
			if (randomRotationValue > maxRotationSpeed) 
			{
				randomRotationValue = maxRotationSpeed;
			}

			// 速度設定が0の場合は初期デフォルト値を割り当てて動作を保証
			float activeMoveSpeed = moveSpeed == 0 ? 15.0f : moveSpeed;
			float activeRotationSpeed = rotationSpeed == 0 ? 10.0f : rotationSpeed;

			//Add force to the gas tank 
			if (rb != null)
			{
				rb.AddRelativeForce(Vector3.down * activeMoveSpeed * 50 * Time.deltaTime);
			}

			//Rotate the gas tank, based on the random rotation values
			transform.Rotate (randomRotationValue,0,randomValue * 
			                  activeRotationSpeed * Time.deltaTime); 

			//Play the flame particles
			flameParticles.Play ();
			//Play the smoke particles
			smokeParticles.Play ();

			//Increase the flame sound pitch over time
			flameSound.pitch += audioPitchIncrease * Time.deltaTime;

			//If the audio has not played, play it
			if (!audioHasPlayed) 
			{
				flameSound.Play ();
				//Audio has played
				audioHasPlayed = true;
			}

			if (routineStarted == false) 
			{
				//Start the explode coroutine
				StartCoroutine(Explode());
				routineStarted = true;
				//Set the light intensity to 3
				lightObject.intensity = 3;
			}
		}
	}
	private void OnCollisionEnter (Collision collision) {
		//Play the impact sound on every collision
		impactSound.Play ();
	}

	private IEnumerator Explode ()
	{
		//Wait for set amount of time
		yield return new WaitForSeconds(explosionTimer);
		
		//Spawn the destroyed gas tank prefab
		Instantiate (destroyedGasTankPrefab, transform.position, 
		             transform.rotation); 
		
		//Explosion force
		Vector3 explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
		foreach (Collider hit in colliders) 
		{
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
			
			//If the gas tank explosion hits other gas tanks with the tag "GasTank"
			if (hit.transform.tag == "GasTank") {
				
				//Toggle the isHit bool on the gas tank object
				hit.transform.gameObject.GetComponent<GasTankScript>().isHit = true;
			}
			//If the gas tank explosion hits any explosive barrel
			if (hit.transform.tag == "ExplosiveBarrel") {
				//Toggle explode bool on explosive barrel object
				hit.transform.gameObject.GetComponent<ExplosiveBarrelScript>().explode = true;
			}
		}
		
		//Spawn the explosion prefab
		Transform explosionInstance = Instantiate (explosionPrefab, transform.position, 
		             transform.rotation); 

		// 爆発範囲のプレイヤーまで音が届くように調整
		AdjustExplosionAudio(explosionInstance.gameObject);

		//Destroy the current gas tank object
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
			// 自動適用
			if (DeadLab.SettingsManager.SeMixerGroupInstance != null)
			{
				audioSource.outputAudioMixerGroup = DeadLab.SettingsManager.SeMixerGroupInstance;
			}
			// 音が届く最大距離を爆発範囲の1.5倍に引き上げる
			audioSource.maxDistance = Mathf.Max(audioSource.maxDistance, explosionRadius * 1.5f);
			// 距離による減衰の開始点も広げ、近くでは安定した大音量で聞こえるようにする
			audioSource.minDistance = Mathf.Max(audioSource.minDistance, explosionRadius * 0.3f);
			// 線形減衰（Linear）に変更して、距離に応じて滑らかに聞こえるようにする
			audioSource.rolloffMode = AudioRolloffMode.Linear;
		}
	}
}