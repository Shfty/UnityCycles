using UnityEngine;
using System.Collections;

public class Projectile : PooledObject
{
	// Fields
	Vector3 initVelocity;
	Vector3 initAngularVelocity;

	// Properties
	public GameObject Owner;

	// Unity Methods
	public override void OnEnable()
	{
		// Call the parent's OnEnable method
		base.OnEnable();

		// Store projectile-specific initial state
		initVelocity = rigidbody.velocity;
		initAngularVelocity = rigidbody.angularVelocity;
	}

	public override void OnDisable()
	{
		// Call the parent's OnDisable method
		base.OnDisable();

		// Restore projectile-specific initial state
		rigidbody.velocity = initVelocity;
		rigidbody.angularVelocity = initAngularVelocity;
	}
}
