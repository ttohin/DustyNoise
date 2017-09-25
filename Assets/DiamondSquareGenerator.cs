using System;
using System.Collections;
using System.Collections.Generic;
using Common;

public class DiamondSquareGenerator {

    public Buffer<float> data;
    private Random random;
    private float roughness;
    private float randDelta;
    private bool enableOverflow;
    public DiamondSquareGenerator (int width, int height, float roughness, float randDelta, bool enableOverflow) {
        data = new Buffer<float> (width, height);
        random = new Random (Guid.NewGuid ().GetHashCode ());
        this.roughness = roughness;
        this.randDelta = randDelta;
        this.enableOverflow = enableOverflow;
    }

    public void Generate () {
        data.Fill (0);
        for (int l = (int) Math.Floor (data.width / 1.0f); l > 0; l = (int) Math.Floor (l / 2.0f)) {
            for (int i = 0; i < data.width; i += l) {
                for (int j = 0; j < data.height; j += l) {
                    SquareDiamond (i, j, l, l);
                }
            }
        }

        float max = data.Get(0, 0);
        float min = data.Get(0, 0);
        data.ForEach((value, x, y) => {
            if (value > max)
                max = value;
            if (value < min)
                min = value;
        });

        data.ForEach((value, x, y) => {
            float normilizedValue = (value - min) / (max - min);
            data.Set(normilizedValue, x, y);
        });

    }

    void SquareDiamond (int x, int y, int xBlockSize, int yBlockSize) {
        int xBlockMiddele = (int) Math.Floor ((float) xBlockSize / 2.0f);
        int yBlockMiddele = (int) Math.Floor ((float) yBlockSize / 2.0f);

        Square (x + xBlockMiddele, y + yBlockMiddele, xBlockMiddele, yBlockMiddele);
        Diamond (x, y + yBlockMiddele, xBlockMiddele, yBlockMiddele);
        Diamond (x + xBlockMiddele, y, xBlockMiddele, yBlockMiddele);
        Diamond (x + xBlockSize, y + yBlockMiddele, xBlockMiddele, yBlockMiddele);
        Diamond (x + xBlockMiddele, y + yBlockSize, xBlockMiddele, yBlockMiddele);
    }

    float Displace (float value, int blockSize, float roughness) {
        return value + ((float) random.NextDouble () + randDelta) * 2 * blockSize / data.width * roughness;
    }

    void Square (int x, int y, int xBlockSize, int yBlockSize) {
        float numberOfValus = 0;
        float valueAccumulator = 0;
        float value = 0;
        if (data.Get (x - xBlockSize, y - yBlockSize, out value)) {
            valueAccumulator += value;
            numberOfValus += 1;
        }
        if (data.Get (x + xBlockSize, y - yBlockSize, out value)) {
            valueAccumulator += value;
            numberOfValus += 1;
        }
        if (data.Get (x - xBlockSize, y + yBlockSize, out value)) {
            valueAccumulator += value;
            numberOfValus += 1;
        }
        if (data.Get (x + xBlockSize, y + yBlockSize, out value)) {
            valueAccumulator += value;
            numberOfValus += 1;
        }

        float result = Displace (valueAccumulator / numberOfValus, xBlockSize + yBlockSize, roughness);
        data.Set (result, x, y);

    }
void Diamond(int x, int y, int xBlockSize, int yBlockSize)
{
  float numberOfValus = 0;
  float valueAccumulator = 0;
  float value = 0;
  if (data.Get(x - xBlockSize, y, out value))
  {
    valueAccumulator += value;
    numberOfValus += 1;
  }
  if (data.Get(x + xBlockSize, y, out value))
  {
    valueAccumulator += value;
    numberOfValus += 1;
  }
  if (data.Get(x, y - yBlockSize, out value))
  {
    valueAccumulator += value;
    numberOfValus += 1;
  }
  if (data.Get(x, y + yBlockSize, out value))
  {
    valueAccumulator += value;
    numberOfValus += 1;
  }
  
  float result = Displace(valueAccumulator / numberOfValus, xBlockSize + yBlockSize, roughness);
  data.Set(result, x, y);
}
}