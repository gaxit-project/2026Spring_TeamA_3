using System;
using UnityEngine;
using System.Collections;
using InfimaGames.LowPolyShooterPack;
using Random = UnityEngine.Random;
using DeadLab;

public class Projectile : MonoBehaviour {

	[Range(5, 100)]
	[Tooltip("After how long time should the bullet prefab be destroyed?")]
	public float destroyAfter;
	[Tooltip("If enabled the bullet destroys on impact")]
	public bool destroyOnImpact = false;

	// すでに衝突したかどうかを記録するフラグ
	private bool hasCollided;
	[Tooltip("Minimum time after impact that the bullet is destroyed")]
	public float minDestroyTime;
	[Tooltip("Maximum time after impact that the bullet is destroyed")]
	public float maxDestroyTime;

	[Header("Impact Effect Prefabs")]
	public Transform [] bloodImpactPrefabs;
	public Transform [] metalImpactPrefabs;
	public Transform [] dirtImpactPrefabs;
	public Transform []	concreteImpactPrefabs;

	private void Start ()
	{
		//Grab the game mode service, we need it to access the player character!
		var gameModeService = ServiceLocator.Current.Get<IGameModeService>();
		//Ignore the main player character's collision. A little hacky, but it should work.
		Physics.IgnoreCollision(gameModeService.GetPlayerCharacter().GetComponent<Collider>(), GetComponent<Collider>());
		
		//Start destroy timer
		StartCoroutine (DestroyAfter ());
	}

