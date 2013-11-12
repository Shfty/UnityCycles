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
	float prevTimeSinceStart;
	float independentDeltaTime;
	float targetTimeScale = 0f;

	// Properties
	public int LocalPlayerCount = 1;
	public int MaxPickups = 15;
	public float PickupTimeout = 2f;
	public GameObject MapCamera;
	public bool GameActive = true;
	public float TimeLerpFactor = 3f;
	public float TerrainDetailFactor = 5.12f;
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
	}

	void Start()
	{
		// Check for Game Parameters
		GameObject go = GameObject.Find( "Game Parameters" );
		if( go != null )
		{
			GameParameters gameParameters = go.GetComponent<GameParameters>();
			LocalPlayerCount = gameParameters.PlayerCount;
			MaxPickups = gameParameters.MaxPickups;
			PickupTimeout = gameParameters.PickupRespawnDelay;

			GameRules gameRules = GameObject.Find( "Game Rules" ).GetComponent<GameRules>();
			gameRules.ScoreLimit = gameParameters.ScoreLimit;

			Terrain terrain = Terrain.activeTerrain;
			terrain.terrainData.heightmapResolution = (int)( gameParameters.TerrainSize * TerrainDetailFactor );
			terrain.terrainData.size = new Vector3( gameParameters.TerrainSize, gameParameters.TerrainHeight, gameParameters.TerrainSize );
			terrain.transform.position = new Vector3( -gameParameters.TerrainSize * .5f, 0f, -gameParameters.TerrainSize * .5f );
			terrain.Flush();
			terrain.GetComponent<WorleyNoiseTerrain>().GridDivisions = new Vector2( gameParameters.TerrainNoiseSubdivisions, gameParameters.TerrainNoiseSubdivisions );
			terrain.GetComponent<WorleyNoiseTerrain>().MaxPointsPerCell = gameParameters.TerrainTurbulence;
			terrain.GetComponent<WorleyNoiseTerrain>().DistanceMetric = gameParameters.TerrainType;
			terrain.GetComponent<TerrainTypeTexture>().Alternate = gameParameters.AltTerrain;
			terrain.GetComponent<TerrainBoundary>().Sides = gameParameters.ArenaSides;
			if( gameParameters.UseRandomSeed )
			{
				terrain.GetComponent<WorleyNoiseTerrain>().UseRandomSeed = true;
				terrain.GetComponent<WorleyNoiseTerrain>().RandomSeed = gameParameters.RandomSeed;
			}
		}

		// Make sure the local player count is valid
		LocalPlayerCount = Mathf.Clamp( LocalPlayerCount, 0, 4 );

		// Begin with time frozen
		Time.timeScale = 0f;

		// Get spawn points and map camera anchors
		spawnPoints = GameObject.FindGameObjectsWithTag( "PlayerSpawn" );

		// Setup and start the game
		ResetGame();
		StartGame();
	}

	void Update()
	{
		// Track delta time independently of Time class
		independentDeltaTime = Time.realtimeSinceStartup - prevTimeSinceStart;

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

		if( GameActive )
		{
			// Increment the pickup timer if there are less pickups than the limit
			if( Pickups.Count < MaxPickups )
			{
				pickupTimer += Time.deltaTime;
				if( pickupTimer >= PickupTimeout )
				{
					// Spawn random pickup
					int randomPickup = Random.Range( 1, Enum.GetNames( typeof( PickupInfo.Type ) ).Length );
					Vector3 randomPosition = new Vector3( Random.Range( 0, mapBounds.x ) - mapBounds.x * .5f, 25f + mapBounds.y * .5f, Random.Range( 0, mapBounds.z ) - mapBounds.z * .5f );
					Quaternion randomRotation = Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up );
					SpawnPickup( (PickupInfo.Type)randomPickup, randomPosition, randomRotation );

					pickupTimer = 0f;
				}
			}
		}

		// Lerp timeScale
		Time.timeScale = Mathf.Lerp( Time.timeScale, targetTimeScale, TimeLerpFactor * independentDeltaTime );

		// Track previous time
		prevTimeSinceStart = Time.realtimeSinceStartup;
	}

	// Utility Methods
	public void ResetGame()
	{
		// Despawn all objects
		foreach( GameObject playerOverlay in PlayerOverlays )
		{
			playerOverlay.GetComponent<PlayerOverlay>().DespawnSelf();
		}

		foreach( GameObject pickupOverlay in PickupOverlays )
		{
			pickupOverlay.GetComponent<Billboard>().DespawnSelf();
		}

		DronePool.DespawnAll();
		MiscPool.DespawnAll();
		PickupPool.DespawnAll();
		PlayerPool.DespawnAll();
		ProjectilePool.DespawnAll();
		SharedPool.DespawnAll();

		// Clear references
		foreach( GameObject player in Players )
		{
			Destroy( player );
		}
		Players.Clear();
		Pickups.Clear();
		PlayerOverlays.Clear();
		PickupOverlays.Clear();

		// Reset GameRules
		GameRules.Instance.Reset();

		mapBounds = Terrain.activeTerrain.terrainData.size;
		// Spawn players
		for( int i = 0; i < LocalPlayerCount; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( 0, mapBounds.x ) - mapBounds.x * .5f, 25f, Random.Range( 0, mapBounds.z ) - mapBounds.z * .5f );
			SpawnLocalPlayer( randomPosition, i );
		}

		// Spawn player overlays
		for( int i = 0; i < Players.Count; ++i )
		{
			GameObject playerOverlay = BillboardPool.Spawn( "Player Overlay" );
			ResetPlayerOverlay( playerOverlay, Players[ i ] );
			PlayerOverlays.Add( playerOverlay );
			playerOverlay.GetComponent<PlayerOverlay>().LateStart();
		}

		// Spawn pickups
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
	}

	public void StartGame()
	{
		targetTimeScale = 1f;
		GameActive = true;
	}

	public void EndGame( GameObject winner )
	{
		foreach( GameObject player in Players )
		{
			player.SendMessage( "GameOver", winner, SendMessageOptions.DontRequireReceiver );
			Transform avatarTransform = player.transform.Find( "Avatar" );
			if( avatarTransform != null )
			{
				avatarTransform.SendMessage( "GameOver", winner, SendMessageOptions.DontRequireReceiver );
			}
			player.transform.Find( "Camera" ).SendMessage( "GameOver", winner, SendMessageOptions.DontRequireReceiver );
		}

		GameObject.Find( "Overlay Camera" ).SendMessage( "GameOver", winner, SendMessageOptions.DontRequireReceiver );

		GameActive = false;

		targetTimeScale = 0f;
	}

	// PLAYERS
	GameObject SpawnPooledAvatar( Vector3 position, Quaternion rotation, GameObject player )
	{
		// Spawn it's world avatar
		GameObject avatar = PlayerPool.Spawn( "Avatar" );
		avatar.transform.parent = player.transform;
		avatar.transform.position = position;
		avatar.transform.rotation = rotation;
		return avatar;
	}

	GameObject SpawnPooledCamera( Vector3 position, Quaternion rotation, GameObject player )
	{
		// Instantiate a new camera object
		GameObject camera = PlayerPool.Spawn( "Camera" );
		camera.transform.position = position;
		camera.transform.rotation = rotation;
		camera.transform.parent = player.transform;
		return camera;
	}

	void BindPlayerMembers( GameObject player )
	{
		// Find gameobjects
		GameObject camera = player.transform.Find( "Camera" ).gameObject;
		GameObject avatar = player.transform.Find( "Avatar" ).gameObject;

		// Find components
		Avatar avatarScript = avatar.GetComponent<Avatar>();
		MarbleMovement marbleScript = avatar.GetComponent<MarbleMovement>();
		WheelOrientation wheelOrientation = avatar.GetComponent<WheelOrientation>();
		WheelParticles wheelParticles = avatar.GetComponent<WheelParticles>();

		AvatarCamera cameraScript = camera.GetComponent<AvatarCamera>();
		AvatarGUI guiScript = camera.GetComponent<AvatarGUI>();

		// Tell the camera to follow the avatar
		cameraScript.Target = avatar.transform.Find( "Marble" );

		// Setup avatar movement camera reference
		marbleScript.Camera = camera.transform;

		// Setup InputWrapper references
		InputWrapper playerInput = player.GetComponent<InputWrapper>();

		avatarScript.InputWrapper = playerInput;
		marbleScript.InputWrapper = playerInput;
		wheelOrientation.InputWrapper = playerInput;
		wheelParticles.InputWrapper = playerInput;

		cameraScript.InputWrapper = playerInput;
		guiScript.InputWrapper = playerInput;
	}

	void SpawnLocalPlayer( Vector3 position, int idx )
	{
		// Instantiate a new player container
		GameObject player = new GameObject( "Player" );
		player.transform.parent = playerContainer.transform;
		InputWrapper playerInput = player.AddComponent<InputWrapper>();
		playerInput.LocalPlayerIndex = Players.Count;
		playerInput.WeakRumbleDecayRate = 12f;
		playerInput.StrongRumbleDecayRate = 6f;
		playerInput.Init();

		// Spawn child objects
		GameObject avatar = SpawnPooledAvatar( position, Quaternion.identity, player );
		GameObject camera = SpawnPooledCamera( position, Quaternion.identity, player );

		// Connect the player and it's children
		BindPlayerMembers( player );

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

	public void RespawnPlayer( GameObject player )
	{
		if( Players.Contains( player ) )
		{
			// AVATAR
			int playerIndex = Players.IndexOf( player );

			// Spawn it's world avatar
			Vector3 randomPosition = new Vector3( Random.Range( 0, mapBounds.x ) - mapBounds.x * .5f, 25f + mapBounds.y * .5f, Random.Range( 0, mapBounds.z ) - mapBounds.z * .5f );
			SpawnPooledAvatar( randomPosition, Quaternion.identity, player );

			// Reenable and reset the appropriate overlay
			PlayerOverlay overlay = PlayerOverlays[ playerIndex ].GetComponent<PlayerOverlay>();
			overlay.SetInvisible( false );
			ResetPlayerOverlay( PlayerOverlays[ playerIndex ], Players[ playerIndex ] );

			// Connect the player and it's children
			BindPlayerMembers( player );

			// Reset the GUI DeathCam
			AvatarGUI cameraGui = player.transform.Find( "Camera" ).GetComponent<AvatarGUI>();
			cameraGui.DeathCam = false;
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
			AvatarGUI guiScript = player.transform.Find( "Camera" ).GetComponent<AvatarGUI>();
			guiScript.DeathCam = true;

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
			PlayerOverlay overlay = PlayerOverlays[ playerIndex ].GetComponent<PlayerOverlay>();
			overlay.SetInvisible( true );

			PlayerPool.Despawn( go );
		}
	}

	void ResetPlayerOverlay( GameObject playerOverlay, GameObject player )
	{
		playerOverlay.GetComponent<KinematicHover>().Target = player.transform.Find( "Avatar/Marble" );
		playerOverlay.GetComponent<PlayerOverlay>().Player = player;
	}

	Rect CalculateViewport( int idx )
	{
		// Setup camera rect based on player count and index
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

	// PICKUPS
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

		// Spawn and attach overlay
		GameObject pickupOverlay = BillboardPool.Spawn( "Pickup Overlay" );
		ResetPickupOverlay( pickupOverlay, pickup );
		PickupOverlays.Add( pickupOverlay );
		pickupOverlay.GetComponent<Billboard>().LateStart();
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

			// Despawn pickup graphics
			foreach( Transform child in pickup.transform )
			{
				SharedPool.Despawn( child.gameObject );
			}

			int pickupIdx = Pickups.IndexOf( pickup.gameObject );
			PickupOverlays[ pickupIdx ].GetComponent<Billboard>().DespawnSelf();
			PickupPool.Despawn( pickup.gameObject );
			PickupOverlays.RemoveAt( pickupIdx );
			Pickups.RemoveAt( pickupIdx );
		}
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
	}
}
