using UnityEngine;
using System.Collections;

public class CollisionRedirect : MonoBehaviour
{
	// Properties
	public MarbleMovement MarbleScript;

	// Unity Methods
	void OnCollisionEnter( Collision col )
	{
		MarbleScript.OnCollisionEnter( col );
	}

	void OnCollisionStay( Collision col )
	{
		MarbleScript.OnCollisionStay( col );
	}

	void OnCollisionExit()
	{
		MarbleScript.OnCollisionExit();
	}
}
