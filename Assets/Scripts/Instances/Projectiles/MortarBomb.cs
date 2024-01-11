using UnityEngine;
using System.Collections;

public class MortarBomb : Projectile
{
	// Unity Methods
	public void PooledStart()
	{
		// Add the projectile's initial force
		GetComponent<Rigidbody>().AddForce( transform.forward * ProjectileInfo.Properties.MortarBomb.InitialForce, ForceMode.Impulse );
	}

	public virtual void OnCollisionEnter()
	{
		// Create an explosion and deactivate the projectile
		GameObject explosion = GameControl.ProjectilePool.Spawn( "Mortar Bomb Explosion" );
		explosion.transform.position = transform.position;
		explosion.GetComponent<Explosion>().Owner = Owner;
        explosion.GetComponent<Explosion>().LateStart();

		GameControl.ProjectilePool.Despawn( gameObject );
	}
}
