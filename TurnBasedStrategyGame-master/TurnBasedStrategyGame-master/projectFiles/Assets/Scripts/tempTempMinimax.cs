using System.Collections;
using System.Collections.Generic;
using 
using UnityEngine;

public class tempTempMinimax : MonoBehaviour
{
    private const int MaxDepth = 3;
    private tempGenerateAllState generate;

    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int MinimaxAlgorithm(int[][][][] state, int depth, bool maximizingPlayer, int[][] map, int startUnitAmount)
    {
        if (depth == 0)
        {
            return EvaluateBoard(state);
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            List<int[][][][]> allStatesMax = generate.generateAllStateMax(state, map, startUnitAmount);
            foreach (var childState in allStatesMax)
            {
                int eval = MinimaxAlgorithm(childState, depth - 1, false, map, startUnitAmount);
                maxEval = Mathf.Max(maxEval, eval);
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            List<int[][][][]> allStatesMin = generate.generateAllStateMin(state, map, startUnitAmount);
            foreach (var childState in allStatesMin)
            {
                int eval = MinimaxAlgorithm(childState, depth - 1, true, map, startUnitAmount);
                minEval = Mathf.Min(minEval, eval);
            }
            return minEval;
        }
    }

    private bool IsGameOver(int[][][][] state)
    {
        // Implement logic to determine if the game is over.
        return false;
    }

    private int EvaluateBoard(int[][][][] state)
    {
        // Implement your board evaluation logic here.
        int score = 0;
        // Example: Evaluate based on material count
        for (int y = 0; y < state.Length; y++)
        {
            for (int x = 0; x < state[y].Length; x++)
            {
                int unitType = state[y][x][0][0];
                int unitHP = state[y][x][0][1];
                if (unitType > 0)
                {
                    score += unitHP; // Positive score for AI units
                }
                else if (unitType < 0)
                {
                    score -= unitHP; // Negative score for Human units
                }
            }
        }
        return score;
    }
}