	//If the bullet collides with anything
	private void OnCollisionEnter (Collision collision)
	{
		//Ignore collisions with other projectiles.
		if (collision.gameObject.GetComponent<Projectile>() != null)
			return;

		// 多重衝突を防ぐガード処理
		if (hasCollided)
			return;
		hasCollided = true;
		
		// IDamageable（ゾンビ、プレイヤー等）へのダメージ適用と血液エフェクト生成
		var damageable = collision.transform.GetComponent<IDamageable>();
		if (damageable == null)
		{
			damageable = collision.transform.GetComponentInParent<IDamageable>();
		}

		if (damageable != null)
		{
			// 1発あたり15ダメージを適用
			damageable.TakeDamage(15);

			// アセットのエフェクトは使わず、スクリプトから直接「小さな赤い火花」を生成する
			if (collision.contacts.Length > 0)
			{
				// URPでも動作する標準のテクスチャなしシェーダーを検索してマテリアルを動的作成
				Shader particleShader = Shader.Find("Sprites/Default");
				if (particleShader == null)
				{
					particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
				}
				Material vfxMaterial = new Material(particleShader);

				// 空のゲームオブジェクトを作成
				GameObject bloodObj = new GameObject("ZombieBloodVFX");
				bloodObj.transform.position = transform.position;
				bloodObj.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);

				// パーティクルシステムコンポーネントをアタッチ
				ParticleSystem ps = bloodObj.AddComponent<ParticleSystem>();

				// アタッチ時の自動再生を停止しクリアする（エラー対策）
				ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

				// パーティクルレンダラーに元の正しいマテリアルを設定
				ParticleSystemRenderer psRenderer = bloodObj.GetComponent<ParticleSystemRenderer>();
				if (psRenderer != null && vfxMaterial != null)
				{
					psRenderer.material = vfxMaterial;
				}

				// 基本パラメータ設定
				var main = ps.main;
				main.playOnAwake = false;
				main.startColor = new Color(0.6f, 0.1f, 0.1f, 0.8f); // マイルドな赤色
				main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f); // 極小サイズ
				main.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 3.0f); // 飛び散る速度
				main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f); // 0.3〜0.6秒で消滅するように設定
				main.duration = 0.3f;
				main.loop = false;
				main.stopAction = ParticleSystemStopAction.Destroy; // 再生が終わったら自動でオブジェクトを消滅させる

				// バースト設定（8粒だけ一瞬で放出）
				var emission = ps.emission;
				emission.rateOverTime = 0;
				var burst = new ParticleSystem.Burst(0f, 8);
				emission.SetBursts(new ParticleSystem.Burst[] { burst });

				// 放出されるコーン角度の調整
				var shape = ps.shape;
				shape.shapeType = ParticleSystemShapeType.Cone;
				shape.angle = 20.0f;
				shape.radius = 0.02f;

				// 再生
				ps.Play();
			}

			// 弾丸を消滅させて判定を終了
			Destroy(gameObject);
			return;
		}
		
		// //Ignore collision if bullet collides with "Player" tag
		// if (collision.gameObject.CompareTag("Player")) 
		// {
		// 	//Physics.IgnoreCollision (collision.collider);
		// 	Debug.LogWarning("Collides with player");
		// 	//Physics.IgnoreCollision(GetComponent<Collider>(), GetComponent<Collider>());
		//
		// 	//Ignore player character collision, otherwise this moves it, which is quite odd, and other weird stuff happens!
		// 	Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
		//
		// 	//Return, otherwise we will destroy with this hit, which we don't want!
		// 	return;
		// }
		//
		//If destroy on impact is false, start 
		//coroutine with random destroy timer
		if (!destroyOnImpact) 
		{
			StartCoroutine (DestroyTimer ());
		}
		//Otherwise, destroy bullet on impact
		else 
		{
			Destroy (gameObject);
		}

		//If bullet collides with "Blood" tag
		if (collision.transform.tag == "Blood") 
		{
			//Instantiate random impact prefab from array
			Instantiate (bloodImpactPrefabs [Random.Range 
				(0, bloodImpactPrefabs.Length)], transform.position, 
				Quaternion.LookRotation (collision.contacts [0].normal));
			//Destroy bullet object
			Destroy(gameObject);
		}

		//If bullet collides with "Metal" tag
		if (collision.transform.tag == "Metal") 
		{
			//Instantiate random impact prefab from array
			Instantiate (metalImpactPrefabs [Random.Range 
				(0, bloodImpactPrefabs.Length)], transform.position, 
				Quaternion.LookRotation (collision.contacts [0].normal));
			//Destroy bullet object
			Destroy(gameObject);
		}

		//If bullet collides with "Dirt" tag
		if (collision.transform.tag == "Dirt") 
		{
			//Instantiate random impact prefab from array
			Instantiate (dirtImpactPrefabs [Random.Range 
				(0, bloodImpactPrefabs.Length)], transform.position, 
				Quaternion.LookRotation (collision.contacts [0].normal));
			//Destroy bullet object
			Destroy(gameObject);
		}

		//If bullet collides with "Concrete" tag
		if (collision.transform.tag == "Concrete") 
		{
			//Instantiate random impact prefab from array
			Instantiate (concreteImpactPrefabs [Random.Range 
				(0, bloodImpactPrefabs.Length)], transform.position, 
				Quaternion.LookRotation (collision.contacts [0].normal));
			//Destroy bullet object
			Destroy(gameObject);
		}

		//If bullet collides with "Target" tag
		if (collision.transform.tag == "Target") 
		{
			//Toggle "isHit" on target object
			collision.transform.gameObject.GetComponent
				<TargetScript>().isHit = true;
			//Destroy bullet object
			Destroy(gameObject);
		}
			
		//If bullet collides with "ExplosiveBarrel" tag
		if (collision.transform.tag == "ExplosiveBarrel") 
		{
			//Toggle "explode" on explosive barrel object
			collision.transform.gameObject.GetComponent
				<ExplosiveBarrelScript>().explode = true;
			//Destroy bullet object
			Destroy(gameObject);
		}

		//If bullet collides with "GasTank" tag
		if (collision.transform.tag == "GasTank") 
		{
			//Toggle "isHit" on gas tank object
			collision.transform.gameObject.GetComponent
				<GasTankScript> ().isHit = true;
			//Destroy bullet object
			Destroy(gameObject);
		}
	}

	private IEnumerator DestroyTimer () 
	{
		//Wait random time based on min and max values
		yield return new WaitForSeconds
			(Random.Range(minDestroyTime, maxDestroyTime));
		//Destroy bullet object
		Destroy(gameObject);
	}

	private IEnumerator DestroyAfter () 
	{
		//Wait for set amount of time
		yield return new WaitForSeconds (destroyAfter);
		//Destroy bullet object
		Destroy (gameObject);
	}
}