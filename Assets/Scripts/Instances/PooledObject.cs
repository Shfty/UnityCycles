using UnityEngine;
using System.Collections;

public class PooledObject : MonoBehaviour
{
	// Fields
	Vector3 initPosition;
	Quaternion initRotation;
	Vector3 initScale;

	// Unity Methods
	public virtual void OnEnable()
	{
		// Store init state
		initPosition = transform.position;
		initRotation = transform.rotation;
		initScale = transform.localScale;
	}

	public virtual void OnDisable()
	{
		// Reset object to init state
		transform.position = initPosition;
		transform.rotation = initRotation;
		transform.localScale = initScale;
	}

	public virtual void Deactivate()
	{
		gameObject.SetActive( false );
	}
}
