using UnityEngine;
using System.Collections;

public class Seeker : Projectile
{
	// Fields
	float elapsedTime = 0f;
	bool seeking = false;

	// Properties
	public Transform SeekTarget = null;
	public Vector3 SeekPoint;

	// Unity Methods
	public void PooledStart()
	{
		// Add initial force and torque
		rigidbody.AddForce( transform.forward * ProjectileInfo.Properties.Seeker.InitialForce, ForceMode.VelocityChange );
		rigidbody.AddTorque( transform.right * 5f, ForceMode.Impulse );
	}

	public void Update()
	{
		// Update the time, update orientation if necessary
		elapsedTime += Time.deltaTime;
		if( seeking )
		{
			Seek();
		}
	}

	public void FixedUpdate()
	{
		// Check if the seek delay has elapsed
		if( seeking == false && elapsedTime > ProjectileInfo.Properties.Seeker.SeekDelay )
		{
			// If so, set the seeking flag and stop the missile
			seeking = true;
			if( !rigidbody.isKinematic )
			{
				rigidbody.velocity = Vector3.zero;
				rigidbody.angularVelocity = Vector3.zero;
			}

			// Update orientation
			Seek();
		}

		// If the missile is seeking, accellerate in it's facing direction
		if( seeking )
		{
			rigidbody.AddForce( transform.forward * ProjectileInfo.Properties.Seeker.Accelleration, ForceMode.VelocityChange );
		}
	}

	public void OnCollisionEnter()
	{
		// Create an explosion and wait until the particle system is finished before deactivating
		GameObject explosion = GameControl.ProjectilePool.Spawn( "Seeker Explosion" );
		explosion.transform.position = transform.position;
		explosion.GetComponent<Explosion>().Owner = Owner;

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

		GameControl.ProjectilePool.Despawn( gameObject );
	}

	void Seek()
	{
		// If there's a seek target, look toward it. If not, look toward the static seek point
		if( SeekTarget == null )
		{
			rigidbody.rotation = Quaternion.LookRotation( SeekPoint - transform.position );
		}
		else
		{
			rigidbody.rotation = Quaternion.LookRotation( ( SeekTarget.position + ( SeekTarget.rigidbody.velocity * Time.deltaTime * 2f ) ) - transform.position );
		}
	}
}
