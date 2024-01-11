using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WheelParticles : MonoBehaviour
{
	// Properties
	public InputWrapper InputWrapper { get; set; }

	// Private
	MarbleMovement marbleScript;
	int terrainMask;
	bool gameActive = true;

	// Public
	public Transform DustParticles;
	public float SpinSpeedFactor = .2f;
	public float ParticleSpinDeadzone = .25f;
	public List<Transform> JumpJets;
	public List<Transform> DropJets;
	public List<Transform> JumpBursts;
	public List<Transform> DropBursts;
	public List<Transform> DashJets;
	public List<Transform> DashBursts;
	public bool JumpJetsEnabled = false;
	public bool DropJetsEnabled = false;

	// Unity Methods
	void Awake()
	{
		marbleScript = gameObject.GetComponent<MarbleMovement>();
		terrainMask = 1 << LayerMask.NameToLayer( "Terrain" );
	}
	
	void OnEnable()
    {
        gameActive = true;
	}
	
	void Update()
	{
		if( gameActive )
		{
			// Dust Particles
			if( marbleScript.Grounded )
			{
				// Raycast UV and get texture colour
				Ray colorRay = new Ray( transform.position, Vector3.Normalize( marbleScript.GroundPoint - transform.position ) );
				RaycastHit rayHit;
				Color dustColor = Color.black;
				if( Physics.Raycast( colorRay, out rayHit, 5000.0f, terrainMask ) )
				{
					Material terrainMaterial = Terrain.activeTerrain.materialTemplate;
					Texture2D terrainTexture = (Texture2D)terrainMaterial.GetTexture( "_Base" );
					Vector2 scale = terrainMaterial.GetTextureScale( "_Base" );
					Vector2 offset = terrainMaterial.GetTextureOffset( "_Base" );
					dustColor = terrainTexture.GetPixelBilinear( rayHit.textureCoord.x * scale.x + offset.x, rayHit.textureCoord.y * scale.y + offset.y );
					dustColor = Color.Lerp( dustColor, new Color( 1, 1, 1 ), .25f );
					dustColor.a = 1f;
				}

				// Dust Particles
				DustParticles.GetComponent<ParticleSystem>().startSpeed = marbleScript.Marble.GetComponent<Rigidbody>().angularVelocity.magnitude * SpinSpeedFactor;
				DustParticles.GetComponent<ParticleSystem>().startColor = dustColor;

				if( marbleScript.Marble.GetComponent<Rigidbody>().angularVelocity.magnitude > ParticleSpinDeadzone )
				{
					if( !DustParticles.GetComponent<ParticleSystem>().isPlaying )
					{
						DustParticles.GetComponent<ParticleSystem>().Play();
					}
				}
				else
				{
					if( DustParticles.GetComponent<ParticleSystem>().isPlaying )
					{
						DustParticles.GetComponent<ParticleSystem>().Stop();
					}
				}
			}
			else
			{
				if( DustParticles.GetComponent<ParticleSystem>().isPlaying )
				{
					DustParticles.GetComponent<ParticleSystem>().Stop();
				}
			}

			// Dash jets
			foreach( Transform jet in DashJets )
			{
				float df = GetComponent<Avatar>().Dash / GetComponent<Avatar>().MaxDash;
				jet.GetComponent<ParticleSystem>().emissionRate = df * 100;
				jet.GetComponent<ParticleSystem>().startSize = df * .4f;
			}

			// Enable the jump jets if the button is pressed
			if( InputWrapper.Jump.Pressed && marbleScript.JumpFired == false )
			{
				ParticleBurst( true );
			}

			if( InputWrapper.Jump.Down && !JumpJetsEnabled )
			{
				SetJetsEnabled( true, true );
			}

			if( !InputWrapper.Jump.Down && JumpJetsEnabled )
			{
				// Otherwise, deactivate them
				SetJetsEnabled( true, false );
			}

			// Enable the drop jets if the button is pressed
			if( InputWrapper.Drop.Pressed && marbleScript.DropFired == false )
			{
				ParticleBurst( false );
			}

			if( InputWrapper.Drop.Down && !DropJetsEnabled )
			{
				SetJetsEnabled( false, true );
			}
			
			if( !InputWrapper.Drop.Down && DropJetsEnabled )
			{
				SetJetsEnabled( false, false );
			}
		}
	}

	// Utility Methods
	public void GameOver()
	{
		gameActive = false;
	}

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
				jet.GetComponent<ParticleSystem>().Play();
			}
			else
			{
				jet.GetComponent<ParticleSystem>().Stop();
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
			burst.GetComponent<ParticleSystem>().Play();
		}
	}

	public void DashBurst()
	{
		foreach( Transform burst in DashBursts )
		{
			burst.GetComponent<ParticleSystem>().Play();
		}
	}
}
