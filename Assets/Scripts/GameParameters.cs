using UnityEngine;
using System.Collections;

public class GameParameters : MonoBehaviour
{
	// Variables
	// Game
	public int PlayerCount = 1;
	public int MaxPickups = 15;
	public float PickupRespawnDelay = 2f;
	public int ScoreLimit = 500;

	// Arena
	public float TerrainSize = 100;
	public float TerrainHeight = 10;
	public int TerrainNoiseSubdivisions = 100;
	public int TerrainTurbulence = 10;
	public WorleyNoiseTerrain.DistMetric TerrainType = WorleyNoiseTerrain.DistMetric.Linear;
	public bool AltTerrain = false;
	public int ArenaSides = 4;
	public bool UseRandomSeed = false;
	public int RandomSeed = 0;

	// Unity Methods
	void Awake()
	{
		DontDestroyOnLoad( gameObject );
	}
}
