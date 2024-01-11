namespace DroneInfo
{
	// Enums
	public enum Type
	{
		None = 0,
		Rocket = 1,
		Mortar = 2,
		Seeker = 3,
		ShotgunStorm = 4,
		SlagCannon = 5
	}

	public static class Rocket
	{
		public static float HoverSpeed = 6f;
	}

	public static class Mortar
	{
		public static int AimLineSubdivisions = 10;
	}
}