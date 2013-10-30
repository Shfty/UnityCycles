using UnityEngine;
using System.Collections;

public class LineRendererInstance : MonoBehaviour
{
	// Fields
	LineRenderer lineRenderer;

	void Awake()
	{
		// Find and store the object's line renderer
		lineRenderer = gameObject.GetComponent<LineRenderer>();
	}

	public void OnEnable()
	{
		// Reset object to init state
		lineRenderer.SetVertexCount( 0 );
		lineRenderer.SetColors( Color.white, Color.white );
	}
}
