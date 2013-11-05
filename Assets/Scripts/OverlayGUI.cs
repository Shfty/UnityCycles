using UnityEngine;
using System.Collections;

public class OverlayGUI : MonoBehaviour
{
	// Variables
	float prevTimeSinceStart;
	/* Fade states
	 * 0 - Fully transparent
	 * 1 - Partial transparency following game over
	 * 2 - Fully opaque
	 * 3 - Waiting on reset
	 */
	int fadeState = 0;
	string winnerText = "";
	string rematchText = "Aim to restart match";
	string backToMenuText = "Fire to return to Menu";
	float prevAim = 0f;

	public GUISkin Skin;
	public Texture OverlayTexture;

	public float OverlayOpacity = 1f;
	public float TargetOverlayOpacity = 0f;
	public float OverlayLerpFactor = .1f;

	public float TextOpacity = 0f;
	public float TargetTextOpacity = 0f;
	public float TextLerpFactor = .1f;

	// Unity Methods
	void Update()
	{
		// Track delta time independently of Time class
		float independentDeltaTime = Time.realtimeSinceStartup - prevTimeSinceStart;

		float targetOverlayAlpha = 0f;
		float targetTextAlpha = 0f;
		switch( fadeState )
		{
			case 0:
				targetOverlayAlpha = 0f;
				targetTextAlpha = 0f;
				break;
			case 1:
				targetOverlayAlpha = TargetOverlayOpacity;
				targetTextAlpha = TargetTextOpacity;
				break;
			case 2:
			case 3:
				targetOverlayAlpha = 1f;
				targetTextAlpha = TargetTextOpacity;
				break;
			default:
				break;
		}

		OverlayOpacity = Mathf.Lerp( OverlayOpacity, targetOverlayAlpha, OverlayLerpFactor * independentDeltaTime );
		TextOpacity = Mathf.Lerp( TextOpacity, targetTextAlpha, OverlayLerpFactor * independentDeltaTime );

		if( GameControl.Instance.Players.Count > 0 )
		{
			InputWrapper inputWrapper = GameControl.Instance.Players[ 0 ].GetComponent<InputWrapper>();
			if( fadeState == 1 && inputWrapper.Aim == 1f && prevAim == 0f )
			{
				fadeState = 2;
			}
			prevAim = inputWrapper.Aim;
		}

		if( fadeState == 2 && OverlayOpacity >= targetOverlayAlpha - .005f )
		{
			GameControl.Instance.ResetGame();
			GameControl.Instance.StartGame();
			fadeState = 0;
			TargetTextOpacity = 0f;
		}

		// Track previous time
		prevTimeSinceStart = Time.realtimeSinceStartup;
	}
	
	void OnGUI()
	{
		GUI.skin = Skin;
		GUI.color = new Color( 1f, 1f, 1f, OverlayOpacity );
		GUI.DrawTexture( camera.pixelRect, OverlayTexture );

		GUI.color = new Color( 1f, 1f, 1f, TextOpacity );

		GUIContent winnerContent = new GUIContent( winnerText );
		Vector2 winnerBounds = Skin.label.CalcSize( winnerContent );
		Rect winnerRect = new Rect( camera.pixelWidth * .5f - winnerBounds.x * .5f, 0 + camera.pixelHeight * .1f, winnerBounds.x, winnerBounds.y );
		GUI.Label( winnerRect, winnerContent );

		GUIContent backToMenuContent = new GUIContent( backToMenuText );
		Vector2 backToMenuBounds = Skin.label.CalcSize( backToMenuContent );
		Rect backToMenuRect = new Rect( camera.pixelWidth * .5f - backToMenuBounds.x * .5f, camera.pixelHeight - camera.pixelHeight * .1f, backToMenuBounds.x, backToMenuBounds.y );
		GUI.Label( backToMenuRect, backToMenuContent );

		GUIContent rematchContent = new GUIContent( rematchText );
		Vector2 rematchBounds = Skin.label.CalcSize( rematchContent );
		Rect rematchRect = new Rect( camera.pixelWidth * .5f - rematchBounds.x * .5f, camera.pixelHeight - camera.pixelHeight * .1f - backToMenuRect.height, rematchBounds.x, rematchBounds.y );
		GUI.Label( rematchRect, rematchContent );
	}

	// Utility Methods
	public void GameOver( GameObject winner )
	{
		winnerText = "Player " + ( GameControl.Instance.Players.IndexOf( winner ) + 1 ) + " Wins";
		fadeState = 1;
	}
}
