using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using GUIControls;

public class MenuGUI : MonoBehaviour
{
	// Enums
	enum MenuState
	{
		Root = 0,
		GameMenu,
		Options,
		Exit,
		StartGame
	}

	// Fields
	MenuState _state;

	// Properties
	MenuState State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
			switch( _state )
			{
				case MenuState.Root:
					targetScrollPosition = new Vector2( GetComponent<Camera>().pixelWidth, 0 );
					break;
				case MenuState.GameMenu:
					targetScrollPosition = new Vector2( GetComponent<Camera>().pixelWidth * 2f, 0 );
					break;
				case MenuState.Options:
					targetScrollPosition = new Vector2( 0, 0 );
					break;
				case MenuState.Exit:
					Application.Quit();
					break;
				case MenuState.StartGame:
					BeginGame();
					break;
				default:
					break;
			}

			menuIndex = 0;
		}
	}

	// Variables
	// Game
	PrimitiveRef<float> playerCount = new PrimitiveRef<float>();
	PrimitiveRef<float> maxPickups = new PrimitiveRef<float>();
	PrimitiveRef<float> pickupRespawnDelay = new PrimitiveRef<float>();
	PrimitiveRef<float> scoreLimit = new PrimitiveRef<float>();

	// Arena
	PrimitiveRef<float> terrainSize = new PrimitiveRef<float>();
	PrimitiveRef<float> terrainHeightFactor = new PrimitiveRef<float>();
	PrimitiveRef<float> terrainDensity = new PrimitiveRef<float>();
	PrimitiveRef<float> terrainTurbulence = new PrimitiveRef<float>();
	PrimitiveRef<int> terrainType = new PrimitiveRef<int>();
	List<string> terrainTypeStrings = new List<string>
	{
		"Desert",
		"Plains",
		"Blasted Canyon",
		"Asteroid",
		"Pyramid",
		"Glacier",
		"Tech World",
		"Crystal",
		"Distant World",
		"Frozen Sea",
		"Mountain Range",
		"Martian Expanse"
	};
	PrimitiveRef<float> arenaSides = new PrimitiveRef<float>();
	PrimitiveRef<bool> useRandomSeed = new PrimitiveRef<bool>();
	PrimitiveRef<string> randomSeed = new PrimitiveRef<string>();

	// Menu
	Vector2 scrollPosition;
	Vector2 targetScrollPosition;
	List<MenuItem> rootMenuItems = new List<MenuItem>();
	List<MenuItem> optionsMenuItems = new List<MenuItem>();
	List<MenuItem> gameMenuItems = new List<MenuItem>();
	int menuIndex = 0;
	bool prevUp = false;
	bool prevDown = false;
	bool prevSideStick = false;
	bool prevSidePad = false;
	float stickRefireTimer = 0f;
	float stickRefireTimeout = 0f;

	InputWrapper inputWrapper;

	public float OptimumSize = 1080f;
	public GUISkin Skin;
	public float ScrollLerpFactor = .015f;
	public float StickRefireStartTimeout = 0.5f;
	public float StickRefireMinTimeout = 0.04f;
	public float StickRefireAccelleration = 1f;
	public int RootMenuBoxPadding = 15;
	public GameParameters GameParameters;
	
	// Unity Methods
	void Awake()
	{
		targetScrollPosition = new Vector2( GetComponent<Camera>().pixelWidth, 0 );
		scrollPosition = targetScrollPosition;
		Time.timeScale = 1f;
		inputWrapper = GetComponent<InputWrapper>();

		// Setup Variables
		playerCount.Set( 1 );
		maxPickups.Set( 15 );
		pickupRespawnDelay.Set( 2 );
		scoreLimit.Set( 500 );
		terrainSize.Set( 100 );
		terrainHeightFactor.Set( 1 );
		terrainDensity.Set( 5 );
		terrainTurbulence.Set( 10 );
		terrainType.Set( 0 );
		arenaSides.Set( 4 );
		useRandomSeed.Set( false );
		randomSeed.Set( "123" );

		// Setup Menus
		rootMenuItems.Add( new MenuButton( "Start Game", GameMenuButtonPressed ) );
		rootMenuItems.Add( new MenuButton( "Options", OptionsMenuButtonPressed ) );
		rootMenuItems.Add( new MenuButton( "Exit Game", ExitGameButtonPressed ) );

		optionsMenuItems.Add( new MenuButton( "Nothing to see here", null ) );
		optionsMenuItems.Add( new MenuButton( "Back", () => { State = MenuState.Root; } ) );

		float barWidth = GetComponent<Camera>().pixelWidth * .5f;
		gameMenuItems.Add( new MenuSlider(
			"Players", playerCount, 1f, 4f, barWidth, "f0",
			null,
			( int s ) => { playerCount.Set( Mathf.Clamp( playerCount.Get() + s, 1f, 4f ) ); }
		) );
		gameMenuItems.Add( new MenuSlider(
			"Max Pickups", maxPickups, 0f, 30f, barWidth, "f0",
			null,
			( int s ) => { maxPickups.Set( Mathf.Clamp( maxPickups.Get() + s, 0f, 30f ) ); }
		) );
		gameMenuItems.Add( new MenuSlider(
			"Pickup Respawn Delay", pickupRespawnDelay, 1f, 10f, barWidth, "f1",
			null,
			( int s ) => { pickupRespawnDelay.Set( Mathf.Clamp( pickupRespawnDelay.Get() + s * .1f, 1f, 10f ) ); }
		) );
		gameMenuItems.Add( new MenuSlider(
			"Score Limit", scoreLimit, 100f, 5000f, barWidth, "f0",
			null,
			( int s ) => { scoreLimit.Set( Mathf.Clamp( scoreLimit.Get() + s * 100f, 100f, 5000f ) ); }
		) );
		gameMenuItems.Add( new MenuSlider(
			"Terrain Size", terrainSize, 50f, 250f, barWidth, "f1",
			null,
			( int s ) => { terrainSize.Set( Mathf.Clamp( terrainSize.Get() + s * 10f, 50f, 250f ) ); }
		) );
		gameMenuItems.Add( new MenuSlider(
			"Terrain Height", terrainHeightFactor, .5f, 1.5f, barWidth, "f1",
			null,
			( int s ) => { terrainHeightFactor.Set( Mathf.Clamp( terrainHeightFactor.Get() + s * .1f, .5f, 1.5f ) ); }
		) );
		gameMenuItems.Add( new MenuSlider(
			"Terrain Density", terrainDensity, 1f, 8f, barWidth, "f0",
			null,
			( int s ) => { terrainDensity.Set( Mathf.Clamp( terrainDensity.Get() + s, 1f, 8f ) ); }
		) );
		gameMenuItems.Add( new MenuSlider(
			"Terrain Turbulence", terrainTurbulence, 2f, 15f, barWidth, "f0",
			null,
			( int s ) => { terrainTurbulence.Set( Mathf.Clamp( terrainTurbulence.Get() + s, 2f, 15f ) ); }
		) );
		gameMenuItems.Add( new MenuIndexedString(
			"Map Variant", terrainType, terrainTypeStrings, barWidth,
			null,
			( int s ) => { terrainType.Set( Mathf.Clamp( terrainType.Get() + s, 0, terrainTypeStrings.Count - 1 ) ); }
		) );
		gameMenuItems.Add( new MenuSlider(
			"Arena Sides", arenaSides, 4f, 32f, barWidth, "f0",
			null,
			( int s ) => { arenaSides.Set( Mathf.Clamp( arenaSides.Get() + s, 4f, 32f ) ); }
		) );
		gameMenuItems.Add( new MenuCheckbox( "Use Random Seed", useRandomSeed, () => { useRandomSeed.Set( !useRandomSeed.Get() ); } ) );
		gameMenuItems.Add( new MenuTextField(
			"Random Seed", randomSeed, barWidth,
			null,
			( int s ) => {
				int seed;
				int.TryParse( randomSeed.Get(), out seed );
				seed = Mathf.Max( seed + s, 0 );
				randomSeed.Set( seed.ToString() );
			}
		) );
		gameMenuItems.Add( new MenuButton( "Start Game", BeginGame ) );
		gameMenuItems.Add( new MenuButton( "Back", () => { State = MenuState.Root; } ) );

		State = MenuState.Root;
	}
	
	void Start()
	{
		inputWrapper.Init();
	}

	void Update()
	{
		// Scroll toward target position
		scrollPosition = Vector2.Lerp( scrollPosition, targetScrollPosition, ScrollLerpFactor );

		// Left Stick Refire
		if( inputWrapper.LeftStick.Value.magnitude > .25f )
		{
			stickRefireTimer -= Time.deltaTime;
			stickRefireTimeout -= StickRefireAccelleration * Time.deltaTime;
			stickRefireTimeout = Mathf.Max( stickRefireTimeout, StickRefireMinTimeout );
		}
		else
		{
			stickRefireTimeout = StickRefireStartTimeout;
			stickRefireTimer = StickRefireStartTimeout;
		}

		// Get current menu item count
		int menuItemCount = 0;
		switch( State )
		{
			case MenuState.Root:
				menuItemCount = rootMenuItems.Count;
				break;
			case MenuState.GameMenu:
				menuItemCount = gameMenuItems.Count;
				break;
			case MenuState.Options:
				menuItemCount = optionsMenuItems.Count;
				break;
			default:
				break;
		}


		// Up
		bool up = ( inputWrapper.LeftStick.Value.y < -.25f && Mathf.Abs( inputWrapper.LeftStick.Value.x ) < .5f )
			   || ( inputWrapper.DPad.Value.y < 0 );
		if(  up && !prevUp )
		{
			menuIndex++;
			if( menuIndex > menuItemCount - 1 )
			{
				menuIndex = 0;
			}
		}
		if( stickRefireTimer > 0f )
		{
			prevUp = up;
		}
		else
		{
			prevUp = false;
		}

		// Down
		bool down = inputWrapper.LeftStick.Value.y > .25f && Mathf.Abs( inputWrapper.LeftStick.Value.x ) < .5f
			   || ( inputWrapper.DPad.Value.y > 0 );
		if( down && !prevDown )
		{
			menuIndex--;
			if( menuIndex < 0 )
			{
				menuIndex = menuItemCount - 1;
			}
		}
		if( stickRefireTimer > 0f )
		{
			prevDown = down;
		}
		else
		{
			prevDown = false;
		}

		// Side (Stick)
		bool sideStick = ( inputWrapper.LeftStick.Value.x < -.25f || inputWrapper.LeftStick.Value.x > .25f ) && Mathf.Abs( inputWrapper.LeftStick.Value.y ) < .5f;
		if( sideStick )
		{
			if( !prevSideStick )
			{
				if( State == MenuState.GameMenu )
				{
					if( gameMenuItems[ menuIndex ].ValueChangeCallback != null )
					{
						gameMenuItems[ menuIndex ].ValueChangeCallback( (int)Mathf.Sign( inputWrapper.LeftStick.Value.x ) );
					}
				}
			}
		}
		if( stickRefireTimer > 0f )
		{
			prevSideStick = sideStick;
		}
		else
		{
			prevSideStick = false;
		}

		// Side (Pad)
		bool sidePad = ( inputWrapper.DPad.Value.x != 0f );
		if( sidePad )
		{
			if( !prevSidePad )
			{
				if( State == MenuState.GameMenu )
				{
					if( gameMenuItems[ menuIndex ].ValueChangeCallback != null )
					{
						gameMenuItems[ menuIndex ].ValueChangeCallback( (int)Mathf.Sign( inputWrapper.DPad.Value.x ) );
					}
				}
			}
		}
		prevSidePad = sidePad;

		// Reset refire timer
		if( stickRefireTimer <= 0f )
		{
			stickRefireTimer = stickRefireTimeout;
		}

		// Select
		if( inputWrapper.Jump.Pressed )
		{
			switch( State )
			{
				case MenuState.Root:
					if( rootMenuItems[ menuIndex ].ActivateCallback != null )
					{
						rootMenuItems[ menuIndex ].ActivateCallback();
					}
					break;
				case MenuState.GameMenu:
					if( gameMenuItems[ menuIndex ].ActivateCallback != null )
					{
						gameMenuItems[ menuIndex ].ActivateCallback();
					}
					break;
				case MenuState.Options:
					if( optionsMenuItems[ menuIndex ].ActivateCallback != null )
					{
						optionsMenuItems[ menuIndex ].ActivateCallback();
					}
					break;
				default:
					break;
			}
		}

		// Back
		if( inputWrapper.Drop.Pressed )
		{
			State = MenuState.Root;
		}
	}

	void OnGUI()
	{
		GUI.skin = Skin;

		//float sf = Mathf.Min( camera.pixelWidth, camera.pixelHeight ) / OptimumSize;

		// "Page" wrapper
		GUILayout.BeginArea( new Rect( 0, 0, GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight ) );
		{
			// Header area
			GUILayout.BeginVertical();
			{
				GUILayout.FlexibleSpace();
				// Main Title
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label( "UnityCycles", Skin.GetStyle( "titletext" ) );
				}
				GUILayout.EndHorizontal();
				// Subtitle
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label( "Alpha", Skin.GetStyle( "subtext" ) );
				}
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndVertical();

			// Content wrapper
			GUILayout.BeginScrollView( scrollPosition, GUILayout.ExpandHeight( true ) );
			{
				GUILayout.BeginHorizontal( GUILayout.ExpandHeight( true ) );
				{
					// Left ( Options Menu )
					GUILayout.BeginVertical( Skin.GetStyle( "contentsubbox" ), GUILayout.Width( GetComponent<Camera>().pixelWidth ), GUILayout.ExpandHeight( true ) );
					{
						GUILayout.FlexibleSpace();

						// Control container
						GUILayout.BeginHorizontal( Skin.GetStyle( "contentsubbox" ) );
						{
							GUILayout.FlexibleSpace();

							GUILayout.BeginVertical( Skin.GetStyle( "Box" ), GUILayout.ExpandHeight( true ) );
							{
								for( int i = 0; i < optionsMenuItems.Count; ++i )
								{
									optionsMenuItems[ i ].DrawGUI( MenuIndexStyle( i ) );
								}
							}
							GUILayout.EndVertical();

							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();

						GUILayout.FlexibleSpace();
					}
					GUILayout.EndVertical();

					// Center ( Root Menu )
					GUILayout.BeginVertical( Skin.GetStyle( "contentsubbox" ), GUILayout.Width( GetComponent<Camera>().pixelWidth ), GUILayout.ExpandHeight( true ) );
					{
						GUILayout.FlexibleSpace();

						// Control container
						GUILayout.BeginHorizontal( Skin.GetStyle( "contentsubbox" ) );
						{
							GUILayout.FlexibleSpace();

							GUILayout.BeginVertical( Skin.GetStyle( "Box" ), GUILayout.ExpandHeight( true ) );
							{
								for( int i = 0; i < rootMenuItems.Count; ++i )
								{
									rootMenuItems[ i ].DrawGUI( MenuIndexStyle( i ) );
								}
							}
							GUILayout.EndVertical();

							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();

						GUILayout.FlexibleSpace();
					}
					GUILayout.EndVertical();

					// Right ( Game Menu )
					GUILayout.BeginVertical( Skin.GetStyle( "contentsubbox" ), GUILayout.Width( GetComponent<Camera>().pixelWidth ), GUILayout.ExpandHeight( true ) );
					{
						GUILayout.FlexibleSpace();

						// Control container
						GUILayout.BeginVertical( Skin.GetStyle( "Box" ), GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
						{
							for( int i = 0; i < gameMenuItems.Count; ++i )
							{
								gameMenuItems[ i ].DrawGUI( MenuIndexStyle( i ) );
							}
						}
						GUILayout.EndVertical();

						GUILayout.FlexibleSpace();
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}
		GUILayout.EndArea();

		// Clamp values appropriately
		playerCount.Set( (int)playerCount.Get() );
		maxPickups.Set( (int)maxPickups.Get() );
		scoreLimit.Set( (int)scoreLimit.Get() );
		arenaSides.Set( (int)arenaSides.Get() );
	}

	// Utility Methods
	GUIStyle MenuIndexStyle( int index )
	{
		if( menuIndex == index )
		{
			return Skin.GetStyle( "MenuItemHighlight" );
		}
		else
		{
			return Skin.GetStyle( "MenuItem" );
		}
	}

	// Activation Callbacks
	void GameMenuButtonPressed()
	{
		State = MenuState.GameMenu;
	}

	void OptionsMenuButtonPressed()
	{
		State = MenuState.Options;
	}

	void ExitGameButtonPressed()
	{
		State = MenuState.Exit;
	}

	void BeginGame()
	{
		// Game
		GameParameters.PlayerCount = (int)playerCount.Get();
		GameParameters.MaxPickups = (int)maxPickups.Get();
		GameParameters.PickupRespawnDelay = pickupRespawnDelay.Get();
		GameParameters.ScoreLimit = (int)scoreLimit.Get();

		// Arena
		GameParameters.TerrainSize = terrainSize.Get();
		GameParameters.TerrainHeight = terrainSize.Get() * terrainHeightFactor.Get() * .1f;
		GameParameters.TerrainNoiseSubdivisions = (int)terrainDensity.Get();
		GameParameters.TerrainTurbulence = (int)terrainTurbulence.Get();
		GameParameters.TerrainType = (WorleyNoiseTerrain.DistMetric)( terrainType.Get() / 2 );
		GameParameters.AltTerrain = terrainType.Get() % 2 == 1 ? true : false;

		GameParameters.ArenaSides = (int)arenaSides.Get();

		GameParameters.UseRandomSeed = useRandomSeed.Get();

		int rs;
		if( int.TryParse( randomSeed.Get(), out rs ) )
		{
			GameParameters.RandomSeed = rs;
		}

		Application.LoadLevel( "Game" );
	}

	// Value Change Callbacks
	void ChangePlayerCount( float s )
	{
		if( s > 0 )
		{
			playerCount.Set( playerCount.Get() + Mathf.Ceil( s ) );
		}
		else
		{
			playerCount.Set( playerCount.Get() + Mathf.Floor( s ) );
		}

		playerCount.Set( Mathf.Clamp( playerCount.Get(), 1f, 4f ) );
	}

	void ChangeMaxPickups( float s )
	{
		if( s > 0 )
		{
			maxPickups.Set( maxPickups.Get() + Mathf.Ceil( s ) );
		}
		else
		{
			maxPickups.Set( maxPickups.Get() + Mathf.Floor( s ) );
		}

		maxPickups.Set( Mathf.Clamp( maxPickups.Get(), 0f, 30f ) );
	}
}
