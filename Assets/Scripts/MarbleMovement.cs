﻿using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class MarbleMovement : MonoBehaviour
{
	// Properties
	public InputWrapper InputWrapper { get; set; }

	// Variables
	// Private
	Vector3 forwardVector;
	float prevJump = 0f;
	float prevDrop = 0f;
	bool gameActive = true;

	Vector3 initPosition;
	Quaternion initRotation;
	Vector3 initVelocity;
	Vector3 initAngularVelocity;

	// Public
	public GameObject Marble;
	public Transform Camera;
	public float GroundForceFactor = 10f;
	public float AirForceFactor = 10f;
	public float GroundTorqueFactor = 15f;
	public float AirTorqueFactor = 15f;
	public float JumpForce = 7f;
	public float HoverForce = 350f;
	public float DownBurstForce = 15f;
	public float DownJetForce = 1000f;
	public float DashForce = 7f;

	public float WeakRumbleForceFactor = .02f;
	public float StrongRumbleForceFactor = .02f;

	// Read by WheelOrientation
	public Vector3 Right;
	public Vector3 Up;
	public Vector3 Forward;
	public bool Grounded;
	public Vector3 GroundPoint;
	public bool ObtuseAngle;
	public bool JumpFired = false;
	public bool DropFired = false;
	public Vector3 Velocity;

	// Unity Methods
	void Awake()
	{
	}

	void OnEnable()
	{
		// Store init state
		initPosition = Marble.transform.position;
		initRotation = Marble.transform.rotation;
		initVelocity = Marble.rigidbody.velocity;
		initAngularVelocity = Marble.rigidbody.angularVelocity;
		ResetSurfaceNormal();
        gameActive = true;
	}

	void OnDisable()
	{
		// Reset object to init state
		Marble.transform.position = initPosition;
		Marble.transform.rotation = initRotation;
		Marble.rigidbody.velocity = initVelocity;
		Marble.rigidbody.angularVelocity = initAngularVelocity;
	}

	void Update() // Update is called once per frame
	{
		if( gameActive )
		{
			// Update properties
			int playerMask = 1 << gameObject.layer;
			Velocity = Marble.rigidbody.velocity;

			// Rotation / Directional Force
			float horz = InputWrapper.LeftStick.x;
			float vert = InputWrapper.LeftStick.y;

			// Calculate camera-relative XZ axes
			Quaternion nq = Quaternion.FromToRotation( Vector3.up, Up );
			Forward = nq * Quaternion.AngleAxis( Camera.eulerAngles.y, Vector3.up ) * Vector3.forward;
			Right = nq * Camera.right;

			// Add force based on whether the marble is grounded
			if( Grounded )
			{
				Marble.rigidbody.AddForce( Forward * vert * GroundForceFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.rigidbody.AddForce( Right * horz * GroundForceFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.rigidbody.AddTorque( Forward * -horz * GroundTorqueFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.rigidbody.AddTorque( Right * vert * GroundTorqueFactor * Time.deltaTime, ForceMode.Impulse );
			}
			else
			{
				Marble.rigidbody.AddForce( Forward * vert * AirForceFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.rigidbody.AddForce( Right * horz * AirForceFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.rigidbody.AddTorque( Forward * -horz * AirTorqueFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.rigidbody.AddTorque( Right * vert * AirTorqueFactor * Time.deltaTime, ForceMode.Impulse );
			}

			// Calculate jump vector as halfway between the surface and world up
			Vector3 jumpVector = Vector3.Lerp( Vector3.up, Up, .5f );
			if( InputWrapper.Jump > 0f )
			{
				if( JumpFired == false && prevJump == 0f )
				{
					// If the jump jets are ready, apply jump force and set jumpFired flag
					Marble.rigidbody.AddForce( jumpVector * JumpForce, ForceMode.Impulse );
					InputWrapper.WeakRumbleForce += JumpForce * WeakRumbleForceFactor;
				}
				else
				{
					// Otherwise, apply hover force
					Marble.rigidbody.AddForce( jumpVector * HoverForce * Time.deltaTime, ForceMode.Force );
					InputWrapper.WeakRumbleBaseForce = HoverForce * WeakRumbleForceFactor;
				}
				JumpFired = true;
			}
			prevJump = InputWrapper.Jump;

			// Drop
			if( InputWrapper.Drop > 0 )
			{
				if( DropFired == false && prevDrop == 0f )
				{
					// If the drop jets are ready, apply drop force and set dropFired flag
					Marble.rigidbody.AddForce( Up * -DownBurstForce, ForceMode.Impulse );
					InputWrapper.WeakRumbleForce += DownBurstForce * WeakRumbleForceFactor;
				}
				else
				{
					// Otherwise, apply downward accelleration
					Marble.rigidbody.AddForce( Vector3.up * -DownJetForce * Time.deltaTime, ForceMode.Force );
					InputWrapper.WeakRumbleBaseForce = DownJetForce * WeakRumbleForceFactor;
				}
				DropFired = true;
			}
			prevDrop = InputWrapper.Drop;

			// Disable jet rumble in the air if needed
			if( InputWrapper.Jump == 0f && InputWrapper.Drop == 0f && !Grounded )
			{
				InputWrapper.WeakRumbleBaseForce = 0f;
			}

			// Dash
			if( InputWrapper.Dash > 0f && GetComponent<Avatar>().Dash > 0f )
			{
				float dash = GetComponent<Avatar>().Dash;
				float force = Mathf.Min( dash, 1f );
				Marble.rigidbody.velocity = Vector3.zero;
				Marble.rigidbody.AddForce( force * Forward * InputWrapper.DashVector.y * DashForce, ForceMode.VelocityChange );
				Marble.rigidbody.AddForce( force * Right * InputWrapper.DashVector.x * DashForce, ForceMode.VelocityChange );
				GetComponent<WheelParticles>().DashBurst();
				dash -= force;
				GetComponent<Avatar>().Dash = dash;
				InputWrapper.WeakRumbleForce += ( ( force * DashForce ) * ( force * DashForce ) ) * WeakRumbleForceFactor;
			}

			// Surface normal dot up - Make sure player isn't wall riding
			ObtuseAngle = Vector3.Dot( Up, Vector3.up ) <= 0f;
			if( Grounded && !ObtuseAngle )
			{
				JumpFired = false;
				DropFired = false;
			}
		}
	}

	void OnDrawGizmos()
	{
		// Draw the ground check sphere and local surface-relative axes
		if( GameInfo.Properties.Debug )
		{
			// Ground check sphere
			Gizmos.DrawWireSphere( Marble.transform.position, .6f );

			// Local force axes
			Gizmos.color = Color.red;
			Gizmos.DrawRay( Marble.transform.position - Right, Right * 2 );
			Gizmos.color = Color.green;
			Gizmos.DrawRay( Marble.transform.position - Up, Up * 2 );
			Gizmos.color = Color.blue;
			Gizmos.DrawRay( Marble.transform.position - Forward, Forward * 2 );
		}
	}

	public void OnCollisionEnter( Collision col )
	{
		if( gameActive )
		{
			InputWrapper.StrongRumbleForce += col.relativeVelocity.magnitude * StrongRumbleForceFactor;
		}
		CalculateSurfaceNormal( col );
	}

	public void OnCollisionStay( Collision col )
	{
		if( gameActive )
		{
			InputWrapper.WeakRumbleBaseForce = col.relativeVelocity.magnitude * WeakRumbleForceFactor;
		}
		CalculateSurfaceNormal( col );
	}

	public void OnCollisionExit()
	{
		if( gameActive )
		{
			InputWrapper.WeakRumbleBaseForce = 0f;
		}
		ResetSurfaceNormal();
	}

	// Utility Methods
	public void GameOver()
	{
		gameActive = false;
	}

	void ResetSurfaceNormal()
	{
		Up = Vector3.up;
		Grounded = false;
	}

	void CalculateSurfaceNormal( Collision col )
	{
		// Don't stick to players
		if( col.gameObject.name == "Marble" )
		{
			return;
		}

		// Calculate the average collision normal and update the surface vector with it
		Vector3 averagePoint = Vector3.zero;
		Vector3 averageNormal = Vector3.zero;
		foreach( ContactPoint contact in col )
		{
			averagePoint += contact.point;
			averageNormal += contact.normal;
		}
		averagePoint /= col.contacts.Length;
		averageNormal /= col.contacts.Length;

		Grounded = true;
		GroundPoint = averagePoint;
		Up = averageNormal;
	}
}
