// Online C# Editor for free
// Write, Edit and Run your C# code using C# Online Compiler

using System;
using System.Collections.Generic;

public class HelloWorld
{
    public static void Main(string[] args)
    {
        Console.WriteLine ("Try programiz.pro");
    }
    
    public bool isPrune(List<int[][][][]> states, int[][][][] cur_state){
        foreach (int[][][][] s in states){
            if (is3DArraySamePos(s, cur_state)){
                if (isAttResSame(s, cur_state)) return true;
            }
        }
        return false;
    }
    
    public bool is3DArraySamePos(int[][][][] a, int[][][][] b){
        for (int i=0;i<a.Length;i++){
                for (int j=0;j<a[i].Length;j++){
                    if (a[i][j][0][0]!=b[i][j][0][0]){
                        return false;
                    }
                }
            }
            return true;
    }
    
    public bool isAttResSame(int[][][][] a, int[][][][] b){
        for (int i=0;i<a.Length;i++){
                for (int j=0;j<a[i].Length;j++){
                    if (a[i][j][1].Length!=0){
                        int x=a[i][j][1][0];
                        int y=a[i][j][1][1];
                        
                        if (a[x][y][0][0]==0) {
                            int k=b[i][j][1][0];
                            int l=b[i][j][1][1];
                            
                            if (b[k][l][0][0]==0) return true;
                        } else {
                            return false;
                        }
                    }
                }
            }
            
        for (int i=0;i<b.Length;i++){
                for (int j=0;j<b[i].Length;j++){
                    if (b[i][j][1].Length!=0){
                        return false;
                    }
                }
            }  
        return true;
    }
}