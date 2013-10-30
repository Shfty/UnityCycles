using UnityEngine;
using System.Collections;

public class ResetObject : MonoBehaviour
{
	// Fields
	Vector3 initPosition;
	Quaternion initRotation;
	Vector3 initScale;

	Vector3 initVelocity;
	Vector3 initAngularVelocity;

	// Unity Methods
	public virtual void Awake()
	{
		// Store init state
		initPosition = transform.position;
		initRotation = transform.rotation;
		initScale = transform.localScale;

		if( rigidbody != null )
		{
			initVelocity = rigidbody.velocity;
			initAngularVelocity = rigidbody.angularVelocity;
		}
	}

	public virtual void OnEnable()
	{
		// Reset object to init state
		transform.position = initPosition;
		transform.rotation = initRotation;
		transform.localScale = initScale;

		if( rigidbody != null )
		{
			rigidbody.velocity = initVelocity;
			rigidbody.angularVelocity = initAngularVelocity;
		}
	}

	// Utility Methods
	public virtual void Deactivate()
	{
		foreach( Transform child in transform )
		{
			child.SendMessage( "Deactivate", SendMessageOptions.DontRequireReceiver );
		}
	}
}
