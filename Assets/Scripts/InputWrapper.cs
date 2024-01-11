using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

public class InputWrapper : MonoBehaviour
{
	// Button Class
	public abstract class Button
	{
		// Fields
		protected bool _down = false;
		bool _pressed = false;

		// Properties
		public bool Down
		{
			get { return _down; }
		}
		public bool Pressed
		{
			get { return _pressed; }
		}

		// Variables
		string axisName;
		Func<bool> xboxCallback;
		bool prevDown = false;

		public abstract void FixedUpdate();

		public void Update()
		{
			_pressed = _down && !prevDown;
			prevDown = _down;
		}
	}

	public class UnityButton : Button
	{
		string axisName;

		public UnityButton( string an )
		{
			axisName = an;
		}

		public override void FixedUpdate()
		{
			_down = Input.GetAxisRaw( axisName ) > 0f;
		}
	}

	public class CustomButton : Button
	{
		Func<bool> callback;

		public CustomButton( Func<bool> cb )
		{
			callback = cb;
		}

		public override void FixedUpdate()
		{
			_down = callback();
		}
	}

	public abstract class Axis2D
	{
		// Fields
		protected Vector2 _value;
		bool _moved = false;
		bool _pushed = false;
		Vector2 _moveDelta;

		// Properties
		public Vector2 Value
		{
			get { return _value; }
		}
		public bool Moved
		{
			get { return _moved; }
		}
		public bool Pushed
		{
			get { return _pushed; }
		}
		public Vector2 MoveDelta
		{
			get { return _moveDelta; }
		}

		// Variables
		string xAxisName;
		string yAxisName;
		Func<Vector2> xboxCallback;
		Vector2 prevValue;

		public abstract void FixedUpdate();

		public void Update()
		{
			if( _value != prevValue )
			{
				_moveDelta = _value - prevValue;
				_moved = true;
				if( prevValue == Vector2.zero )
				{
					_pushed = true;
				}
				else
				{
					_pushed = false;
				}
			}
			else
			{
				_moveDelta = Vector2.zero;
				_moved = false;
				_pushed = false;
			}
			prevValue = _value;
		}
	}

	public class UnityAxis2D : Axis2D
	{
		string xAxisName;
		string yAxisName;
		float sensitivity;

		public UnityAxis2D( string xAn, string yAn, float s = 1f )
		{
			xAxisName = xAn;
			yAxisName = yAn;
			sensitivity = s;
		}

		public override void FixedUpdate()
		{
			_value = new Vector2( Input.GetAxisRaw( xAxisName ) * sensitivity, Input.GetAxisRaw( yAxisName ) * sensitivity );
		}
	}

	public class CustomAxis2D : Axis2D
	{
		Func<Vector2> callback;
		float sensitivity;

		public CustomAxis2D( Func<Vector2> cb, float s = 1f )
		{
			callback = cb;
			sensitivity = s;
		}

		public override void FixedUpdate()
		{
			_value = callback() * sensitivity;
		}
	}

	// Enums
	public enum InputType
	{
		None,
		UnityPad,
		XboxPad
	}

	// Variables
	GamePadState xboxPadState;
	InputType type = InputType.None;

	PlayerIndex xboxPadIndex;
	string typeSuffix;
	bool gameActive = true;
	List<Axis2D> axes2D = new List<Axis2D>();
	List<Button> buttons = new List<Button>();
	
	public int LocalPlayerIndex;
	public Axis2D LeftStick;
	public Axis2D RightStick;
	public Axis2D DPad;

	public Button Jump;
	public Button Drop;
	public Button Aim;
	public Button Fire;
	public Button SwitchLeft;
	public Button SwitchRight;

	// Left = Strong Motor, Right = Weak Motor
	public float StrongRumbleBaseForce = 0f;
	public float StrongRumbleForce = 0f;
	public float StrongRumbleDecayRate = 1.0f;
	public float WeakRumbleBaseForce = 0f;
	public float WeakRumbleForce = 0f;
	public float WeakRumbleDecayRate = 1.0f;

	public float Dash;
	public Vector2 DashVector;

	public bool Rematch;
	public bool BackToMenu;

