using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowCamera : MonoBehaviour
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
	public Texture CrosshairTexture;
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

	void OnGUI()
	{
		// Get the center of the viewport in screen space
		Vector3 pt = camera.ViewportToScreenPoint( new Vector3( .5f, .5f, 0f ) );
		// Convert between screen space and GUI space
		pt.y = Screen.height - pt.y;
		// Window Size
		float pixWidth = camera.pixelWidth;
		float pixHeight = camera.pixelHeight;

		// Crosshair
		float minSize = Mathf.Min( pixWidth, pixHeight );
		float size = minSize * .004f;
		float halfSize = size * .5f;
		GUI.DrawTexture( new Rect( pt.x - halfSize, pt.y - halfSize, size, size ), CrosshairTexture );

		// Player UI
		if( Target.gameObject.name == "Marble" )
		{
			// Health
			int health = Target.parent.GetComponent<PlayerInstance>().Health;
			string healthText = "Health: " + health.ToString();
			Vector2 healthTextBounds = GUIStyle.none.CalcSize( new GUIContent( healthText ) );
			Vector2 healthBarBounds = new Vector2( 100, healthTextBounds.y * 2 );
			GUI.Label( new Rect( ( pt.x - pixWidth / 2 ) + 5, ( pt.y + pixHeight / 2 ) - 10 - healthBarBounds.y * 2, healthTextBounds.x, healthTextBounds.y * 2f ), healthText );
			GUI.HorizontalSlider( new Rect( ( pt.x - pixWidth / 2 ) + 5, ( pt.y + pixHeight / 2 ) - healthBarBounds.y - 5, healthBarBounds.x, healthBarBounds.y * 2f ), health, 0, 100 );

			// Dash
			float dash = Target.parent.GetComponent<PlayerInstance>().Dash;
			float maxDash = Target.parent.GetComponent<PlayerInstance>().MaxDash;
			string dashText = "Dash: " + dash.ToString();
			Vector2 dashTextBounds = GUIStyle.none.CalcSize( new GUIContent( dashText ) );
			Vector2 dashBarBounds = new Vector2( 100, dashTextBounds.y * 2 );
			GUI.Label( new Rect( ( pt.x + pixWidth / 2 ) - dashBarBounds.x - 5, ( pt.y + pixHeight / 2 ) - 10 - dashBarBounds.y * 2, dashTextBounds.x, dashTextBounds.y * 2f ), dashText );
			GUI.HorizontalSlider( new Rect( ( pt.x + pixWidth / 2 ) - dashBarBounds.x - 5, ( pt.y + pixHeight / 2 ) - dashBarBounds.y - 5, dashBarBounds.x, dashBarBounds.y * 2f ), dash, 0f, maxDash );

			// Drones
			// Active
			List<GameObject> drones = Target.parent.GetComponent<PlayerInstance>().Drones;
			if( drones.Count > 0 )
			{
				Drone mDrone = drones[ 0 ].GetComponent<Drone>();
				DrawDroneLabel( pt, new Vector2( pixWidth, pixHeight ), GetDroneLetter( mDrone ), mDrone.Ammo.ToString() );
				if( drones.Count > 1 )
				{
					Drone lDrone = drones[ 1 ].GetComponent<Drone>();
					DrawDroneLabel( new Vector2( pt.x - pixWidth * .1f, pt.y - pixHeight * .05f ), new Vector2( pixWidth, pixHeight ), GetDroneLetter( lDrone ), lDrone.Ammo.ToString() );
					if( drones.Count > 2 )
					{
						Drone rDrone = drones[ 2 ].GetComponent<Drone>();
						DrawDroneLabel( new Vector2( pt.x + pixWidth * .1f, pt.y - pixHeight * .05f ), new Vector2( pixWidth, pixHeight ), GetDroneLetter( rDrone ), rDrone.Ammo.ToString() );
					}
				}
			}
		}

		// Death Cam
		if( DeathCam )
		{
			// Respawn Prompt
			string respawnText = "Fire to Respawn";
			Vector2 respawnTextBounds = GUIStyle.none.CalcSize( new GUIContent( respawnText ) );
			GUI.Label( new Rect( pt.x - ( respawnTextBounds.x * .5f ), pt.y + ( respawnTextBounds.y * .5f ), respawnTextBounds.x, respawnTextBounds.y * 2f ), respawnText );
		}
	}

	// Utility Methods
	string GetDroneLetter( Drone drone )
	{
		string droneLetter = "";
		switch( drone.Type )
		{
			case DroneInfo.Type.Rocket:
				droneLetter = "R";
				break;
			case DroneInfo.Type.Mortar:
				droneLetter = "M";
				break;
			case DroneInfo.Type.Seeker:
				droneLetter = "S";
				break;
			default:
				break;
		}
		return droneLetter;
	}

	void DrawDroneLabel( Vector2 pt, Vector2 screenBounds, string label, string ammoCount )
	{
		Vector2 labelBounds = GUIStyle.none.CalcSize( new GUIContent( label ) );
		GUI.Label( new Rect( pt.x - ( labelBounds.x * .5f ), pt.y + ( screenBounds.y * .25f ), labelBounds.x, labelBounds.y * 2f ), label );
		Vector2 ammoLabelBounds = GUIStyle.none.CalcSize( new GUIContent( ammoCount ) );
		GUI.Label( new Rect( pt.x - ( ammoLabelBounds.x * .5f ), pt.y + ( screenBounds.y * .25f ) + 15, ammoLabelBounds.x, ammoLabelBounds.y * 2f ), ammoCount );
	}

	void UpdateTarget()
	{
		// Calculate the camera's position and rotation relative to the target
		Vector3 cameraForward = Vector3.forward;
		cameraForward = Quaternion.AngleAxis( yAngle, Vector3.right ) * cameraForward;
		cameraForward = Quaternion.AngleAxis( xAngle, Vector3.up ) * cameraForward;
		targetPosition = Target.position - ( cameraForward * followDistance );
	}
}