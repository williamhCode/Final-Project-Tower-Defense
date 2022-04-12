using System;
using TowerDefense.NoiseTest;
using Microsoft.Xna.Framework;
using TowerDefense.Maths;

namespace TowerDefense.NoiseTest
{
    public class Noise
    {   
        
        public float[] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, int persistance, int lacunarity, Vector2 offset)
        {
            float[] noiseMap = new float[mapWidth * mapHeight];
            var random = new System.Random(seed);

            if(octaves < 1)
            {
                octaves = 1;
            }

            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = random.Next(-100000, 100000) + offset.X;
                float offsetY = random.Next(-100000, 100000) + offset.Y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            if(scale <= 0f)
            {
                scale = 0.0001f;
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;

            OpenSimplexNoise openSimplexNoise = new OpenSimplexNoise(seed);

            for(int x = 0, y; x < mapWidth; x++)
            {
                for(y = 0; y < mapHeight; y++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for(int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].X;
                        float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].Y;

                        double perlineValue = openSimplexNoise.Evaluate(sampleX, sampleY) * 2 - 1;

                        noiseHeight += (float)perlineValue * amplitude;
                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if(noiseHeight < maxNoiseHeight)
                        maxNoiseHeight = noiseHeight;
                    else if(noiseHeight > minNoiseHeight)
                        minNoiseHeight = noiseHeight;

                    noiseMap[y * mapWidth + x] = noiseHeight;
                }
            }

            for(int x = 0, y; x < mapWidth; x++)
            {
                for(y = 0; y < mapHeight; y++)
                {
                    noiseMap[y * mapWidth + x] = MathFuncs.InvLerp(minNoiseHeight, maxNoiseHeight, noiseMap[y * mapWidth + x]);
                }
            }

            return noiseMap;
        }
    }
}