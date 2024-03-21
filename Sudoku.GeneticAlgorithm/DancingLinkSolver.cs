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
        var dlx = new Dlx();
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
        var rows = new List<int[]>();
        var rowCells = new List<int>(9);
        var rowStrings = solution.RowIndexes
            .Select(rowIndex => internalRows[rowIndex])
            .OrderBy(t => t.Item1)
            .ThenBy(t => t.Item2)
            .GroupBy(t => t.Item1, t => t.Item3)
            .Select(value => string.Concat(value))
            .ToImmutableList();
        foreach (var line in rowStrings.Where(l => l.Length > 0))
        {
            foreach (char c in line)
            {
                //we ignore lines not starting with cell chars
                if (char.IsDigit(c))
                {
                    // if char is a digit, we add it to a cell
                    rowCells.Add((int)Char.GetNumericValue(c));
                }
                else
                {
                    // if char represents an empty cell, we add 0
                    rowCells.Add(0);
                }

                // when 9 cells are entered, we create a row and start collecting cells again.
                if (rowCells.Count == 9)
                {
                    rows.Add(rowCells.ToArray());

                    // we empty the current row collector to start building a new row
                    rowCells.Clear();

                }
            }
        }

        return new SudokuGrid() { Cells = rows.ToArray().To2D() };
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
        var solutionInternalRows = solution.RowIndexes
            .Select(rowIndex => internalRows[rowIndex])
            .ToImmutableList();

        var locationsGroupedByRow = Locations.GroupBy(t => t.Item1);
        var locationsGroupedByCol = Locations.GroupBy(t => t.Item2);
        var locationsGroupedByBox = Locations.GroupBy(t => RowColToBox(t.Item1, t.Item2));

        return
            CheckGroupsOfLocations(solutionInternalRows, locationsGroupedByRow, "row") &&
            CheckGroupsOfLocations(solutionInternalRows, locationsGroupedByCol, "col") &&
            CheckGroupsOfLocations(solutionInternalRows, locationsGroupedByBox, "box");
    }
    private static bool CheckGroupsOfLocations(
        IEnumerable<Tuple<int, int, int, bool>> solutionInternalRows,
        IEnumerable<IGrouping<int, Tuple<int, int>>> groupedLocations,
        string tag)
    {
        return groupedLocations.All(grouping =>
            CheckLocations(solutionInternalRows, grouping, grouping.Key, tag));
    }

    private static bool CheckLocations(
        IEnumerable<Tuple<int, int, int, bool>> solutionInternalRows,
        IEnumerable<Tuple<int, int>> locations,
        int key,
        string tag)
    {
        var digits = locations.SelectMany(location =>
            solutionInternalRows
                .Where(solutionInternalRow =>
                    solutionInternalRow.Item1 == location.Item1 &&
                    solutionInternalRow.Item2 == location.Item2)
                .Select(t => t.Item3));
        return CheckDigits(digits, key, tag);
    }
    private static bool CheckDigits(
        IEnumerable<int> digits,
        int key,
        string tag)
    {
        var actual = digits.OrderBy(v => v);
        if (actual.SequenceEqual(Digits)) return true;
        var values = string.Concat(actual.Select(n => Convert.ToString(n)));
        Console.WriteLine($"{tag} {key}: {values} !!!");
        return false;
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