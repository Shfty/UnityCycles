using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainTypeTexture : MonoBehaviour
{
	// Variables
	public bool Alternate;
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
				if( !Alternate )
				{
					terrain.materialTemplate = Materials.Find( item => item.name == "Pyramid Terrain" );
				}
				else
				{
					terrain.materialTemplate = Materials.Find( item => item.name == "Glacier Terrain" );
				}
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
