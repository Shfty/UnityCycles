using UnityEngine;
using System.Collections;

public class PooledObject : MonoBehaviour
{
	// Fields
	Vector3 initPosition;
	Quaternion initRotation;
	Vector3 initScale;

	Vector3 initVelocity;
	Vector3 initAngularVelocity;

	// Properties
	public GameObject Prefab;

	// Unity Methods
	public virtual void Awake()
	{
		// Store init state
		initPosition = transform.position;
		initRotation = transform.rotation;
		initScale = transform.localScale;

		if( GetComponent<Rigidbody>() != null )
		{
			initVelocity = GetComponent<Rigidbody>().velocity;
			initAngularVelocity = GetComponent<Rigidbody>().angularVelocity;
		}
	}

	public virtual void OnEnable()
	{
		// Reset object to init state
		transform.position = initPosition;
		transform.rotation = initRotation;
		transform.localScale = initScale;

		if( GetComponent<Rigidbody>() != null )
		{
			GetComponent<Rigidbody>().velocity = initVelocity;
			GetComponent<Rigidbody>().angularVelocity = initAngularVelocity;
		}
	}

	// Utility Methods
	protected void PrefabIs( GameObject prefab )
	{
		Prefab = prefab;
	}
}
