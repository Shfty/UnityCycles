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
	public bool RankPerElement = false;

	public List<int> PlayerScores;
	public int ScoreLimit = 5;
	public int BaseScoreMultiplier = 1000;

	public List<int> PlayerKills;
	public int KillLimit = 10;
	public int KillScoreMultiplier = 1;

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
		while( PlayerScores.Count < players.Count )
		{
			PlayerScores.Add( 0 );
		}

		while( PlayerKills.Count < players.Count )
		{
			PlayerKills.Add( 0 );
		}

		for( int i = 0; i < players.Count; ++i )
		{
			if( RankPerElement )
			{
				if( GameElements.Contains( GameElement.Deathmatch ) && PlayerKills[ i ] > KillLimit )
				{
					print( "Player " + ( i + 1 ) + " Wins!" );
				}
			}
			else
			{
				// Average score from individual game elements
				PlayerScores[ i ] = 0;
				if( GameElements.Contains( GameElement.Deathmatch ) )
				{
					PlayerScores[ i ] += PlayerKills[ i ] * KillScoreMultiplier;
				}
				PlayerScores[ i ] /= GameElements.Count;
				PlayerScores[ i ] *= BaseScoreMultiplier;

				// Check to see if anyone has won
				if( PlayerScores[ i ] >= ScoreLimit )
				{
					print( "Player " + ( i + 1 ) + " Wins!" );
				}
			}
		}
	}
	
	// Utility Methods
	public void PlayerDeath( GameObject victim, GameObject killedBy )
	{
		if( GameElements.Contains( GameElement.Deathmatch ) )
		{
			PlayerKills[ players.IndexOf( killedBy ) ] += KillScoreMultiplier;
		}
	}
}
