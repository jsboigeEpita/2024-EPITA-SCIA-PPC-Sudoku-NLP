using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Sudoku.Shared;
using DlxLib;

namespace Sudoku.GeneticAlgorithm;


public class DancingLinkSolverOptimizedCreationForMatrix : ISudokuSolver
{
    /// <summary>
    /// Solves the given Sudoku grid using a backtracking algorithm.
    /// </summary>
    /// <param name="s">The Sudoku grid to be solved.</param>
    /// <returns>
    /// The solved Sudoku grid.
    /// </returns>
    public SudokuGrid Solve(SudokuGrid s)
    {
        //launch the solver
        //each of 9×9 cells is assigned one of 9 numbers,
        var rowNumber = 9;
        var columnNumber = 9;
        var numberRange = 9;
        var possibleNumber = rowNumber * columnNumber * numberRange;

        //9x9 row-columns,  9x9 row-number, 9x9 column-number, 9x9 box-number constraints sets
        var constraintNumber = 9 * 9 * 4;

        matrix = new int[possibleNumber, constraintNumber];

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                var value = s.Cells[i, j];
                //81th first row of DlxMatrix is for the first row of the sudoku
                var rowIndex = i * rowNumber * columnNumber + j * columnNumber;
                //Matrix got constraint area for each constraint
                if (value != 0)
                {
                    // each constraint have 81 columns
                    var rcConstraint = 9 * i + j;
                    var rnConstraint = 81 + 9 * i + value - 1;
                    var cnConstraint = 162 + 9 * j + value - 1;
                    var bConstraint = 243 + ((i / 3) * 3 + j / 3) * 9 + value - 1;
                    
                    matrix[rowIndex, rcConstraint] = 1;
                    matrix[rowIndex, rnConstraint] = 1;
                    matrix[rowIndex, cnConstraint] = 1;
                    matrix[rowIndex, bConstraint] = 1;
                }
                else
                {
                    // Loop for all possibles values 
                    for (int v = 1; v <= 9; v++)
                    {
                        var rcConstraint = 9 * i + j;
                        var rnConstraint = 81 + 9 * i + v - 1;
                        var cnConstraint = 162 + 9 * j + v - 1;
                        var bConstraint = 243 + ((i / 3) * 3 + j / 3) * 9 + v - 1; 

                        matrix[rowIndex, rcConstraint] = 1;
                        matrix[rowIndex, rnConstraint] = 1;
                        matrix[rowIndex, cnConstraint] = 1;
                        matrix[rowIndex, bConstraint] = 1;
                        //put 1 on the diagonal of area : later with row and column position -> find the correct solution
                        rowIndex++;
                    }
                }
            }
        }
        var solutions = new Dlx()
            .Solve(matrix);;
        return SolutionToGrid(solutions.First(), s);
    }
    private SudokuGrid SolutionToGrid(Solution dlxSolution, SudokuGrid s)
    {
        var solution = s.CloneSudoku();
        // DlxSolution contains only row of solution
        foreach (int row in dlxSolution.RowIndexes)
        {
            int x = row / 81;
            int y = (row % 81) / 9;
            int nb = (row % 9) + 1;

            if (solution.Cells[x, y] == 0)
                solution.Cells[x, y] = nb;
        }

        return solution;
    }
    private int[,] matrix;
}