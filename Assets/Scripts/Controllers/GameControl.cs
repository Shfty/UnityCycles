using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameControl : MonoBehaviour
{
	// Fields
	ObjectPool playerPool;
	ObjectPool pickupPool;
	ObjectPool sharedPool;

	// Properties
	public GameObject CameraPrefab;
	public GameObject PlayerPrefab;
	public GameObject PickupPrefab;
	public GameObject RocketDronePrefab;
	public GameObject MortarDronePrefab;
	public GameObject SeekerDronePrefab;
	public int LocalPlayerCount = 1;
	public List<GameObject> Cameras;
	public List<GameObject> LocalPlayers;

	// Unity Methods
	void Awake()
	{
		// Persist this object between scenes
		GameObject.DontDestroyOnLoad( this );

		// Find object pools
		playerPool = GameObject.Find( "Player Pool" ).GetComponent<ObjectPool>();
		pickupPool = GameObject.Find( "Pickup Pool" ).GetComponent<ObjectPool>();
		sharedPool = GameObject.Find( "Shared Pool" ).GetComponent<ObjectPool>();

		// Instantiate player list
		LocalPlayers = new List<GameObject>();

		// Make sure the local player count is valid
		LocalPlayerCount = Mathf.Clamp( LocalPlayerCount, 1, 4 );
	}

	void Start()
	{
		// Spawn players
		List<GameObject> spawnPoints = GameObject.FindGameObjectsWithTag( "PlayerSpawn" ).ToList<GameObject>();
		for( int i = 0; i < LocalPlayerCount; ++i )
		{
			int random = Random.Range( 0, spawnPoints.Count );
			SpawnLocalPlayer( spawnPoints[ random ].transform, i );
			spawnPoints.RemoveAt( random );
		}

		// Spawn pickups
		Terrain terrain = GameObject.Find( "Terrain" ).GetComponent<Terrain>();
		Vector3 bounds = terrain.terrainData.size;

		// Rocket Drones
		for( int i = 0; i < 5; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( 0, bounds.x ) - bounds.x * .5f, 25f + Random.Range( 0f, 75f ), Random.Range( 0, bounds.z ) - bounds.z * .5f );
			Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
			SpawnPickup( PickupInfo.Type.Rocket, randomPosition, randomRotation );
		}

		// Mortar Drones
		for( int i = 0; i < 5; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( 0, bounds.x ) - bounds.x * .5f, 25f + Random.Range( 0f, 75f ), Random.Range( 0, bounds.z ) - bounds.z * .5f );
			Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
			SpawnPickup( PickupInfo.Type.Mortar, randomPosition, randomRotation );
		}

		// Seeker Drones
		for( int i = 0; i < 5; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( 0, bounds.x ) - bounds.x * .5f, 25f + Random.Range( 0f, 75f ), Random.Range( 0, bounds.z ) - bounds.z * .5f );
			Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
			SpawnPickup( PickupInfo.Type.Seeker, randomPosition, randomRotation );
		}
	}

	void Update()
	{
		// Grab the cursor when the window is clicked
		if( !Screen.lockCursor && Input.GetMouseButtonDown( 0 ) )
		{
			Screen.lockCursor = true;
		}

		// Un-grab it when escape is pressed
		if( Screen.lockCursor && Input.GetKeyDown( "escape" ) )
		{
			Screen.lockCursor = false;
		}
	}

	// Utility Methods
	void SpawnLocalPlayer( Transform spawnPoint, int idx )
	{
		// Instantiate a new player object
		GameObject player = playerPool.Spawn( PlayerPrefab );
		player.transform.position = spawnPoint.position;
		player.transform.rotation = spawnPoint.rotation;

		// Setup input wrapper
		InputWrapper inputScript = player.GetComponent<InputWrapper>();
		inputScript.LocalPlayerIndex = LocalPlayers.Count;
		inputScript.Init();
		
		// Instantiate a new camera object
		GameObject camera = playerPool.Spawn( CameraPrefab );
		camera.transform.position = player.transform.position;
		camera.transform.rotation = player.transform.rotation;
		Cameras.Add( camera );

		FollowCamera cameraScript = player.GetComponent<FollowCamera>();
		cameraScript.Camera = camera;

		// Setup movement script
		MarbleMovement movementScript = player.GetComponent<MarbleMovement>();
		movementScript.Camera = camera.transform;

		// Calculate camera viewport & culling mask
		camera.camera.rect = CalculateViewport( idx );
		int cameraMask = 0;
		for( int i = 0; i < 4; ++i )
		{
			if( i == idx )
				continue;

			int layerMask = 1 << LayerMask.NameToLayer( "Camera " + ( i + 1 ) );
			cameraMask |= layerMask;
		}
		Camera cameraComponent = camera.GetComponent<Camera>();
		cameraComponent.cullingMask = ~cameraMask;

		PlayerOverlay playerOverlay = player.transform.Find( "Overlay" ).GetComponent<PlayerOverlay>();
		playerOverlay.GameControl = this;

		// Add to the player list
		LocalPlayers.Add( player );
	}

	void SpawnPickup( PickupInfo.Type type, Vector3 position, Quaternion rotation )
	{
		// Get a blank pickup from the object pool, spawn the appropriate graphic and attach
		GameObject pickup = pickupPool.Spawn( PickupPrefab );
		GameObject pickupMesh = null;
		switch( type )
		{
			case PickupInfo.Type.Rocket:
				pickupMesh = sharedPool.Spawn( RocketDronePrefab );
				break;
			case PickupInfo.Type.Mortar:
				pickupMesh = sharedPool.Spawn( MortarDronePrefab );
				break;
			case PickupInfo.Type.Seeker:
				pickupMesh = sharedPool.Spawn( SeekerDronePrefab );
				break;
			default:
				break;
		}
		pickupMesh.transform.parent = pickup.transform;

		// Spawn and configure the pickup glow billboard
		GameObject pickupGlow = pickup.transform.Find( "Overlay" ).gameObject;
		Billboard pickupOverlay = pickupGlow.GetComponent<Billboard>();
		pickupOverlay.GameControl = this;

		// Setup transform
		pickup.transform.position = position;
		pickup.transform.rotation = rotation;

		// Set script properties
		PickupInstance pickupScript = pickup.GetComponent<PickupInstance>();
		pickupScript.Type = type;

		// Start the script
		pickupScript.Start();
	}

	// Setup camera rect based on player count and index
	Rect CalculateViewport( int idx )
	{
		Rect cameraRect = new Rect( 0f, 0f, 1f, 1f );

		switch( LocalPlayerCount )
		{
			case 1:
				cameraRect = new Rect( 0f, 0f, 1f, 1f );
				break;
			case 2:
				switch( idx )
				{
					case 0:
						cameraRect = new Rect( 0f, .5f, 1f, .5f );
						break;
					case 1:
						cameraRect = new Rect( 0f, 0f, 1f, .5f );
						break;
					default:
						break;
				}
				break;
			case 3:
				switch( idx )
				{
					case 0:
						cameraRect = new Rect( 0f, .5f, 1f, .5f );
						break;
					case 1:
						cameraRect = new Rect( 0f, 0f, .5f, .5f );
						break;
					case 2:
						cameraRect = new Rect( .5f, 0f, .5f, .5f );
						break;
					default:
						break;
				}
				break;
			case 4:
				switch( idx )
				{
					case 0:
						cameraRect = new Rect( 0f, .5f, .5f, .5f );
						break;
					case 1:
						cameraRect = new Rect( .5f, .5f, .5f, .5f );
						break;
					case 2:
						cameraRect = new Rect( 0f, 0f, .5f, .5f );
						break;
					case 3:
						cameraRect = new Rect( .5f, 0f, .5f, .5f );
						break;
					default:
						break;
				}
				break;
		}

		return cameraRect;
	}
}
