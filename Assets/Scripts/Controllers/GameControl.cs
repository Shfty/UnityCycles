using UnityEngine;
using Enum = System.Enum;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour
{
	// Fields
	GameObject playerContainer;
	GameObject[] spawnPoints;
	float pickupTimer = 0f;
	Vector3 mapBounds;

	// Properties
	public int LocalPlayerCount = 1;
	int MaxPickups = 15;
	float PickupTimeout = 2f;
	public List<Material> OverlayMaterials;
	public List<GameObject> Players;
	public List<GameObject> Pickups;
	public List<GameObject> PlayerOverlays;
	public List<GameObject> PickupOverlays;

	// Statics
	public static GameControl Instance;
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

		// Set instance static
		Instance = this;

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

		// Spawn player overlays
		for( int i = 0; i < Players.Count; ++i )
		{
			GameObject playerOverlay = BillboardPool.Spawn( "Player Overlay" );
			ResetPlayerOverlay( playerOverlay, Players[ i ] );
			PlayerOverlays.Add( playerOverlay );
		}

		// Spawn pickups
		mapBounds = Terrain.activeTerrain.terrainData.size;

		// Rocket Drones
		for( int i = 0; i < 5; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( 0, mapBounds.x ) - mapBounds.x * .5f, 25f + Random.Range( 0f, 75f ), Random.Range( 0, mapBounds.z ) - mapBounds.z * .5f );
			Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
			SpawnPickup( PickupInfo.Type.Rocket, randomPosition, randomRotation );
		}

		// Mortar Drones
		for( int i = 0; i < 5; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( 0, mapBounds.x ) - mapBounds.x * .5f, 25f + Random.Range( 0f, 75f ), Random.Range( 0, mapBounds.z ) - mapBounds.z * .5f );
			Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
			SpawnPickup( PickupInfo.Type.Mortar, randomPosition, randomRotation );
		}

		// Seeker Drones
		for( int i = 0; i < 5; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( 0, mapBounds.x ) - mapBounds.x * .5f, 25f + Random.Range( 0f, 75f ), Random.Range( 0, mapBounds.z ) - mapBounds.z * .5f );
			Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
			SpawnPickup( PickupInfo.Type.Seeker, randomPosition, randomRotation );
		}

		// Spawn pickup overlays
		for( int i = 0; i < Pickups.Count; ++i )
		{
			GameObject pickupOverlay = BillboardPool.Spawn( "Pickup Overlay" );
			ResetPickupOverlay( pickupOverlay, Pickups[ i ] );
			PickupOverlays.Add( pickupOverlay );
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

		// Check for any deactivated pickups
		for( int i = 0; i < Pickups.Count; ++i )
		{
			if( !Pickups[ i ].activeSelf )
			{
				BillboardPool.Despawn( PickupOverlays[ i ] );
				Pickups.RemoveAt( i );
				PickupOverlays.RemoveAt( i );
			}
		}

		// Increment the pickup timer if there are less pickups than the limit
		if( Pickups.Count < MaxPickups )
		{
			pickupTimer += Time.deltaTime;
			if( pickupTimer >= PickupTimeout )
			{
				// Spawn random pickup
				int randomPickup = Random.Range( 1, Enum.GetNames( typeof( PickupInfo.Type ) ).Length );
				Vector3 randomPosition = new Vector3( Random.Range( 0, mapBounds.x ) - mapBounds.x * .5f, 25f + Random.Range( 0f, 75f ), Random.Range( 0, mapBounds.z ) - mapBounds.z * .5f );
				Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
				SpawnPickup( ( PickupInfo.Type )randomPickup, randomPosition, randomRotation );

				// Attach overlay
				GameObject pickupOverlay = BillboardPool.Spawn( "Pickup Overlay" );
				ResetPickupOverlay( pickupOverlay, Pickups[ Pickups.Count - 1 ] );
				PickupOverlays.Add( pickupOverlay );

				pickupTimer = 0f;
			}
		}
	}

	// Utility Methods
	void SpawnLocalPlayer( Transform spawnPoint, int idx )
	{
		// Instantiate a new player container
		GameObject player = new GameObject( "Player" );
		player.transform.parent = playerContainer.transform;
		player.AddComponent<Player>();

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
		
		// CAMERA
		// Instantiate a new camera object
		GameObject camera = PlayerPool.Spawn( "Camera" );
		camera.transform.position = avatar.transform.position;
		camera.transform.rotation = avatar.transform.rotation;
		camera.transform.parent = player.transform;

		// Tell the camera to follow the avatar
		AvatarCamera cameraScript = camera.GetComponent<AvatarCamera>();
		cameraScript.Target = avatar.transform.Find( "Marble" );

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

		// Setup transform
		pickup.transform.position = position;
		pickup.transform.rotation = rotation;

		// Set script properties
		PickupInstance pickupScript = pickup.GetComponent<PickupInstance>();
		pickupScript.Type = type;

		// Add to pickup list
		Pickups.Add( pickup );
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

	public void PickupGrabbed( PickupInstance pickup, Avatar player )
	{
		// If the player has an empty drone slot

		if( player.Drones.Count < 3 )
		{

			// Spawn a drone
			GameObject drone = GameControl.DronePool.Spawn( "Drone" );
			DroneInfo.Type droneType = new DroneInfo.Type();

			// Set type
			switch( pickup.Type )
			{
				case PickupInfo.Type.Rocket:
					droneType = DroneInfo.Type.Rocket;
					break;
				case PickupInfo.Type.Mortar:
					droneType = DroneInfo.Type.Mortar;
					break;
				case PickupInfo.Type.Seeker:
					droneType = DroneInfo.Type.Seeker;
					break;
				default:
					break;
			}

			// Position and rotate the drone to match this pickup
			drone.transform.position = pickup.transform.position;
			drone.transform.rotation = pickup.transform.rotation;

			// Setup the drone's scripts
			Drone droneScript = drone.GetComponent<Drone>();
			droneScript.Player = player.transform.parent.gameObject;
			droneScript.Type = droneType;
			droneScript.PooledStart();
			player.Drones.Add( drone );

			// Assign type-specific ammo
			switch( pickup.Type )
			{
				case PickupInfo.Type.Rocket:
					droneScript.Ammo = 3;
					break;
				case PickupInfo.Type.Mortar:
					droneScript.Ammo = 2;
					break;
				case PickupInfo.Type.Seeker:
					droneScript.Ammo = 1;
					break;
				default:
					break;
			}

			// Despawn drone graphics
			foreach( Transform child in pickup.transform )
			{
				SharedPool.Despawn( child.gameObject );
			}
			PickupPool.Despawn( pickup.gameObject );
		}
	}

	public void PlayerDeath( GameObject go, GameObject killedBy )
	{
		if( Players.Contains( go.transform.parent.gameObject ) )
		{
			// Find the player and it's instance script
			int playerIndex = Players.IndexOf( go.transform.parent.gameObject );
			GameObject player = Players[ playerIndex ];
			Avatar playerScript = go.GetComponent<Avatar>();

			// Ensure all drones are deactivated
			foreach( GameObject drone in playerScript.Drones )
			{
				drone.GetComponent<Drone>().Deactivate();
			}
			playerScript.Drones.Clear();

			AvatarCamera cameraScript = player.transform.Find( "Camera" ).GetComponent<AvatarCamera>();
			cameraScript.DeathCam = true;

			// If killed by another player, target the camera at them
			if( killedBy != go.transform.parent.gameObject )
			{
				// Trigger Game Rules callback
				GameRules.Instance.PlayerDeath( go, killedBy );

				// Set the victim's camera follow target
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

			// Disable billboard
			PlayerOverlay overlay = PlayerOverlays[ playerIndex ].GetComponent <PlayerOverlay>();
			overlay.gameObject.SetActive( false );

			PlayerPool.Despawn( go );
		}
	}

	void ResetPlayerOverlay( GameObject playerOverlay, GameObject player )
	{
		playerOverlay.GetComponent<KinematicHover>().Target = player.transform.Find( "Avatar/Marble" );
		playerOverlay.GetComponent<PlayerOverlay>().Player = player;
		playerOverlay.GetComponent<PlayerOverlay>().LateStart();
	}

	void ResetPickupOverlay( GameObject pickupOverlay, GameObject pickup )
	{
		pickupOverlay.GetComponent<KinematicHover>().Target = pickup.transform;
		Billboard billboardScript = pickupOverlay.GetComponent<Billboard>();
		// Setup material
		switch( pickup.GetComponent<PickupInstance>().Type )
		{
			case PickupInfo.Type.Rocket:
				billboardScript.Material = OverlayMaterials.Find( item => item.name == "Rocket Overlay" );
				break;
			case PickupInfo.Type.Mortar:
				billboardScript.Material = OverlayMaterials.Find( item => item.name == "Mortar Overlay" );
				break;
			case PickupInfo.Type.Seeker:
				billboardScript.Material = OverlayMaterials.Find( item => item.name == "Seeker Overlay" );
				break;
			default:
				break;
		}
		pickupOverlay.GetComponent<Billboard>().LateStart();
	}

	public void Respawn( GameObject go )
	{
		if( Players.Contains( go ) )
		{
			int playerIndex = Players.IndexOf( go );
			GameObject player = Players[ playerIndex ];

			int i = Random.Range( 0, spawnPoints.Length );
			Transform spawnPoint = spawnPoints[ i ].transform;

			AvatarCamera cameraScript = player.transform.Find( "Camera" ).GetComponent<AvatarCamera>();

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

			// Reenable and reset the appropriate overlay
			PlayerOverlay overlay = PlayerOverlays[ playerIndex ].GetComponent<PlayerOverlay>();
			overlay.gameObject.SetActive( true );
			ResetPlayerOverlay( PlayerOverlays[ playerIndex ], Players[ playerIndex ] );

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
