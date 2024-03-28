using Google.OrTools.ConstraintSolver;
using Sudoku.Shared;

namespace Sudoku.ORTools;

public class OrToolsCpSolver : ISudokuSolver
{
    private const int Dimension = 9;
    private const int SubGrid = 3;

    private readonly Solver _solver = new Solver("CpSimple");

    public SudokuGrid Solve(SudokuGrid s)
    {
        IntVar[][] grid = CreateModel(s);
        DecisionBuilder db = _solver.MakePhase(grid.Flatten(), Solver.INT_VAR_SIMPLE, Solver.INT_VALUE_SIMPLE);
        _solver.NewSearch(db);
        
        if (_solver.NextSolution())
        {
            return MakeSolution(grid);
        }

        throw new Exception();
    }
    
    private IntVar[][] CreateModel(SudokuGrid sudokuGrid)
    {
        IntVar[][] grid = new IntVar[Dimension][];
        
        // Create variables
        for (int i = 0; i < Dimension; i++)
        {
            grid[i] = new IntVar[Dimension];
            for (int j = 0; j < Dimension; j++)
            {
                grid[i][j] = _solver.MakeIntVar(1, Dimension, $"Cell({i},{j})");
            }
        }

        // Each cells must have distinct values.
        for (int i = 0; i < Dimension; i++)
        {
            _solver.Add(_solver.MakeAllDifferent(grid[i]));
        }

        // Each row must have distinct values.
        for (int j = 0; j < Dimension; j++)
        {
            IntVar[] column = new IntVar[Dimension];
            for (int i = 0; i < Dimension; i++)
            {
                column[i] = grid[i][j];
            }
            _solver.Add(_solver.MakeAllDifferent(column));
        }

        // Each cells in subgrid must have distinct values.
        for (int i = 0; i < Dimension; i += SubGrid)
        {
            for (int j = 0; j < Dimension; j += SubGrid)
            {
                IntVar[] square = new IntVar[Dimension];
                int index = 0;
                for (int x = 0; x < SubGrid; x++)
                {
                    for (int y = 0; y < SubGrid; y++)
                    {
                        square[index++] = grid[i + x][j + y];
                    }
                }
                _solver.Add(_solver.MakeAllDifferent(square));
            }
        }

        // Add constraints from the input grid : cells with values must be equal to the input grid
        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                if (sudokuGrid.Cells[i, j] != 0)
                {
                    _solver.Add(grid[i][j] == sudokuGrid.Cells[i, j]);
                }
            }
        }

        return grid;
    }
    
    private SudokuGrid MakeSolution(IntVar[][] grid)
    {
        SudokuGrid result = new SudokuGrid();
        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                result.Cells[i, j] = (int)grid[i][j].Value();
            }
        }
        
        return result;
    }
}