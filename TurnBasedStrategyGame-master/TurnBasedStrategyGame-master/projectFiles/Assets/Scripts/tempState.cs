using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempState : MonoBehaviour
{
    //Sean

    // Define the dimensions of your 4D array
    int sizeX = 10; // Board X
    int sizeY = 10; // Board Y
    int unitOnBoard = 2; // Unit on board
    int unitStatus = 2; // Unit status

    int[][][][] my4DArray;

    // Start is called before the first frame update
    void Start()
    {
        InitializeArray();
    }

    void InitializeArray()
    {
        // Create array
        my4DArray = new int[sizeX][][][];

        // Initialize the array
        for (int i = 0; i < sizeX; i++)
        {
            my4DArray[i] = new int[sizeY][][];

            for (int j = 0; j < sizeY; j++)
            {
                my4DArray[i][j] = new int[unitOnBoard][];

                for (int k = 0; k < unitOnBoard; k++)
                {
                    my4DArray[i][j][k] = new int[unitStatus];
                }
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
