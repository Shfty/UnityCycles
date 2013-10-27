using UnityEngine;
using System.Collections;

public class TerrainBoundary : MonoBehaviour
{
	// Fields
	Terrain terrain;
	GameObject container;
	GameObject leftBoundary;
	GameObject rightBoundary;
	GameObject frontBoundary;
	GameObject backBoundary;

	// Properties
	public float BoundaryThickness = 20f;
	public float BoundaryExtraHeight = 20f;

	// Unity Methods
	void Awake()
	{
		container = new GameObject();
		container.name = "Boundaries";
	}

	void Start()
	{
		// Store relevant variables
		terrain = GetComponent<Terrain>();
		Vector3 bounds = terrain.terrainData.size;
		int wallLayer = LayerMask.NameToLayer( "Walls" );

		// Generate terrain side boundaries
		leftBoundary = GameObject.CreatePrimitive( PrimitiveType.Cube );
		leftBoundary.transform.parent = container.transform;
		leftBoundary.layer = wallLayer;
		leftBoundary.transform.localScale = new Vector3( BoundaryThickness, bounds.y + BoundaryExtraHeight, bounds.z + BoundaryThickness * 2f );
		leftBoundary.transform.position = new Vector3( -bounds.x * .5f - BoundaryThickness * .5f, ( bounds.y + BoundaryExtraHeight ) * .5f, 0f );

		rightBoundary = GameObject.CreatePrimitive( PrimitiveType.Cube );
		rightBoundary.transform.parent = container.transform;
		rightBoundary.layer = wallLayer;
		rightBoundary.transform.localScale = new Vector3( BoundaryThickness, bounds.y + BoundaryExtraHeight, bounds.z + BoundaryThickness * 2f );
		rightBoundary.transform.position = new Vector3( bounds.x * .5f + BoundaryThickness * .5f, ( bounds.y + BoundaryExtraHeight ) * .5f, 0f );

		frontBoundary = GameObject.CreatePrimitive( PrimitiveType.Cube );
		frontBoundary.transform.parent = container.transform;
		frontBoundary.layer = wallLayer;
		frontBoundary.transform.localScale = new Vector3( bounds.z + BoundaryThickness * 2f, bounds.y + BoundaryExtraHeight, BoundaryThickness );
		frontBoundary.transform.position = new Vector3( 0f, ( bounds.y + BoundaryExtraHeight ) * .5f, -bounds.z * .5f - BoundaryThickness * .5f );

		backBoundary = GameObject.CreatePrimitive( PrimitiveType.Cube );
		backBoundary.transform.parent = container.transform;
		backBoundary.layer = wallLayer;
		backBoundary.transform.localScale = new Vector3( bounds.z + BoundaryThickness * 2f, bounds.y + BoundaryExtraHeight, BoundaryThickness );
		backBoundary.transform.position = new Vector3( 0f, ( bounds.y + BoundaryExtraHeight ) * .5f, bounds.z * .5f + BoundaryThickness * .5f );
	}
}
