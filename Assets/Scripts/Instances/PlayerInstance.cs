using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInstance : PooledObject
{
	// Fields
	InputWrapper inputWrapper;
	float prevFire;
	float prevSwitchLeft;
	float prevSwitchRight;

	int initHealth;
	
	// Properties
	public GameControl GameControl;
	public List<GameObject> Drones;
	public List<Transform> DroneAnchors;
	public int Health = 100;

	// Unity Methods
	void Awake()
	{
	}
	
	void Start()
	{
		// Find and store the input wrapper
		inputWrapper = gameObject.GetComponent<InputWrapper>();
	}

	public override void OnEnable()
	{
		base.OnEnable();

		initHealth = Health;
	}

	public override void OnDisable()
	{
		base.OnDisable();

		Health = initHealth;
	}

	void Update()
	{
		// If an active drone is present
		if( Drones.Count > 0 )
		{
			if( Drones[ 0 ].activeSelf )
			{
				// Set the drone's Aim property
				Drone droneScript = Drones[ 0 ].GetComponent<Drone>();
				if( inputWrapper.Aim == 1f )
				{
					droneScript.Aim = true;
				}
				else
				{
					droneScript.Aim = false;
				}

				// Relay fire events
				if( inputWrapper.Fire == 1f && prevFire == 0f )
				{
					droneScript.Fire();
				}
				prevFire = inputWrapper.Fire;

				// Switch drones
				if( inputWrapper.SwitchRight == 1f && prevSwitchRight == 0f )
				{
					SwitchDrone( true );
				}
				prevSwitchRight = inputWrapper.SwitchRight;

				if( inputWrapper.SwitchLeft == 1f && prevSwitchLeft == 0f )
				{
					SwitchDrone( false );
				}
				prevSwitchLeft = inputWrapper.SwitchLeft;
			}
			else
			{
				// If the drone is inactive, set it to null
				Drones[ 0 ] = null;
			}
		}

		// Check if any drones disabled themselves & remove references
		for( int i = 0; i < Drones.Count; ++i )
		{
			if( Drones[ i ] != null && !Drones[ i ].activeSelf )
			{
				Drones.RemoveAt( i );
				--i;
			}
		}

		// Reshuffle drones into their respective positions
		for( int i = 0; i < Drones.Count; ++i )
		{
			if( Drones[ i ] != null )
			{
				KinematicHover droneHover = Drones[ i ].GetComponent<KinematicHover>();
				droneHover.Target = DroneAnchors[ i ];
			}
		}
	}

	// Utility Methods
	public override void Deactivate()
	{
		// Ensure all drones are deactivated
		foreach( GameObject drone in Drones )
		{
			drone.GetComponent<Drone>().Deactivate();
		}
		Drones.Clear();

		base.Deactivate();
	}

	void SwitchDrone( bool right )
	{
		GameObject temp;

		if( !right )
		{
			// If switching left, shuffle all the drones down by one index
			temp = Drones[ 0 ];
			for( int i = 0; i < Drones.Count - 1; ++i )
			{
				Drones[ i ].GetComponent<Drone>().Aim = false;
				Drones[ i ] = Drones[ i + 1 ];
			}
			Drones[ Drones.Count - 1 ] = temp;
		}
		else
		{
			// If switching right, shuffle all the drones up by one index
			temp = Drones[ Drones.Count - 1 ];
			for( int i = Drones.Count - 1; i > 0; --i )
			{
				Drones[ i ] = Drones[ i - 1 ];
				Drones[ i ].GetComponent<Drone>().Aim = false;
			}
			Drones[ 0 ] = temp;
		}
	}

	void ApplyDamage( object[] args )
	{
		int damage = (int)args[ 0 ];
		GameObject owner = (GameObject)args[ 1 ];

		Health -= damage;
		if( Health <= 0 )
		{
			GameControl.PlayerDeath( gameObject, owner );
		}
	}
}