	// Startup function to be called manually by instantiator
	public void Init()
	{
		// Get the joystick's devicename strings
		string[] joystickNames = Input.GetJoystickNames();

		// If there are enough joysticks
		if( joystickNames.Length > LocalPlayerIndex )
		{
			// Determine if we should use XInput or Unity Input
			switch( Input.GetJoystickNames()[ LocalPlayerIndex ] )
			{
				case "Controller (XBOX 360 For Windows)":
					// Only works under Windows
					if( Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor )
					{
						// Set the appropriate type and instantiate the XInput pad index
						type = InputType.XboxPad;
						xboxPadIndex = (PlayerIndex)LocalPlayerIndex;
						GamePad.SetVibration( xboxPadIndex, 0f, 0f );
					}
					else
					{
						goto default;
					}
					break;
				default:
					// Set the appropriate type and create an axis name suffix for this joystick index
					type = InputType.UnityPad;
					typeSuffix = " (Joy " + ( LocalPlayerIndex + 1 ) + ")";
					break;
			}
		}
		else if( joystickNames.Length == LocalPlayerIndex )
		{
			// If there are no joysticks left, use the keyboard
			type = InputType.UnityPad;
			typeSuffix = " (Keyboard)";
		}
		else
		{
			// If there are no joysticks left and the keyboard is taken, log a warning message
			Debug.LogWarning( "InputWrapper " + LocalPlayerIndex + " unable to bind joystick" );
		}

		if( type == InputType.UnityPad )
		{
			// Setup stick objects
			LeftStick = new UnityAxis2D( "Move Horizontal" + typeSuffix, "Move Vertical" + typeSuffix );
			axes2D.Add( LeftStick );
			RightStick = new UnityAxis2D( "Camera Horizontal" + typeSuffix, "Camera Vertical" + typeSuffix, typeSuffix == " (Keyboard)" ? GameInfo.Properties.MouseAimSensitivity : GameInfo.Properties.GamePadAimSensitivity );
			axes2D.Add( RightStick );
			DPad = new UnityAxis2D( "D-Pad X" + typeSuffix, "D-Pad Y" + typeSuffix );
			axes2D.Add( DPad );

			// Setup button objects
			Jump = new UnityButton( "Jump" + typeSuffix );
			buttons.Add( Jump );
			Drop = new UnityButton( "Drop" + typeSuffix );
			buttons.Add( Drop );
			Aim = new UnityButton( "Aim" + typeSuffix );
			buttons.Add( Aim );
			Fire = new UnityButton( "Fire" + typeSuffix );
			buttons.Add( Fire );
			SwitchLeft = new CustomButton( () => Input.GetAxisRaw( "Switch Left" + typeSuffix ) > 0f || Input.GetAxisRaw( "Switch (Mouse)" ) < 0f );
			buttons.Add( SwitchLeft );
			SwitchRight = new CustomButton( () => Input.GetAxisRaw( "Switch Right" + typeSuffix ) > 0f || Input.GetAxisRaw( "Switch (Mouse)" ) > 0f );
			buttons.Add( SwitchRight );
		}
		else if( type == InputType.XboxPad )
		{
			// Setup stick objects
			LeftStick = new CustomAxis2D( () => new Vector2( xboxPadState.ThumbSticks.Left.X, xboxPadState.ThumbSticks.Left.Y ) );
			axes2D.Add( LeftStick );
			RightStick = new CustomAxis2D( () => new Vector2( xboxPadState.ThumbSticks.Right.X, xboxPadState.ThumbSticks.Right.Y ), GameInfo.Properties.GamePadAimSensitivity );
			axes2D.Add( RightStick );
			DPad = new CustomAxis2D(
				() => new Vector2(
					( xboxPadState.DPad.Left == ButtonState.Pressed ? -1f : 0f )
				  + ( xboxPadState.DPad.Right == ButtonState.Pressed ? 1f : 0f ),
					( xboxPadState.DPad.Up == ButtonState.Pressed ? 1f : 0f )
				  + ( xboxPadState.DPad.Down == ButtonState.Pressed ? -1f : 0f )
				)
			);
			axes2D.Add( DPad );

			// Setup button objects
			Jump = new CustomButton( () => xboxPadState.Buttons.LeftShoulder == ButtonState.Pressed );
			buttons.Add( Jump );
			Drop = new CustomButton( () => xboxPadState.Buttons.RightShoulder == ButtonState.Pressed );
			buttons.Add( Drop );
			Aim = new CustomButton( () => xboxPadState.Triggers.Left > .25f );
			buttons.Add( Aim );
			Fire = new CustomButton( () => xboxPadState.Triggers.Right > .25f );
			buttons.Add( Fire );
			SwitchLeft = new CustomButton( () => xboxPadState.Buttons.X == ButtonState.Pressed );
			buttons.Add( SwitchLeft );
			SwitchRight = new CustomButton( () => xboxPadState.Buttons.B == ButtonState.Pressed );
			buttons.Add( SwitchRight );
		}
	}

