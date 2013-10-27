using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
	// Fields
	
	// Properties
	public float Force;

	// Unity Methods
	void Awake()
	{
	
	}
	
	void Start()
	{
		
	}
	
	void Update()
	{
		// Destroy the game object when the particle system finishes running
		if( particleSystem.isStopped )
		{
			Destroy( gameObject );
		}
	}

	void OnTriggerEnter( Collider col )
	{
		// If coming into contact with a rigidbody collider, blast it away
		SphereCollider sc = (SphereCollider)collider;

		if( col.rigidbody != null )
		{
			col.rigidbody.AddExplosionForce( Force, transform.position, sc.radius );
		}
	}
	
	// Utility Methods
}
