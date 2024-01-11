using UnityEngine;
using System.Collections;

public class Drone : MonoBehaviour
{
	// Fields
	Quaternion targetRotation;
	LineRenderer aimingLine = null;
	Projector aimingHolo = null;
	Transform lockTarget = null;
	GameObject droneMesh = null;
	float reloadTimer = 0f;

	GameObject initPlayer;
	DroneInfo.Type initType;
	int initAmmo;
	bool initAim = false;
	Vector3 initAimVector;
	Vector3 initAimPoint;

	// Properties
	public GameObject Player;
	public LayerMask raycastAimMask;
	public LayerMask raycastLockMask;
	public DroneInfo.Type Type;
	public int Ammo;
	public float ReloadTimeout = 1f;
	public bool Aim;
	public Vector3 AimVector;
	public Vector3 AimPoint;

	// Unity Methods
	public void Awake()
	{
		// Store initial drone-related state
		initPlayer = Player;
		initType = Type;
		initAmmo = Ammo;
		initAim = Aim;
		initAimVector = AimVector;
		initAimPoint = AimPoint;
	}

	public void OnEnable()
	{
		// Restore initial drone-related state
		Player = initPlayer;
		Type = initType;
		Ammo = initAmmo;
		Aim = initAim;
		AimVector = initAimVector;
		AimPoint = initAimPoint;

		aimingHolo = null;
	}

	public void PooledStart()
	{
		// Spawn the aiming line
		aimingLine = GameControl.MiscPool.Spawn( "Line Renderer" ).GetComponent<LineRenderer>();
		aimingLine.transform.parent = transform;
		aimingLine.gameObject.layer = Player.transform.Find( "Camera" ).GetComponent<AvatarCamera>().RenderLayer;

		// Configure the aiming line per-type and spawn type-specific extras
		switch( Type )
		{
			case DroneInfo.Type.Rocket:
				// Mesh
				droneMesh = GameControl.SharedPool.Spawn( "Rocket Drone" );

				// LineRenderer
				aimingLine.SetVertexCount( 2 );
				break;
			case DroneInfo.Type.Mortar:
				// Mesh
				droneMesh = GameControl.SharedPool.Spawn( "Mortar Drone" );

				// LineRenderer
				aimingLine.SetVertexCount( DroneInfo.Mortar.AimLineSubdivisions + 1 );

				// Aim Holo
				aimingHolo = GameControl.MiscPool.Spawn( "Mortar Holo" ).GetComponent<Projector>();
				aimingHolo.gameObject.layer = Player.transform.Find( "Camera" ).GetComponent<AvatarCamera>().RenderLayer;
				break;
			case DroneInfo.Type.Seeker:
				// Mesh
				droneMesh = GameControl.SharedPool.Spawn( "Seeker Drone" );

				// LineRenderer
				aimingLine.SetVertexCount( 3 );
				break;
			case DroneInfo.Type.ShotgunStorm:
				// Mesh
				droneMesh = GameControl.SharedPool.Spawn( "Shotgun Storm Drone" );

				// LineRenderer
				aimingLine.SetVertexCount( 2 );
				break;
			case DroneInfo.Type.SlagCannon:
				// Mesh
				droneMesh = GameControl.SharedPool.Spawn( "Slag Cannon Drone" );

				// LineRenderer
				aimingLine.SetVertexCount( 2 );
				break;
			default:
				break;
		}

		// Parent it to the blank drone
		droneMesh.transform.parent = transform;
		droneMesh.transform.position = transform.position;
		droneMesh.transform.rotation = transform.rotation;

		// Inform the player's GUI that this drone now exists
		Player.transform.Find( "Camera" ).GetComponent<AvatarGUI>().DroneSpawned( this );
	}

	void FixedUpdate()
	{
		// If the drone is a seeker and currently aiming, check for players in sightline and lock on
		// Otherwise, nullify the lock target
		if( Type == DroneInfo.Type.Seeker && Aim )
		{
			Ray lockRay = new Ray( transform.position, transform.forward );
			RaycastHit[] raycastHits = Physics.SphereCastAll( lockRay, 10f, 5000f, raycastLockMask );
			foreach( RaycastHit hitInfo in raycastHits )
			{
				if( hitInfo.collider.transform.parent.parent.gameObject != Player )
				{
					lockTarget = hitInfo.collider.transform;
					break;
				}
				else if( lockTarget != null )
				{
					lockTarget = null;
				}
			}
		}
		else if( lockTarget != null )
		{
			lockTarget = null;
		}
	}

