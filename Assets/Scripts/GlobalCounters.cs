using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalCounters : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        var totalPreys = GameObject.FindGameObjectsWithTag("Bunny").Length;
        var totalPredators = GameObject.FindGameObjectsWithTag("Fox").Length;
        var totalPlants = GameObject.FindGameObjectsWithTag("Plant").Length;
        var totalTrees = GameObject.FindGameObjectsWithTag("Tree").Length;
        
        this.gameObject.GetComponent<Text>().text = $"Preys: {totalPreys}  Predators: {totalPredators}  Trees: {totalTrees}  Plants: {totalPlants}";
    }
}
