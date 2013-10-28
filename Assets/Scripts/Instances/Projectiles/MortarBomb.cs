using UnityEngine;
using System.Collections;

public class MortarBomb : Projectile
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
		// Add the projectile's initial force
		rigidbody.AddForce( transform.forward * ProjectileInfo.Properties.MortarBomb.InitialForce, ForceMode.Impulse );
	}

	public virtual void FixedUpdate()
	{

	}

	public virtual void OnCollisionEnter()
	{
		// Create an explosion and deactivate the projectile
		GameObject explosion = projectilePool.Spawn( ExplosionPrefab );
		explosion.transform.position = transform.position;

		Deactivate();
	}
}
