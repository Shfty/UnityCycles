using UnityEngine;
using System.Collections;

public class LineRendererInstance : PooledObject
{
	// Fields
	LineRenderer lineRenderer;

	void Awake()
	{
		// Find and store the object's line renderer
		lineRenderer = gameObject.GetComponent<LineRenderer>();
	}

	public override void OnDisable()
	{
		base.OnDisable();

		// Reset object to init state
		lineRenderer.SetVertexCount( 0 );
		lineRenderer.SetColors( Color.white, Color.white );
	}
}
