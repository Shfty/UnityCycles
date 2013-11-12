using UnityEngine;
using System.Collections;

public class MenuWheelRotate : MonoBehaviour
{
	// Variables
	public float RotateSpeed;
	
	// Unity Methods
	void Update()
	{
		transform.Rotate( Vector3.up, RotateSpeed * Time.deltaTime );
	}
	
	// Utility Methods
}
