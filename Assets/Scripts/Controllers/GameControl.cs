using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour
{
	// Fields
	GameObject playerContainer;
	GameObject[] spawnPoints;

	// Properties
	public int LocalPlayerCount = 1;
	public List<GameObject> Players;

	// Statics
	public static ObjectPool BillboardPool;
	public static ObjectPool DronePool;
	public static ObjectPool MiscPool;
	public static ObjectPool PickupPool;
	public static ObjectPool PlayerPool;
	public static ObjectPool ProjectilePool;
	public static ObjectPool SharedPool;

	// Unity Methods
	void Awake()
	{
		// Persist this object between scenes
		GameObject.DontDestroyOnLoad( this );

		// Find object pools
		BillboardPool = GameObject.Find( "Billboard Pool" ).GetComponent<ObjectPool>();
		DronePool = GameObject.Find( "Drone Pool" ).GetComponent<ObjectPool>();
		MiscPool = GameObject.Find( "Misc Pool" ).GetComponent<ObjectPool>();
		PickupPool = GameObject.Find( "Pickup Pool" ).GetComponent<ObjectPool>();
		PlayerPool = GameObject.Find( "Player Pool" ).GetComponent<ObjectPool>();
		ProjectilePool = GameObject.Find( "Projectile Pool" ).GetComponent<ObjectPool>();
		SharedPool = GameObject.Find( "Shared Pool" ).GetComponent<ObjectPool>();

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
		bool mouseInWindowX = Input.mousePosition.x > 0 && Input.mousePosition.x < Screen.width;
		bool mouseInWindowY = Input.mousePosition.y > 0 && Input.mousePosition.y < Screen.height;
		if( !Screen.lockCursor && mouseInWindowX && mouseInWindowY && Input.GetMouseButtonDown( 0 ) )
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
		GameObject avatar = PlayerPool.Spawn( "Avatar" );
		avatar.transform.parent = player.transform;
		avatar.transform.position = spawnPoint.position;
		avatar.transform.rotation = spawnPoint.rotation;

		// Setup avatar input wrapper
		InputWrapper avatarInputScript = avatar.GetComponent<InputWrapper>();
		avatarInputScript.LocalPlayerIndex = Players.Count;
		avatarInputScript.Init();

		// Setup avatar script
		PlayerInstance avatarScript = avatar.GetComponent<PlayerInstance>();
		avatarScript.GameControl = this;
		
		// CAMERA
		// Instantiate a new camera object
		GameObject camera = PlayerPool.Spawn( "Camera" );
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

		// Setup overlay last
		PlayerOverlay avatarOverlay = avatar.transform.Find( "Overlay" ).GetComponent<PlayerOverlay>();
		avatarOverlay.GameControl = this;
		avatarOverlay.Player = player;
		avatarOverlay.LateStart();

		// Add to the player list
		Players.Add( player );
	}

	void SpawnPickup( PickupInfo.Type type, Vector3 position, Quaternion rotation )
	{
		// Get a blank pickup from the object pool, spawn the appropriate graphic and attach
		GameObject pickup = PickupPool.Spawn( "Pickup" );
		GameObject pickupMesh = null;
		switch( type )
		{
			case PickupInfo.Type.Rocket:
				pickupMesh = SharedPool.Spawn( "Rocket Drone" );
				break;
			case PickupInfo.Type.Mortar:
				pickupMesh = SharedPool.Spawn( "Mortar Drone" );
				break;
			case PickupInfo.Type.Seeker:
				pickupMesh = SharedPool.Spawn( "Seeker Drone" );
				break;
			default:
				break;
		}
		pickupMesh.transform.parent = pickup.transform;

		// Spawn and configure the pickup glow billboard
		GameObject pickupGlow = pickup.transform.Find( "Overlay" ).gameObject;
		Billboard pickupOverlay = pickupGlow.GetComponent<Billboard>();
		pickupOverlay.GameControl = this;
		pickupOverlay.LateStart();

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
			PlayerInstance playerScript = go.GetComponent<PlayerInstance>();

			// Ensure all drones are deactivated
			foreach( GameObject drone in playerScript.Drones )
			{
				DronePool.Despawn( drone );
			}
			playerScript.Drones.Clear();

			FollowCamera cameraScript = player.transform.Find( "Camera" ).GetComponent<FollowCamera>();
			cameraScript.DeathCam = true;

			// If killed by another player, target the camera at them
			if( killedBy != go.transform.parent.gameObject )
			{
				Transform target = killedBy.transform.Find( "Avatar/Marble" );
				if( target != null && target.gameObject.activeSelf )
				{
					cameraScript.Target = target;
				}
			}
			else
			{
				cameraScript.Target = null;
			}

			PlayerPool.Despawn( go );
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
			GameObject avatar = PlayerPool.Spawn( "Avatar" );
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
			avatarOverlay.Player = player;
			avatarOverlay.LateStart();

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
