using UnityEngine;
using System.Collections;

public class TerrainBoundary : MonoBehaviour
{
	// Fields
	float _sideLength;

	// Properties
	public float SideLength
	{
		get { return _sideLength; }
	}

	// Variables
	Terrain terrain;
	GameObject container;
	GameObject leftBoundary;
	GameObject rightBoundary;
	GameObject frontBoundary;
	GameObject backBoundary;
	int wallLayer;
	float inradius;
	Vector3 bounds;

	public int Sides = 4;
	public float LipSize;
	public float BoundaryThickness = 20f;
	public float InvisibleWallHeight = 20f;
	public bool InfinitePlanes = true;
	public Material PlaneMaterial;

	// Unity Methods
	void Awake()
	{
	}

	void Start()
	{
		// Create container object for walls
		container = new GameObject( "Boundaries" );

		// Store relevant variables
		terrain = GetComponent<Terrain>();
		bounds = terrain.terrainData.size;
		wallLayer = LayerMask.NameToLayer( "Walls" );

		// Calculate side length
		inradius = bounds.z * .5f + BoundaryThickness;
		_sideLength = 2f * inradius * Mathf.Tan( ( 180f * Mathf.Deg2Rad ) / Sides );

		// Base boundary transform
		Vector3 boundaryPosition = new Vector3( 0f, bounds.y * .5f, inradius - BoundaryThickness * .5f );
		Quaternion boundaryRotation = Quaternion.identity;
		Vector3 boundaryScale = new Vector3( _sideLength, bounds.y + LipSize, BoundaryThickness );

		float angle = 360f / Sides;
		for( int i = 0; i < Sides; ++i )
		{
			GameObject boundary = CreateBoundary(
				boundaryPosition + new Vector3( 0f, LipSize, 0f ) * .5f,
				boundaryRotation,
				boundaryScale
			);
			boundary.GetComponent<MeshRenderer>().castShadows = false;
			boundary.GetComponent<MeshRenderer>().receiveShadows = false;
			boundary.layer = wallLayer;
			Quaternion rotation = Quaternion.AngleAxis( angle, Vector3.up );
			boundaryPosition = rotation * boundaryPosition;
			boundaryRotation *= rotation;
		}

		if( InvisibleWallHeight > 0f )
		{
			boundaryPosition = new Vector3( 0f, bounds.y * .5f, bounds.z * .5f + BoundaryThickness * .5f );
			boundaryRotation = Quaternion.identity;
			boundaryScale = new Vector3( _sideLength, InvisibleWallHeight, BoundaryThickness );

			for( int i = 0; i < Sides; ++i )
			{
				GameObject boundary = CreateBoundary(
					boundaryPosition + new Vector3( 0f, bounds.y * .5f + InvisibleWallHeight * .5f, 0f ) + new Vector3( 0f, LipSize, 0f ),
					boundaryRotation,
					boundaryScale
				);
				boundary.GetComponent<MeshRenderer>().castShadows = false;
				boundary.GetComponent<MeshRenderer>().receiveShadows = false;
				boundary.GetComponent<MeshRenderer>().enabled = false;
				Quaternion rotation = Quaternion.AngleAxis( angle, Vector3.up );
				boundaryPosition = rotation * boundaryPosition;
				boundaryRotation *= rotation;
			}
		}

		if( InfinitePlanes )
		{
			float farPlane = 1000f;

			boundaryPosition = new Vector3( 0f, bounds.y * .5f, bounds.z * .5f + BoundaryThickness + farPlane * .5f );
			boundaryRotation = Quaternion.identity;
			boundaryScale = new Vector3( _sideLength, 1f, farPlane * .1f );

			for( int i = 0; i < Sides; ++i )
			{
				GameObject boundary = CreatePlane(
					boundaryPosition + new Vector3( 0f, bounds.y * .5f, 0f ) + new Vector3( 0f, LipSize, 0f ),
					boundaryRotation,
					boundaryScale
				);
				boundary.GetComponent<MeshRenderer>().castShadows = false;
				boundary.GetComponent<MeshRenderer>().receiveShadows = false;
				Quaternion rotation = Quaternion.AngleAxis( angle, Vector3.up );
				boundaryPosition = rotation * boundaryPosition;
				boundaryPosition += new Vector3( 0f, 0.01f, 0f );
				boundaryRotation *= rotation;
			}
		}
	}

	// Utility Methods
	GameObject CreateBoundary( Vector3 position, Quaternion rotation, Vector3 scale )
	{
		GameObject boundary = GameObject.CreatePrimitive( PrimitiveType.Cube );
		boundary.transform.parent = container.transform;
		boundary.transform.position = position;
		boundary.transform.rotation = rotation;
		boundary.transform.localScale = scale;
		boundary.GetComponent<MeshRenderer>().material = PlaneMaterial;
		return boundary;
	}

	GameObject CreatePlane( Vector3 position, Quaternion rotation, Vector3 scale )
	{
		GameObject plane = GameObject.CreatePrimitive( PrimitiveType.Plane );
		plane.transform.parent = container.transform;
		plane.transform.position = position;
		plane.transform.rotation = rotation;
		plane.transform.localScale = scale;
		plane.GetComponent<MeshRenderer>().material = PlaneMaterial;
		return plane;
	}
}
