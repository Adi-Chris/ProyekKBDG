using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Minimax untuk melihat kemungkinan state yang dia bisa bergerak untuk masing2 pionnya, trs dihitung fitness function terbaik dia untuk bergerak


public class tempMinimax : MonoBehaviour
{
    List<int[][]> map;
    tempGenerateAllState tempGenerateAllState;
    tempFitnessFunction tempFitnessFunction;
    tileMapScript tileMapScript;

    // Start is called before the first frame update
    void Start()
    {
        //
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
