using UnityEngine;
using System.Collections;

public class Drone : PooledObject
{
	// Fields
	ObjectPool dronePool;
	ObjectPool projectilePool;
	ObjectPool miscPool;
	Quaternion targetRotation;
	LineRenderer aimingLine = null;
	Projector aimingHolo = null;
	Transform lockTarget = null;

	GameObject initPlayer;
	DroneInfo.Type initType;
	int initAmmo;
	bool initAim = false;
	Vector3 initAimVector;
	Vector3 initAimPoint;

	// Properties
	public GameObject Player;
	public GameObject LineRendererPrefab;
	public GameObject MortarHoloPrefab;
	public GameObject ExplosionPrefab;
	public GameObject RocketPrefab;
	public GameObject MortarPrefab;
	public GameObject SeekerPrefab;
	public LayerMask raycastAimMask;
	public LayerMask raycastLockMask;
	public DroneInfo.Type Type;
	public int Ammo;
	public bool Aim;
	public Vector3 AimVector;
	public Vector3 AimPoint;

	// Unity Methods
	void Awake()
	{
		// Need to call this here, PooledStart runs before Start
		dronePool = GameObject.Find( "Drone Pool" ).GetComponent<ObjectPool>();
		projectilePool = GameObject.Find( "Projectile Pool" ).GetComponent<ObjectPool>();
		miscPool = GameObject.Find( "Misc Pool" ).GetComponent<ObjectPool>();
	}

	public override void OnEnable()
	{
		// Call parent OnEnable method
		base.OnEnable();

		// Store initial droid-related state
		initPlayer = Player;
		initType = Type;
		initAmmo = Ammo;
		initAim = Aim;
		initAimVector = AimVector;
		initAimPoint = AimPoint;
	}

	public override void OnDisable()
	{
		// Call parent OnDisable method
		base.OnDisable();

		// Restore initial droid-related state
		Player = initPlayer;
		Type = initType;
		Ammo = initAmmo;
		Aim = initAim;
		AimVector = initAimVector;
		AimPoint = initAimPoint;
	}

	public void PooledStart()
	{
		// Spawn the aiming line
		aimingLine = miscPool.Spawn( LineRendererPrefab ).GetComponent<LineRenderer>();
		aimingLine.transform.parent = transform;

		// Configure the aiming line per-type and spawn type-specific extras
		switch( Type )
		{
			case DroneInfo.Type.Rocket:
				// LineRenderer
				aimingLine.SetVertexCount( 2 );
				break;
			case DroneInfo.Type.Mortar:
				// LineRenderer
				aimingLine.SetVertexCount( DroneInfo.Mortar.AimLineSubdivisions );

				// Aim Holo
				aimingHolo = miscPool.Spawn( MortarHoloPrefab ).GetComponent<Projector>();
				break;
			case DroneInfo.Type.Seeker:
				aimingLine.SetVertexCount( 3 );
				break;
			default:
				break;
		}
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
		switch( Type )
		{
			case DroneInfo.Type.Rocket:
			case DroneInfo.Type.Seeker:
				// Look straight at the target
				targetRotation = Quaternion.LookRotation( AimVector );
				break;
			case DroneInfo.Type.Mortar:
				// Calculate yaw
				Vector3 av = AimVector;
				av.y = 0;
				Vector3 hcf = Vector3.Cross( av, Vector3.forward );
				float dir = Vector3.Dot( hcf, Vector3.up );
				float yaw = Vector3.Angle( av, Vector3.forward ) * ( dir >= 0 ? -1 : 1 );

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

				float pitch = Mathf.Atan( ( v2 - Mathf.Sqrt( sqrFormula ) ) / divisor );

				// rotate
				targetRotation = Quaternion.AngleAxis( yaw, Vector3.up ) * Quaternion.AngleAxis( -pitch * Mathf.Rad2Deg, Vector3.right );
				break;
			default:
				break;
		}

		// Interpolate toward the target rotation
		transform.rotation = Quaternion.Slerp( transform.rotation, targetRotation, 20f * Time.deltaTime );
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
			switch( Type )
			{
				case DroneInfo.Type.Rocket:
					// Straight line
					aimingLine.SetPosition( 0, transform.position );
					aimingLine.SetPosition( 1, AimPoint );
					aimingLine.SetColors( Color.red, Color.red );
					break;
				case DroneInfo.Type.Mortar:
					// Calculate subdivided vertex arc from projectile rotation & force
					float x = transform.position.x - AimPoint.x;
					float z = transform.position.z - AimPoint.z;

					float g = -Physics.gravity.y;
					float theta = -targetRotation.eulerAngles.x * Mathf.Deg2Rad;
					float v = ProjectileInfo.Properties.MortarShell.InitialForce;
					float d = Mathf.Sqrt( x * x + z * z );

					for( int i = 0; i < DroneInfo.Mortar.AimLineSubdivisions; ++i )
					{
						float dist = ( d / DroneInfo.Mortar.AimLineSubdivisions ) * i;
						float div = ( g * ( dist * dist ) ) / ( 2f * Mathf.Pow( v * Mathf.Cos( theta ), 2f ) );
						float y = ( dist * Mathf.Tan( theta ) ) - div;

						Vector3 forwardXZ = Quaternion.AngleAxis( targetRotation.eulerAngles.y, Vector3.up ) * Vector3.forward;
						Vector3 pos = forwardXZ * dist + new Vector3( 0f, y, 0f );

						aimingLine.SetPosition( i, transform.position + pos );
					}

					aimingLine.SetColors( Color.yellow, Color.yellow );
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
					aimingLine.SetColors( Color.blue, Color.blue );
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
	public override void Deactivate()
	{
		// Manually deactivate aiming holo as it's not parented
		aimingHolo.GetComponent<PooledObject>().Deactivate();

		base.Deactivate();
	}

	public void Fire()
	{
		switch( Type )
		{
			case DroneInfo.Type.Rocket:
				// Spawn a rocket, activate it and decrement ammo
				GameObject rocket = projectilePool.Spawn( RocketPrefab );
				rocket.transform.position = transform.position;
				rocket.transform.rotation = transform.rotation;
				Rocket rocketScript = rocket.GetComponent<Rocket>();
				rocketScript.PooledStart();
				--Ammo;
				break;
			case DroneInfo.Type.Mortar:
				// Spawn a mortar shell, set it's target distance, activate it and decrement ammo
				GameObject mortar = projectilePool.Spawn( MortarPrefab );
				mortar.transform.position = transform.position;
				mortar.transform.rotation = transform.rotation;
				MortarShell mortarScript = mortar.GetComponent<MortarShell>();
				Vector3 diff = AimPoint - mortar.transform.position;
				diff.y = 0;
				mortarScript.TargetDist = diff.magnitude;
				mortarScript.PooledStart();
				--Ammo;
				break;
			case DroneInfo.Type.Seeker:
				// Spawn a seeker missile
				GameObject seeker = projectilePool.Spawn( SeekerPrefab );
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

				// Activate it and decrement ammo
				seekerScript.PooledStart();
				--Ammo;
				break;
			default:
				break;
		}

		// If the drone is out of ammo, create an explosion and deactivate it
		if( Ammo == 0 )
		{
			GameObject explosion = dronePool.Spawn( ExplosionPrefab );
			explosion.transform.position = transform.position;
			Deactivate();
		}
	}
}
