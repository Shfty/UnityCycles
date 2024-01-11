using UnityEngine;
using System.Collections;

public class MortarShell : Projectile
{
	// Fields
	Vector3 startPos;
	float timer = 0f;

	// Properties
	public float TargetDist;
	public int Bombs;

	// Unity Methods
	public void PooledStart()
	{
		// Store starting position
		startPos = transform.position;

		// Add initial force and torque
		GetComponent<Rigidbody>().AddForce( transform.forward * ProjectileInfo.Properties.MortarShell.InitialForce, ForceMode.VelocityChange );
		GetComponent<Rigidbody>().AddTorque( transform.right * ProjectileInfo.Properties.MortarShell.InitialTorque, ForceMode.Impulse );
	}

	public void FixedUpdate()
	{
		// Update the timer
		timer += Time.deltaTime;

		// Track distance in the XZ plane, detonate after passing TargetDist
		Vector3 diff = transform.position - startPos;
		diff.y = 0;

		if( diff.magnitude > TargetDist || timer > ProjectileInfo.Properties.MortarShell.Timeout )
		{
			// Create an explosion prefab
			GameObject explosion = GameControl.ProjectilePool.Spawn( "Rocket Explosion" );
			explosion.transform.position = transform.position;
            explosion.GetComponent<Explosion>().Owner = Owner;
            explosion.GetComponent<Explosion>().LateStart();

			// Release mortar 6 bombs downward
			float angleSlice = 360f / Bombs;
			Vector3 bombVector = Quaternion.AngleAxis( 85f, Vector3.right ) * Vector3.forward;
			for( int i = 0; i < angleSlice; ++i )
			{
				GameObject mortarBomb = GameControl.ProjectilePool.Spawn( "Mortar Bomb" );
				mortarBomb.transform.position = transform.position;
				mortarBomb.transform.rotation = Quaternion.FromToRotation( Vector3.forward, bombVector );
				MortarBomb bombScript = mortarBomb.GetComponent<MortarBomb>();
				bombScript.Owner = Owner;
				bombScript.PooledStart();

				bombVector = Quaternion.AngleAxis( angleSlice, Vector3.up ) * bombVector;
			}

			// Deactivate the mortar shell
			GameControl.ProjectilePool.Despawn( gameObject );
		}
	}
}
