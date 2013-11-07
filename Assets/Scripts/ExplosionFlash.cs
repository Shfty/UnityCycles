using UnityEngine;
using System.Collections;

public class ExplosionFlash : MonoBehaviour
{
    // Variables
    public float startTime;
    public float transparency = 1f;

    public float FadeTime = 1f;

    // Unity Methods
    void OnEnable()
    {
        startTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        float progress = ( Time.realtimeSinceStartup - startTime ) / FadeTime;
        transparency = Mathf.Lerp( 1f, 0f, progress );

        Color color = GetComponent<MeshRenderer>().material.color;
        color.a = transparency;
        GetComponent<MeshRenderer>().material.color = color;

        if( progress >= 1f )
        {
            GameControl.ProjectilePool.Despawn( gameObject );
        }
    }
}
