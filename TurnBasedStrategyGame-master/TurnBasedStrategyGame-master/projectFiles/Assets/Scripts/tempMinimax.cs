//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//// Minimax untuk melihat kemungkinan state yang dia bisa bergerak untuk masing2 pionnya, trs dihitung fitness function terbaik dia untuk bergerak


//public class tempMinimax : MonoBehaviour
//{
//    tempGenerateAllState tempGenerateAllState;
//    tempFitnessFunction tempFitnessFunction;
//    tempPruning tempPruning;
//    gameManagerScript tempGame;
//    tileMapScript tileMapScript;

//    // Start is called before the first frame update
//    void Start()
//    {
//        Debug.Log("Minimax adalah:" + tempGenerateAllState.generateAllStateMax(tempGame.start_state, tempGenerateAllState.map, 10));
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

//    int Minimax(int[][][][] currState, int[][] map, int depth, bool maximizingPlayer, List<int[][][][]> currentLevelState, bool isRoot)
//    {
//        if (depth == 0 || tempPruning.isPrune(currentLevelState, currState) || !isRoot)
//        {
//            //return tempFitnessFunction.Evaluate();
//        }

//        //List<int[][][][]> possibleMoves = tempGenerateAllState.generateAllState(startState, map, 3);
//        if (maximizingPlayer)
//        {
//            int maxEval = int.MinValue;
//            //foreach (int[][][][] state in possibleMoves)
//            //{
//            //    int[][] tile = new int[11][11];
//            //    //tile.ApplyMove(move);
//            //    int eval = Minimax(currState, map, depth - 1, maximizingPlayer, currentLevelState, isRoot);
//            //    maxEval = Mathf.Max(maxEval, eval);
//            //}
//            return maxEval;
//        }
//        else
//        {
//            int minEval = int.MaxValue;
//            //foreach (int[][][][] state in possibleMoves)
//            {
//                //ChessBoard simulatedBoard = new ChessBoard(board);
//                //simulatedBoard.ApplyMove(move);
//                int eval = Minimax(currState, map, depth, maximizingPlayer, currentLevelState, isRoot);
//                minEval = Mathf.Min(minEval, eval);
//            }
//            return minEval;
//        }
//    }

//    //private int minimax(int[,,,] state, int depth, bool maximizingPlayer, int[][] map, int unitAmount)
//    //{
//    //    if (depth == 0 || isTerminalState(state))
//    //    {
//    //        return evaluateState(state);
//    //    }

//    //    if (maximizingPlayer)
//    //    {
//    //        int maxEval = int.MinValue;
//    //        List<int[,,,]> states = generateAllStateMax(state, map, unitAmount);
//    //        foreach (var child in states)
//    //        {
//    //            int eval = minimax(child, depth - 1, false, map, unitAmount);
//    //            maxEval = Math.Max(maxEval, eval);
//    //        }
//    //        return maxEval;
//    //    }
//    //    else
//    //    {
//    //        int minEval = int.MaxValue;
//    //        List<int[,,,]> states = generateAllStateMin(state, map, unitAmount);
//    //        foreach (var child in states)
//    //        {
//    //            int eval = minimax(child, depth - 1, true, map, unitAmount);
//    //            minEval = Math.Min(minEval, eval);
//    //        }
//    //        return minEval;
//    //    }
//    //}

//    //private int[,,,] bestMove(int[,,,] board, int[][] map, int unitAmount)
//    //{
//    //    int[,,,] startState = boardToState();
//    //    int bestVal = int.MinValue;
//    //    int[,,,] bestState = null;

//    //    List<int[,,,]> states = generateAllStateMax(startState, map, unitAmount);
//    //    foreach (var state in states)
//    //    {
//    //        int moveVal = minimax(state, 3, false, map, unitAmount);
//    //        if (moveVal > bestVal)
//    //        {
//    //            bestVal = moveVal;
//    //            bestState = state;
//    //        }
//    //    }

//    //    return bestState;
//    //}

//    //void Update()
//    //{
//    //    if (isAITurn)
//    //    {
//    //        int[,,,] currentBoard = boardToState();
//    //        int[,,,] newBoard = bestMove(currentBoard, map, unitAmount);
//    //        stateToBoard(newBoard);
//    //        isAITurn = false;
//    //    }
//    //}
//}
