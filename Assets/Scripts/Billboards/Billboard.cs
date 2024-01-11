using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Billboard : MonoBehaviour
{
	// Fields
	protected List<Camera> cameras;
	protected List<GameObject> billboards;

	// Properties
	public GameObject BillboardPrefab;
	public Material Material;

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
		for( int i = 0; i < billboards.Count; ++i )
		{
			billboards[ i ].transform.position = transform.position;
			billboards[ i ].transform.rotation = Quaternion.LookRotation( cameras[ i ].transform.forward );
		}
	}

	// Utility Methods
	public virtual void LateStart()
	{
		if( cameras != null )
		{
			// Check the player count, create a billboard for each camera and set it to the respective layer
			for( int i = 0; i < GameControl.Instance.Players.Count; ++i )
			{

				GameObject cam = GameControl.Instance.Players[ i ].transform.Find( "Camera" ).gameObject;
				cameras.Add( cam.GetComponent<Camera>() );
				GameObject billboard = GameControl.BillboardPool.Spawn( BillboardPrefab );
				billboard.layer = LayerMask.NameToLayer( "Camera " + ( i + 1 ) );
				billboard.transform.parent = transform;
				billboard.transform.position = transform.position;
				billboard.transform.rotation = Quaternion.LookRotation( cameras[ i ].transform.forward );
				billboard.GetComponent<MeshRenderer>().material = Material;
				billboards.Add( billboard );
			}
		}
	}

	public void SetInvisible( bool invisible )
	{
		foreach( GameObject billboard in billboards )
		{
			billboard.GetComponent<MeshRenderer>().enabled = !invisible;
		}
	}

	public void DespawnSelf()
	{
		cameras.Clear();

		// Reset billboard state
		SetInvisible( false );

		// Despawn children
		foreach( GameObject billboard in billboards )
		{
			GameControl.BillboardPool.Despawn( billboard );
		}
		// Respawn self
		billboards.Clear();

		GameControl.BillboardPool.Despawn( gameObject );
	}
}
