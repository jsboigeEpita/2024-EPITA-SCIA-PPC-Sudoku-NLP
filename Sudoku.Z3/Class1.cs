using System;
using System.Collections;
using System.Collections.Generic;
using Sudoku.Shared;
using Microsoft.Z3;
using System.Runtime.InteropServices;
using System.ComponentModel;
// namespace SudokuZ3;

// public class Class1
// {

// }
//
namespace SudokuZ3
{
    public class Z3 : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid ss)
        {
            Context ctx = new Context(new Dictionary<string, string>() { { "model", "true" } });
           
            Console.WriteLine("SudokuExample");

            // 9x9 matrix of integer variables
            IntExpr[][] X = new IntExpr[9][];
            for (uint i = 0; i < 9; i++)
            {
                X[i] = new IntExpr[9];
                for (uint j = 0; j < 9; j++)
                    X[i][j] = (IntExpr)ctx.MkConst(ctx.MkSymbol("x_" + (i + 1) + "_" + (j + 1)), ctx.IntSort);
            }

            // each cell contains a value in {1, ..., 9}
            Expr[][] cells_c = new Expr[9][];
            for (uint i = 0; i < 9; i++)
            {
                cells_c[i] = new BoolExpr[9];
                for (uint j = 0; j < 9; j++)
                    cells_c[i][j] = ctx.MkAnd(ctx.MkLe(ctx.MkInt(1), X[i][j]),
                                              ctx.MkLe(X[i][j], ctx.MkInt(9)));
            }

            // each row contains a digit at most once
            BoolExpr[] rows_c = new BoolExpr[9];
            for (uint i = 0; i < 9; i++)
                rows_c[i] = ctx.MkDistinct(X[i]);

            // each column contains a digit at most once
            BoolExpr[] cols_c = new BoolExpr[9];
            for (uint j = 0; j < 9; j++)
            {
                IntExpr[] column = new IntExpr[9];
                for (uint i = 0; i < 9; i++)
                    column[i] = X[i][j];

                cols_c[j] = ctx.MkDistinct(column);
            }

            // each 3x3 square contains a digit at most once
            BoolExpr[][] sq_c = new BoolExpr[3][];
            for (uint i0 = 0; i0 < 3; i0++)
            {
                sq_c[i0] = new BoolExpr[3];
                for (uint j0 = 0; j0 < 3; j0++)
                {
                    IntExpr[] square = new IntExpr[9];
                    for (uint i = 0; i < 3; i++)
                        for (uint j = 0; j < 3; j++)
                            square[3 * i + j] = X[3 * i0 + i][3 * j0 + j];
                    sq_c[i0][j0] = ctx.MkDistinct(square);
                }
            }

            BoolExpr sudoku_c = ctx.MkTrue();
            foreach (BoolExpr[] t in cells_c)
                sudoku_c = ctx.MkAnd(ctx.MkAnd(t), sudoku_c);
            sudoku_c = ctx.MkAnd(ctx.MkAnd(rows_c), sudoku_c);
            sudoku_c = ctx.MkAnd(ctx.MkAnd(cols_c), sudoku_c);
            foreach (BoolExpr[] t in sq_c)
                sudoku_c = ctx.MkAnd(ctx.MkAnd(t), sudoku_c);

            // sudoku instance, we use '0' for empty cells
            // int[,] instance = {{0,0,0,0,9,4,0,3,0},
            //                    {0,0,0,5,1,0,0,0,7},
            //                    {0,8,9,0,0,0,0,4,0},
            //                    {0,0,0,0,0,0,2,0,8},
            //                    {0,6,0,2,0,1,0,5,0},
            //                    {1,0,2,0,0,0,0,0,0},
            //                    {0,7,0,0,0,0,5,2,0},
            //                    {9,0,0,0,6,5,0,0,0},
            //                    {0,4,0,9,7,0,0,0,0}};
            int[,] instance = ss.Cells;



            BoolExpr instance_c = ctx.MkTrue();
            for (uint i = 0; i < 9; i++)
                for (uint j = 0; j < 9; j++)
                    instance_c = ctx.MkAnd(instance_c,
                        (BoolExpr)
                        ctx.MkITE(ctx.MkEq(ctx.MkInt(instance[i, j]), ctx.MkInt(0)),
                                    ctx.MkTrue(),
                                    ctx.MkEq(X[i][j], ctx.MkInt(instance[i, j]))));

            Solver s = ctx.MkSolver();
            s.Assert(sudoku_c);
            s.Assert(instance_c);
            var res = new SudokuGrid();
            if (s.Check() == Status.SATISFIABLE)
            {
                Model m = s.Model;
                Expr[,] R = new Expr[9, 9];
                for (uint i = 0; i < 9; i++)
                    for (uint j = 0; j < 9; j++)
                    {
                        R[i, j] = m.Evaluate(X[i][j]);
                        res.Cells[i, j] = (int)((IntNum)R[i, j]).UInt64; 
                    }
                Console.WriteLine("Sudoku solution:");
                for (uint i = 0; i < 9; i++)
                {
                    for (uint j = 0; j < 9; j++)
                        Console.Write(" " + R[i, j]);
                    Console.WriteLine();
                }
                
              
            }
            else
            {
                Console.WriteLine("Failed to solve sudoku");
            }
            
            return res;
        }

    }
}