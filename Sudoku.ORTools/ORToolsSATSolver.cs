using Sudoku.Shared;
using Google.OrTools.Sat;

namespace Sudoku.ORTools;

public class OrToolsSatSolver : ISudokuSolver
{
    private const int Dimension = 9;
    private const int SubGrid = 3;
    
    private readonly CpSolver _solver = new();
    public SudokuGrid Solve(SudokuGrid s)
    {
        (CpModel model, IntVar[,]? grid) = CreateModel(s);
        CpSolverStatus status = _solver.Solve(model);

        if (status is CpSolverStatus.Feasible or CpSolverStatus.Optimal)
        {
            return MakeSolution(_solver, grid);
        }

        throw new Exception();
    }

    private SudokuGrid MakeSolution(CpSolver solver, IntVar[,]? grid)
    {
        SudokuGrid result = new SudokuGrid();

        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                result.Cells[i, j] = (int)solver.Value(grid[i, j]);
            }
        }
        
        return result;
    }

    private static (CpModel model, IntVar[,]? grid) CreateModel(SudokuGrid sudokuGrid)
    {
        CpModel model = new();
        IntVar[,] grid = new IntVar[Dimension, Dimension];

        CreateVariables(model, grid, sudokuGrid);
        AddConstraints(model, grid);

        return (model, grid);
    }

    private static void AddConstraints(CpModel model, IntVar[,] grid)
    {
        for (int i = 0; i < Dimension; i++)
        {
            AddRowConstraint(model, grid, i);
            AddColumnConstraint(model, grid, i);
        }

        for (int i = 0; i < Dimension; i += SubGrid)
        {
            for (int j = 0; j < Dimension; j += SubGrid)
            {
                AddCellConstraint(model, grid, i, j);
            }
        }
    }

    private static void AddCellConstraint(CpModel model, IntVar[,] grid, int i, int j)
    {
        IntVar[] cellVariables = Enumerable
            .Range(0, SubGrid)
            .SelectMany(x => Enumerable.Range(0, SubGrid).Select(y => grid[i + x, j + y]))
            .ToArray();

        model.AddAllDifferent(cellVariables);
    }

    private static void AddColumnConstraint(CpModel model, IntVar[,] grid, int i)
    {
        IntVar[] colVariables = Enumerable
            .Range(0, Dimension)
            .Select(row => grid[row, i])
            .ToArray();

        model.AddAllDifferent(colVariables);
    }

    private static void AddRowConstraint(CpModel model, IntVar[,] grid, int i)
    {
        IntVar[] rowVariables = Enumerable
            .Range(0, Dimension)
            .Select(col => grid[i, col])
            .ToArray();

        model.AddAllDifferent(rowVariables);
    }

    private static void CreateVariables(CpModel model, IntVar[,] grid, SudokuGrid sudokuGrid)
    {
        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                grid[i, j] = model.NewIntVar(1, Dimension, $"grid[{i},{j}]");
                if (sudokuGrid.Cells[i, j] != 0)
                {
                    model.Add(grid[i, j] == sudokuGrid.Cells[i, j]);
                }
            }
        }
    }
}