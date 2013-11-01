using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
	// Fields
	bool firstFrame = true;
	bool applyDamage = true;

	// Properties
	public int Damage;
	public float Force;
	public GameObject Owner;

	// Unity Methods
	void FixedUpdate()
	{
		// Destroy the game object when the particle system finishes running
		if( particleSystem.isStopped )
		{
			Destroy( gameObject );
		}

		if( firstFrame )
		{
			firstFrame = false;
		}
		else
		{
			applyDamage = false;
		}
	}

	void OnTriggerEnter( Collider col )
	{
		if( applyDamage && collider != null )
		{
			// If coming into contact with a rigidbody collider, blast it away
			SphereCollider sc = (SphereCollider)collider;

			if( col.rigidbody != null )
			{
				col.rigidbody.AddExplosionForce( Force, transform.position, sc.radius );
				object[] args = { Damage, Owner };
				col.gameObject.SendMessageUpwards( "ApplyDamage", args, SendMessageOptions.DontRequireReceiver );
			}
		}
	}
}
