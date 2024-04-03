using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Sudoku.Shared;
using DlxLib;

namespace Sudoku.GeneticAlgorithm;


public class DancingLinkSolver : ISudokuSolver
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
        var internalRows = BuildInternalRowsForGrid(s);
        var dlxRows = BuildDlxRows(internalRows);
        var solutions = new Dlx()
            .Solve(dlxRows, d => d, r => r)
            .ToImmutableList();

        return SolutionToGrid(internalRows, solutions.First());
    }
    
    private static IImmutableList<Tuple<int, int, int, bool>> BuildInternalRowsForGrid(SudokuGrid grid)
    {
        var internalRows = new List<Tuple<int, int, int, bool>>();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int value = grid.Cells[row, col];
                internalRows.AddRange(BuildInternalRowsForCell(row, col, value));
            }
        }

        return internalRows.ToImmutableList();
    }

    private static IImmutableList<Tuple<int, int, int, bool>> BuildInternalRowsForCell(int row, int col, int value)
    {
        if (value >= 1 && value <= 9)
        {
            // If the value is valid, create a single tuple representing the cell
            return ImmutableList.Create(Tuple.Create(row, col, value, true));
        }
        else
        {
            // If the value is not valid, manually create tuples for each possible value
            var internalRows = new List<Tuple<int, int, int, bool>>(9);
            for (int v = 1; v <= 9; v++)
            {
                internalRows.Add(Tuple.Create(row, col, v, false));
            }
            return internalRows.ToImmutableList();
        }
    }

    private static IImmutableList<IImmutableList<int>> BuildDlxRows(IEnumerable<Tuple<int, int, int, bool>> internalRows)
    {
        var dlxRows = new List<IImmutableList<int>>();
        foreach (var internalRow in internalRows)
        {
            dlxRows.Add(BuildDlxRow(internalRow));
        }
        return dlxRows.ToImmutableList();
    }

    private static IImmutableList<int> BuildDlxRow(Tuple<int, int, int, bool> internalRow)
    {
        var row = internalRow.Item1;
        var col = internalRow.Item2;
        var value = internalRow.Item3;
        var box = RowColToBox(row, col);

        // Check if row, col, and value are within valid ranges
        if (row < 0 || row >= 9 || col < 0 || col >= 9 || value < 1 || value > 9)
        {
            throw new ArgumentOutOfRangeException("Invalid row, col, or value.");
        }

        var result = new int[4 * 9 * 9]; // Increase array size to accommodate all possible positions

        // Encode position, row, column, and box values
        result[row * 9 + col] = 1; // Position value
        result[9 * 9 + row * 9 + value - 1] = 1; // Row value
        result[2 * 9 * 9 + col * 9 + value - 1] = 1; // Column value
        result[3 * 9 * 9 + box * 9 + value - 1] = 1; // Box value

        return result.ToImmutableList();
    }


    private static int RowColToBox(int row, int col)
    {
        return row - (row%3) + (col/3);
    }

    private static SudokuGrid SolutionToGrid(
        IReadOnlyList<Tuple<int, int, int, bool>> internalRows,
        Solution solution)
    {
        var grid = new int[9, 9];

        foreach (var (row, col, value, _) in solution.RowIndexes.Select(rowIndex => internalRows[rowIndex]))
        {
            grid[row, col] = value;
        }
        return new SudokuGrid { Cells = grid };
    }
}