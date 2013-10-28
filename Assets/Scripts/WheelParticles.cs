using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WheelParticles : MonoBehaviour
{
	// Fields
	MarbleMovement marbleScript;
	InputWrapper inputWrapper;
	int terrainMask;
	float prevJump = 0f;
	float prevDrop = 0f;

	// Properties
	public Transform DustParticles;
	public float ParticleStickDeadzone = .25f;
	public List<Transform> JumpJets;
	public List<Transform> DropJets;
	public List<Transform> JumpBursts;
	public List<Transform> DropBursts;
	public bool JumpJetsEnabled = false;
	public bool DropJetsEnabled = false;

	// Unity Methods
	void Awake()
	{
		marbleScript = gameObject.GetComponent<MarbleMovement>();
		inputWrapper = gameObject.GetComponent<InputWrapper>();
		terrainMask = 1 << LayerMask.NameToLayer( "Terrain" );
	}
	
	void Start()
	{
	}
	
	void LateUpdate()
	{
		if( marbleScript.Grounded )
		{
			// Raycast UV and get texture colour
			Ray colorRay = new Ray( transform.position, Vector3.Normalize( marbleScript.GroundPoint - transform.position ) );
			RaycastHit rayHit;
			Color dustColor = Color.black;
			if( Physics.Raycast( colorRay, out rayHit, 5000.0f, terrainMask ) )
			{
				Texture2D terrainTexture = Terrain.activeTerrain.terrainData.splatPrototypes[ 0 ].texture;
				dustColor = terrainTexture.GetPixelBilinear( rayHit.textureCoord.x, rayHit.textureCoord.y );
				dustColor *= .5f;
				dustColor.a = 1f;
			}

			// Particles
			Vector3 heading = new Vector3( inputWrapper.LeftStick.x, 0f, inputWrapper.LeftStick.y );
			DustParticles.particleSystem.startSpeed = heading.magnitude * 5f;
			DustParticles.particleSystem.startColor = dustColor;

			if( heading.magnitude > ParticleStickDeadzone )
			{
				if( !DustParticles.particleSystem.isPlaying )
				{
					DustParticles.particleSystem.Play();
				}
			}
			else
			{
				if( DustParticles.particleSystem.isPlaying )
				{
					DustParticles.particleSystem.Stop();
				}
			}
		}
		else
		{
			if( DustParticles.particleSystem.isPlaying )
			{
				DustParticles.particleSystem.Stop();
			}
		}

		// Enable the jump jets if the button is pressed
		if( inputWrapper.Jump == 1f )
		{
			if( !JumpJetsEnabled )
			{
				SetJetsEnabled( true, true );
			}

			if( marbleScript.JumpFired == false && prevJump == 0f )
			{
				ParticleBurst( true );
			}
		}
		else
		{
			// Otherwise, deactivate them
			if( JumpJetsEnabled )
			{
				SetJetsEnabled( true, false );
			}
		}
		prevJump = inputWrapper.Jump;

		// Enable the drop jets if the button is pressed
		if( inputWrapper.Drop == 1f )
		{
			if( !DropJetsEnabled )
			{
				SetJetsEnabled( false, true );
			}

			if( marbleScript.DropFired == false && prevDrop == 0f )
			{
				ParticleBurst( false );
			}
		}
		else
		{
			// Otherwise, deactivate them
			if( DropJetsEnabled )
			{
				SetJetsEnabled( false, false );
			}
		}
		prevDrop = inputWrapper.Drop;
	}

	// Utility Methods
	public void SetJetsEnabled( bool jump, bool enabled )
	{
		List<Transform> jets;
		if( jump )
		{
			jets = JumpJets;
			JumpJetsEnabled = enabled;
		}
		else
		{
			jets = DropJets;
			DropJetsEnabled = enabled;
		}

		foreach( Transform jet in jets )
		{
			if( enabled )
			{
				jet.particleSystem.Play();
			}
			else
			{
				jet.particleSystem.Stop();
			}
		}
	}

	public void ParticleBurst( bool jump )
	{
		List<Transform> bursts;
		if( jump )
		{
			bursts = JumpBursts;
		}
		else
		{
			bursts = DropBursts;
		}

		foreach( Transform burst in bursts )
		{
			if( enabled )
			{
				burst.particleSystem.Play();
			}
			else
			{
				burst.particleSystem.Stop();
			}
		}
	}
}
