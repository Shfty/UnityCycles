using UnityEngine;
using System.Collections;

public class PooledObject : MonoBehaviour
{
	// Fields
	Vector3 initPosition;
	Quaternion initRotation;
	Vector3 initScale;

	// Properties
	public ObjectPool OriginPool;
	public GameObject Prefab;

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

	// Utility Methods
	protected void OriginPoolIs( ObjectPool objectPool )
	{
		OriginPool = objectPool;
	}

	protected void PrefabIs( GameObject prefab )
	{
		Prefab = prefab;
	}

	public virtual void Deactivate()
	{
		foreach( Transform child in transform )
		{
			child.SendMessage( "Deactivate", SendMessageOptions.DontRequireReceiver );
		}

		OriginPool.Despawn( Prefab, gameObject );
	}
}
