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
	public GameObject BillboardPrefab;

	// Unity Methods
	public void Awake()
	{
		// Instantiate camera and billboard lists
		cameras = new List<Camera>();
		billboards = new List<GameObject>();
	}

	public void Update()
	{
		// Orient the billboards toward their respective cameras
		for( int i = 0; i < cameras.Count; ++i )
		{
			billboards[ i ].transform.position = transform.position;
			billboards[ i ].transform.rotation = Quaternion.LookRotation( cameras[ i ].transform.forward );
		}
	}

	// Utility Methods
	public void LateStart()
	{
		RespawnBillboards();
	}

	public virtual void RespawnBillboards()
	{
		if( cameras != null )
		{
			cameras.Clear();

			foreach( GameObject billboard in billboards )
			{
				GameControl.BillboardPool.Despawn( billboard );
			}
			billboards.Clear();

			// Check the player count, create a billboard for each camera and set it to the respective layer
			for( int i = 0; i < GameControl.Players.Count; ++i )
			{

				GameObject cam = GameControl.Players[ i ].transform.Find( "Camera" ).gameObject;
				cameras.Add( cam.camera );
				GameObject billboard = GameControl.BillboardPool.Spawn( BillboardPrefab );
				billboard.layer = LayerMask.NameToLayer( "Camera " + ( i + 1 ) );
				billboard.transform.parent = transform;
				billboards.Add( billboard );
			}
		}
	}
}
