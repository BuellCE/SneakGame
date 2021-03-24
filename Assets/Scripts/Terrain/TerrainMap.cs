using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMap{

    public float[,] FillMap(int width, int height, float scaling, int[] offset) {

        float[,] map = new float[width, height];

        for (int w = 0; w < width; w++) {
            for (int h = 0; h < height; h++) {

                float point = Mathf.PerlinNoise((w * scaling) + offset[0], (h * scaling) + offset[1]);

                map[w, h] = point;
            }
        }
        return map;
    }


}
