using UnityEngine;
using System.Collections;

public class WheelOrientation : MonoBehaviour
{
	// Properties
	public InputWrapper InputWrapper { get; set; }

	// Variables
	// Private
	MarbleMovement marbleScript;
	Transform wheelMesh;
	Quaternion baseOrientation;
	Quaternion targetRotation;
	float turnAngle;
	bool gameActive = true;

	// Public
	public GameObject Wheel;
	public float RotationLerpFactor = 6f;
	public float WheelSpinFactor = 1.5f;
	public float SpinDecayRate = .75f;
	public float TurnStickDeadzone = .25f;

	// Unity Methods
	void Awake()
	{
		// Store the base orientation
		baseOrientation = Wheel.transform.rotation;
	}

    void OnEnable()
    {
        gameActive = true;
    }

	void Start()
	{
		// Find and store the marble movement script, wheel mesh and input wrapper
		marbleScript = gameObject.GetComponent<MarbleMovement>();
		wheelMesh = Wheel.transform.Find( "Wheel Mesh Wrapper" );
	}
	
	void Update()
	{
		Vector3 heading = Vector3.zero;
		if( gameActive )
		{
			// Update the heading based on impulse direction
			heading = new Vector3( InputWrapper.LeftStick.Value.x, 0f, InputWrapper.LeftStick.Value.y );
		}

		// Update the turn angle if the stick is pressed far enough
		if( heading.magnitude > TurnStickDeadzone )
		{
			Vector3 normHeading = heading.normalized;
			Vector3 hcf = Vector3.Cross( normHeading, Vector3.forward );
			float dir = Vector3.Dot( hcf, Vector3.up );
			turnAngle = Vector3.Angle( normHeading, Vector3.forward ) * ( dir >= 0 ? -1 : 1 );
		}

		// Calculate the rotation for the current surface
		Quaternion surfaceRotation = Quaternion.FromToRotation( Vector3.up, marbleScript.Up );

		// Calculate the camera-relative rotation
		Vector3 cameraForwardXZ = new Vector3( marbleScript.Camera.forward.x, 0f, marbleScript.Camera.forward.z ).normalized;
		Quaternion cameraRotation = Quaternion.FromToRotation( Vector3.forward, cameraForwardXZ );

		// Assemble the target rotation quaternion
		targetRotation = surfaceRotation * Quaternion.AngleAxis( turnAngle, Vector3.up ) * cameraRotation * baseOrientation;

		// Apply rotation to the wheel mesh
		wheelMesh.Rotate( Vector3.right, marbleScript.Marble.GetComponent<Rigidbody>().angularVelocity.magnitude * WheelSpinFactor * Time.deltaTime );

		// Interpolate the wheel's rotation toward the target rotation
		Wheel.transform.rotation = Quaternion.Lerp( Wheel.transform.rotation, targetRotation, RotationLerpFactor * Time.deltaTime );
	}

	// Utility Methods
	public void GameOver()
	{
		gameActive = false;
	}
}
