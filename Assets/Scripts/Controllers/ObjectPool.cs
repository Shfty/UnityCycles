using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
	// Fields
	Dictionary<GameObject, List<GameObject>> activeObjects;
	Dictionary<GameObject, List<GameObject>> inactiveObjects;
	Dictionary<GameObject, GameObject> containerObjects;

	// Properties
	public List<GameObject> Prefabs;
	public bool Preallocate = true;
	public int PreallocateCount = 10;
	public bool Limit = true;
	public int LimitCount = 40;

	// Unity Methods
	public void Awake()
	{
		activeObjects = new Dictionary<GameObject, List<GameObject>>();
		inactiveObjects = new Dictionary<GameObject, List<GameObject>>();
		containerObjects = new Dictionary<GameObject, GameObject>();
	}

	public void Start()
	{
		foreach( GameObject prefab in Prefabs )
		{
			GameObject container;
			// Create container objects
			if( Prefabs.Count > 1 )
			{
				container = new GameObject();
				container.name = prefab.name + " Pool";
				container.transform.parent = transform;
				containerObjects.Add( prefab, container );
			}
			else
			{
				container = gameObject;
				containerObjects.Add( prefab, container );
			}

			// Create key-value pairs
			activeObjects.Add( prefab, new List<GameObject>() );
			inactiveObjects.Add( prefab, new List<GameObject>() );

			// Preallocate objects
			if( Preallocate )
			{
				// Fill the object pool with instances
				for( int i = 0; i < PreallocateCount; ++i )
				{
					instantiate( prefab, container.transform );
				}
			}
		}
	}

	// Utility Methods
	GameObject instantiate( GameObject prefab, Transform parent )
	{
		// Create an instance of prefab, parent it to the respective pool holder and store in inactiveObjects
		GameObject go = (GameObject)Instantiate( prefab );
		go.name = prefab.name;
		go.SendMessage( "OriginPoolIs", this, SendMessageOptions.DontRequireReceiver );
		go.SendMessage( "PrefabIs", prefab, SendMessageOptions.DontRequireReceiver );
		go.SetActive( false );
		go.transform.parent = parent;
		inactiveObjects[ prefab ].Add( go );

		return go;
	}

	void activate( GameObject prefab, GameObject go )
	{
		// Move the gameobject from the inactive to active list and set active
		inactiveObjects[ prefab ].Remove( go );
		activeObjects[ prefab ].Add( go );
		go.SetActive( true );
	}

	public GameObject Spawn( string prefabName )
	{
		GameObject prefab = null;
		foreach( KeyValuePair<GameObject, List<GameObject>> pair in inactiveObjects )
		{
			if( pair.Key.name == prefabName )
			{
				prefab = pair.Key;
			}
		}

		if( prefab != null )
		{
			return Spawn( prefab );
		}
		else
		{
			Debug.LogWarning( this + " could not spawn " + prefab + ": not present in Object Pool" );
			return null;
		}
	}

	public GameObject Spawn( GameObject prefab )
	{
		// If there are any objects of the type requested, activate one and return it
		if( inactiveObjects[prefab].Count > 0 )
		{
			GameObject go = inactiveObjects[ prefab ][ 0 ];
			activate( prefab, go );

			return go;
		}
		else
		{
			// Check if there's a hard limit on objects
			if( !Limit )
			{
				// If not, create a new instance, activate and return it
				GameObject go = instantiate( prefab, containerObjects[prefab].transform );
				activate( prefab, go );

				return go;
			}
			else
			{
				// If so, check whether the limit has been reached
				if( activeObjects.Count + inactiveObjects.Count < LimitCount )
				{
					// If not, create a new instance, activate and return it
					GameObject go = instantiate( prefab, containerObjects[prefab].transform );
					activate( prefab, go );

					return go;
				}
				else
				{
					// If so, log a warning message and return null
					Debug.LogWarning( "Cannot spawn GameObject: pool full." );
					return null;
				}
			}
		}
	}

	public bool Despawn( GameObject go )
	{
		if( go != null )
		{
			GameObject prefab = go.GetComponent<PooledObject>().Prefab;

			// Make sure this prefab is pooled
			if( !activeObjects.ContainsKey( prefab ) )
			{
				Debug.LogWarning( "Could not despawn " + prefab + ": not present in Object Pool" );
				return false;
			}

			// Check if prefab is present in the active list
			if( activeObjects[ prefab ].Contains( go ) )
			{
				// If so, move it to the inactive list, re-parent it and deactivate it
				activeObjects[ prefab ].Remove( go );
				inactiveObjects[ prefab ].Add( go );
				go.transform.parent = containerObjects[ prefab ].transform;
				go.SetActive( false );
				return true;
			}
			else
			{
				// Otherwise, log a warning message and return false
				Debug.LogWarning( gameObject.name + ": Cannot despawn " + go + ": not present in active pool." );
				return false;
			}
		}

		return false;
	}

	public void DespawnAll()
	{
		List<GameObject> objects = new List<GameObject>();
		foreach( KeyValuePair<GameObject, List<GameObject>> kvp in activeObjects )
		{
			foreach( GameObject go in kvp.Value )
			{
				objects.Add( go );
			}
		}

		for( int i = 0; i < objects.Count; ++i )
		{
			Despawn( objects[ i ] );
		}
	}
}
