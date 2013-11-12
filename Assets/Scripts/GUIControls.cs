using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GUIControls
{
	class PrimitiveRef<T>
	{
		T _value;

		public void Set( T value )
		{
			_value = value;
		}

		public T Get()
		{
			return _value;
		}
	}

	abstract class MenuItem
	{
		public delegate void Activate();
		public delegate void ValueChange( int sign );

		public string Label;
		public Activate ActivateCallback = null;
		public ValueChange ValueChangeCallback = null;

		public MenuItem( string label, Activate activate = null, ValueChange valueChange = null )
		{
			Label = label;
			ActivateCallback = activate;
			ValueChangeCallback = valueChange;
		}

		public abstract void DrawGUI( GUIStyle style );
	}

	class MenuButton : MenuItem
	{
		public int BoxPadding = 10;

		public MenuButton( string label, Activate activate = null ) : base( label, activate ) { }

		public override void DrawGUI( GUIStyle style )
		{
			GUILayout.BeginHorizontal( style );
			{
				GUILayout.Space( BoxPadding );
				if( GUILayout.Button( Label, GUILayout.Height( 32 ) ) )
				{
					if( ActivateCallback != null )
					{
						ActivateCallback();
					}
				}
				GUILayout.Space( BoxPadding );
			}
			GUILayout.EndHorizontal();
		}
	}

	class MenuSlider : MenuItem
	{
		PrimitiveRef<float> Value;
		float Min;
		float Max;
		float BarWidth;
		string NumberFormat;

		public MenuSlider( string label, PrimitiveRef<float> value, float min, float max, float barWidth, string numberFormat, Activate activate = null, ValueChange valueChange = null )
			: base( label, activate, valueChange )
		{
			Value = value;
			Min = min;
			Max = max;
			BarWidth = barWidth;
			NumberFormat = numberFormat;
		}

		public override void DrawGUI( GUIStyle style )
		{
			GUILayout.BeginHorizontal( style );
			{
				GUILayout.BeginHorizontal( GUILayout.Height( 32 ) );
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label( Label, GUI.skin.GetStyle( "subtext" ), GUILayout.ExpandHeight( true ) );
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal( GUILayout.Width( BarWidth ), GUILayout.Height( 32 ) );
				{
					GUILayout.Box( Value.Get().ToString( NumberFormat ), GUILayout.Width( 48 ) );
					GUILayout.Space( 10 );
					Value.Set( GUILayout.HorizontalSlider( Value.Get(), Min, Max, GUILayout.Width( BarWidth - 68 ), GUILayout.ExpandHeight( true ) ) );
					GUILayout.Space( 10 );
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndHorizontal();
		}
	}

	class MenuCheckbox : MenuItem
	{
		public PrimitiveRef<bool> Value;

		public MenuCheckbox( string label, PrimitiveRef<bool> value, Activate activate = null )
			: base( label, activate )
		{
			Value = value;
		}

		public override void DrawGUI( GUIStyle style )
		{
			GUILayout.BeginHorizontal( style, GUILayout.ExpandWidth( true ) );
			{
				GUILayout.FlexibleSpace();
				Value.Set( GUILayout.Toggle( Value.Get(), "Use Random Seed", GUILayout.ExpandHeight( true ) ) );
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}
	}

	class MenuIndexedString : MenuItem
	{
		PrimitiveRef<int> Value;
		List<string> Strings;
		float BarWidth;

		public MenuIndexedString( string label, PrimitiveRef<int> value, List<string> strings, float barWidth, Activate activate = null, ValueChange valueChange = null )
			: base( label, activate, valueChange )
		{
			Value = value;
			Strings = strings;
			BarWidth = barWidth;
		}

		public override void DrawGUI( GUIStyle style )
		{
			GUILayout.BeginHorizontal( style );
			{
				GUILayout.BeginHorizontal( GUILayout.Height( 32 ) );
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label( Label, GUI.skin.GetStyle( "subtext" ), GUILayout.ExpandHeight( true ) );
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal( GUILayout.Width( BarWidth ), GUILayout.Height( 32 ) );
				{
					if( GUILayout.Button( "<", GUILayout.Width( 48 ), GUILayout.ExpandHeight( true ) ) )
					{
						Value.Set( Value.Get() - 1 );
						if( Value.Get() < 0 )
						{
							Value.Set( Strings.Count - 1 );
						}
					}
					GUILayout.Space( 10 );
					GUILayout.Box( Strings[ (int)Value.Get() ], GUILayout.Width( BarWidth - 4 - 30 - 48 * 2 ), GUILayout.ExpandHeight( true ) );
					GUILayout.Space( 10 );
					if( GUILayout.Button( ">", GUILayout.Width( 48 ), GUILayout.ExpandHeight( true ) ) )
					{
						Value.Set( Value.Get() + 1 );
						if( Value.Get() > Strings.Count - 1 )
						{
							Value.Set( 0 );
						}
					}
					GUILayout.Space( 10 );
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndHorizontal();
		}
	}

	class MenuTextField : MenuItem
	{
		public PrimitiveRef<string> Value;
		public float BarWidth;

		public MenuTextField( string label, PrimitiveRef<string> value, float barWidth, Activate activate = null, ValueChange valueChange = null )
			: base( label, activate, valueChange )
		{
			Value = value;
			BarWidth = barWidth;
		}

		public override void DrawGUI( GUIStyle style )
		{
			GUILayout.BeginHorizontal( style );
			{
				GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ), GUILayout.Height( 32 ) );
				{
					GUILayout.Label( Label, GUI.skin.GetStyle( "subtext" ), GUILayout.ExpandHeight( true ) );
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal( GUILayout.Width( BarWidth ), GUILayout.Height( 32 ) );
				{
					GUILayout.FlexibleSpace();
					Value.Set( GUILayout.TextField( Value.Get(), GUILayout.Width( 48 ), GUILayout.ExpandHeight( true ) ) );
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndHorizontal();
		}
	}
}