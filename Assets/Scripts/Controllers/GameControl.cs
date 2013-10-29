using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour
{
	// Fields
	ObjectPool playerPool;
	ObjectPool pickupPool;
	ObjectPool sharedPool;
	GameObject playerContainer;
	GameObject[] spawnPoints;
	GameObject[] mapCameraAnchors;

	// Properties
	public GameObject AvatarPrefab;
	public GameObject CameraPrefab;
	public GameObject PickupPrefab;
	public GameObject RocketDronePrefab;
	public GameObject MortarDronePrefab;
	public GameObject SeekerDronePrefab;
	public int LocalPlayerCount = 1;
	public List<GameObject> Players;

	// Unity Methods
	void Awake()
	{
		// Persist this object between scenes
		GameObject.DontDestroyOnLoad( this );

		// Find object pools
		playerPool = GameObject.Find( "Player Pool" ).GetComponent<ObjectPool>();
		pickupPool = GameObject.Find( "Pickup Pool" ).GetComponent<ObjectPool>();
		sharedPool = GameObject.Find( "Shared Pool" ).GetComponent<ObjectPool>();

		// Create player container
		playerContainer = new GameObject( "Players" );

		// Instantiate player list
		Players = new List<GameObject>();

		// Make sure the local player count is valid
		LocalPlayerCount = Mathf.Clamp( LocalPlayerCount, 1, 4 );
	}

	void Start()
	{
		// Get spawn points and map camera anchors
		spawnPoints = GameObject.FindGameObjectsWithTag( "PlayerSpawn" );
		mapCameraAnchors = GameObject.FindGameObjectsWithTag( "MapCamera" );

		// Spawn players
		for( int i = 0; i < LocalPlayerCount; ++i )
		{
			SpawnLocalPlayer( spawnPoints[ i + 1 ].transform, i );
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
		// Instantiate a new player container
		GameObject player = new GameObject( "Player" );
		player.transform.parent = playerContainer.transform;

		// AVATAR
		// Spawn it's world avatar
		GameObject avatar = playerPool.Spawn( AvatarPrefab );
		avatar.transform.position = spawnPoint.position;
		avatar.transform.rotation = spawnPoint.rotation;
		avatar.transform.parent = player.transform;

		// Setup avatar input wrapper
		InputWrapper avatarInputScript = avatar.GetComponent<InputWrapper>();
		avatarInputScript.LocalPlayerIndex = Players.Count;
		avatarInputScript.Init();

		// Setup avatar and overlay GameControl references
		PlayerInstance avatarScript = avatar.GetComponent<PlayerInstance>();
		avatarScript.GameControl = this;
		PlayerOverlay avatarOverlay = avatar.transform.Find( "Overlay" ).GetComponent<PlayerOverlay>();
		avatarOverlay.GameControl = this;
		
		// CAMERA
		// Instantiate a new camera object
		GameObject camera = playerPool.Spawn( CameraPrefab );
		camera.transform.position = avatar.transform.position;
		camera.transform.rotation = avatar.transform.rotation;
		camera.transform.parent = player.transform;

		// Tell the camera to follow the avatar
		FollowCamera cameraScript = camera.GetComponent<FollowCamera>();
		cameraScript.Target = avatar.transform.Find( "Marble" );
		cameraScript.GameControl = this;

		// Setup camera input wrapper
		InputWrapper cameraInputScript = camera.GetComponent<InputWrapper>();
		cameraInputScript.LocalPlayerIndex = Players.Count;
		cameraInputScript.Init();

		// Setup avatar movement script
		MarbleMovement movementScript = avatar.GetComponent<MarbleMovement>();
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

		// Add to the player list
		Players.Add( player );
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

	public void PlayerDeath( GameObject go, GameObject killedBy )
	{
		if( Players.Contains( go.transform.parent.gameObject ) )
		{
			GameObject player = Players[ Players.IndexOf( go.transform.parent.gameObject ) ];

			FollowCamera cameraScript = player.transform.Find( "Camera" ).GetComponent<FollowCamera>();
			cameraScript.DeathCam = true;

			// If killed by another player, target the camera at them
			if( killedBy != go.transform.parent.gameObject )
			{
				cameraScript.Target = killedBy.transform.Find( "Avatar/Marble" );
			}
			else // If killed by self, switch to a random map camera
			{
				// Randomly pick a camera anchor
				int i = Random.Range( 0, mapCameraAnchors.Length );
				GameObject cameraAnchor = mapCameraAnchors[ i ];
				cameraScript.Target = cameraAnchor.transform;
			}

			go.GetComponent<PlayerInstance>().Deactivate();
		}
	}

	public void Respawn( GameObject go )
	{
		if( Players.Contains( go ) )
		{
			GameObject player = Players[ Players.IndexOf( go ) ];

			int i = Random.Range( 0, spawnPoints.Length );
			Transform spawnPoint = spawnPoints[ i ].transform;

			FollowCamera cameraScript = player.transform.Find( "Camera" ).GetComponent<FollowCamera>();

			// AVATAR
			// Spawn it's world avatar
			GameObject avatar = playerPool.Spawn( AvatarPrefab );
			avatar.transform.position = spawnPoint.position;
			avatar.transform.rotation = spawnPoint.rotation;
			avatar.transform.parent = player.transform;

			// Setup avatar input wrapper
			InputWrapper avatarInputScript = avatar.GetComponent<InputWrapper>();
			avatarInputScript.LocalPlayerIndex = Players.IndexOf( player );
			avatarInputScript.Init();

			// Setup avatar and overlay GameControl references
			PlayerInstance avatarScript = avatar.GetComponent<PlayerInstance>();
			avatarScript.GameControl = this;
			PlayerOverlay avatarOverlay = avatar.transform.Find( "Overlay" ).GetComponent<PlayerOverlay>();
			avatarOverlay.GameControl = this;

			// Setup avatar movement script
			MarbleMovement movementScript = avatar.GetComponent<MarbleMovement>();
			movementScript.Camera = player.transform.Find( "Camera" );

			// CAMERA
			// Tell the camera to follow the avatar
			cameraScript.Target = avatar.transform.Find( "Marble" );
			cameraScript.DeathCam = false;
		}
	}
}
