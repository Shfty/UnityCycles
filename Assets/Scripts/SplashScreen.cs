using UnityEngine;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
	// Unity Methods
	void Awake()
	{
		Application.LoadLevel( "Menu" );
	}
}
