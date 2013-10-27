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
			// Create container objects
			GameObject container = new GameObject();
			container.name = prefab.name + " Pool";
			containerObjects.Add( prefab, container );

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

	public void LateUpdate()
	{
		List<GameObject> keys = new List<GameObject>();
		List<GameObject> recentlyDeactivated = new List<GameObject>();
		foreach( KeyValuePair<GameObject, List<GameObject>> kvp in activeObjects )
		{
			foreach( GameObject go in kvp.Value )
			{
				// Check for any recently-deactivated objects and store in recentlyDeactivated
				if( !go.activeSelf )
				{
					keys.Add( kvp.Key );
					recentlyDeactivated.Add( go );
				}
			}
		}

		// Deactivate the objects stored in recentlyDeactivated
		for( int i = 0; i < keys.Count; ++i )
		{
			Despawn( keys[ i ], recentlyDeactivated[ i ] );
		}
	}

	// Utility Methods
	GameObject instantiate( GameObject prefab, Transform parent )
	{
		// Create an instance of prefab, parent it to the respective pool holder and store in inactiveObjects
		GameObject go = (GameObject)Instantiate( prefab );
		go.transform.parent = parent;
		go.SetActive( false );
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

	public GameObject Spawn( GameObject prefab )
	{
		if( !inactiveObjects.ContainsKey( prefab ) )
		{
			Debug.LogWarning( "Could not spawn " + prefab + ": not present in Object Pool" );
			return null;
		}

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

	public bool Despawn( GameObject prefab, GameObject go )
	{
		// Check if prefab is present in the active list
		if( activeObjects[prefab].Contains( go ) )
		{
			// If so, move it to the inactive list, re-parent it and deactivate it
			activeObjects[prefab].Remove( go );
			inactiveObjects[ prefab ].Add( go );
			go.transform.parent = containerObjects[ prefab ].transform;
			go.SetActive( false );
			return true;
		}
		else
		{
			// Otherwise, log a warning message and return false
			Debug.LogWarning( "Cannot despawn GameObject: not present in active pool." );
			return false;
		}
	}
}