	void Update()
	{
		// If the aiming holo is present, set it's position and enable/disable
		if( aimingHolo != null )
		{
			aimingHolo.transform.position = AimPoint + Vector3.up * ProjectileInfo.Properties.MortarShell.TargetYOffset;
			if( Aim && !aimingHolo.enabled )
			{
				aimingHolo.enabled = true;
			}
			else if( !Aim && aimingHolo.enabled )
			{
				aimingHolo.enabled = false;
			}
		}

		// Calculate camera-relative aim and store hit information
		Camera camera = Player.transform.Find( "Camera" ).GetComponent<Camera>();

		Ray aimRay = camera.ViewportPointToRay( new Vector3( .5f, .5f ) );
		RaycastHit rayHitInfo;
		Physics.Raycast( aimRay, out rayHitInfo, 1000f, raycastAimMask );

		AimVector = rayHitInfo.point - transform.position;
		AimPoint = rayHitInfo.point;

		// Type-specific drone orientation
		Avatar avatar = Player.transform.Find( "Avatar" ).GetComponent<Avatar>();
		bool isActive = avatar.Drones[ avatar.ActiveDroneIndex ].GetComponent<Drone>() == this;
		switch( Type )
		{
			case DroneInfo.Type.Rocket:
				Quaternion upRotation = Quaternion.AngleAxis( 90f, Vector3.Cross( AimVector, Vector3.up ) ) * Quaternion.LookRotation( AimVector );
				if( isActive )
				{
					targetRotation = Quaternion.Slerp( upRotation, Quaternion.LookRotation( AimVector ), 1f - reloadTimer / ReloadTimeout );
				}
				else
				{
					targetRotation = upRotation;
				}
				break;
			case DroneInfo.Type.Mortar:
				// Calculate yaw
				Vector3 av = AimVector;
				av.y = 0;
				Vector3 hcf = Vector3.Cross( av, Vector3.forward );
				float dir = Vector3.Dot( hcf, Vector3.up );
				float yaw = Vector3.Angle( av, Vector3.forward ) * ( dir >= 0 ? -1 : 1 );

				float pitch;
				if( isActive )
				{
					// Calculate pitch
					float x = transform.position.x - AimPoint.x;
					float z = transform.position.z - AimPoint.z;

					float v = ProjectileInfo.Properties.MortarShell.InitialForce;

					float v2 = v * v;
					float v4 = v * v * v * v;
					float g = -Physics.gravity.y;
					float d = Mathf.Sqrt( x * x + z * z );
					float d2 = d * d;
					float y = ( AimPoint.y - transform.position.y ) + ProjectileInfo.Properties.MortarShell.TargetYOffset;

					float sqrFormula = v4 - g * ( ( g * d2 ) + ( 2 * y * v2 ) );
					float divisor = g * d;

					pitch = Mathf.Atan( ( v2 - Mathf.Sqrt( sqrFormula ) ) / divisor );

					pitch += Mathf.Deg2Rad * 45f * reloadTimer / ReloadTimeout;
				}
				else
				{
					pitch = Mathf.Deg2Rad * 90f;
				}

				// rotate
				targetRotation = Quaternion.AngleAxis( yaw, Vector3.up ) * Quaternion.AngleAxis( -pitch * Mathf.Rad2Deg, Vector3.right );
				break;
			case DroneInfo.Type.Seeker:
				Quaternion downRotation = Quaternion.AngleAxis( -135, Vector3.Cross( AimVector, Vector3.up ) ) * Quaternion.LookRotation( AimVector );
				if( isActive )
				{
					targetRotation = Quaternion.Slerp( downRotation, Quaternion.LookRotation( AimVector ), 1f - reloadTimer / ReloadTimeout );
				}
				else
				{
					targetRotation = downRotation;
				}
				break;
			case DroneInfo.Type.ShotgunStorm:
			case DroneInfo.Type.SlagCannon:
				gameObject.GetComponent<KinematicHover>().Offset = -transform.forward * reloadTimer / ReloadTimeout;

				Quaternion inactiveRotation = Quaternion.AngleAxis( 90f, Vector3.Cross( AimVector, Vector3.up ) ) * Quaternion.LookRotation( AimVector );
				if( isActive )
				{
					targetRotation = Quaternion.LookRotation( AimVector );
				}
				else
				{
					targetRotation = inactiveRotation;
				}
				break;
			default:
				break;
		}

		// Interpolate toward the target rotation
		transform.rotation = Quaternion.Slerp( transform.rotation, targetRotation, 20f * Time.deltaTime );

		// Reload timer
		if( reloadTimer > 0f )
		{
			reloadTimer = Mathf.Max( reloadTimer -= Time.deltaTime, 0f );
		}
	}

