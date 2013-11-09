using UnityEngine;
using System.Collections;

public class TerrainTextureOffset : MonoBehaviour
{
	// Unity Methods
	void Start()
	{
		Randomize();
	}

	public void Randomize()
	{
		Material terrainMaterial = Terrain.activeTerrain.materialTemplate;
		Vector2 scale = terrainMaterial.GetTextureScale( "_Base" );

		Vector2 offset = Vector2.zero;
		offset.x = Random.Range( 0f, 1f - scale.x );
		offset.y = Random.Range( 0f, 1f - scale.y );

		Terrain.activeTerrain.materialTemplate.SetTextureOffset( "_Base", offset );
	}
}
