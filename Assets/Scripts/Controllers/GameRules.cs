using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameRules : MonoBehaviour
{
	// Fields
	List<GameObject> players;

	// Enums
	public enum GameElement
	{
		Deathmatch = 0
	}

	// Properties
	public List<GameElement> GameElements;
	public int ScoreLimit = 10;
	public static GameRules Instance;

	// Unity Methods
	void Awake()
	{
		Instance = this;
	}
	
	void Start()
	{
		players = GameObject.Find( "Game Control" ).GetComponent<GameControl>().Players;
	}
	
	void Update()
	{
	
	}
	
	// Utility Methods
	public void PlayerDeath( GameObject victim, GameObject killedBy )
	{
		if( GameElements.Contains( GameElement.Deathmatch ) )
		{
			killedBy.GetComponent<Player>().Score++;
			foreach( GameObject go in players )
			{
				Player player = go.GetComponent<Player>();
				if( player.Score >= ScoreLimit )
				{
					print( "Player " + ( players.IndexOf( go ) + 1 ) + " Wins!" );
				}
			}
		}
	}
}
