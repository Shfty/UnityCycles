using UnityEngine;
using System.Collections;

public class Rocket : Projectile
{
	// Fields
	ObjectPool projectilePool;

	// Properties
	public GameObject ExplosionPrefab;

	// Unity Methods
	public void Start()
	{
		projectilePool = GameObject.Find( "Projectile Pool" ).GetComponent<ObjectPool>();
	}

	public void PooledStart()
	{
		// Add Initial Force
		rigidbody.AddForce( transform.forward * ProjectileInfo.Properties.Rocket.InitialForce, ForceMode.Impulse );
	}

	public void FixedUpdate()
	{
		// Add accelleration force
		rigidbody.AddForce( transform.forward * ProjectileInfo.Properties.Rocket.Accelleration * Time.deltaTime, ForceMode.Acceleration );
	}

	public void OnCollisionEnter()
	{
		// Create an explosion and wait until the particle system is finished before deactivating
		GameObject explosion = projectilePool.Spawn( ExplosionPrefab );
		explosion.transform.position = transform.position;

		ParticleSystem ps = transform.Find( "Exhaust" ).GetComponent<ParticleSystem>();
		StartCoroutine( WaitForParticles( ps ) );
	}

	// Utility Methods
	IEnumerator WaitForParticles( ParticleSystem ps )
	{
		// Turn off rocket physics, rendering and particle emission
		rigidbody.isKinematic = true;
		ps.enableEmission = false;

		yield return new WaitForSeconds( ps.startLifetime );

		// Reverse changes and disable rocket
		rigidbody.isKinematic = false;
		ps.enableEmission = true;

		Deactivate();
	}
}
