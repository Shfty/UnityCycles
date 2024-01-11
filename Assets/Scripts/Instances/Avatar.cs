using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Avatar : MonoBehaviour
{
	// Properties
	public InputWrapper InputWrapper { get; set; }

	// Variables
	// Private
	float prevFire;
	float prevSwitchLeft;
	float prevSwitchRight;
	int initHealth;
	bool gameActive = true;
	
	// Public
	public int ActiveDroneIndex = 0;
	public List<GameObject> Drones;
	public List<Transform> DroneAnchors;
	public int Health = 100;
	public int MaxHealth = 100;
	public float Dash = 0f;
	public float MaxDash = 1f;
	public int Kills = 0;
	public float DashRechargeSpinFactor = 0.01f;

	public float DamageRumbleFactor = .02f;

	// Unity Methods
	void Awake()
	{
		initHealth = Health;
	}

	public void OnEnable()
	{
        Health = initHealth;
        gameActive = true;
	}

	void Update()
	{
		// Recharge Dash
		if( Dash < MaxDash && GetComponent<MarbleMovement>().Grounded && !GetComponent<MarbleMovement>().ObtuseAngle )
		{
			Rigidbody rb = transform.Find( "Marble" ).GetComponent<Rigidbody>();
			Dash = Mathf.Min( Dash + ( rb.angularVelocity.magnitude * DashRechargeSpinFactor * Time.deltaTime ), MaxDash );
		}

		// If game is active, take input
		if( gameActive )
		{
			// If an active drone is present
			if( Drones.Count > 0 )
			{
				if( Drones[ ActiveDroneIndex ].activeSelf )
				{
					// Set the drone's Aim property
					Drone droneScript = Drones[ ActiveDroneIndex ].GetComponent<Drone>();
					droneScript.Aim = InputWrapper.Aim.Down;

					// Relay fire events
					if( InputWrapper.Fire.Pressed )
					{
						droneScript.Fire();
					}
					if( InputWrapper.Fire.Down )
					{
						droneScript.FireHold();
					}

					// Switch drones
					if( InputWrapper.SwitchRight.Pressed || ( InputWrapper.DPad.Pushed && InputWrapper.DPad.MoveDelta.x > 0f ) )
					{
						SwitchDrone( true );
					}

					if( InputWrapper.SwitchLeft.Pressed || ( InputWrapper.DPad.Pushed && InputWrapper.DPad.MoveDelta.x < 0f ) )
					{
						SwitchDrone( false );
					}
				}
			}
		}

		for( int i = 0; i < Drones.Count; ++i )
		{
			GameObject drone = Drones[ i ];
			// Check if any drones disabled themselves & remove references
			if( drone != null && !drone.activeSelf )
			{
				Drones.RemoveAt( i );
				if( Drones.Count > 0 && ActiveDroneIndex >= Drones.Count - 1 )
				{
					SwitchDrone( false );
				}
				--i;
				continue;
			}

			// Disable aim if not the active drone
			Drone droneScript = drone.GetComponent<Drone>();
			if( i != ActiveDroneIndex && droneScript.Aim )
			{
				droneScript.Aim = false;
			}
		}

		// Reshuffle into position
		if( Drones.Count > 0 )
		{
			KinematicHover mDroneScript = Drones[ 0 ].GetComponent<KinematicHover>();
			if( mDroneScript.Target != DroneAnchors[ ActiveDroneIndex ] )
			{
				mDroneScript.Target = DroneAnchors[ ActiveDroneIndex ];
			}
			if( Drones.Count > 1 )
			{
				KinematicHover lDroneScript = Drones[ 1 ].GetComponent<KinematicHover>();
				if( lDroneScript.Target != DroneAnchors[ WrapIndex( ActiveDroneIndex - 1, 2 ) ] )
				{
					lDroneScript.Target = DroneAnchors[ WrapIndex( ActiveDroneIndex - 1, 2 ) ];
				}
				if( Drones.Count > 2 )
				{
					KinematicHover rDroneScript = Drones[ 2 ].GetComponent<KinematicHover>();
					if( rDroneScript.Target != DroneAnchors[ WrapIndex( ActiveDroneIndex + 1, 2 ) ] )
					{
						rDroneScript.Target = DroneAnchors[ WrapIndex( ActiveDroneIndex + 1, 2 ) ];
					}
				}
			}
		}
	}

	// Utility Methods
	public void GameOver()
	{
		gameActive = false;
	}

	public int WrapIndex( int idx, int maxIdx )
	{
		if( idx < 0 )
		{
			return maxIdx;
		}
		if( idx > maxIdx )
		{
			return 0;
		}
		return idx;
	}

	void SwitchDrone( bool right )
	{
		if( right )
		{
			ActiveDroneIndex = WrapIndex( ActiveDroneIndex - 1, Drones.Count - 1 );
		}
		else
		{
			ActiveDroneIndex = WrapIndex( ActiveDroneIndex + 1, Drones.Count - 1 );
		}

		foreach( GameObject drone in Drones )
		{
			drone.GetComponent<Drone>().ResetReloadTimer();
		}
	}

	void ApplyDamage( object[] args )
	{
		int damage = (int)args[ 0 ];
		GameObject owner = (GameObject)args[ 1 ];

		Health -= damage;

		// Only apply rumble if the game is in progress
		if( gameActive )
		{
			InputWrapper.StrongRumbleForce += damage * DamageRumbleFactor;
		}

		if( Health <= 0 )
		{
			// Spawn Explosion
			GameObject explosion = GameControl.ProjectilePool.Spawn( "Avatar Explosion" );
			explosion.transform.position = transform.Find( "Marble" ).position;

			GameControl.Instance.PlayerDeath( gameObject, owner );
		}
	}
}
