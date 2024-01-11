namespace PickupInfo
{
	public enum Type
	{
		None = 0,
		Rocket = 1,
		Mortar = 2,
		Seeker = 3,
		ShotgunStorm = 4,
		SlagCannon = 5
	}

	static class Properties
	{
		public static float RotatePerSecond = 360f;
		public static float Gravity = 1f;
		public static float TerrainCollisionRadius = 1.5f;
		public static float SmallValue = .01f;
	}
}