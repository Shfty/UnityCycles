using UnityEngine;
using System.Collections;

public class ShotgunPellet : Projectile
{
	// Unity Methods
	public void PooledStart()
	{
		// Add the projectile's initial force
		GetComponent<Rigidbody>().AddForce( transform.forward * ProjectileInfo.Properties.ShotgunPellet.InitialForce, ForceMode.Impulse );
	}

	public virtual void OnCollisionEnter()
	{
		// Create an explosion and deactivate the projectile
		GameObject explosion = GameControl.ProjectilePool.Spawn( "Shotgun Pellet Explosion" );
		explosion.transform.position = transform.position;
		explosion.GetComponent<Explosion>().Owner = Owner;
		explosion.GetComponent<Explosion>().LateStart();

		GameControl.ProjectilePool.Despawn( gameObject );
	}
}
