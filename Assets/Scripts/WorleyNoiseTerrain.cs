using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorleyNoiseTerrain : MonoBehaviour
{
	// Fields
	Terrain t;
	Vector2 bounds;
	Vector2 cellSize;
	List< List < Vector2 > > pointGrid;

	// Enums
	public enum DistMetric
	{
		Linear = 0,
		Linear2 = 1,
		Manhattan = 2,
		Chebyshev = 3,
		Quadratic = 4,
		Minkowski = 5
	}

	// Properties
	public bool RegenerateOnLoad;
	public Vector2 GridDivisions;
	public int MaxPointsPerCell;
	public DistMetric DistanceMetric;
	public float MinkowskiNumber;
	public int FValue;
	public bool UseRandomSeed;
	public int Seed;

	// Unity Methods
	void Start() // Use this for initialization
	{
		if( RegenerateOnLoad == false ) return;

		if( UseRandomSeed )
		{
			Random.seed = Seed;
		}
		t = GetComponent<Terrain>();
		bounds = new Vector2( t.terrainData.heightmapWidth, t.terrainData.heightmapHeight );

		// Setup
		SetupCells();

		// Terrain
		FlattenTerrain( t.terrainData );
		NoiseTerrain( t.terrainData );
	}

	// Utility Methods
	void SetupCells()
	{
		cellSize = new Vector2( bounds.x / GridDivisions.x,	bounds.y / GridDivisions.y );

		pointGrid = new List< List< Vector2 > >();
		int count = (int)( GridDivisions.x * GridDivisions.y );
		for( int i = 0; i < count; ++i )
		{
			pointGrid.Add( new List< Vector2 >() );
		}

		for( int i = 0; i < pointGrid.Count; ++i )
		{
			int cellX = i % (int)GridDivisions.x;
			int cellY = i / (int)GridDivisions.x;
			int pointCount = (int)( Random.Range( 0f, 1f ) * MaxPointsPerCell );
			for( int j = 0; j < pointCount; ++j )
			{
				float offsetX = cellX * cellSize.x;
				float offsetY = cellY * cellSize.y;
				float randX = Random.Range( 0f, 1f ) * cellSize.x;
				float randY = Random.Range( 0f, 1f ) * cellSize.y;
				pointGrid[ i ].Add( new Vector2( offsetX + randX, offsetY + randY ) );
			}
		}
	}

	float Dist( Vector2 a, Vector2 b, int metric )
	{
		float distance = 0;

		Vector2 d = a - b;

		switch( metric )
		{
			case 0: // Linear
			{
				distance = d.magnitude;
				break;
			}
			case 1: // Linear Squared
			{
				distance = d.sqrMagnitude;
				break;
			}
			case 2: // Manhattan
			{
				distance = Mathf.Abs( d.x ) + Mathf.Abs( d.y );
				break;
			}
			case 3: // Chebyshev
			{
				float x = Mathf.Abs( d.x );
				float y = Mathf.Abs( d.y );
				if( x == y || x < y )
				{
					distance = y;
				}
				else
				{
					distance = x;
				}
				break;
			}
			case 4: // Quadratic
			{
				distance = ( d.x * d.x + d.x * d.y + d.y * d.y );
				break;
			}
			case 5: // Minkowski
			{
				distance = Mathf.Pow( Mathf.Pow( Mathf.Abs( d.x ), MinkowskiNumber ) + Mathf.Pow( Mathf.Abs( d.y ), MinkowskiNumber ), ( 1f / MinkowskiNumber ) );
				break;
			}
			default:
			{
				break;
			}
		}

		return distance;
	}

	float Noise2D( Vector2 pt )
	{
		// Return error if the point is out of bounds
		if( pt.x < 0 || pt.x > t.terrainData.heightmapWidth || pt.y < 0 || pt.y > t.terrainData.heightmapHeight ) return -1;

		// Calculate grid coordinates
		int m_cellX = (int)Mathf.Floor( pt.x / bounds.x * GridDivisions.x );
		int m_cellY = (int)Mathf.Floor( pt.y / bounds.y * GridDivisions.y );

		// Add 3x3 block of cells surrounding point to search candidates
		List< Vector2 > searchPoints = new List< Vector2 >();
		for( int i = 0; i < 9; ++i )
		{
			int xOff = ( i % 3 ) - 1;
			int yOff = ( i / 3 ) - 1;
			if( m_cellX + xOff < 0 ) continue;
			if( m_cellX + xOff > GridDivisions.x - 1 ) continue;
			if( m_cellY + yOff < 0 ) continue;
			if( m_cellY + yOff > GridDivisions.y - 1 ) continue;
			int cellIdx = (int)( ( m_cellY + yOff ) * GridDivisions.x + m_cellX + xOff );
			List< Vector2 > cell = pointGrid[ cellIdx ];
			searchPoints.InsertRange( searchPoints.Count, cell );
		}

		// Return error if the fValue is greater than the number of potential search points
		if( FValue == 0 || FValue > searchPoints.Count ) return -1;

		// Sort the points from near-far
		searchPoints.Sort(
			delegate( Vector2 a, Vector2 b )
			{
				float distA = Dist( pt, a, (int)DistanceMetric );
				float distB = Dist( pt, b, (int)DistanceMetric );
				return distA.CompareTo( distB );
			}
		);

		// Calculate the distance and maximum length using our predefined metric
		float distance = Dist( pt, searchPoints[ FValue - 1 ], (int)DistanceMetric );
		float maxLength = Dist( new Vector2( 0, 0 ), cellSize, (int)DistanceMetric );

		// Adjust maxLength to account for squared outputs
		if( DistanceMetric == DistMetric.Linear2 || DistanceMetric == DistMetric.Quadratic )
		{
			maxLength *= .1f;
		}
		else
		{
			maxLength *= .5f;
		}

		float normDist = distance / maxLength;

		return normDist;
	}

	void FlattenTerrain( TerrainData td )
	{
		float[ , ] heights = new float[ td.heightmapWidth, td.heightmapHeight ];
		for( int x = 0; x < td.heightmapWidth; ++x )
		{
			for( int y = 0; y < td.heightmapHeight; ++y )
			{
				heights[ x, y ] = 0f;
			}
		}

		td.SetHeights( 0, 0, heights );
	}

	void NoiseTerrain( TerrainData td )
	{
		float[ , ] heights = new float[ td.heightmapWidth, td.heightmapHeight ];
		for( int x = 0; x < td.heightmapWidth; ++x )
		{
			for( int y = 0; y < td.heightmapHeight; ++y )
			{
				heights[ x, y ] = Noise2D( new Vector2( x, y ) );
			}
		}

		td.SetHeights( 0, 0, heights );
	}
}
