using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class InputWrapper : MonoBehaviour
{
	// Fields
	GamePadState xboxPadState;
	PlayerIndex xboxPadIndex;
	InputType type = InputType.None;
	string typeSuffix;

	// Enums
	public enum InputType
	{
		None,
		UnityPad,
		XboxPad
	}
	
	// Properties
	public int LocalPlayerIndex;
	public Vector2 LeftStick;
	public Vector2 RightStick;
	public float Jump;
	public float Drop;
	public float Aim;
	public float Fire;
	public float SwitchLeft;
	public float SwitchRight;

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
					// Set the appropriate type and instantiate the XInput pad index
					type = InputType.XboxPad;
					xboxPadIndex = (PlayerIndex)LocalPlayerIndex;
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
			Debug.LogWarning( "InputWrapper #" + LocalPlayerIndex + " unable to bind joystick" );
		}
	}

	// Unity Methods
	void Update()
	{
		// Set values from Unity/XInput based on the appropriate axis + suffix
		switch( type )
		{
			case InputType.UnityPad:
				LeftStick.x = Input.GetAxis( "Move Horizontal" + typeSuffix );
				LeftStick.y = Input.GetAxis( "Move Vertical" + typeSuffix );

				if( typeSuffix != " (Keyboard)" )
				{
					RightStick.x = Input.GetAxis( "Camera Horizontal" + typeSuffix );
					RightStick.y = Input.GetAxis( "Camera Vertical" + typeSuffix );
				}
				else
				{
					RightStick.x = Input.GetAxisRaw( "Camera Horizontal" + typeSuffix );
					RightStick.y = Input.GetAxisRaw( "Camera Vertical" + typeSuffix );
				}

				Jump = Input.GetAxis( "Jump" + typeSuffix );
				Drop = Input.GetAxis( "Drop" + typeSuffix );
				Aim = Input.GetAxis( "Aim" + typeSuffix );
				Fire = Input.GetAxis( "Fire" + typeSuffix );
				SwitchLeft = Input.GetAxis( "Switch Left" + typeSuffix );
				SwitchRight = Input.GetAxis( "Switch Right" + typeSuffix );
				break;
			case InputType.XboxPad:
				xboxPadState = GamePad.GetState( xboxPadIndex );

				LeftStick.x = xboxPadState.ThumbSticks.Left.X;
				LeftStick.y = xboxPadState.ThumbSticks.Left.Y;

				RightStick.x = xboxPadState.ThumbSticks.Right.X;
				RightStick.y = xboxPadState.ThumbSticks.Right.Y;

				Jump = xboxPadState.Buttons.LeftShoulder == ButtonState.Pressed ? 1f : 0f;
				Drop = ( xboxPadState.Buttons.RightShoulder == ButtonState.Pressed ? 1f : 0f );
				Aim = ( xboxPadState.Triggers.Left >= .25f ? 1f : 0f );
				Fire = ( xboxPadState.Triggers.Right >= .25f ? 1f : 0f );
				SwitchLeft = ( xboxPadState.Buttons.X == ButtonState.Pressed ? 1f : 0f );
				SwitchRight = ( xboxPadState.Buttons.B == ButtonState.Pressed ? 1f : 0f );
				break;
		}

	}
}
