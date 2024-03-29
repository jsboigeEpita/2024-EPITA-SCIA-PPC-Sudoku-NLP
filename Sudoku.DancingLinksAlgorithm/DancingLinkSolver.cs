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
            .Where(solution => VerifySolution(internalRows, solution))
            .ToImmutableList();

        Console.WriteLine();

        return SolutionToGrid(internalRows, solutions.First());
    }
    
    private static void DrawSolution(
        IReadOnlyList<Tuple<int, int, int, bool>> internalRows,
        Solution solution)
    {
        Console.WriteLine(SolutionToGrid(internalRows, solution).ToString());
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

    private static int RowColToBox(int row, int col)
    {
        return row - (row%3) + (col/3);
    }

    private static IEnumerable<int> Encode(int major, int minor)
    {
        var result = new int[81];
        result[major*9 + minor] = 1;
        return result.ToImmutableList();
    }

    private static bool VerifySolution(
        IReadOnlyList<Tuple<int, int, int, bool>> internalRows,
        Solution solution)
    {
        var solutionValues = solution.RowIndexes.Select(rowIndex => internalRows[rowIndex]).ToList();
        
        for (int i = 0; i < 9; i++)
        {
            var rowSet = new HashSet<int>();
            var colSet = new HashSet<int>();
            var boxSet = new HashSet<int>();

            for (int j = 0; j < 9; j++)
            {
                if (!rowSet.Add(solutionValues[i * 9 + j].Item3))
                    return false;

                // Check column
                if (!colSet.Add(solutionValues[j * 9 + i].Item3))
                    return false;

                // Check box
                int row = (i / 3) * 3 + (j / 3);
                int col = (i % 3) * 3 + (j % 3);
                if (!boxSet.Add(solutionValues[row * 9 + col].Item3))
                    return false;
            }
        }
        return true;
    }
    
    private static IImmutableList<int> BuildDlxRow(Tuple<int, int, int, bool> internalRow)
    {
        var row = internalRow.Item1;
        var col = internalRow.Item2;
        var value = internalRow.Item3;
        var box = RowColToBox(row, col);

        var posVals = Encode(row, col);
        var rowVals = Encode(row, value - 1);
        var colVals = Encode(col, value - 1);
        var boxVals = Encode(box, value - 1);

        return posVals.Concat(rowVals).Concat(colVals).Concat(boxVals).ToImmutableList();
    }
    private static IImmutableList<IImmutableList<int>> BuildDlxRows(
        IEnumerable<Tuple<int, int, int, bool>> internalRows)
    {
        return internalRows.Select(BuildDlxRow).ToImmutableList();
    }

    private static IEnumerable<int> Rows => Enumerable.Range(0, 9);
    private static IEnumerable<int> Cols => Enumerable.Range(0, 9);
    private static IEnumerable<Tuple<int, int>> Locations =>
        from row in Rows
        from col in Cols
        select Tuple.Create(row, col);
    private static IEnumerable<int> Digits => Enumerable.Range(1, 9);

    private static IImmutableList<Tuple<int, int, int, bool>> BuildInternalRowsForGrid(SudokuGrid grid)
    {
        var rowsByCols =
            from row in Rows
            from col in Cols
            let value = grid.Cells[row,col]
            select BuildInternalRowsForCell(row, col, value);

        return rowsByCols.SelectMany(cols => cols).ToImmutableList();
    }
    private static IImmutableList<Tuple<int, int, int, bool>> BuildInternalRowsForCell(int row, int col, int value)
    {
        if (value >= 1 && value <= 9)
            return ImmutableList.Create(Tuple.Create(row, col, value, true));

        return Digits.Select(v => Tuple.Create(row, col, v, false)).ToImmutableList();
    }
}