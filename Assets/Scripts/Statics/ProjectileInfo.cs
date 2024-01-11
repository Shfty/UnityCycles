namespace ProjectileInfo
{
	// Enums
	public enum Type
	{
		None = 0,
		Rocket = 1,
		MortarShell = 2,
		MortarBomb = 3,
		SeekerMissile = 4,
		ShotgunPellet = 5,
		SlagBall = 6
	}

	namespace Properties
	{
		static class Rocket
		{
			public static float InitialForce = 40f;
			public static float Accelleration = 20f;
			public static float ExplosionRadius = 10f;
		}
		static class MortarShell
		{
			public static float TargetYOffset = 25f;
			public static float InitialForce = 80f;
			public static float InitialTorque = 5f;
			public static float Timeout = 3f;
		}
		static class MortarBomb
		{
			public static float InitialForce = 20f;
		}
		static class Seeker
		{
			public static float SeekDelay = .5f;
			public static float InitialForce = 40f;
			public static float Accelleration = 2.5f;
		}
		static class ShotgunPellet
		{
			public static float InitialForce = 40f;
		}
		static class SlagBall
		{
			public static float InitialForce = 50f;
		}
	}
}