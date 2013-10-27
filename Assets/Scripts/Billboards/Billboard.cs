using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Billboard : MonoBehaviour
{
	// Fields
	protected List<Camera> cameras;
	protected List<GameObject> billboards;

	// Properties
	public GameControl GameControl;
	public ObjectPool ObjectPool;
	public GameObject BillboardPrefab;

	// Unity Methods
	public virtual void Start()
	{
		// Instantiate camera and billboard lists
		cameras = new List<Camera>();
		billboards = new List<GameObject>();

		// Check the player count, create a billboard for each camera and set it to the respective layer
		List<GameObject> players = GameControl.Players;
		for( int i = 0; i < players.Count; ++i )
		{
			cameras.Add( players[ i ].transform.Find( "Camera" ).camera );
			GameObject billboard = ObjectPool.Spawn( BillboardPrefab );
			billboard.layer = LayerMask.NameToLayer( "Camera " + ( i + 1 ) );
			billboards.Add( billboard );
		}
	}

	public virtual void Update()
	{
		// Orient the billboards toward their respective cameras
		for( int i = 0; i < cameras.Count; ++i )
		{
			billboards[ i ].transform.position = transform.position;
			billboards[ i ].transform.rotation = Quaternion.LookRotation( cameras[ i ].transform.forward );
		}
	}

	// Utility Methods
	public virtual void Deactivate()
	{
		foreach( GameObject billboard in billboards )
		{
			billboard.SetActive( false );
		}
		gameObject.SetActive( false );
	}
}
