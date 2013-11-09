using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarGUI : MonoBehaviour
{
	// Properties
	public InputWrapper InputWrapper { get; set; }

	// Variables
	// Private
	float gridSize;
	List<GameObject> drones;
	int activeDroneIndex = 0;
	List<Vector2> droneIconPositions;
	List<Vector2> droneIconTargets;
	float health = 0;
	float healthTarget;
	float dash = 0;
	float dashTarget;
	float prevRespawn = 0f;
	float scaleFactor;

	// Public
	public GUISkin Skin;
	public Texture CrosshairTexture;
	public List<Texture> DroneIconTextures;
	public float IconLerpFactor = 5f;
	public float BarLerpFactor = 5f;

	public bool DeathCam = false;

	// Unity Methods
	void Awake()
	{
		// Init drone position containers
		droneIconPositions = new List<Vector2>();
		droneIconTargets = new List<Vector2>();
		Vector2 droneStartPos = new Vector2( Screen.width * .5f, Screen.height * 2f );
		for( int i = 0; i < 3; ++i )
		{
			droneIconPositions.Add( droneStartPos );
			droneIconTargets.Add( droneStartPos );
		}
	}

	void OnEnable()
	{
		// Restore init state
		DeathCam = false;
	}
	
	void Update()
	{
		// Check respawn
		if( DeathCam )
		{
			if( InputWrapper.Fire > 0f && prevRespawn == 0f )
			{
				GameControl.Instance.RespawnPlayer( transform.parent.gameObject );
			}
		}
		prevRespawn = InputWrapper.Fire;
	}
	
	void OnGUI()
	{
		// Setup skin
		scaleFactor = camera.pixelHeight / GUIInfo.Player.OptimalViewHeight;
		GUI.skin = Skin;
		GUI.skin.label.fontSize = GUI.skin.GetStyle( "healthtext" ).fontSize = GUI.skin.GetStyle( "dashtext" ).fontSize = (int)( GUIInfo.Player.OptimalFontSize * scaleFactor );
		if( GUI.skin.label.fontSize < 18 )
		{
			GUI.skin.label.fontStyle = GUI.skin.GetStyle( "healthtext" ).fontStyle = GUI.skin.GetStyle( "dashtext" ).fontStyle = FontStyle.Bold;
		}
		else
		{
			GUI.skin.label.fontStyle = GUI.skin.GetStyle( "healthtext" ).fontStyle = GUI.skin.GetStyle( "dashtext" ).fontStyle = FontStyle.Normal;
		}
		GUI.skin.horizontalSlider.fixedHeight = GUI.skin.horizontalSliderThumb.fixedWidth = GUI.skin.horizontalSliderThumb.fixedHeight = GUIInfo.Player.OptimalBarSize * scaleFactor;
		// Get viewport metrics
		float pixWidth = camera.pixelWidth;
		float pixHeight = camera.pixelHeight;
		float halfWidth = pixWidth * .5f;
		float halfHeight = pixHeight * .5f;
		gridSize = 64f * scaleFactor;

		// Get viewport center
		Vector2 centerPt = camera.pixelRect.center;
		// Convert from viewport space to screen space
		centerPt.y = Screen.height - centerPt.y;

		Transform target = GetComponent<AvatarCamera>().Target;
		if( target != null )
		{
			// Target stat readout
			if( target.name == "Marble" )
			{
				Avatar avatar = target.parent.GetComponent<Avatar>();
				drones = avatar.Drones;
				activeDroneIndex = avatar.ActiveDroneIndex;

				// Crosshair
				if( !DeathCam )
				{
					float minSize = Mathf.Min( pixWidth, pixHeight );
					float size = minSize * .005f;
					float halfSize = size * .5f;
					GUI.DrawTexture( new Rect( centerPt.x - halfSize, centerPt.y - halfSize, size, size ), CrosshairTexture );
				}

				// Health
				float healthTarget = target.parent.GetComponent<Avatar>().Health;
				health = Mathf.Lerp( health, healthTarget, BarLerpFactor * Time.deltaTime );
				GUIContent healthText = new GUIContent( "Health" );
				GUIContent healthNumber = new GUIContent( health.ToString() );

				Vector2 healthTextBounds = GUI.skin.GetStyle( "healthtext" ).CalcSize( healthText );
				Vector2 healthNumberBounds = GUI.skin.GetStyle( "healthtext" ).CalcSize( healthNumber );
				Vector2 healthBarBounds = new Vector2( 100, healthTextBounds.y * 1.2f );

				Rect healthTextRect = new Rect( centerPt.x - gridSize - healthBarBounds.x, centerPt.y + gridSize * 4f - healthTextBounds.y - healthNumberBounds.y, healthTextBounds.x, healthTextBounds.y * 2f );
				Rect healthNumberRect = new Rect( centerPt.x - gridSize - healthBarBounds.x, centerPt.y + gridSize * 4f - healthNumberBounds.y, healthNumberBounds.x, healthNumberBounds.y * 2f );
				Rect healthBarRect = new Rect( centerPt.x - gridSize - healthBarBounds.x, centerPt.y + gridSize * 4f, healthBarBounds.x, healthBarBounds.y * 2f );

				GUI.Label( healthTextRect, healthText, "healthtext" );
				GUI.Label( healthNumberRect, healthNumber, "healthtext" );
				GUI.HorizontalSlider( healthBarRect, health, 0, 100 );

				// Dash
				dashTarget = target.parent.GetComponent<Avatar>().Dash;
				dash = Mathf.Lerp( dash, dashTarget, BarLerpFactor * Time.deltaTime );
				float maxDash = target.parent.GetComponent<Avatar>().MaxDash;
				GUIContent dashText = new GUIContent( "Dash" );
				GUIContent dashNumber = new GUIContent( dash.ToString( "p2" ) );

				Vector2 dashTextBounds = GUI.skin.GetStyle( "dashtext" ).CalcSize( dashText );
				Vector2 dashNumberBounds = GUI.skin.GetStyle( "dashtext" ).CalcSize( dashNumber );
				Vector2 dashBarBounds = new Vector2( 100, dashTextBounds.y * 1.2f );

				Rect dashTextRect = new Rect( centerPt.x + gridSize, centerPt.y + gridSize * 4f - dashTextBounds.y - dashNumberBounds.y, dashTextBounds.x, dashTextBounds.y );
				Rect dashNumberRect = new Rect( centerPt.x + gridSize, centerPt.y + gridSize * 4f - dashNumberBounds.y, dashNumberBounds.x, dashNumberBounds.y );
				Rect dashBarRect = new Rect( centerPt.x + gridSize, centerPt.y + gridSize * 4f, dashBarBounds.x, dashBarBounds.y );

				GUI.Label( dashTextRect, dashText, "dashtext" );
				GUI.Label( dashNumberRect, dashNumber, "dashtext" );
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

			// Fire to Respawn prompt
			if( DeathCam )
			{
				GUIContent respawnText = new GUIContent( "Fire to Respawn" );
				Vector2 respawnTextBounds = GUI.skin.label.CalcSize( respawnText );
				Rect respawnTextRect = new Rect( centerPt.x - respawnTextBounds.x * .5f, centerPt.y - respawnTextBounds.y, respawnTextBounds.x, respawnTextBounds.y * 2f );
				GUI.Label( respawnTextRect, respawnText );
			}

			// Camera parent's score
			string scoreText = "Score";
            if( GameControl.Instance.Players.Contains( transform.parent.gameObject ) )
            {
			    int playerScoresIndex = GameControl.Instance.Players.IndexOf( transform.parent.gameObject );
                if( playerScoresIndex < GameRules.Instance.PlayerScores.Count )
                {
                    string scoreNumber = GameRules.Instance.PlayerScores[playerScoresIndex].ToString();

                    Vector2 scoreTextBounds = GUI.skin.label.CalcSize( new GUIContent( scoreText ) );
                    Vector2 scoreNumberBounds = GUI.skin.label.CalcSize( new GUIContent( scoreNumber ) );

                    GUI.Label( new Rect( centerPt.x - scoreTextBounds.x * .5f, centerPt.y - halfHeight + gridSize * .5f, scoreTextBounds.x, scoreTextBounds.y * 2f ), scoreText );
                    GUI.Label( new Rect( centerPt.x - scoreNumberBounds.x * .5f, centerPt.y - halfHeight + gridSize * .5f + scoreTextBounds.y, scoreNumberBounds.x, scoreNumberBounds.y * 2f ), scoreNumber );
                }
            }
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
		Vector2 ammoLabelBounds = GUI.skin.label.CalcSize( new GUIContent( ammoString ) );
		GUI.Label( new Rect( pt.x - ( ammoLabelBounds.x * .5f ), pt.y + gridSize * .5f, ammoLabelBounds.x, ammoLabelBounds.y * 2f ), ammoString );
	}
}
