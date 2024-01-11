using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarCamera : MonoBehaviour
{
	// Fields
	InputWrapper _inputWrapper = null;
	int _renderLayer;

	// Properties
	public InputWrapper InputWrapper
	{
		get { return _inputWrapper; }
		set
		{
			_inputWrapper = value;
			if( _inputWrapper != null )
			{
				RenderLayer = LayerMask.NameToLayer( "Camera " + ( _inputWrapper.LocalPlayerIndex + 1 ) );
			}
		}
	}
	public int RenderLayer
	{
		get { return _renderLayer; }
		set { _renderLayer = value; }
	}

	// Variables
	// Private
	Vector3 targetPosition;
	Vector3 velocity;
	float xAngle = 0f;
	float yAngle = 0f;
	float followDistance;
	GameObject[] mapCameraAnchors;
	delegate bool CheckAngle( out RaycastHit th );

	int terrainMask;
	int wallMask;
	int pickupMask;
	int projectileMask;

	// Public
	public Transform Target;
	public Vector3 Offset;
	public float MinFollowDistance = 1f;
	public float MaxFollowDistance = 10f;
	public float CameraRadius = .5f;
	public float SmallValue = .05f;
	
	// Unity Methods
	void Awake()
	{
		// Start at maximum follow distance
		followDistance = MaxFollowDistance;
	}

	void Start()
	{
		// Find fallback camera anchors
		mapCameraAnchors = GameObject.FindGameObjectsWithTag( "MapCamera" );

		// Set the target position behind the target and init the camera position to it
		targetPosition = Target.position - ( Vector3.forward * followDistance );
		transform.position = targetPosition;

		// Create layer masks
		terrainMask = 1 << LayerMask.NameToLayer( "Terrain" );
		wallMask = 1 << LayerMask.NameToLayer( "Walls" );
	}

	void Update()
	{
		// Fallback to a map anchor if the follow target is null or inactive
		if( Target == null || !Target.parent.gameObject.activeSelf )
		{
			// Randomly pick a camera anchor
			int i = Random.Range( 0, mapCameraAnchors.Length );
			GameObject cameraAnchor = mapCameraAnchors[ i ];
			Target = cameraAnchor.transform;
		}

		// Take Input and clamp Y Axis
		xAngle += InputWrapper.RightStick.Value.x * 90f * Time.deltaTime;
		yAngle += InputWrapper.RightStick.Value.y * -90f * Time.deltaTime;
		yAngle = Mathf.Clamp( yAngle, -80f, 80f );

		CalculateTargetPosition();

		// Correct positioning
		Vector3 tc = TargetToCameraVector();

		// Adjust angle from terrain
		RaycastHit terrainHitInfo = new RaycastHit();
		bool clipTerrain = false;
		Vector3 targetOffset = new Vector3( 0, -Target.GetComponent<SphereCollider>().radius * .5f, 0 );

		CheckAngle checkTerrain = ( out RaycastHit th ) => Physics.Raycast( Target.transform.position + targetOffset, tc.normalized, out th, MaxFollowDistance, terrainMask );
		clipTerrain = checkTerrain( out terrainHitInfo );
		while( clipTerrain )
		{
			yAngle += SmallValue;
			CalculateTargetPosition();
			tc = TargetToCameraVector();
			clipTerrain = checkTerrain( out terrainHitInfo );
		}

		// Adjust distance from walls
		RaycastHit wallHitInfo;
		bool clipWall = false;

		clipWall = Physics.Raycast( Target.transform.position, tc.normalized, out wallHitInfo, tc.magnitude, wallMask );
		if( clipWall )
		{
			tc = tc.normalized * ( wallHitInfo.distance - CameraRadius );
		}

		targetPosition = Target.position + tc;

		// Apply transform changes
		float distScale = tc.magnitude / MaxFollowDistance;
		transform.position = targetPosition;
		transform.rotation = Quaternion.LookRotation( Vector3.Normalize( ( Target.position + Offset * distScale ) - transform.position ), Vector3.up );
	}

	void CalculateTargetPosition()
	{
		// Calculate the 'ideal' camera position
		Vector3 cameraForward = Quaternion.AngleAxis( yAngle, Vector3.right ) * Vector3.forward;
		cameraForward = Quaternion.AngleAxis( xAngle, Vector3.up ) * cameraForward;
		targetPosition = Target.position - ( cameraForward * followDistance );
	}

	Vector3 TargetToCameraVector()
	{
		return targetPosition - Target.position;
	}
}