using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Billboard : MonoBehaviour
{
	// Fields
	protected List<Camera> cameras;
	protected List<GameObject> billboards;
	ObjectPool billboardPool;

	// Properties
	public GameControl GameControl;
	public GameObject BillboardPrefab;

	// Unity Methods
	public virtual void Start()
	{
		billboardPool = GameObject.Find( "Billboard Pool" ).GetComponent<ObjectPool>();

		// Instantiate camera and billboard lists
		cameras = new List<Camera>();
		billboards = new List<GameObject>();

		// Check the player count, create a billboard for each camera and set it to the respective layer
		for( int i = 0; i < GameControl.Cameras.Count; ++i )
		{

			cameras.Add( GameControl.Cameras[ i ].camera );
			GameObject billboard = billboardPool.Spawn( BillboardPrefab );
			billboard.layer = LayerMask.NameToLayer( "Camera " + ( i + 1 ) );
			billboard.transform.parent = transform;
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
}
