using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
	// Fields
	bool firstFrame = true;
	bool applyDamage = true;

	// Properties
	public int Damage;
	public float Force;
	public GameObject Owner;

	// Unity Methods
	public void OnEnable()
	{
		firstFrame = true;
		applyDamage = true;
	}

    public void LateStart()
    {
        GameObject flash = GameControl.ProjectilePool.Spawn( "Explosion Flash" );
        if( flash != null )
        {
            flash.transform.position = transform.position;
            flash.transform.rotation = transform.rotation;
            SphereCollider sc = GetComponent<SphereCollider>();
            if( sc )
            {
                float r = sc.radius * 2;
                flash.transform.localScale = new Vector3( r, r, r );
            }
        }
    }

	void FixedUpdate()
	{
		// Destroy the game object when the particle system finishes running
		if( GetComponent<ParticleSystem>() != null )
		{
			if( GetComponent<ParticleSystem>().isStopped )
			{
				GameControl.ProjectilePool.Despawn( gameObject );
			}
		}

		if( firstFrame )
        {
			firstFrame = false;
		}
		else
		{
			applyDamage = false;
		}
	}

	void OnTriggerEnter( Collider col )
	{
		if( applyDamage && GetComponent<Collider>() != null )
		{
			// If coming into contact with a rigidbody collider, blast it away
			SphereCollider sc = (SphereCollider)GetComponent<Collider>();

			if( col.GetComponent<Rigidbody>() != null )
			{
				col.GetComponent<Rigidbody>().AddExplosionForce( Force, transform.position, sc.radius );
				object[] args = { Damage, Owner };
				col.gameObject.SendMessageUpwards( "ApplyDamage", args, SendMessageOptions.DontRequireReceiver );
			}
		}
	}
}
