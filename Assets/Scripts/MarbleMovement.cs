using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class MarbleMovement : MonoBehaviour
{
	// Fields
	Vector3 forwardVector;
	float prevJump = 0f;
	float prevDrop = 0f;
	InputWrapper inputWrapper;

	Vector3 initPosition;
	Quaternion initRotation;
	Vector3 initVelocity;
	Vector3 initAngularVelocity;

	// Properties
	public GameObject Marble;
	public Transform Camera;
	public float GroundForceFactor = 2f;
	public float AirForceFactor = 5f;
	public float GroundTorqueFactor = 10f;
	public float AirTorqueFactor = 5f;
	public float JumpForce = 250f;
	public float HoverForce = 5f;
	public float DownBurstForce = 250f;
	public float DownJetForce = 5f;

	// Read by WheelOrientation
	public Vector3 Right;
	public Vector3 Up;
	public Vector3 Forward;
	public bool Grounded;
	public Vector3 GroundPoint;
	public bool JumpFired = false;
	public bool DropFired = false;
	public Vector3 Velocity;

	// Unity Methods
	void Awake()
	{
		inputWrapper = gameObject.GetComponent<InputWrapper>();
		ResetSurfaceNormal();
	}

	void OnEnable()
	{
		// Store init state
		initPosition = Marble.transform.position;
		initRotation = Marble.transform.rotation;
		initVelocity = Marble.rigidbody.velocity;
		initAngularVelocity = Marble.rigidbody.angularVelocity;
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
		// Update properties
		int playerMask = 1 << gameObject.layer;
		Grounded = Physics.CheckSphere( Marble.transform.position, .6f, ~playerMask );
		Velocity = Marble.rigidbody.velocity;

		// Rotation / Directional Force
		float horz = inputWrapper.LeftStick.x;
		float vert = inputWrapper.LeftStick.y;

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
		if( inputWrapper.Jump == 1f )
		{
			if( JumpFired == false && prevJump == 0f )
			{
				// If the jump jets are ready, apply jump force and set jumpFired flag
				Marble.rigidbody.AddForce( jumpVector * JumpForce, ForceMode.Impulse );
				JumpFired = true;
			}
			else
			{
				// Otherwise, apply hover force
				Marble.rigidbody.AddForce( jumpVector * HoverForce * Time.deltaTime, ForceMode.Force );
			}
		}
		prevJump = inputWrapper.Jump;

		// Drop
		if( inputWrapper.Drop == 1f )
		{
			if( DropFired == false && prevDrop == 0f )
			{
				// If the drop jets are ready, apply drop force and set dropFired flag
				Marble.rigidbody.AddForce( Up * -DownBurstForce, ForceMode.Impulse );
				DropFired = true;
			}
			else
			{
				// Otherwise, apply downward accelleration
				Marble.rigidbody.AddForce( Vector3.up * -DownJetForce * Time.deltaTime, ForceMode.Force );
			}
		}
		prevDrop = inputWrapper.Drop;

		// Surface normal dot up - Make sure player isn't wall riding
		float sndu = Vector3.Dot( Up, Vector3.up );
		if( Grounded && sndu > .5f )
		{
			JumpFired = false;
			DropFired = false;
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
		CalculateSurfaceNormal( col );
	}

	public void OnCollisionStay( Collision col )
	{
		CalculateSurfaceNormal( col );
	}

	public void OnCollisionExit()
	{
		ResetSurfaceNormal();
	}

	// Utility Methods
	void ResetSurfaceNormal()
	{
		Up = Vector3.up;
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

		GroundPoint = averagePoint;
		Up = averageNormal;
	}
}
