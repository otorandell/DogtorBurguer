namespace DogtorBurguer
{
    public static class Rng
    {
        private static readonly System.Random _random = new();

        public static int Range(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }

        public static float Range(float min, float max)
        {
            return min + (float)_random.NextDouble() * (max - min);
        }

        public static float Value => (float)_random.NextDouble();
    }
}