	void LateUpdate()
	{
		// Configure aiming LineRenderer
		if( Aim )
		{
			if( !aimingLine.enabled )
			{
				aimingLine.enabled = true;
			}

			float opacity = 1f - reloadTimer / ReloadTimeout;

			switch( Type )
			{
				case DroneInfo.Type.Rocket:
					// Straight line
					aimingLine.SetPosition( 0, transform.position );
					Vector3 aimPointRotated = AimPoint;
					aimPointRotated = Quaternion.AngleAxis( 90f * reloadTimer / ReloadTimeout, Vector3.Cross( AimPoint, Vector3.up ) ) * aimPointRotated;
					aimingLine.SetPosition( 1, aimPointRotated );
					Color red = new Color( 1f, 0f, 0f, opacity );
					aimingLine.SetColors( red, red );
					break;
				case DroneInfo.Type.Mortar:
					// Calculate subdivided vertex arc from projectile rotation & force
					float x = transform.position.x - AimPoint.x;
					float z = transform.position.z - AimPoint.z;

					float g = -Physics.gravity.y;
					float theta = -targetRotation.eulerAngles.x * Mathf.Deg2Rad;
					float v = ProjectileInfo.Properties.MortarShell.InitialForce;
					float d = Mathf.Sqrt( x * x + z * z );

					int i;
					for( i = 0; i < DroneInfo.Mortar.AimLineSubdivisions; ++i )
					{
						float dist = ( d / DroneInfo.Mortar.AimLineSubdivisions ) * i;
						float div = ( g * ( dist * dist ) ) / ( 2f * Mathf.Pow( v * Mathf.Cos( theta ), 2f ) );
						float y = ( dist * Mathf.Tan( theta ) ) - div;

						Vector3 forwardXZ = Quaternion.AngleAxis( targetRotation.eulerAngles.y, Vector3.up ) * Vector3.forward;
						Vector3 pos = forwardXZ * dist + new Vector3( 0f, y, 0f );

						aimingLine.SetPosition( i, transform.position + pos );
					}
					aimingLine.SetPosition( i, AimPoint );

					Color yellow = new Color( 1f, 1f, 0f, opacity );
					aimingLine.SetColors( yellow, yellow );
					break;
				case DroneInfo.Type.Seeker:
					// Straight line to the point where seek delay ends
					aimingLine.SetPosition( 0, transform.position );
					aimingLine.SetPosition( 1, transform.position + Vector3.Normalize( transform.forward + transform.up ) * ProjectileInfo.Properties.Seeker.InitialForce * ProjectileInfo.Properties.Seeker.SeekDelay );

					// Straight line to the seek target/static seek point
					if( lockTarget == null )
					{
						aimingLine.SetPosition( 2, AimPoint );
					}
					else
					{
						aimingLine.SetPosition( 2, lockTarget.position );
					}

					Color blue = new Color( 0f, 0f, 1f, opacity );
					aimingLine.SetColors( blue, blue );
					break;
				case DroneInfo.Type.ShotgunStorm:
					// Straight line
					aimingLine.SetPosition( 0, transform.position );
					aimingLine.SetPosition( 1, AimPoint );
					Color orange = new Color( 1f, .25f, 0f, opacity );
					aimingLine.SetColors( orange, orange );
					break;
				case DroneInfo.Type.SlagCannon:
					// Straight line
					aimingLine.SetPosition( 0, transform.position );
					aimingLine.SetPosition( 1, AimPoint );
					Color purple = new Color( 1f, 0f, .25f, opacity );
					aimingLine.SetColors( purple, purple );
					break;
				default:
					break;
			}
		}
		else
		{
			// Disable the aiming line if not aiming
			if( aimingLine.enabled )
			{
				aimingLine.enabled = false;
			}
		}
	}

	void OnDrawGizmos()
	{
		// Draw debug aim graphics
		if( GameInfo.Properties.Debug )
		{
			if( Aim )
			{
				switch( Type )
				{
					case DroneInfo.Type.Rocket:
						Gizmos.color = Color.red;
						Gizmos.DrawWireSphere( AimPoint, ProjectileInfo.Properties.Rocket.ExplosionRadius );
						break;
					case DroneInfo.Type.Mortar:
						Gizmos.color = Color.yellow;
						Gizmos.DrawWireSphere( AimPoint + new Vector3( 0f, ProjectileInfo.Properties.MortarShell.TargetYOffset, 0f ), 1f );
						break;
					case DroneInfo.Type.Seeker:
						Gizmos.color = Color.blue;
						if( lockTarget == null )
						{
							Gizmos.DrawWireSphere( AimPoint, 10f );
						}
						else
						{
							Gizmos.DrawWireSphere( lockTarget.position, 10f );
						}
						break;
					default:
						break;
				}
			}
		}
	}

