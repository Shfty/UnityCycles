using UnityEngine;
using System.Collections;

public class MortarShell : Projectile
{
	// Fields
	ObjectPool objectPool;
	Vector3 startPos;
	float timer = 0f;

	// Properties
	public GameObject ExplosionPrefab;
	public GameObject MortarBombPrefab;
	public float TargetDist;

	// Unity Methods
	public void Awake()
	{
		objectPool = GameObject.FindGameObjectWithTag( "GameControl" ).GetComponent<ObjectPool>();
	}

	public void PooledStart()
	{
		// Store starting position
		startPos = transform.position;

		// Add initial force and torque
		rigidbody.AddForce( transform.forward * ProjectileInfo.Properties.MortarShell.InitialForce, ForceMode.VelocityChange );
		rigidbody.AddTorque( transform.right * ProjectileInfo.Properties.MortarShell.InitialTorque, ForceMode.Impulse );
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
			Instantiate( ExplosionPrefab, transform.position, Quaternion.identity );

			// Release mortar 6 bombs downward
			Vector3 bombVector = Quaternion.AngleAxis( 85f, Vector3.right ) * Vector3.forward;
			for( int i = 0; i < 6; ++i )
			{
				GameObject mortarBomb = objectPool.Spawn( MortarBombPrefab );
				mortarBomb.transform.position = transform.position;
				mortarBomb.transform.rotation = Quaternion.FromToRotation( Vector3.forward, bombVector );
				MortarBomb bombScript = mortarBomb.GetComponent<MortarBomb>();
				bombScript.PooledStart();

				bombVector = Quaternion.AngleAxis( 60f, Vector3.up ) * bombVector;
			}

			// Deactivate the mortar shell
			Deactivate();
		}
	}
}
