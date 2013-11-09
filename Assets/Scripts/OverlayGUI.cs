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
	float prevAim = 0f;
	/* Fade states
	 * 0 - Fully transparent
	 * 1 - Partial transparency following game over
	 * 2 - Fully opaque
	 * 3 - Waiting on reset
	 */
	int fadeState = 0;
	public float overlayOpacity = 0f;

	delegate void FinishCallback();

	public GUISkin Skin;
	public Texture OverlayTexture;
	public float OverlayFadeTime = 2f;

	// Unity Methods
	void Start()
	{
		overlayOpacity = 1f;
		SetOverlayAnimationParameters( 1f, 0f, OverlayFadeTime );
		StartCoroutine( PauseAndStartAnimation( 1f ) );
	}

	void Update()
	{
		// Track delta time independently of Time class
		independentDeltaTime = Time.realtimeSinceStartup - prevTimeSinceStart;

		if( GameControl.Instance.Players.Count > 0 )
		{
			InputWrapper inputWrapper = GameControl.Instance.Players[ 0 ].GetComponent<InputWrapper>();
			if( fadeState == 1 && animationFinished && inputWrapper.Aim > 0f && prevAim == 0f )
			{
				fadeState = 2;
				SetOverlayAnimationParameters( overlayOpacity, 1f, OverlayFadeTime );
				StartOverlayAnimation();
			}
			prevAim = inputWrapper.Aim;
		}

		if( fadeState == 2 && animationFinished )
		{
			GameControl.Instance.ResetGame();
			fadeState = 3;
			SetOverlayAnimationParameters( overlayOpacity, 0f, OverlayFadeTime );
			StartCoroutine( PauseAndStartAnimation( 1f ) );
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
		GUI.color = new Color( 1f, 1f, 1f, overlayOpacity );
		GUI.DrawTexture( camera.pixelRect, OverlayTexture );

		GUI.color = new Color( 1f, 1f, 1f, overlayOpacity );

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
}
