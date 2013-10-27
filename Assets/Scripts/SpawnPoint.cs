using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
	// Unity Methods
	void OnDrawGizmos()
	{
		// Draw a green wiresphere
		if( GameInfo.Properties.Debug )
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere( transform.position, 1f );
		}
	}
}
