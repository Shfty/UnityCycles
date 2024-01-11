using UnityEngine;
using System.Collections;

public class OverlayGUI : MonoBehaviour
{
	// Variables
	float independentDeltaTime;
	float prevTimeSinceStart;
	string winnerText = "";
	string rematchText = "Aim to restart match";
	string backToMenuText = "Fire to return to Menu";
	bool prevRematch = false;
	bool prevMenu = false;
	/* Fade states
	 * 0 - Fully transparent
	 * 1 - Partial transparency following game over
	 * 2 - Fully opaque
	 * 3 - Waiting on reset
	 */
	int fadeState = 0;
	int fadeDestination = -1;
	public float overlayOpacity = 0f;

	delegate void FinishCallback();

	public GUISkin Skin;
	public Texture OverlayTexture;
	public float OverlayFadeTime = 2f;

	// Unity Methods
	void Update()
	{
		// Track delta time independently of Time class
		independentDeltaTime = Time.realtimeSinceStartup - prevTimeSinceStart;

		if( GameControl.Instance.Players.Count > 0 )
		{
			bool rematch = false;
			foreach( GameObject player in GameControl.Instance.Players )
			{
				InputWrapper iw = player.GetComponent<InputWrapper>();
				rematch |= iw.Rematch;
			}

			bool menu = false;
			foreach( GameObject player in GameControl.Instance.Players )
			{
				InputWrapper iw = player.GetComponent<InputWrapper>();
				menu |= iw.BackToMenu;
			}

			if( fadeState == 1 && animationFinished && rematch && !prevRematch )
			{
				fadeDestination = 0;
				fadeState = 2;
				SetOverlayAnimationParameters( overlayOpacity, 1f, OverlayFadeTime );
				StartOverlayAnimation();
			}
			prevRematch = rematch;

			if( fadeState == 1 && animationFinished && menu && !prevMenu )
			{
				fadeDestination = 1;
				fadeState = 2;
				SetOverlayAnimationParameters( overlayOpacity, 1f, OverlayFadeTime );
				StartOverlayAnimation();
			}
			prevMenu = menu;
		}

		if( fadeState == 2 && animationFinished )
		{
			if( fadeDestination == 0 )
			{
				GameControl.Instance.ResetGame();
				fadeState = 3;
				SetOverlayAnimationParameters( overlayOpacity, 0f, OverlayFadeTime );
				StartCoroutine( PauseAndStartAnimation( 2f ) );
			}
			else if( fadeDestination == 1 )
			{
				StartCoroutine( PauseAndReturnToMenu( 1f ) );
			}
			fadeDestination = -1;
		}

		if( fadeState == 3 && animationFinished )
		{
			fadeState = 0;
			StartCoroutine( PauseAndStartGame( 2f ) );
		}

		StartCoroutine( AnimateOverlay( OverlayAnimationFinished ) );

		// Track previous time
		prevTimeSinceStart = Time.realtimeSinceStartup;
	}
	
	void OnGUI()
	{
		GUI.skin = Skin;
		GUI.skin.label.wordWrap = false;
		GUI.color = new Color( 1f, 1f, 1f, overlayOpacity );
		GUI.DrawTexture( GetComponent<Camera>().pixelRect, OverlayTexture );

		GUI.color = new Color( 1f, 1f, 1f, overlayOpacity );

		GUILayout.BeginArea( GetComponent<Camera>().pixelRect );
		{
			GUILayout.BeginVertical( GUILayout.ExpandWidth( true ) );
			{
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label( winnerText );
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();

				GUILayout.FlexibleSpace();
				GUILayout.FlexibleSpace();

				GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
				{
					GUILayout.FlexibleSpace();
					GUILayout.BeginVertical();
					{
						GUILayout.Label( rematchText, GUILayout.ExpandWidth( true ) );
						GUILayout.Label( backToMenuText, GUILayout.ExpandWidth( true ) );
					}
					GUILayout.EndVertical();
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndArea();
	}

	// Utility Methods
	public void GameOver( GameObject winner )
	{
		winnerText = "Player " + ( GameControl.Instance.Players.IndexOf( winner ) + 1 ) + " Wins";
		fadeState = 1;
		SetOverlayAnimationParameters( 0f, .8f, OverlayFadeTime );
		StartCoroutine( PauseAndStartAnimation( 2f ) );
	}

	float from;
	float to;
	float duration;
	float dist;
	void SetOverlayAnimationParameters( float f, float t, float d )
	{
		from = f;
		to = t;
		duration = d;
		dist = to - from;
	}

	bool animating = false;
	bool animationFinished = false;
	void StartOverlayAnimation()
	{
		animating = true;
		animationFinished = false;
	}

	void StopOverlayAnimation()
	{
		animating = false;
		animationFinished = true;
	}

	void OverlayAnimationFinished()
	{
		animating = false;
		animationFinished = true;
	}

	IEnumerator AnimateOverlay( FinishCallback callback )
	{
		if( animating )
		{
			if( dist > 0 )
			{
				for( float i = from; i < to; i += ( dist / duration ) * independentDeltaTime )
				{
					overlayOpacity = i;
					yield return null;
				}
			}
			else
			{
				for( float i = from; i > to; i += ( dist / duration ) * independentDeltaTime )
				{
					overlayOpacity = i;
					yield return null;
				}
			}
			callback();
		}

		yield break;
	}

	IEnumerator PauseAndStartAnimation( float duration )
	{
		float startTime = Time.realtimeSinceStartup;
		while( Time.realtimeSinceStartup < startTime + duration )
		{
			yield return null;
		}
		StartOverlayAnimation();
	}

	IEnumerator PauseAndStartGame( float duration )
	{
		float startTime = Time.realtimeSinceStartup;
		while( Time.realtimeSinceStartup < startTime + duration )
		{
			yield return null;
		}
		GameControl.Instance.StartGame();
	}

	IEnumerator PauseAndReturnToMenu( float duration )
	{
		float startTime = Time.realtimeSinceStartup;
		while( Time.realtimeSinceStartup < startTime + duration )
		{
			yield return null;
		}
		Application.LoadLevel( "Menu" );
	}
}
