using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempFitnessFunction : MonoBehaviour
{
    //Sean
    public tileMapScript MAP;
    public gameManagerScript GMS;
    public int EvaluatePosition(int[][][][] sourceArray)
    {
        int AIUnits = 0;
        int AITotalHP = 0;
        int opponentUnits = 0;
        int opponentTotalHP = 0;
        int positionalAdvantage = 0;

        int[,] positionWeights = new int[10, 10]
        {
            { 1, 2, 2, 2, 2, 2, 2, 2, 2, 1},
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
            { 2, 2, 0, 0, 2, 2, 0, 0, 2, 2},
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
            { 2, 2, 0, 0, 2, 2, 0, 0, 2, 2},
            { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
            { 1, 2, 2, 2, 2, 2, 2, 2, 2, 1},
        };

        int[][][][] destinationArray = new int[10][][][];

        for (int i = 0; i < 10; i++)
        {
            destinationArray[i] = new int[10][][];
            for (int j = 0; j < 10; j++)
            {
                destinationArray[i][j] = new int[2][];
                for (int k = 0; k < 2; k++)
                {
                    destinationArray[i][j][k] = new int[2];
                    for (int l = 0; l < 2; l++)
                    {
                        destinationArray[i][j][k][l] = sourceArray[i][j][k][l];
                        if (destinationArray[i][j][0][0] < 0)
                        {
                            AIUnits++;
                            AITotalHP += destinationArray[i][j][0][1];
                            positionalAdvantage += positionWeights[i, j];
                        }
                        else
                        {
                            opponentUnits++;
                            opponentTotalHP += destinationArray[i][j][0][1];
                            positionalAdvantage -= positionWeights[i, j];
                        }
                    }
                }
            }
        }

        int AIScore = AIUnits * 10 + AITotalHP;
        int opponentScore = opponentUnits * 10 + opponentTotalHP;
        int positionalScore = positionalAdvantage;
        //int unitImportanceScore = jumlah total unit berdasarkan value

        return AIScore - opponentScore + positionalScore; // + unitImportanceScore
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
