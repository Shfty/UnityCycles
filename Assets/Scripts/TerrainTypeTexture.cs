using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainTypeTexture : MonoBehaviour
{
	// Fields
	
	// Properties

	// Variables
	public List<Material> Materials;
	
	// Unity Methods
	void Start()
	{
		Terrain terrain = Terrain.activeTerrain;
		WorleyNoiseTerrain noiseScript = Terrain.activeTerrain.gameObject.GetComponent<WorleyNoiseTerrain>();
		switch( noiseScript.DistanceMetric )
		{
			case WorleyNoiseTerrain.DistMetric.Linear:
				terrain.materialTemplate = Materials.Find( item => item.name == "Sand Terrain" );
				break;
			case WorleyNoiseTerrain.DistMetric.Linear2:
				terrain.materialTemplate = Materials.Find( item => item.name == "Blasted Canyon Terrain" );
				break;
			case WorleyNoiseTerrain.DistMetric.Manhattan:
				terrain.materialTemplate = Materials.Find( item => item.name == "Glacier Terrain" );
				break;
			case WorleyNoiseTerrain.DistMetric.Chebyshev:
				terrain.materialTemplate = Materials.Find( item => item.name == "Tech World Terrain" );
				break;
			case WorleyNoiseTerrain.DistMetric.Quadratic:
				terrain.materialTemplate = Materials.Find( item => item.name == "Distant World Terrain" );
				break;
			default:
				break;
		}
	}
	
	// Utility Methods
}
