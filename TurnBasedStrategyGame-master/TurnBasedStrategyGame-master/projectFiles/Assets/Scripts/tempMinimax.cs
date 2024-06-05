//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//// Minimax untuk melihat kemungkinan state yang dia bisa bergerak untuk masing2 pionnya, trs dihitung fitness function terbaik dia untuk bergerak


//public class tempMinimax : MonoBehaviour
//{
//    List<int[][]> map;
//    tempGenerateAllState tempGenerateAllState;
//    tempFitnessFunction tempFitnessFunction;
//    tileMapScript tileMapScript;

//    // Start is called before the first frame update

//    int[][][][] startState =
//{
//    new int[][][] { new int[][] { new int[] { 1 }, new int[] { } },
//                     new int[][] { new int[] { 2 }, new int[] { } },
//                     new int[][] { new int[] { 3 }, new int[] { } } },

//    new int[][][] { new int[][] { new int[] { 0 }, new int[] { } },
//                     new int[][] { new int[] { 0 }, new int[] { } },
//                     new int[][] { new int[] { 0 }, new int[] { } } },

//    new int[][][] { new int[][] { new int[] { -2 }, new int[] { } },
//                     new int[][] { new int[] { -1 }, new int[] { } },
//                     new int[][] { new int[] { -3 }, new int[] { } } }
//     };

//    public bool isPrune(List<int[][][][]> states, int[][][][] cur_state) // Level yang sama
//    {
//        foreach (int[][][][] s in states)
//        {
//            if (is3DArraySamePos(s, cur_state))
//            {
//                if (isAttResSame(s, cur_state)) return true;
//            }
//        }
//        return false;
//    }

//    public bool is3DArraySamePos(int[][][][] a, int[][][][] b)
//    {
//        for (int i = 0; i < a.Length; i++)
//        {
//            for (int j = 0; j < a[i].Length; j++)
//            {
//                if (a[i][j][0][0] != b[i][j][0][0])
//                {
//                    return false;
//                }
//            }
//        }
//        return true;
//    }

//    public bool isAttResSame(int[][][][] a, int[][][][] b)
//    {
//        for (int i = 0; i < a.Length; i++)
//        {
//            for (int j = 0; j < a[i].Length; j++)
//            {
//                if (a[i][j][1].Length != 0)
//                {
//                    int x = a[i][j][1][0];
//                    int y = a[i][j][1][1];

//                    if (a[x][y][0][0] == 0)
//                    {
//                        int k = b[i][j][1][0];
//                        int l = b[i][j][1][1];

//                        if (b[k][l][0][0] == 0) return true;
//                    }
//                    else
//                    {
//                        return false;
//                    }
//                }
//            }
//        }

//        for (int i = 0; i < b.Length; i++)
//        {
//            for (int j = 0; j < b[i].Length; j++)
//            {
//                if (b[i][j][1].Length != 0)
//                {
//                    return false;
//                }
//            }
//        }
//        return true;
//    }
//    void Start()
//    {
//        //
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }

//    int Minimax(int [][][][] currState, int[][] map, int depth, bool maximizingPlayer, List<int[][][][]> currentLevelState, bool isRoot)
//    {
//        if (depth == 0 || isPrune(currentLevelState, currState) || !isRoot)
//        {
//            return tempFitnessFunction.Evaluate();
//        }

//        List<int [][][][]> possibleMoves = tempGenerateAllState.generateAllState(startState, map, 3);
//        if (maximizingPlayer)
//        {
//            int maxEval = int.MinValue;
//            foreach (int[][][][] state in possibleMoves)
//            {
//                int[][] tile = new int[11][11];
//                tile.ApplyMove(move);
//                int eval = Minimax(currState, map, depth -1, maximizingPlayer, currentLevelState, isRoot);
//                maxEval = Mathf.Max(maxEval, eval);         
//            }
//            return maxEval;
//        }
//        else
//        {
//            int minEval = int.MaxValue;
//            foreach (int[][][][] state in possibleMoves)
//            {
//                ChessBoard simulatedBoard = new ChessBoard(board);
//                simulatedBoard.ApplyMove(move);
//                int eval = Minimax(currState, map, depth, maximizingPlayer, currentLevelState, isRoot);
//                minEval = Mathf.Min(minEval, eval);
//            }
//            return minEval;
//        }
//    }
//}
