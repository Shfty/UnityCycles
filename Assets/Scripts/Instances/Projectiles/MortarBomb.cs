using UnityEngine;
using System.Collections;

public class MortarBomb : Projectile
{
	// Properties
	public GameObject ExplosionPrefab;

	// Unity Methods
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
		Instantiate( ExplosionPrefab, transform.position, Quaternion.identity );
		Deactivate();
	}
}
