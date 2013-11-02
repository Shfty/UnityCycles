using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarGUI : MonoBehaviour
{
	// Fields
	float gridSize;
	List<GameObject> drones;
	int activeDroneIndex = 0;
	List<Vector2> droneIconPositions;
	List<Vector2> droneIconTargets;
	int health = 0;
	int healthTarget;
	float dash = 0;
	float dashTarget;

	// Properties
	public Texture CrosshairTexture;
	public List<Texture> DroneIconTextures;
	public float IconLerpFactor = 5f;
	public float BarLerpFactor = 5f;

	// Unity Methods
	void Awake()
	{
		droneIconPositions = new List<Vector2>();
		droneIconTargets = new List<Vector2>();
		Vector2 droneStartPos = new Vector2( Screen.width * .5f, Screen.height * 2f );
		for( int i = 0; i < 3; ++i )
		{
			droneIconPositions.Add( droneStartPos );
			droneIconTargets.Add( droneStartPos );
		}
	}
	
	void Start()
	{
	
	}
	
	void OnGUI()
	{
		Transform target = GetComponent<AvatarCamera>().Target;

		if( target != null )
		{
			float pixWidth = camera.pixelWidth;
			float pixHeight = camera.pixelHeight;
			float halfWidth = pixWidth * .5f;
			float halfHeight = pixHeight * .5f;
			gridSize = 64f * camera.pixelHeight / Screen.height;

			Vector2 centerPt = new Vector2( camera.pixelWidth * .5f, camera.pixelHeight * .5f );
			centerPt.y = Screen.height - centerPt.y;

			if( target.name == "Marble" )
			{
				Avatar avatar = target.parent.GetComponent<Avatar>();
				drones = avatar.Drones;
				activeDroneIndex = avatar.ActiveDroneIndex;

				// Crosshair
				float minSize = Mathf.Min( pixWidth, pixHeight );
				float size = minSize * .005f;
				float halfSize = size * .5f;
				GUI.DrawTexture( new Rect( centerPt.x - halfSize, centerPt.y - halfSize, size, size ), CrosshairTexture );

				// Health
				int healthTarget = target.parent.GetComponent<Avatar>().Health;
				health = (int)Mathf.Lerp( health, healthTarget, BarLerpFactor * Time.deltaTime );
				GUIContent healthText = new GUIContent( "Health: " + health.ToString() );

				Vector2 healthTextBounds = GUIStyle.none.CalcSize( healthText );
				Vector2 healthBarBounds = new Vector2( 100, healthTextBounds.y * 1.2f );

				Rect healthTextRect = new Rect( centerPt.x - gridSize - healthBarBounds.x, centerPt.y + gridSize * 4f - healthBarBounds.y, healthTextBounds.x, healthTextBounds.y * 2f );
				Rect healthBarRect = new Rect( centerPt.x - gridSize - healthBarBounds.x, centerPt.y + gridSize * 4f, healthBarBounds.x, healthBarBounds.y * 2f );

				GUI.Label( healthTextRect, healthText );
				GUI.HorizontalSlider( healthBarRect, health, 0, 100 );

				// Dash
				dashTarget = target.parent.GetComponent<Avatar>().Dash;
				dash = Mathf.Lerp( dash, dashTarget, BarLerpFactor * Time.deltaTime );
				float maxDash = target.parent.GetComponent<Avatar>().MaxDash;
				GUIContent dashText = new GUIContent( "Dash: " + dash.ToString( "p2" ) );

				Vector2 dashTextBounds = GUIStyle.none.CalcSize( new GUIContent( dashText ) );
				Vector2 dashBarBounds = new Vector2( 100, dashTextBounds.y * 1.2f );

				Rect dashTextRect = new Rect( centerPt.x + gridSize, centerPt.y + gridSize * 4f - dashBarBounds.y, dashTextBounds.x, dashTextBounds.y * 2f );
				Rect dashBarRect = new Rect( centerPt.x + gridSize, centerPt.y + gridSize * 4f, dashBarBounds.x, dashBarBounds.y * 2f );

				GUI.Label( dashTextRect, dashText );
				GUI.HorizontalSlider( dashBarRect, dash, 0f, maxDash );

				// Drones
				// Calculate Target Positions
				if( drones.Count > 0 )
				{
					Vector2 mPosition = new Vector2( centerPt.x, centerPt.y + gridSize * 4f  );
					droneIconTargets[ activeDroneIndex ] = mPosition;
					if( drones.Count > 1 )
					{
						Vector2 lPosition = new Vector2( centerPt.x - gridSize * 1.75f, centerPt.y + gridSize * 4f + gridSize );
						droneIconTargets[ avatar.WrapIndex( activeDroneIndex + 1, drones.Count - 1 ) ] = lPosition;
						if( drones.Count > 2 )
						{
							Vector2 rPosition = new Vector2( centerPt.x + gridSize * 1.75f, centerPt.y + gridSize * 4f + gridSize );
							droneIconTargets[ avatar.WrapIndex( activeDroneIndex - 1, drones.Count - 1 ) ] = rPosition;
						}
					}
				}

				for( int i = 0; i < drones.Count; ++i )
				{
					// Interpolate toward target positions
					droneIconPositions[ i ] = Vector2.Lerp( droneIconPositions[ i ], droneIconTargets[ i ], IconLerpFactor * Time.deltaTime );

					// Render icons
					DrawDroneIcon( drones[ i ].GetComponent<Drone>(), droneIconPositions[ i ] );
				}
			}

			// Score
			string scoreText = "Score: " + transform.parent.GetComponent<Player>().Score.ToString();
			Vector2 scoreTextBounds = GUIStyle.none.CalcSize( new GUIContent( scoreText ) );
			GUI.Label( new Rect( centerPt.x - scoreTextBounds.x * .5f, centerPt.y - halfHeight + gridSize * .5f, scoreTextBounds.x, scoreTextBounds.y * 2f ), scoreText );
		}
	}

	// Utility Methods
	Texture GetDroneIcon( Drone drone )
	{
		Texture droneIcon = null;
		switch( drone.Type )
		{
			case DroneInfo.Type.Rocket:
				droneIcon = DroneIconTextures.Find( item => item.name == "Rocket Overlay" );
				break;
			case DroneInfo.Type.Mortar:
				droneIcon = DroneIconTextures.Find( item => item.name == "Mortar Overlay" );
				break;
			case DroneInfo.Type.Seeker:
				droneIcon = DroneIconTextures.Find( item => item.name == "Seeker Overlay" );
				break;
			default:
				break;
		}
		return droneIcon;
	}

	void DrawDroneIcon( Drone drone, Vector2 pt )
	{
		float pixWidth = camera.pixelWidth;
		float pixHeight = camera.pixelHeight;
		float minScreenDimension = Mathf.Min( pixWidth, pixHeight );

		GUI.DrawTexture( new Rect( pt.x - gridSize * .5f, pt.y - gridSize * .5f, gridSize, gridSize ), GetDroneIcon( drone ) );
		string ammoString = drone.Ammo.ToString();
		Vector2 ammoLabelBounds = GUIStyle.none.CalcSize( new GUIContent( ammoString ) );
		GUI.Label( new Rect( pt.x - ( ammoLabelBounds.x * .5f ), pt.y + gridSize * .5f, ammoLabelBounds.x, ammoLabelBounds.y * 2f ), ammoString );
	}
}
