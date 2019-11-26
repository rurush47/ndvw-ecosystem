using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TerrainGeneration;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class GameObjectFloatDict : SerializableDictionary<GameObject, float> { }
public class ObjectSpawner : MonoBehaviour
{
    public GameObjectFloatDict spawnWeightDictionary;

    public void SpawnObjects(List<TileData> tileData)
    {
        foreach (var data in tileData)
        {
            if (data.biome.type == TerrainGenerator.BiomeType.Water)
            {
                continue;
            }

            GameObject newObj = GetRouletteSpinObj(spawnWeightDictionary);

            if (newObj == null)
            {
                continue;
            }
            
            Instantiate(newObj, data.worldPos, Quaternion.identity);
        }
    }

    public T GetRouletteSpinObj<T>(IDictionary<T, float> dict)
    {
        float sum = 0;

        var valuesDict = new Dictionary<float, T>();
        foreach (var kvp in dict)
        {
            sum += kvp.Value;
            valuesDict.Add(sum, kvp.Key);
        }
        
        float randomVal = Random.Range(0, sum);

        foreach (var kvp in valuesDict)
        {
            if (randomVal <= kvp.Key)
            {
                return kvp.Value;
            }
        }

        return default(T);
    }
}