	// Utility Methods
	public void Fire()
	{
		if( reloadTimer > 0f )
		{
			return;
		}

		switch( Type )
		{
			case DroneInfo.Type.Rocket:
				// Spawn a rocket, activate it and decrement ammo
				GameObject rocket = GameControl.ProjectilePool.Spawn( "Rocket" );
				rocket.transform.position = transform.position;
				rocket.transform.rotation = transform.rotation;
				Rocket rocketScript = rocket.GetComponent<Rocket>();
				rocketScript.Owner = Player;
				rocketScript.PooledStart();
				--Ammo;
				break;
			case DroneInfo.Type.Mortar:
				// Spawn a mortar shell, set it's target distance, activate it and decrement ammo
				GameObject mortar = GameControl.ProjectilePool.Spawn( "Mortar Shell" );
				mortar.transform.position = transform.position;
				mortar.transform.rotation = transform.rotation;
				MortarShell mortarScript = mortar.GetComponent<MortarShell>();
				Vector3 diff = AimPoint - mortar.transform.position;
				diff.y = 0;
				mortarScript.TargetDist = diff.magnitude;
				mortarScript.Owner = Player;
				mortarScript.PooledStart();
				--Ammo;
				break;
			case DroneInfo.Type.Seeker:
				// Spawn a seeker missile
				GameObject seeker = GameControl.ProjectilePool.Spawn( "Seeker" );
				seeker.transform.position = transform.position;

				// Launch it at a 45 degree angle
				Vector3 av = AimVector;
				av.y = 0;
				Vector3 hcf = Vector3.Cross( av, Vector3.forward );
				float dir = Vector3.Dot( hcf, Vector3.up );
				float yaw = Vector3.Angle( av, Vector3.forward ) * ( dir >= 0 ? -1 : 1 );
				seeker.transform.rotation = Quaternion.AngleAxis( yaw, Vector3.up ) * Quaternion.AngleAxis( -45f, Vector3.right );

				// Setup it's script properties
				Seeker seekerScript = seeker.GetComponent<Seeker>();
				seekerScript.SeekPoint = AimPoint;
				if( lockTarget != null )
				{
					seekerScript.SeekTarget = lockTarget;
					lockTarget = null;
				}
				seekerScript.Owner = Player;

				// Activate it and decrement ammo
				seekerScript.PooledStart();
				--Ammo;
				break;
			case DroneInfo.Type.ShotgunStorm:
				// Spawn a rocket, activate it and decrement ammo
				int xCount = 4;
				int yCount = 4;
				float aimPlane = 25f;
				for( int y = 0; y < 4; ++y )
				{
					for( int x = 0; x < 4; ++x )
					{
						GameObject shot = GameControl.ProjectilePool.Spawn( "Shotgun Pellet" );
						shot.transform.position = transform.position;
						Vector3 shotVector = new Vector3( x - xCount * .5f, y - yCount * .5f, aimPlane ).normalized;
						shot.transform.rotation = Quaternion.LookRotation( transform.rotation * shotVector );
						ShotgunPellet shotScript = shot.GetComponent<ShotgunPellet>();
						shotScript.Owner = Player;
						shotScript.PooledStart();
					}
				}
				--Ammo;
				break;
			case DroneInfo.Type.SlagCannon:
				// Spawn a rocket, activate it and decrement ammo
				GameObject slagBall = GameControl.ProjectilePool.Spawn( "Slag Ball" );
				slagBall.transform.position = transform.position;
				slagBall.transform.rotation = transform.rotation;
				SlagBall ballScript = slagBall.GetComponent<SlagBall>();
				ballScript.Owner = Player;
				ballScript.PooledStart();
				--Ammo;
				break;
			default:
				break;
		}

		ResetReloadTimer();

		// If the drone is out of ammo, create an explosion and deactivate it
		if( Ammo == 0 )
		{
			Deactivate();
		}
	}

	public void FireHold()
	{
		switch( Type )
		{
			case DroneInfo.Type.SlagCannon:
				if( reloadTimer == 0f )
				{
					Fire();
				}
				break;
			default:
				break;
		}
	}

	public void Deactivate()
	{
		// Spawn Explosion
		GameObject explosion = GameControl.ProjectilePool.Spawn( "Drone Explosion" );
        if( explosion != null )
        {
            explosion.transform.position = transform.position;
        }

		// Inform the avatar GUI that this drone is despawning
		Player.transform.Find( "Camera" ).GetComponent<AvatarGUI>().DroneDespawned( this );

		// Deactivate spawned children
		GameControl.MiscPool.Despawn( aimingLine.gameObject );

		if( aimingHolo != null )
		{
			GameControl.MiscPool.Despawn( aimingHolo.gameObject );
		}

		GameControl.SharedPool.Despawn( droneMesh );

		// Deactivate self
		GameControl.DronePool.Despawn( gameObject );
	}

	public void ResetReloadTimer()
	{
		reloadTimer = ReloadTimeout;
	}
}
