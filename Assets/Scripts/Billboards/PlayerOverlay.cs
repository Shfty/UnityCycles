﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerOverlay : Billboard
{
	// Properties
	public GameObject Player;

	// Utility Methods
	public override void LateStart()
	{
		if( cameras != null )
		{
			cameras.Clear();

			foreach( GameObject billboard in billboards )
			{
				GameControl.BillboardPool.Despawn( billboard );
			}
			billboards.Clear();

			// Check the player count, create a billboard for each camera and set it to the respective layer
			for( int i = 0; i < GameControl.Players.Count; ++i )
			{
				if( GameControl.Players[ i ] == Player )
				{
					continue;
				}

				GameObject cam = GameControl.Players[ i ].transform.Find( "Camera" ).gameObject;
				cameras.Add( cam.camera );
				GameObject billboard = GameControl.BillboardPool.Spawn( BillboardPrefab );
				billboard.layer = LayerMask.NameToLayer( "Camera " + ( i + 1 ) );
				billboard.transform.parent = transform;
				billboards.Add( billboard );
			}
		}
	}
}
