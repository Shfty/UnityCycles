using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerOverlay : Billboard
{
	// Properties
	public GameObject Player;

	// Unity Methods
	public override void Start()
	{
		// Spawn cameras and billboards via base class
		base.Start();

		// Prune current player's overlay
		int playerIndex = Player.GetComponent<InputWrapper>().LocalPlayerIndex;
		cameras.RemoveAt( playerIndex );
		GameObject billboard = billboards[ playerIndex ];
		billboard.GetComponent<PooledObject>().Deactivate();
		billboards.RemoveAt( playerIndex );
	}
}
