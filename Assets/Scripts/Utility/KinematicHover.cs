using UnityEngine;
using System.Collections;

public class KinematicHover : MonoBehaviour
{
    // Enums
    public enum LerpMode
    {
        Linear,
        Spherical
    }

	// Variables
	Vector3 targetPosition;

	public Transform Target;
	public Vector3 Offset;
	public bool Lerp = false;
	public LerpMode LerpType = LerpMode.Linear;
	public float LerpFactor = .2f;

	// Unity Methods
	void Update()
	{
		if( Target != null )
		{
			targetPosition = Target.position + Offset;

			if( Lerp )
			{
				switch( LerpType )
				{
					case LerpMode.Linear:
						transform.position = Vector3.Lerp( transform.position, targetPosition, LerpFactor * Time.deltaTime );
						break;
					case LerpMode.Spherical:
						transform.position = Vector3.Slerp( transform.position, targetPosition, LerpFactor * Time.deltaTime );
						break;
				}
			}
			else
			{
				transform.position = targetPosition;
			}
		}
	}
}
