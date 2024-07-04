using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class tempTempMinimax : MonoBehaviour
{
    private const int MaxDepth = 3;
    [SerializeField] tempGenerateAllState generate;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public int[][][][] MinimaxAlgorithm(int[][][][] state, int depth, bool maximizingPlayer, int[][] map, int startUnitAmount, int alpha, int beta)
    {
        if (depth == 0)
        {
            return state; // Return the current state at leaf nodes or terminal states
        }

        int[][][][] bestState = null;

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            List<int[][][][]> allStatesMax = generate.generateAllStateMax(state, map, startUnitAmount);
            foreach (var childState in allStatesMax)
            {
                int[][][][] evalState = MinimaxAlgorithm(childState, depth - 1, false, map, startUnitAmount, alpha, beta);
                int eval = EvaluateBoard(evalState);
                if (eval > maxEval)
                {
                    maxEval = eval;
                    bestState = childState;
                }
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha)
                {
                    break; // Beta cut-off
                }
            }
        }
        else
        {
            int minEval = int.MaxValue;
            List<int[][][][]> allStatesMin = generate.generateAllStateMin(state, map, startUnitAmount);
            foreach (var childState in allStatesMin)
            {
                int[][][][] evalState = MinimaxAlgorithm(childState, depth - 1, true, map, startUnitAmount, alpha, beta);
                int eval = EvaluateBoard(evalState);
                if (eval < minEval)
                {
                    minEval = eval;
                    bestState = childState;
                }
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha)
                {
                    break; // Alpha cut-off
                }
            }
        }

        return bestState;
    }

    private bool IsGameOver(int[][][][] state)
    {
        // Implement logic to determine if the game is over.
        return false;
    }

    private int EvaluateBoard(int[][][][] state)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{\n");
        for (int i = 0; i < 10; i++)
        {
            
            sb.Append("{");
            for (int j = 0; j < 10; j++)
            {
                
                sb.Append("{{" + state[i][j][0][0] + "},{");
                if (state[i][j][1].Length > 1) {
                    sb.Append(state[i][j][1][0] + ", " + state[i][j][1][1] + "}}\t");
                } else {
                    sb.Append("}}\t");
                }
            }
            sb.Append("}\n");
        }
        sb.Append("\n}");
        Debug.Log("BOARDDD masuk Eval:");
        Debug.Log(sb.ToString());

        int score = 0;
        sb = new StringBuilder();

        // Iterate through the board and evaluate each unit
        for (int y = 0; y < state.Length; y++)
        {
            for (int x = 0; x < state[y].Length; x++)
            {
                int unitType = state[y][x][0][0];
                int unitHP = state[y][x][0][1];
                int unitAttack = state[y][x][0][2];
                sb.Append(unitType + ", ");

                if (unitType != 0) // If there is a unit. If not, no
                {
                    int unitScore = 0;

                    string unitName = GetUnitName(unitType);

                    // Calculate the unit's score based on its type
                    if (unitHP > 0)
                    {
                        if (string.Equals(unitName, "Giga Mungus"))
                        {
                            unitScore = (unitHP + unitAttack) * 10;
                        }
                        else if (string.Equals(unitName, "Skeleton Archer") || string.Equals(unitName, "Skeleton Archer Bald"))
                        {
                            unitScore = (unitHP + unitAttack) * 2;
                        }
                        else
                        {
                            unitScore = unitHP + unitAttack;
                        }

                        // Add to the total score based on whether the unit belongs to AI or Human
                        if (unitType > 0) // AI unit
                        {
                            score += unitScore;
                        }
                        else if (unitType < 0) // Human unit
                        {
                            score -= unitScore;
                        }
                    }
                }
            }
            sb.Append("\n");
        }
        Debug.Log("Score: " + score + "\n" + sb.ToString());
        return score;
    }

    private string GetUnitName(int unitType)
    {
        // Implement logic to return unit name based on its type
        switch (unitType)
        {
            case 1:
                return "Giga Mungus"; // Example type for Giga Mungus
            case 2:
                return "Skeleton Archer"; // Example type for Skeleton Archer
            case 3:
                return "Skeleton Archer Bald"; // Example type for Skeleton Archer Bald
            case 4:
                return "Skeleton Soldier"; // Example type for Skeleton Soldier
            default:
                return "Unknown";
        }
    }

    // Helper function untuk initialize 4d state
    private int[][][][] init4dState()
    {
        int[][][][] state = new int[10][][][];
        for (int i = 0; i < 10; i++)
        {
            state[i] = new int[10][][];
            for (int j = 0; j < 10; j++)
            {
                state[i][j] = new int[2][];
                for (int k = 0; k < 2; k++)
                {
                    state[i][j][k] = new int[2];
                    state[i][j][k][0] = 0;
                }
            }
        }
        return state;
    }
}