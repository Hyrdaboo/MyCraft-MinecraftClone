using UnityEngine;


[CreateAssetMenu(menuName = "HeightMap")]
public class MapGenerator : ScriptableObject
{
    public Noise.NormalizeMode normalizeMode = Noise.NormalizeMode.Global;
    public int width = 18;
    public int seed = 27;
    public float scale = 500f;
    [Range(1f, 5f)]
    public int octaves = 4;
    [Range(0, 1f)]
    public float persistance = 0.29f;
    public float lacunarity = 3.14f;
    public float hilliness = 130;

    public float[,] GenerateHeightMap(Vector2 offset)
    {
        float[,] heightMap1 = Noise.GenerateNoiseMap(width, width, seed, scale, octaves, persistance, lacunarity, offset, normalizeMode);
        float[,] heightMap2 = Noise.GenerateNoiseMap(width, width, seed, scale, octaves, persistance*2, lacunarity/2, offset, normalizeMode);
        float[,] heightMap3 = Noise.GenerateNoiseMap(width, width, seed, scale, octaves, persistance*4, lacunarity/4, offset, normalizeMode);

        if (hilliness >= 255) hilliness = 255;
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heightMap1[x, y] *= heightMap2[x, y]+heightMap3[x, y];
                heightMap1[x, y] = Mathf.Clamp(heightMap1[x, y], 0, 1);
                heightMap1[x, y] *= hilliness;
            }
        }

        return heightMap1;
    }

    public float Generate3D(FastNoise fn, Vector2 offset, int x, int y, int z, float frequency)
    {
        
        if (y == 0 || y == 1) return 1;
        fn.SetNoiseType(FastNoise.NoiseType.Simplex);
        fn.SetFrequency(frequency/10);
        
        fn.SetSeed(seed);
        float noiseValue = fn.GetNoise((x + offset.x), y, (z - offset.y));
        return noiseValue;
    }

    public float GenerateMask(FastNoise fn, Vector2 offset, int x, int y)
    {
        fn.SetNoiseType(FastNoise.NoiseType.Perlin);
        
        fn.SetSeed(seed);
        float noiseValue = fn.GetNoise((x + offset.x), (y - offset.y));
        return noiseValue;
    }
}