	// Unity Methods
	void FixedUpdate()
	{
		if( type == InputType.XboxPad )
		{
			xboxPadState = GamePad.GetState( xboxPadIndex );
		}

		foreach( Axis2D axis2D in axes2D )
		{
			axis2D.FixedUpdate();
		}

		foreach( Button button in buttons )
		{
			button.FixedUpdate();
		}

		// Vibration
		if( StrongRumbleForce > StrongRumbleBaseForce )
		{
			StrongRumbleForce = Mathf.Lerp( StrongRumbleForce, StrongRumbleBaseForce, StrongRumbleDecayRate * Time.deltaTime );
		}
		else
		{
			StrongRumbleForce = StrongRumbleBaseForce;
		}

		if( WeakRumbleForce > WeakRumbleBaseForce )
		{
			WeakRumbleForce = Mathf.Lerp( WeakRumbleForce, WeakRumbleBaseForce, WeakRumbleDecayRate * Time.deltaTime );
		}
		else
		{
			WeakRumbleForce = WeakRumbleBaseForce;
		}

		// Set rumble
		if( type == InputType.XboxPad )
		{
			if( gameActive )
			{
				GamePad.SetVibration( xboxPadIndex, StrongRumbleForce, WeakRumbleForce );
			}
			else
			{
				KillVibration();
			}
		}
	}

	void Update()
	{
		foreach( Axis2D axis2D in axes2D )
		{
			axis2D.Update();
		}

		foreach( Button button in buttons )
		{
			button.Update();
		}

		// Set values from Unity/XInput based on the appropriate axis + suffix
		switch( type )
		{
			case InputType.UnityPad:
				Rematch = Aim.Pressed;
				BackToMenu = Fire.Pressed;
				break;
			case InputType.XboxPad:
				xboxPadState = GamePad.GetState( xboxPadIndex );
				Rematch = ( xboxPadState.Triggers.Left >= .25f ? true : false );
				BackToMenu = ( xboxPadState.Triggers.Right >= .25f ? true : false );
				break;
			default:
				break;
		}

		// Dash detection
		if( typeSuffix != " (Keyboard)" )
		{
			CheckJoyDashInput();
		}
		else
		{
			CheckKeyDashInput();
		}
	}

	void OnApplicationQuit()
	{
		KillVibration();
	}

	// Utility Methods
	public void GameOver()
	{
		gameActive = false;
	}

	public void KillVibration()
	{
		if( type == InputType.XboxPad )
		{
			GamePad.SetVibration( xboxPadIndex, 0f, 0f );
		}
	}

	/* Dash states:
	 * 0 - Stick outside outer zone
	 * 1 - Stick within inner zone
	 * 2 - Stick passed out of inner zone and into outer zone
	 * 3 - Stick passed back into inner zone
	 */
	int dashInputState = 1;
	float innerZoneBoundary = 0.25f;
	float outerZoneBoundary = 0.75f;
	float dashTimer = 0f;
	float dashTimeout = .2f;

	void CheckJoyDashInput()
	{
		if( dashTimer > 0f )
		{
			dashTimer -= Time.deltaTime;
		}

		if( dashTimer <= 0f )
		{
			if( dashInputState != 0 )
			{
				dashInputState = 0;
			}
		}

		if( dashInputState == 4 )
		{
			Dash = 0f;
			dashInputState = 0;
			dashTimer = 0f;
		}

		if( dashInputState == 0 && LeftStick.Value.magnitude < innerZoneBoundary )
		{
			dashInputState = 1;
			dashTimer = dashTimeout;
		}
		if( dashInputState == 1 && LeftStick.Value.magnitude > outerZoneBoundary )
		{
			dashInputState = 2;
			dashTimer = dashTimeout;
			DashVector = LeftStick.Value.normalized;
		}
		if( dashInputState == 2 && LeftStick.Value.magnitude < innerZoneBoundary )
		{
			dashInputState = 3;
			dashTimer = dashTimeout;
		}
		if( dashInputState == 3 )
		{
			Dash = 1f;
			dashInputState = 4;
			dashTimer = dashTimeout;
		}
	}

	float prevDash = 0f;
	void CheckKeyDashInput()
	{
		if( Dash == 1f )
		{
			Dash = 0f;
		}

		if( prevDash == 0f && Input.GetKey( KeyCode.LeftShift ) )
		{
			DashVector = LeftStick.Value.normalized;
			Dash = 1f;
		}
		prevDash = Input.GetAxis( "Dash" + typeSuffix );
	}
}
