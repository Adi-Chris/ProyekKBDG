using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempGenerateAllState : MonoBehaviour
{
    int[][] map; // Mapnya ini nanti masukin ke Minimax aja, karena toh sama semua mapnya pasti
    // Jika nilainya 0, tidak walkable
    // Jika nilainya 1, berarti walkable
    // Map berformat [x][y]

    // Sementara dihardcode sesuai trello
    int[][][][] startState =
{
    new int[][][] { new int[][] { new int[] { 1 }, new int[] { } },
                     new int[][] { new int[] { 2 }, new int[] { } },
                     new int[][] { new int[] { 3 }, new int[] { } } },

    new int[][][] { new int[][] { new int[] { 0 }, new int[] { } },
                     new int[][] { new int[] { 0 }, new int[] { } },
                     new int[][] { new int[] { 0 }, new int[] { } } },

    new int[][][] { new int[][] { new int[] { -2 }, new int[] { } },
                     new int[][] { new int[] { -1 }, new int[] { } },
                     new int[][] { new int[] { -3 }, new int[] { } } }
};



    private void Start()
    {
        // generateMap(); // Tidak bisa dipakai karena startState sekarang terlalu kecil
        generateMap3By3();

        // generatePossibleMovement(0, 0, startState);


        generateAllStateMax(startState, map, 3);
    }

    public List<int[][][][]> generateAllStateMax(int[][][][] startState, int[][] map, int startUnitAmount)
    {
        List<int[][][][]> successorStates = new List<int[][][][]>();
        int unitAlreadyGenerated = 0;

        // int[y][x][z][a]
        // y menyatakan sumbu kebawah dari grid map
        // x menyatakan sumbu kekanan dari grid map
        // z menyatakan parameter tiap unit
        // z[0]a[0] menyatakan tipe unit , z[0]a[0] dan z[0]a[1] berformat (b, c) yaitu posisi x dan y dari attack unit
        // AWAS!: Formatnya memang y,x,z,a untuk state! Jadi state [1][2] berarti x=2 dan y=1

        // Cari unit di seluruh startState [sumbu y]
        for (int i = 0; i < startState.Length; i++)
        {
            // Jika sudah digenerate semua, break
            if (unitAlreadyGenerated >= startUnitAmount)
            {
                break;
            }

            // Cari unit di seluruh startState [sumbu x]
            for (int j = 0; j < startState[i].Length; j++)
            {
                // Jika sudah digenerate semua, break
                if (unitAlreadyGenerated >= startUnitAmount)
                {
                    break;
                }

                // Cek unit, berada di z=0 dan a=0
                if (startState[i][j][0][0] > 0)
                {
                    // Jika ada unit milik AI (karena unit > 0)

                    // Generate semua kemungkinan gerakan, berdasarkan unitnya
                    HashSet<Tuple<int, int>> possibleMovement = generatePossibleMovementMax(j, i, startState);

                    // cek tiap possible movement, bisa attack kemana aja, dan terjadi apa setelah attack. Masuk state baru
                    foreach (Tuple<int, int> tuple in possibleMovement)
                    {
                        int xMoveTo = tuple.Item1;
                        int yMoveTo = tuple.Item2;

                        // Copy start state
                        int[][][][] stateAfterMove = generate4dState(startState);
                        // Jika pergerakan bukan ke diri sendiri
                        if (((yMoveTo == i) && (xMoveTo == j)) == false)
                        {
                            Debug.Log(xMoveTo + " " + yMoveTo);
                            // Pindahkan unit ke tempat bergeraknya
                            for (int k = 0; k < startState[i][j].Length; k++)
                            {
                                // Create a new array and copy the elements from the original array
                                stateAfterMove[yMoveTo][xMoveTo][k] = new int[startState[i][j][k].Length];
                                Array.Copy(startState[i][j][k], stateAfterMove[yMoveTo][xMoveTo][k], startState[yMoveTo][xMoveTo][k].Length);
                            }
                            // Kosongkan lokasi sebelumnya dari unit yang telah bergerak
                            stateAfterMove[i][j][0][0] = 0; // Reset tipe unit
                            stateAfterMove[i][j][1] = new int[] { }; // Reset attack unit
                        }

                        // // Debug 
                        // // sumbu y
                        // for (int a = 0; a < stateAfterMove.Length; a++)
                        // {
                        //     string rowLog = "";
                        //     // sumbu x
                        //     for (int b = 0; b < stateAfterMove[a].Length; b++)
                        //     {
                        //         rowLog += "[(" + stateAfterMove[a][b][0][0] + ", " + "HP" + ") ";

                        //         if (stateAfterMove[a][b][1].Length > 0) {

                        //             rowLog += "(" + stateAfterMove[a][b][1][0] + ", ";
                        //             rowLog += "(" + stateAfterMove[a][b][1][1] + ")]    ";
                        //         } else {
                        //             rowLog += "()]    ";
                        //         }

                        //     }
                        //     Debug.Log(rowLog);
                        // }


                        generatePossibleAttackMax(xMoveTo, yMoveTo, stateAfterMove, successorStates);
                    }

                }




                // startState[]
            }
        }

        // Debug successor states
        foreach (var state in successorStates)
        {
            Debug.Log("State");
            // sumbu y
            for (int a = 0; a < state.Length; a++)
            {
                string rowLog = "";
                // sumbu x
                for (int b = 0; b < state[a].Length; b++)
                {
                    rowLog += "[(" + state[a][b][0][0] + ", " + "HP" + ") ";

                    if (state[a][b][1].Length > 0)
                    {

                        rowLog += "(" + state[a][b][1][0] + ", ";
                        rowLog += "(" + state[a][b][1][1] + ")]    ";
                    }
                    else
                    {
                        rowLog += "()]    ";
                    }

                }
                Debug.Log(rowLog);
            }
        }

        return successorStates;
    }

    #region Unit Possible Movement and Possible Attack
    // Note: unitCode adalah di z=0 dan a=0
    HashSet<Tuple<int, int>> generatePossibleMovementMax(int startPosX, int startPosY, int[][][][] startState)
    {
        // Tuple<int, int>
        // tuple.Item1 dan tuple.Item2 berformat (e, f) yaitu posisi x dan posisi y di grid map yang bisa dituju oleh unit
        HashSet<Tuple<int, int>> movementPatternIndex;
        movementPatternIndex = new HashSet<Tuple<int, int>>();

        int unitCode = startState[startPosY][startPosX][0][0];
        int currentUnitMoveSpeed = 0;

        // Generate gerakan kemungkinan

        switch (unitCode)
        {
            // Soldier
            case 1:
                currentUnitMoveSpeed = 2;

                // Add possible tile
                // Tile ini sendiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, 0, startState, true);
                // Atas
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, -1, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, -2, startState);
                // Bawah
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, 1, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, 2, startState);
                // Kiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -1, 0, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -2, 0, startState);
                // Kanan
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 1, 0, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 2, 0, startState);

                break;
            case 2:
                // Archer
                currentUnitMoveSpeed = 8;

                // Add possible tile
                // Tile ini sendiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, 0, startState, true);
                // Atas kiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -1, -1, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -2, -2, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -3, -3, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -4, -4, startState);
                // Atas kanan
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 1, -1, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 2, -2, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 3, -3, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 4, -4, startState);
                // Bawah kiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -1, 1, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -2, 2, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -3, 3, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -4, 4, startState);
                // Bawah kanan
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 1, 1, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 2, 2, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 3, 3, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 4, 4, startState);
                break;
            case 3:
                // GigaMungus
                currentUnitMoveSpeed = 2;

                // Add possible tile
                // Tile ini sendiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, 0, startState, true);
                // Atas kiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -1, -1, startState);
                // Atas
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, -1, startState);
                // Atas kanan
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 1, -1, startState);
                // Kanan
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 1, 0, startState);
                // Bawah kanan
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 1, 1, startState);
                // Bawah
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, -1, startState);
                // Bawah kiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -1, 1, startState);
                // Kiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -1, 0, startState);

                break;
            case 4:
                // BaldArcher
                currentUnitMoveSpeed = 3;

                // Add possible tile
                // Tile ini sendiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, 0, startState, true);
                // Kiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -3, 0, startState);
                // Atas kiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -2, -1, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -1, -2, startState);
                // Atas
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, 3, startState);
                // Atas kanan
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 1, -2, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 2, -1, startState);
                // Kanan
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 3, 0, startState);
                // Bawah kanan
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 2, 1, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 1, 2, startState);
                // Bawah
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, 0, 3, startState);
                // Bawah kiri
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -1, 2, startState);
                AddMovementPatternTiles(movementPatternIndex, startPosX, startPosY, -2, 1, startState);

                break;
            default:
                Debug.LogError("Unit Code Not Found!");
                break;
        }

        // Output HashSet contents
        foreach (var tuple in movementPatternIndex)
        {
            Debug.Log("(" + tuple.Item1 + ", " + tuple.Item2 + ")");
        }
        Debug.Log("~~");

        // Lakukan BFS selama costnya tidak melebihi unit moveSpeed. Tiap tile masuk tilesinMoveSpeedRangeIndex
        // Gunanya BFS adalah memastikan unitnya bisa berjalan menuju ke tile tertentu, karena tile berisi unit musuh tidak bisa dilangkahi
        HashSet<Tuple<int, int>> tilesInMoveSpeedRangeIndex;
        tilesInMoveSpeedRangeIndex = new HashSet<Tuple<int, int>>();

        // Mark all the vertices as not visited (by default set as false)
        // Assume map is a square
        bool[,] visited = new bool[map.Length, map[0].Length];

        // Create a queue for BFS
        // Tuple<int, int, int>
        // Tuple.Item1, Tuple.Item2 adalah posisi x dan y di grid map
        // Tuple.Item3 adalah jumlah movement yang sudah dilakukan sekarang
        Queue<Tuple<int, int, int>> queue = new Queue<Tuple<int, int, int>>();

        // Mark the current node as visited and enqueue it
        visited[startPosY, startPosX] = true;
        queue.Enqueue(new Tuple<int, int, int>(startPosX, startPosY, 0));
        tilesInMoveSpeedRangeIndex.Add(new Tuple<int, int>(startPosX, startPosY));

        Tuple<int, int, int> currentTuple = new Tuple<int, int, int>(0, 0, 0);
        while (queue.Count != 0)
        {
            // Dequeue
            currentTuple = queue.Dequeue();
            Console.Write("Dequeued Tuple");

            int currentX = currentTuple.Item1;
            int currentY = currentTuple.Item2;
            int currentMovementUsed = currentTuple.Item3;

            // Jika tidak melebihi moveSpeed
            if (currentMovementUsed < currentUnitMoveSpeed)
            {
                // Cari lokasi disekitar posisi Tuple.
                // Jika valid, mark sebagai visited, masukkan ke queue, dan masukkan ke tilesInMoveSpeedRangeIndex
                // Valid:
                // - Pengecekan didalam map atau tidak
                // - Pengecekan belum visited
                // - Pengecekan walkable tidak (map[y][x] apakah != 0)
                // - Pengecekan tile kosong ATAU unit milik AI (yaitu state[y][x][0][0] >= 0)

                // Atas
                if (currentY - 1 >= 0)
                {
                    if (visited[currentY - 1, currentX] == false)
                    {
                        if (map[currentY - 1][currentX] != 0)
                        {
                            if (startState[currentY - 1][currentX][0][0] >= 0)
                            {
                                visited[currentY - 1, currentX] = true;
                                queue.Enqueue(new Tuple<int, int, int>(currentX, currentY - 1, currentMovementUsed + 1));
                                tilesInMoveSpeedRangeIndex.Add(new Tuple<int, int>(currentX, currentY - 1));
                            }
                        }

                    }

                }
                // Bawah
                if (currentY + 1 < map.Length)
                {
                    if (visited[currentY + 1, currentX] == false)
                    {
                        if (map[currentY + 1][currentX] != 0)
                        {
                            if (startState[currentY + 1][currentX][0][0] >= 0)
                            {
                                visited[currentY + 1, currentX] = true;
                                queue.Enqueue(new Tuple<int, int, int>(currentX, currentY + 1, currentMovementUsed + 1));
                                tilesInMoveSpeedRangeIndex.Add(new Tuple<int, int>(currentX, currentY + 1));
                            }
                        }

                    }
                }
                // Kiri
                if (currentX - 1 >= 0)
                {
                    if (visited[currentY, currentX - 1] == false)
                    {
                        if (map[currentY][currentX - 1] != 0)
                        {
                            if (startState[currentY][currentX - 1][0][0] >= 0)
                            {
                                visited[currentY, currentX - 1] = true;
                                queue.Enqueue(new Tuple<int, int, int>(currentX - 1, currentY, currentMovementUsed + 1));
                                tilesInMoveSpeedRangeIndex.Add(new Tuple<int, int>(currentX - 1, currentY));
                            }
                        }

                    }

                }
                // Kanan
                // Asumsi mapnya persegi/square
                if (currentX + 1 < map[0].Length)
                {
                    if (visited[currentY, currentX + 1] == false)
                    {
                        if (map[currentY][currentX + 1] != 0)
                        {
                            if (startState[currentY][currentX + 1][0][0] >= 0)
                            {
                                visited[currentY, currentX + 1] = true;
                                queue.Enqueue(new Tuple<int, int, int>(currentX + 1, currentY, currentMovementUsed + 1));
                                tilesInMoveSpeedRangeIndex.Add(new Tuple<int, int>(currentX + 1, currentY));
                            }
                        }

                    }

                }
            }

        }

        // Cari intersect antara tilesInMoveSpeedRangeIndex dan movementPatternIndex. Ini adalah tile yang berada pada attack pattern dari unit DAN ada path menuju tile tersebut.
        tilesInMoveSpeedRangeIndex.IntersectWith(movementPatternIndex);

        // Hanya untuk debug
        Debug.Log("tilesInMoveSpeedRangeIndex");
        foreach (var tuple in tilesInMoveSpeedRangeIndex)
        {
            Debug.Log("(" + tuple.Item1 + ", " + tuple.Item2 + ")");
        }
        Debug.Log("~~");

        // Return semua possible pergerakan
        return tilesInMoveSpeedRangeIndex;

    }

    // Define a function to add tiles in a specific direction (e.g., up, down, left, right)
    void AddMovementPatternTiles(HashSet<Tuple<int, int>> possibleIndex, int startPosX, int startPosY, int offsetX, int offsetY, int[][][][] startState, bool isSelf = false)
    {
        int nextX = startPosX + offsetX;
        int nextY = startPosY + offsetY;

        // Assume map is a square
        int gridSizeX = map.Length;
        int gridSizeY = map[0].Length;

        // Jika nextX dan nextY masih dalam map
        if (nextX >= 0 && nextX < gridSizeX && nextY >= 0 && nextY < gridSizeY)
        {
            // Jika nextX dan nextY adalah walkable
            if (map[nextX][nextY] != 0)
            {
                // Jika tidak ada unit di nextX dan nextY, atau merupakan diri sendiri
                if (startState[nextY][nextX][0][0] == 0 || isSelf)
                {
                    possibleIndex.Add(new Tuple<int, int>(nextX, nextY)); // Add the tuple to the HashSet
                }
            }
        }
    }


    void generatePossibleAttackMax(int unitPosX, int unitPosY, int[][][][] stateAfterMove, List<int[][][][]> successorStates)
    {
        // Tuple<int, int>
        // tuple.Item1 dan tuple.Item2 berformat (g, h) yaitu posisi x dan posisi y di grid map yang bisa dituju oleh unit
        List<Tuple<int, int>> attackPatternIndex;
        attackPatternIndex = new List<Tuple<int, int>>();

        // Cari tipe unit
        int unitCode = stateAfterMove[unitPosY][unitPosX][0][0];

        // Switch tipe unit
        switch (unitCode)
        {
            // Soldier
            case 1:
                // Add possible tile
                // Tile ini sendiri
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, 0, stateAfterMove);
                // Atas
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, -1, stateAfterMove);
                // Bawah
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, 1, stateAfterMove);
                // Kiri
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, -1, 0, stateAfterMove);
                // Kanan
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 1, 0, stateAfterMove);


                break;
            case 2:
                // Archer
                // Add possible tile
                // Tile ini sendiri
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, 0, stateAfterMove);
                // Atas
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, -2, stateAfterMove);
                // Kanan Atas
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 1, -1, stateAfterMove);
                // Kanan
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 2, 0, stateAfterMove);
                // Kanan Bawah
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 1, 1, stateAfterMove);
                // Bawah
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, 2, stateAfterMove);
                // Kiri Bawah
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, -1, 1, stateAfterMove);
                // Kiri
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, -2, 0, stateAfterMove);
                // Kiri Atas
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, -1, -1, stateAfterMove);
                break;
            case 3:
                // GigaMungus
                // Add possible tile
                // Tile ini sendiri
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, 0, stateAfterMove);
                // Atas
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, -1, stateAfterMove);
                // Bawah
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, 1, stateAfterMove);
                // Kiri
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, -1, 0, stateAfterMove);
                // Kanan
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 1, 0, stateAfterMove);
                break;
            case 4:
                // BaldArcher
                // Add possible tile
                // Tile ini sendiri
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, 0, stateAfterMove);
                // Kiri
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, -3, 0, stateAfterMove);
                // Kiri atas
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, -2, 1, stateAfterMove);
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, -1, 2, stateAfterMove);
                // Atas
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, 3, stateAfterMove);
                // Kanan atas
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 1, 2, stateAfterMove);
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 2, 1, stateAfterMove);
                // Kanan
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 3, 0, stateAfterMove);
                // Kanan bawah
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 2, -1, stateAfterMove);
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 1, -2, stateAfterMove);
                // Bawah
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, 0, -3, stateAfterMove);
                // Kiri bawah
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, -1, -2, stateAfterMove);
                AddAttackPatternTilesMax(attackPatternIndex, unitPosX, unitPosY, -2, -1, stateAfterMove);
                break;
            default:
                Debug.LogError("Unit Code Not Found! " + unitCode);
                break;
        }

        // Output List contents
        Debug.Log("Attack");
        foreach (var tuple in attackPatternIndex)
        {
            Debug.Log("(" + tuple.Item1 + ", " + tuple.Item2 + ")");
        }
        Debug.Log("~~");

        // TODO: Buat logic attacknya
        // Lakukan logic attack (Mengurangi nyawa, menambahkan attack index ke state), mengubah stateAfterMove


        // Masukkan hasil ke successorStates
        // TODO: Harusnya ini dimasukkan ke successorStates tiap kali logic attack berjalan. Cuman sementara gini dulu.
        foreach (var tuple in attackPatternIndex)
        {
            int xAttackPos = tuple.Item1;
            int yAttackPos = tuple.Item2;

            int[][][][] newSuccessorState = generate4dState(stateAfterMove);
            newSuccessorState[unitPosY][unitPosX][1] = new int[2];
            newSuccessorState[unitPosY][unitPosX][1][0] = xAttackPos;
            newSuccessorState[unitPosY][unitPosX][1][1] = yAttackPos;

            successorStates.Add(newSuccessorState);
        }
    }

    // Define a function to add tiles in a specific direction (e.g., up, down, left, right)
    void AddAttackPatternTilesMax(List<Tuple<int, int>> possibleIndex, int startPosX, int startPosY, int offsetX, int offsetY, int[][][][] startState)
    {
        int nextX = startPosX + offsetX;
        int nextY = startPosY + offsetY;

        // Assume map is a square
        int gridSizeX = map.Length;
        int gridSizeY = map[0].Length;

        // Jika nextX dan nextY masih dalam map
        if (nextX >= 0 && nextX < gridSizeX && nextY >= 0 && nextY < gridSizeY)
        {
            // Jika nextX dan nextY adalah walkable
            if (map[nextX][nextY] != 0)
            {
                // Jika ada unit milik player di nextX dan nextY
                if (startState[nextY][nextX][0][0] < 0)
                {
                    possibleIndex.Add(new Tuple<int, int>(nextX, nextY)); // Add the tuple to the List
                }
            }
        }
    }
    #endregion

    // Hanya untuk debug di code ini
    // Ini map seperti gameplay
    void generateMap()
    {
        // Ini mirip generateMapInfo() di tileMapScript.cs
        map = new int[10][];

        // Initialize each row with an array of length 10
        for (int i = 0; i < 10; i++)
        {
            map[i] = new int[10];
        }

        // Initialize all grid cost as 1 (walkable)
        for (int i = 0; i < map.Length; i++)
        {
            for (int j = 0; j < map[i].Length; j++)
            {
                map[i][j] = 1;
            }
        }
        // map[0][1] = 0;

        // Dibawah ini tidak walkable, maka cost = 0
        // Note: Yang tidak walkable adalah Tile Mountain (bernilai 2 di tileMapScript)
        map[2][7] = 0;
        map[3][7] = 0;

        map[6][7] = 0;
        map[7][7] = 0;

        map[2][2] = 0;
        map[3][2] = 0;

        map[6][2] = 0;
        map[7][2] = 0;
    }

    // Ini map hanya untuk testing seperti di trello
    void generateMap3By3()
    {
        // Ini mirip generateMapInfo() di tileMapScript.cs
        map = new int[3][];

        // Initialize each row with an array of length 10
        for (int i = 0; i < 3; i++)
        {
            map[i] = new int[3];
        }

        // Initialize all grid cost as 1 (walkable)
        for (int i = 0; i < map.Length; i++)
        {
            for (int j = 0; j < map[i].Length; j++)
            {
                map[i][j] = 1;
            }
        }
    }

    // Helper, Mengenerate 4d array kosongan
    int[][][][] generate4dState()
    {
        int[][][][] newState =
        {
        new int[][][] { new int[][] { new int[] { 0 }, new int[] { } },
                        new int[][] { new int[] { 0 }, new int[] { } },
                        new int[][] { new int[] { 0 }, new int[] { } } },

        new int[][][] { new int[][] { new int[] { 0 }, new int[] { } },
                        new int[][] { new int[] { 0 }, new int[] { } },
                        new int[][] { new int[] { 0 }, new int[] { } } },

        new int[][][] { new int[][] { new int[] { 0 }, new int[] { } },
                        new int[][] { new int[] { 0 }, new int[] { } },
                        new int[][] { new int[] { 0 }, new int[] { } } }
            };
        return newState;
    }

    // Helper, untuk copy 4d state
    int[][][][] generate4dState(int[][][][] stateToCopy)
    {
        // Create a new 4D jagged array to hold the deep copy
        int[][][][] anotherState = new int[stateToCopy.Length][][][];

        // Loop through each dimension and copy its elements
        for (int i = 0; i < stateToCopy.Length; i++)
        {
            anotherState[i] = new int[stateToCopy[i].Length][][]; // Create new sub-arrays for the current dimension

            for (int j = 0; j < stateToCopy[i].Length; j++)
            {
                anotherState[i][j] = new int[stateToCopy[i][j].Length][]; // Create new sub-arrays for the current sub-array

                for (int k = 0; k < stateToCopy[i][j].Length; k++)
                {
                    // Create a new array and copy the elements from the original array
                    anotherState[i][j][k] = new int[stateToCopy[i][j][k].Length];
                    Array.Copy(stateToCopy[i][j][k], anotherState[i][j][k], stateToCopy[i][j][k].Length);
                }
            }
        }

        return anotherState;
    }
}
