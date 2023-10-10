namespace Endciv
{
    public static class Random
    {
        public static int Range(int min, int max)
            => UnityEngine.Random.Range(min, max);
        public static float Range(float min, float max)
            => UnityEngine.Random.Range(min, max);
    }
}