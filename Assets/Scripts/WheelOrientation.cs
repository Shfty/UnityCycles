using UnityEngine;
using System.Collections;

public class WheelOrientation : MonoBehaviour
{
	//Fields
	InputWrapper inputWrapper;
	MarbleMovement marbleScript;
	Transform wheelMesh;
	Quaternion baseOrientation;
	Quaternion targetRotation;
	float turnAngle;
	float spinRate = 0f;

	// Properties
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

	void Start()
	{
		// Find and store the marble movement script, wheel mesh and input wrapper
		marbleScript = gameObject.GetComponent<MarbleMovement>();
		wheelMesh = Wheel.transform.Find( "Wheel Mesh Wrapper" );
		inputWrapper = GetComponent<InputWrapper>();
	}
	
	void Update()
	{
		// Update the heading based on impulse direction
		Vector3 heading = new Vector3( inputWrapper.LeftStick.x, 0f, inputWrapper.LeftStick.y ).normalized;
		
		// Update the turn angle if the stick is pressed far enough
		if( heading.magnitude > TurnStickDeadzone )
		{
			Vector3 hcf = Vector3.Cross( heading, Vector3.forward );
			float dir = Vector3.Dot( hcf, Vector3.up );
			turnAngle = Vector3.Angle( heading, Vector3.forward ) * ( dir >= 0 ? -1 : 1 );
		}

		// Calculate the rotation for the current surface
		Quaternion surfaceRotation = Quaternion.FromToRotation( Vector3.up, marbleScript.Up );

		// Calculate the camera-relative rotation
		Vector3 cameraForwardXZ = new Vector3( marbleScript.Camera.forward.x, 0f, marbleScript.Camera.forward.z ).normalized;
		Quaternion cameraRotation = Quaternion.FromToRotation( Vector3.forward, cameraForwardXZ );

		// Assemble the target rotation quaternion
		targetRotation = surfaceRotation * Quaternion.AngleAxis( turnAngle, Vector3.up ) * cameraRotation * baseOrientation;

		if( marbleScript.Grounded )
		{
			// Spin the wheel
			spinRate = heading.magnitude * WheelSpinFactor;
		}
		else
		{
			// Slowly reduce the wheel's spin rate
			spinRate = Mathf.Max( spinRate -= SpinDecayRate * Time.deltaTime, 0f );
		}

		// Apply rotation to the wheel mesh
		wheelMesh.Rotate( Vector3.right, spinRate * Time.deltaTime );

		// Interpolate the wheel's rotation toward the target rotation
		Wheel.transform.rotation = Quaternion.Lerp( Wheel.transform.rotation, targetRotation, RotationLerpFactor * Time.deltaTime );
	}
}
