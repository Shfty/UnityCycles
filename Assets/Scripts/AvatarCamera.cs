using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarCamera : MonoBehaviour
{
	// Fields
	InputWrapper inputWrapper;
	Vector3 targetPosition;
	Vector3 velocity;
	float xAngle = 0f;
	float yAngle = 0f;
	float followDistance;
	float prevRespawn = 0f;
	GameObject[] mapCameraAnchors;

	int terrainMask;
	int wallMask;
	int pickupMask;
	int projectileMask;

	// Properties
	public Transform Target;
	public Vector3 Offset;
	public float MinFollowDistance = 1f;
	public float MaxFollowDistance = 10f;
	public float PositionLerpFactor = 12f;
	public float CameraRadius = .5f;
	public float SmallValue = .01f;
	public bool DeathCam = false;
	public int RenderLayer;
	
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
		// Find fallback camera anchors
		mapCameraAnchors = GameObject.FindGameObjectsWithTag( "MapCamera" );

		// Setup render layer string
		RenderLayer = LayerMask.NameToLayer( "Camera " + ( inputWrapper.LocalPlayerIndex + 1 ) );

		// Set the target position behind the target and init the camera position to it
		targetPosition = Target.position - ( Vector3.forward * followDistance );
		transform.position = targetPosition;

		// Create layer masks
		terrainMask = 1 << LayerMask.NameToLayer( "Terrain" );
		wallMask = 1 << LayerMask.NameToLayer( "Walls" );
	}

	void Update()
	{
		// Fallback to a map anchor if the follow target is unsuitable
		if( Target == null || !Target.parent.gameObject.activeSelf )
		{
			// Randomly pick a camera anchor
			int i = Random.Range( 0, mapCameraAnchors.Length );
			GameObject cameraAnchor = mapCameraAnchors[ i ];
			Target = cameraAnchor.transform;
		}

		// Take Input and clamp Y Axis
		xAngle += inputWrapper.RightStick.x * 90f * Time.deltaTime;
		yAngle += inputWrapper.RightStick.y * -90f * Time.deltaTime;

		yAngle = Mathf.Clamp( yAngle, -80f, 80f );

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

		// Check respawn
		if( DeathCam )
		{
			if( inputWrapper.Fire == 1f && prevRespawn == 0f )
			{
				GameControl.Instance.Respawn( transform.parent.gameObject );
			}
		}
		prevRespawn = inputWrapper.Fire;

		// Apply transform changes
		transform.position = Vector3.Lerp( transform.position, targetPosition, PositionLerpFactor * Time.deltaTime );
		transform.rotation = Quaternion.LookRotation( Vector3.Normalize( ( Target.position + Offset ) - transform.position ), Vector3.up );
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