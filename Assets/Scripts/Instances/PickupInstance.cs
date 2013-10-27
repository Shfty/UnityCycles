using UnityEngine;
using System.Collections;

public class PickupInstance : PooledObject
{
	// Fields
	PickupInfo.Type initType;
	bool initFalling;
	Vector3 initVelocity;

	int terrainMask;

	// Enums

	// Properties
	public ObjectPool ObjectPool;
	public GameObject DronePrefab;
	public GameObject RocketDronePrefab;
	public GameObject MortarDronePrefab;
	public GameObject SeekerDronePrefab;
	public PickupInfo.Type Type = PickupInfo.Type.None;
	public bool Falling = true;
	public Vector3 Velocity;

	// Unity Methods
	public override void OnEnable()
	{
		base.OnEnable();

		// Store pickup-related init state
		initType = Type;
		initFalling = Falling;
		initVelocity = Velocity;
	}

	public override void OnDisable()
	{
		base.OnDisable();

		// Restore pickup-related init state
		Type = initType;
		Falling = initFalling;
		Velocity = initVelocity;
	}

	public void Start()
	{
		// Store the terrain layer mask
		terrainMask = 1 << LayerMask.NameToLayer( "Terrain" );
	}

	void Update()
	{
		// Rotate at a fixed rate
		transform.rotation *= Quaternion.AngleAxis( PickupInfo.Properties.RotatePerSecond * Time.deltaTime, Vector3.up );

		if( Falling )
		{
			// If still in midair, increment velocity and check for collision with the terrain
			Velocity += -Vector3.up * PickupInfo.Properties.Gravity * Time.deltaTime;

			if( Physics.CheckSphere( transform.position, PickupInfo.Properties.TerrainCollisionRadius, terrainMask ) )
			{
				Falling = false;
				Velocity = Vector3.zero;
			}
		}
		else
		{
			// Otherwise, check if embedded in the terrain and push out if so
			while( Physics.CheckSphere( transform.position, PickupInfo.Properties.TerrainCollisionRadius, terrainMask ) )
			{
				transform.position += new Vector3( 0f, PickupInfo.Properties.SmallValue, 0f );
			}
		}

		// Move down relative to velocity
		transform.position += Velocity * Time.deltaTime;
	}

	void OnDrawGizmos()
	{
		// Draw the debug pickup radius
		if( GameInfo.Properties.Debug )
		{
			Gizmos.DrawWireSphere( transform.position, PickupInfo.Properties.TerrainCollisionRadius );
		}
	}

	void OnTriggerEnter( Collider col )
	{
		// check if a player has grabbed this pickup
		if( col.tag == "Player" )
		{
			// Collider is a Marble, get player as parent
			GameObject player = col.transform.parent.gameObject;
			PlayerInstance playerScript = player.GetComponent<PlayerInstance>();

			// If the player has an empty drone slot

			if( playerScript.Drones.Count < 3 )
			{

				// Spawn a drone
				GameObject drone = ObjectPool.Spawn( DronePrefab );
				GameObject droneMesh = null;
				DroneInfo.Type droneType = new DroneInfo.Type();

				// Set type and spawn the relevant graphics
				switch( Type )
				{
					case PickupInfo.Type.Rocket:
						droneMesh = ObjectPool.Spawn( RocketDronePrefab );
						droneType = DroneInfo.Type.Rocket;
						break;
					case PickupInfo.Type.Mortar:
						droneMesh = ObjectPool.Spawn( MortarDronePrefab );
						droneType = DroneInfo.Type.Mortar;
						break;
					case PickupInfo.Type.Seeker:
						droneMesh = ObjectPool.Spawn( SeekerDronePrefab );
						droneType = DroneInfo.Type.Seeker;
						break;
					default:
						break;
				}

				// Parent it to the blank drone
				droneMesh.transform.parent = drone.transform;

				// Position and rotate the drone to match this pickup
				drone.transform.position = transform.position;
				drone.transform.rotation = transform.rotation;

				// Setup the drone's scripts
				Drone droneScript = drone.GetComponent<Drone>();
				droneScript.Player = player;
				droneScript.Type = droneType;
				droneScript.PooledStart();

				KinematicHover droneHover = drone.GetComponent<KinematicHover>();
				droneHover.Target = playerScript.DroneAnchors[ playerScript.Drones.Count ];
				playerScript.Drones.Add( drone );

				// Assign type-specific ammo
				switch( Type )
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

				Deactivate();
			}
		}
	}

	// Utility Methods
	public override void Deactivate()
	{
		Billboard billboard = transform.Find("Overlay").GetComponent<Billboard>();
		billboard.Deactivate();

		base.Deactivate();
	}
}
