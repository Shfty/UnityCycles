using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarGUI : MonoBehaviour
{
	class GUIIcon
	{
		public Vector2 Position;
		public Vector2 TargetPosition;
		public float Size;
		public Texture Texture;
		public string Subtitle;
		public float LerpFactor;

		public GUIIcon( Vector2 startPosition, float size, Texture texture, string subtitle = "", float lerpFactor = 1f )
		{
			Position = startPosition;
			TargetPosition = startPosition;
			Size = size;
			Texture = texture;
			Subtitle = subtitle;
			LerpFactor = lerpFactor;
		}

		public virtual void Update()
		{
			Position = Vector2.Lerp( Position, TargetPosition, LerpFactor * Time.deltaTime );
		}

		public void Draw()
		{
			GUI.DrawTexture( new Rect( Position.x - Size * .5f, Position.y - Size * .5f, Size, Size ), Texture );
			string ammoString = Subtitle;
			Vector2 ammoLabelBounds = GUI.skin.label.CalcSize( new GUIContent( ammoString ) );
			GUI.Label( new Rect( Position.x - ( ammoLabelBounds.x * .5f ), Position.y + Size * .5f, ammoLabelBounds.x, ammoLabelBounds.y * 2f ), ammoString );
		}
	}

	class DroneIcon : GUIIcon
	{
		public Drone Drone;

		public DroneIcon( Vector2 startPosition, float size, Texture texture, Drone drone, float lerpFactor = 1f )
			: base( startPosition, size, texture, "", lerpFactor )
		{
			Drone = drone;
		}

		public override void Update()
		{
			base.Update();

			Subtitle = Drone.Ammo.ToString();
		}
	}

	// Properties
	public InputWrapper InputWrapper { get; set; }

	// Variables
	// Private
	float iconSize;
	List<DroneIcon> droneIcons = new List<DroneIcon>();
	List<Vector2> droneIconAnchors = new List<Vector2>();
	float health = 0;
	float healthTarget;
	float dash = 0;
	float dashTarget;
	float scaleFactor;
	Rect screenRect;

	// Public
	public GUISkin Skin;
	public Texture CrosshairTexture;
	public List<Texture> DroneIconTextures;
	public float DroneIconSize = 64f;
	public float IconLerpFactor = 5f;
	public float BarLerpFactor = 5f;

	public bool DeathCam = false;

	// Unity Methods
	void Awake()
	{
		scaleFactor = GetComponent<Camera>().pixelHeight / GUIInfo.Player.OptimalViewHeight;
		iconSize = DroneIconSize * scaleFactor;
		// Init drone position containers
		Vector2 droneStartPos = new Vector2( Screen.width * .5f, Screen.height * 2f );
		for( int i = 0; i < 3; ++i )
		{
			droneIconAnchors.Add( droneStartPos );
		}
	}

	void OnEnable()
	{
		// Restore init state
		DeathCam = false;
		droneIcons.Clear();
	}
	
	void Update()
	{
		// Check respawn
		if( DeathCam )
		{
			if( InputWrapper.Fire.Pressed )
			{
				GameControl.Instance.RespawnPlayer( transform.parent.gameObject );
			}
		}


		// Drones
		for( int i = 0; i < droneIcons.Count; ++i )
		{
			droneIcons[ i ].TargetPosition = droneIconAnchors[ i ];
			droneIcons[ i ].Update();
		}
	}

	void OnGUI()
	{
		// Setup skin
		scaleFactor = GetComponent<Camera>().pixelHeight / GUIInfo.Player.OptimalViewHeight;
		iconSize = DroneIconSize * scaleFactor;
		GUI.skin = Skin;
		GUI.skin.label.fontSize = GUI.skin.GetStyle( "healthtext" ).fontSize = GUI.skin.GetStyle( "dashtext" ).fontSize = (int)( GUIInfo.Player.OptimalFontSize * scaleFactor );
		GUI.skin.label.wordWrap = false;
		if( GUI.skin.label.fontSize < 18 )
		{
			GUI.skin.label.fontStyle = GUI.skin.GetStyle( "healthtext" ).fontStyle = GUI.skin.GetStyle( "dashtext" ).fontStyle = FontStyle.Bold;
		}
		else
		{
			GUI.skin.label.fontStyle = GUI.skin.GetStyle( "healthtext" ).fontStyle = GUI.skin.GetStyle( "dashtext" ).fontStyle = FontStyle.Normal;
		}
		GUI.skin.horizontalSlider.fixedHeight = GUI.skin.horizontalSliderThumb.fixedWidth = GUI.skin.horizontalSliderThumb.fixedHeight = GUIInfo.Player.OptimalBarSize * scaleFactor;

		Transform target = GetComponent<AvatarCamera>().Target;
		if( target != null )
		{
			// Calculate screen rect
			screenRect = GetComponent<Camera>().pixelRect;
			screenRect.y = Screen.height - screenRect.y - GetComponent<Camera>().pixelHeight; // Camera Space => Screen Space

			// Fixed GUI
			if( DeathCam )
			{
				// Fire to Respawn prompt
				GUIContent respawnText = new GUIContent( "Fire to Respawn" );
				Vector2 respawnTextBounds = GUI.skin.label.CalcSize( respawnText );
				Rect respawnTextRect = new Rect( screenRect.center.x - respawnTextBounds.x * .5f, screenRect.center.y - respawnTextBounds.y, respawnTextBounds.x, respawnTextBounds.y * 2f );
				GUI.Label( respawnTextRect, respawnText );
			}

			if( target.name == "Marble" )
			{
				// Crosshair
				if( !DeathCam )
				{
					float minSize = Mathf.Min( GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight );
					float size = minSize * .005f;
					float halfSize = size * .5f;
					GUI.DrawTexture( new Rect( GetComponent<Camera>().pixelRect.center.x - halfSize, GetComponent<Camera>().pixelRect.center.y - halfSize, size, size ), CrosshairTexture );
				}

				// Drones
				Avatar avatar = target.parent.GetComponent<Avatar>();
				droneIconAnchors[ avatar.ActiveDroneIndex ] = new Vector2( screenRect.center.x, screenRect.center.y + iconSize * 4f );
				droneIconAnchors[ avatar.WrapIndex( avatar.ActiveDroneIndex + 1, 2 ) ] = new Vector2( screenRect.center.x - iconSize * 1.75f, screenRect.center.y + iconSize * 4f + iconSize );
				droneIconAnchors[ avatar.WrapIndex( avatar.ActiveDroneIndex - 1, 2 ) ] = new Vector2( screenRect.center.x + iconSize * 1.75f, screenRect.center.y + iconSize * 4f + iconSize );
				for( int i = 0; i < droneIcons.Count; ++i )
				{
					droneIcons[ i ].Draw();
				}
			}

			// Auto GUI
			// Container Area
			GUILayout.BeginArea( screenRect );
			{
				// Score Area
				GUILayout.Space( 10 * scaleFactor );
				GUILayout.BeginVertical( GUILayout.ExpandWidth( true ) );
				{
					if( GameControl.Instance.Players.Contains( transform.parent.gameObject ) )
					{
						GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
						{
							GUILayout.Space( 10 * scaleFactor );
							GUILayout.BeginVertical();
							{
								int playerScoresIndex = GameControl.Instance.Players.IndexOf( transform.parent.gameObject );
								if( playerScoresIndex < GameRules.Instance.PlayerScores.Count )
								{
									int playerScore = GameRules.Instance.PlayerScores[ playerScoresIndex ];

									GUILayout.Label( "Score", GUILayout.ExpandWidth( true ) );
									GUILayout.Label( playerScore.ToString(), GUILayout.ExpandWidth( true ) );
								}
							}
							GUILayout.EndVertical();
							GUILayout.Space( 10 * scaleFactor );
						}
						GUILayout.EndHorizontal();
					}
				}
				GUILayout.EndVertical();

				// Center Area
				GUILayout.BeginVertical( GUILayout.ExpandWidth( true ) );
				{
					GUILayout.FlexibleSpace();
					GUILayout.FlexibleSpace();
					GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
					{
						GUILayout.FlexibleSpace();

						if( target.name == "Marble" )
						{
							// Health Container
							GUILayout.BeginVertical( GUILayout.Width( iconSize * 4f ) );
							{
								float healthTarget = target.parent.GetComponent<Avatar>().Health;
								health = Mathf.Lerp( health, healthTarget, BarLerpFactor * Time.deltaTime );
								GUILayout.BeginHorizontal();
								{
									GUILayout.Label( "Health" );
									GUILayout.FlexibleSpace();
									GUILayout.Label( health.ToString( "f0" ), GUILayout.Width( GUI.skin.label.CalcSize( new GUIContent( "100" ) ).x ) );
								}
								GUILayout.EndHorizontal();
								GUILayout.HorizontalSlider( health, 0f, 100f, GUILayout.ExpandWidth( true ) );
							}
							GUILayout.EndVertical();

							GUILayout.FlexibleSpace();

							// Dash Container
							GUILayout.BeginVertical( GUILayout.Width( iconSize * 4f ) );
							{
								float dashTarget = target.parent.GetComponent<Avatar>().Dash;
								dash = Mathf.Lerp( dash, dashTarget, BarLerpFactor * Time.deltaTime );
								float maxDash = target.parent.GetComponent<Avatar>().MaxDash;
								GUILayout.BeginHorizontal();
								{
									GUILayout.Label( dash.ToString( "p2" ), GUILayout.Width( GUI.skin.label.CalcSize( new GUIContent( "150.00 %" ) ).x ) );
									GUILayout.FlexibleSpace();
									GUILayout.Label( "Dash" );
								}
								GUILayout.EndHorizontal();
								GUILayout.HorizontalSlider( dash, 0f, maxDash, GUILayout.ExpandWidth( true ) );
							}
							GUILayout.EndVertical();
						}

						GUILayout.FlexibleSpace();
					}
					GUILayout.EndHorizontal();
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndArea();
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
			case DroneInfo.Type.ShotgunStorm:
				droneIcon = DroneIconTextures.Find( item => item.name == "Shotgun Storm Overlay" );
				break;
			case DroneInfo.Type.SlagCannon:
				droneIcon = DroneIconTextures.Find( item => item.name == "Slag Cannon Overlay" );
				break;
			default:
				break;
		}
		return droneIcon;
	}

	public void DroneSpawned( Drone drone )
	{
		Vector2 droneStartPos = screenRect.center + new Vector2( 0f, GetComponent<Camera>().pixelRect.height * .5f );
		droneIcons.Add( new DroneIcon( droneStartPos, iconSize, GetDroneIcon( drone ), drone, IconLerpFactor ) );
	}

	public void DroneDespawned( Drone drone )
	{
		DroneIcon icon = droneIcons.Find( ( DroneIcon d ) => d.Drone == drone );
		droneIcons.Remove( icon );
	}
}
