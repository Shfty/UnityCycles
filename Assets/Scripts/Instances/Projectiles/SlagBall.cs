﻿using UnityEngine;
using System.Collections;

public class SlagBall : Projectile
{
	// Unity Methods
	public void PooledStart()
	{
		// Add Initial Force
		GetComponent<Rigidbody>().AddForce( transform.forward * ProjectileInfo.Properties.SlagBall.InitialForce, ForceMode.Impulse );
	}

	public void OnCollisionEnter()
	{
		// Create an explosion and wait until the particle system is finished before deactivating
		GameObject explosion = GameControl.ProjectilePool.Spawn( "Slag Ball Explosion" );
		explosion.transform.position = transform.position;
        explosion.GetComponent<Explosion>().Owner = Owner;
        explosion.GetComponent<Explosion>().LateStart();

		ParticleSystem ps = transform.Find( "Exhaust" ).GetComponent<ParticleSystem>();
		StartCoroutine( WaitForParticles( ps ) );
	}

	// Utility Methods
	IEnumerator WaitForParticles( ParticleSystem ps )
	{
		// Turn off rocket physics, rendering and particle emission
		GetComponent<Rigidbody>().isKinematic = true;
		ps.enableEmission = false;

		yield return new WaitForSeconds( ps.startLifetime );

		// Reverse changes and disable rocket
		GetComponent<Rigidbody>().isKinematic = false;
		ps.enableEmission = true;

		GameControl.ProjectilePool.Despawn( gameObject );
	}
}
