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
	float prevJump = 0f;
	float prevDrop = 0f;

	// Public
	public Transform DustParticles;
	public Transform ChargeParticles;
	public float ParticleStickDeadzone = .25f;
	public float ChargeEmissionVelocityFactor = 2f;
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
	
	void Start()
	{
	}
	
	void LateUpdate()
	{
		// Ground Particles
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

			// Dust Particles
			Vector3 heading = new Vector3( InputWrapper.LeftStick.x, 0f, InputWrapper.LeftStick.y );
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

			// Charge particles
			if( transform.Find( "Marble" ).rigidbody.angularVelocity.magnitude > 0f )
			{
				if( !ChargeParticles.particleSystem.isPlaying )
				{
					ChargeParticles.particleSystem.Play();
				}
				float dashFactor = GetComponent<Avatar>().Dash / GetComponent<Avatar>().MaxDash;
				ChargeParticles.particleSystem.emissionRate = transform.Find( "Marble" ).rigidbody.angularVelocity.magnitude * ChargeEmissionVelocityFactor * dashFactor;
			}
			else
			{
				if( ChargeParticles.particleSystem.isPlaying )
				{
					ChargeParticles.particleSystem.Stop();
				}
			}
		}
		else
		{
			if( DustParticles.particleSystem.isPlaying )
			{
				DustParticles.particleSystem.Stop();
			}

			if( ChargeParticles.particleSystem.isPlaying )
			{
				ChargeParticles.particleSystem.Stop();
			}
		}

		// Dash jets
		foreach( Transform jet in DashJets )
		{
			float dashFactor = GetComponent<Avatar>().Dash / GetComponent<Avatar>().MaxDash;
			jet.particleSystem.emissionRate = dashFactor * 100;
			jet.particleSystem.startSize = dashFactor * .3f;
		}

		// Enable the jump jets if the button is pressed
		if( InputWrapper.Jump == 1f )
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
		prevJump = InputWrapper.Jump;

		// Enable the drop jets if the button is pressed
		if( InputWrapper.Drop == 1f )
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
		prevDrop = InputWrapper.Drop;
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
			burst.particleSystem.Play();
		}
	}

	public void DashBurst()
	{
		foreach( Transform burst in DashBursts )
		{
			burst.particleSystem.Play();
		}
	}
}
