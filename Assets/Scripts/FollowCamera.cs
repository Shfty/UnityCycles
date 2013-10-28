using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour
{
	// Fields
	InputWrapper inputWrapper;
	Vector3 targetPosition;
	Vector3 velocity;
	float xAngle = 0f;
	float yAngle = 0f;
	float followDistance;

	int terrainMask;
	int wallMask;
	int pickupMask;
	int projectileMask;

	// Properties
	public Transform Target;
	public Texture CrosshairTexture;
	public Vector3 Offset;
	public float MinFollowDistance = 1f;
	public float MaxFollowDistance = 10f;
	public float PositionLerpFactor = 12f;
	public float CameraRadius = .5f;
	public float SmallValue = .01f;
	
	// Unity Methods
	void Awake()
	{
		// Start at maximum follow distance
		followDistance = MaxFollowDistance;

		// Find and store the input wrapper component
		inputWrapper = gameObject.GetComponent<InputWrapper>();
	}

	void Start()
	{
		// Set the target position behind the target and init the camera position to it
		targetPosition = Target.position - ( Vector3.forward * followDistance );
		transform.position = targetPosition;

		// Create layer masks
		terrainMask = 1 << LayerMask.NameToLayer( "Terrain" );
		wallMask = 1 << LayerMask.NameToLayer( "Walls" );
	}
	
	void FixedUpdate()
	{
		// Correct positioning
		bool targetObscured;
		bool clipTerrain;
		bool clipWall;
		do
		{
			// Update the target position
			UpdateTarget();

			// Check line-of-sight between camera and target
			Vector3 cto = Vector3.Normalize( ( Target.position + new Vector3( 0f, -.25f, 0f ) ) - targetPosition );
			targetObscured = Physics.Raycast( new Ray( targetPosition, cto ), followDistance, terrainMask | wallMask );
			if( targetObscured )
			{
				// If it's obscured, move up slightly
				yAngle += SmallValue;
			}

			// Check if camera is clipping into the terrain
			clipTerrain = Physics.CheckSphere( transform.position, CameraRadius, terrainMask );
			if( clipTerrain )
			{
				// If so, move the camera upward by a small amount
				transform.position += new Vector3( 0f, SmallValue, 0f );
			}

			// Check if camera is clipping into a wall
			clipWall = Physics.CheckSphere( targetPosition, CameraRadius, wallMask ) && followDistance > MinFollowDistance;
			if( clipWall )
			{
				// If so, move the camera forward by a small amount
				followDistance -= SmallValue;
			}
		} while( targetObscured || clipTerrain || clipWall );

		// Check if the follow distance needs to increase
		bool canReverse;
		do
		{
			// Update the target position
			UpdateTarget();

			// Check if the camera is able to move backward
			Vector3 ttt = Vector3.Normalize( targetPosition - Target.position );
			canReverse = !Physics.CheckSphere( targetPosition + ttt * SmallValue, 1f, wallMask ) && followDistance < MaxFollowDistance;

			// If so, move backward by a small amount
			if( canReverse )
			{
				followDistance += SmallValue;
			}
		} while( canReverse );
	}

	void Update()
	{
		// Take Input and clamp Y Axis
		xAngle += inputWrapper.RightStick.x * 90f * Time.deltaTime;
		yAngle += inputWrapper.RightStick.y * -90f * Time.deltaTime;

		yAngle = Mathf.Clamp( yAngle, -80f, 80f );

		// Apply transform changes
		transform.position = Vector3.Lerp( transform.position, targetPosition, PositionLerpFactor * Time.deltaTime );
		transform.rotation = Quaternion.LookRotation( Vector3.Normalize( ( Target.position + Offset ) - transform.position ), Vector3.up );
	}

	void OnGUI()
	{
		// Draw the crosshair texture in the center of the viewport
		Vector3 pt = camera.ViewportToScreenPoint( new Vector3( .5f, .5f, 0f ) );
		float pixWidth = camera.pixelWidth;
		float pixHeight = camera.pixelHeight;
		float avgSize = ( pixWidth + pixHeight ) * .5f;
		float size = avgSize * .004f;
		float halfSize = size * .5f;
		GUI.DrawTexture( new Rect( pt.x - halfSize, pt.y - halfSize, size, size ), CrosshairTexture );
	}

	// Utility Methods
	void UpdateTarget()
	{
		// Calculate the camera's position and rotation relative to the target
		Vector3 cameraForward = Vector3.forward;
		cameraForward = Quaternion.AngleAxis( yAngle, Vector3.right ) * cameraForward;
		cameraForward = Quaternion.AngleAxis( xAngle, Vector3.up ) * cameraForward;
		targetPosition = Target.position - ( cameraForward * followDistance );
	}
}