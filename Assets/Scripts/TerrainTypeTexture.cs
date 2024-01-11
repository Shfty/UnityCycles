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
		if( !Alternate )
		{
			switch( noiseScript.DistanceMetric )
			{
				case WorleyNoiseTerrain.DistMetric.Linear:
					terrain.materialTemplate = Materials.Find( item => item.name == "Sand Terrain" );
					break;
				case WorleyNoiseTerrain.DistMetric.Linear2:
					terrain.materialTemplate = Materials.Find( item => item.name == "Blasted Canyon Terrain" );
					break;
				case WorleyNoiseTerrain.DistMetric.Manhattan:
					terrain.materialTemplate = Materials.Find( item => item.name == "Pyramid Terrain" );
					break;
				case WorleyNoiseTerrain.DistMetric.Chebyshev:
					terrain.materialTemplate = Materials.Find( item => item.name == "Tech World Terrain" );
					break;
				case WorleyNoiseTerrain.DistMetric.Quadratic:
					terrain.materialTemplate = Materials.Find( item => item.name == "Distant World Terrain" );
					break;
				case WorleyNoiseTerrain.DistMetric.Minkowski:
					terrain.materialTemplate = Materials.Find( item => item.name == "Mountain Range Terrain" );
					break;
				default:
					break;
			}
		}
		else
		{
			switch( noiseScript.DistanceMetric )
			{
				case WorleyNoiseTerrain.DistMetric.Linear:
						terrain.materialTemplate = Materials.Find( item => item.name == "Plains Terrain" );
					break;
				case WorleyNoiseTerrain.DistMetric.Linear2:
					terrain.materialTemplate = Materials.Find( item => item.name == "Asteroid Terrain" );
					break;
				case WorleyNoiseTerrain.DistMetric.Manhattan:
						terrain.materialTemplate = Materials.Find( item => item.name == "Glacier Terrain" );
					break;
				case WorleyNoiseTerrain.DistMetric.Chebyshev:
						terrain.materialTemplate = Materials.Find( item => item.name == "Crystal Terrain" );
					break;
				case WorleyNoiseTerrain.DistMetric.Quadratic:
						terrain.materialTemplate = Materials.Find( item => item.name == "Frozen Sea Terrain" );
					break;
				case WorleyNoiseTerrain.DistMetric.Minkowski:
						terrain.materialTemplate = Materials.Find( item => item.name == "Martian Expanse Terrain" );
					break;
				default:
					break;
			}
		}
	}
	
	// Utility Methods
}
