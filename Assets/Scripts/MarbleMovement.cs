using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class MarbleMovement : MonoBehaviour
{
	// Properties
	public InputWrapper InputWrapper { get; set; }

	// Variables
	// Private
	Vector3 forwardVector;
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
		initVelocity = Marble.GetComponent<Rigidbody>().velocity;
		initAngularVelocity = Marble.GetComponent<Rigidbody>().angularVelocity;
		ResetSurfaceNormal();
        gameActive = true;
	}

	void OnDisable()
	{
		// Reset object to init state
		Marble.transform.position = initPosition;
		Marble.transform.rotation = initRotation;
		Marble.GetComponent<Rigidbody>().velocity = initVelocity;
		Marble.GetComponent<Rigidbody>().angularVelocity = initAngularVelocity;
	}

	void Update() // Update is called once per frame
	{
		if( gameActive )
		{
			// Update properties
			Velocity = Marble.GetComponent<Rigidbody>().velocity;

			// Rotation / Directional Force
			float horz = InputWrapper.LeftStick.Value.x;
			float vert = InputWrapper.LeftStick.Value.y;

			// Calculate camera-relative XZ axes
			Quaternion nq = Quaternion.FromToRotation( Vector3.up, Up );
			Forward = nq * Quaternion.AngleAxis( Camera.eulerAngles.y, Vector3.up ) * Vector3.forward;
			Right = nq * Camera.right;

			// Add force based on whether the marble is grounded
			if( Grounded )
			{
				Marble.GetComponent<Rigidbody>().AddForce( Forward * vert * GroundForceFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.GetComponent<Rigidbody>().AddForce( Right * horz * GroundForceFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.GetComponent<Rigidbody>().AddTorque( Forward * -horz * GroundTorqueFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.GetComponent<Rigidbody>().AddTorque( Right * vert * GroundTorqueFactor * Time.deltaTime, ForceMode.Impulse );
			}
			else
			{
				Marble.GetComponent<Rigidbody>().AddForce( Forward * vert * AirForceFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.GetComponent<Rigidbody>().AddForce( Right * horz * AirForceFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.GetComponent<Rigidbody>().AddTorque( Forward * -horz * AirTorqueFactor * Time.deltaTime, ForceMode.Impulse );
				Marble.GetComponent<Rigidbody>().AddTorque( Right * vert * AirTorqueFactor * Time.deltaTime, ForceMode.Impulse );
			}

			// Calculate jump vector as halfway between the surface and world up
			Vector3 jumpVector = Vector3.Lerp( Vector3.up, Up, .5f );
			if( InputWrapper.Jump.Pressed )
			{
				if( JumpFired == false )
				{
					// If the jump jets are ready, apply jump force and set jumpFired flag
					Marble.GetComponent<Rigidbody>().AddForce( jumpVector * JumpForce, ForceMode.Impulse );
					InputWrapper.WeakRumbleForce += JumpForce * WeakRumbleForceFactor;
					JumpFired = true;
				}
			}

			if( InputWrapper.Jump.Down )
			{
				// Otherwise, apply hover force
				Marble.GetComponent<Rigidbody>().AddForce( jumpVector * HoverForce * Time.deltaTime, ForceMode.Force );
				InputWrapper.WeakRumbleBaseForce = HoverForce * WeakRumbleForceFactor;
			}

			// Drop
			if( InputWrapper.Drop.Pressed )
			{
				if( DropFired == false )
				{
					// If the drop jets are ready, apply drop force and set dropFired flag
					Marble.GetComponent<Rigidbody>().AddForce( -Up * DownBurstForce, ForceMode.Impulse );
					InputWrapper.WeakRumbleForce += DownBurstForce * WeakRumbleForceFactor;
					DropFired = true;
				}
			}

			if( InputWrapper.Drop.Down )
			{
				// Otherwise, apply downward accelleration
				Marble.GetComponent<Rigidbody>().AddForce( -Vector3.up * DownJetForce * Time.deltaTime, ForceMode.Force );
				InputWrapper.WeakRumbleBaseForce = DownJetForce * WeakRumbleForceFactor;
			}

			// Disable jet rumble in the air if needed
			if( !InputWrapper.Jump.Down && !InputWrapper.Drop.Down && !Grounded )
			{
				InputWrapper.WeakRumbleBaseForce = 0f;
			}

			// Dash
			if( InputWrapper.Dash > 0f && GetComponent<Avatar>().Dash > 0f )
			{
				float dash = GetComponent<Avatar>().Dash;
				float force = Mathf.Min( dash, 1f );
				Marble.GetComponent<Rigidbody>().velocity = Vector3.zero;
				Marble.GetComponent<Rigidbody>().AddForce( force * Forward * InputWrapper.DashVector.y * DashForce, ForceMode.VelocityChange );
				Marble.GetComponent<Rigidbody>().AddForce( force * Right * InputWrapper.DashVector.x * DashForce, ForceMode.VelocityChange );
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

			// Ground rumble
			if( Grounded )
			{
				InputWrapper.WeakRumbleBaseForce = Marble.GetComponent<Rigidbody>().angularVelocity.magnitude * WeakRumbleForceFactor;
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
