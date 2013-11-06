using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class WorleyNoiseTerrain : MonoBehaviour
{
	// Fields
	Terrain t;
	static Vector2 bounds;
	static Vector2 cellSize;
	static List<List<Vector2>> pointGrid;
	static float[ , ] heights;
	static Vector2 gridDivisions;
	static DistMetric distanceMetric;
	static float minkowskiNumber;
	static int fValue;
	int cores;
	static int threadsFinished;

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

	delegate void ThreadCallback();

	// Thread data class
	class ThreadData
	{
		public int startY;
		public int endY;
		public int rowWidth;
		public ThreadCallback callback;

		public ThreadData( int s, int e, int rw, ThreadCallback c )
		{
			startY = s;
			endY = e;
			rowWidth = rw;
			callback = c;
		}
	}

	// Properties
	public bool Threaded = false;
	public bool RegenerateOnLoad;
	public Vector2 GridDivisions;
	public int MaxPointsPerCell;
	public DistMetric DistanceMetric;
	public float MinkowskiNumber;
	public int FValue;
	public bool UseRandomSeed;
	public int Seed;

	// Unity Methods
	void Awake()
	{
		gridDivisions = GridDivisions;
		distanceMetric = DistanceMetric;
		minkowskiNumber = MinkowskiNumber;
		fValue = FValue;
	}

	void Start() // Use this for initialization
	{
		if( RegenerateOnLoad == false ) return;

		if( UseRandomSeed )
		{
			Random.seed = Seed;
		}
		t = Terrain.activeTerrain;
		bounds = new Vector2( t.terrainData.heightmapWidth, t.terrainData.heightmapHeight );

		heights = new float[ t.terrainData.heightmapWidth, t.terrainData.heightmapHeight ];

		// Setup
		SetupCells();

		// Terrain
		FlattenTerrain( t.terrainData );

		cores = Mathf.Min( SystemInfo.processorCount - 1, t.terrainData.heightmapHeight );
		int slice = t.terrainData.heightmapHeight / cores;

		if( cores > 1 && Threaded )
		{
			int i;
			ThreadData threadData;
			List<Thread> threads = new List<Thread>();
			for( i = 0; i < cores; ++i )
			{
				threadData = new ThreadData( slice * i, slice * ( i + 1 ), t.terrainData.heightmapWidth, ThreadFinished );
				Thread thread = new Thread( () => NoiseRows( threadData ) );
				thread.IsBackground = true;
				threads.Add( thread );
				thread.Start();
				Thread.Sleep( 10 ); // Don't know why this is needed, random failures otherwise
			}
		}
		else
		{
			ThreadData threadData = new ThreadData( 0, t.terrainData.heightmapHeight, t.terrainData.heightmapWidth, ThreadFinished );
			NoiseRows( threadData );
			t.terrainData.SetHeights( 0, 0, heights );
		}
	}

	void Update()
	{
		if( threadsFinished == cores && Threaded )
		{
			t.terrainData.SetHeights( 0, 0, heights );
			threadsFinished = 0;
		}
	}

	// Utility Methods
	void ThreadFinished()
	{
		Interlocked.Increment( ref threadsFinished );
	}

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

	private static float Dist( Vector2 a, Vector2 b, int metric )
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
				distance = Mathf.Pow( Mathf.Pow( Mathf.Abs( d.x ), minkowskiNumber ) + Mathf.Pow( Mathf.Abs( d.y ), minkowskiNumber ), ( 1f / minkowskiNumber ) );
				break;
			}
			default:
			{
				break;
			}
		}

		return distance;
	}

	static float Noise2D( Vector2 pt )
	{
		// Return error if the point is out of bounds
		if( pt.x < 0 || pt.x > bounds.x || pt.y < 0 || pt.y > bounds.y ) return -1;

		// Calculate grid coordinates
		int m_cellX = (int)Mathf.Floor( pt.x / bounds.x * gridDivisions.x );
		int m_cellY = (int)Mathf.Floor( pt.y / bounds.y * gridDivisions.y );

		// Add 3x3 block of cells surrounding point to search candidates
		List< Vector2 > searchPoints = new List< Vector2 >();
		for( int i = 0; i < 9; ++i )
		{
			int xOff = ( i % 3 ) - 1;
			int yOff = ( i / 3 ) - 1;
			if( m_cellX + xOff < 0 ) continue;
			if( m_cellX + xOff > gridDivisions.x - 1 ) continue;
			if( m_cellY + yOff < 0 ) continue;
			if( m_cellY + yOff > gridDivisions.y - 1 ) continue;
			int cellIdx = (int)( ( m_cellY + yOff ) * gridDivisions.x + m_cellX + xOff );
			List< Vector2 > cell = pointGrid[ cellIdx ];
			searchPoints.InsertRange( searchPoints.Count, cell );
		}

		// Return error if the fValue is greater than the number of potential search points
		if( fValue == 0 || fValue > searchPoints.Count ) return -1;

		// Sort the points from near-far
		searchPoints.Sort(
			delegate( Vector2 a, Vector2 b )
			{
				float distA = Dist( pt, a, (int)distanceMetric );
				float distB = Dist( pt, b, (int)distanceMetric );
				return distA.CompareTo( distB );
			}
		);

		// Calculate the distance and maximum length using our predefined metric
		float distance = Dist( pt, searchPoints[ fValue - 1 ], (int)distanceMetric );
		float maxLength = Dist( new Vector2( 0, 0 ), cellSize, (int)distanceMetric );

		// Adjust maxLength to account for squared outputs
		if( distanceMetric == DistMetric.Linear2 || distanceMetric == DistMetric.Quadratic )
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
		heights.Initialize();
		td.SetHeights( 0, 0, heights );
	}

	void NoiseRows( ThreadData threadData )
	{
		for( int y = threadData.startY; y < threadData.endY; ++y )
		{
			for( int x = 0; x < threadData.rowWidth; ++x )
			{
				float height = Noise2D( new Vector2( x, y ) );
				heights[ x, y ] = height;
			}
		}
		threadData.callback();
	}
}
