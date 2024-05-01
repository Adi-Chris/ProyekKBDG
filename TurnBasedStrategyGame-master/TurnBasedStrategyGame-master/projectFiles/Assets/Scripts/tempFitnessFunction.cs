using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempFitnessFunction : MonoBehaviour
{
    //Sean

    public tileMapScript map;
    public int EvaluatePosition(int player) //Team 1 atau Team 2
    {
        int playerUnits = 0;
        int playerTotalHP = 0;
        int opponentUnits = 0;
        int opponentTotalHP = 0;

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

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; i=j++)
            {
                if (map.tilesOnMap[i, j].gameObject.CompareTag("Tile"))
                {
                    if (map.tilesOnMap[i, j].GetComponent<ClickableTileScript>().unitOnTile != null)
                    {
                        /*if (unit = player)
                         {
                            playerUnits++;
                            playerTotalHP++;
                            positionalAdvantage += positionWeights[i, j];
                         }
                          else
                         {
                            opponentUnits++;
                            opponentTotalHP++;
                            positionalAdvantage -= positionWeights[i, j];
                         }
                        */
                    }
                }
            }
        }

        int playerScore = playerUnits * 10 + playerTotalHP;
        int opponentScore = opponentUnits * 10 + opponentTotalHP;
        //int positionalScore = positionalAdvantage;
        //int unitImportanceScore = jumlah total unit berdasarkan value

        return playerScore - opponentScore; // + unitImportanceScore + positionalScore
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
