using Google.OrTools.ConstraintSolver;
using Sudoku.Shared;
using System;

namespace Sudoku.ORTools
{
    public class OrToolsCpSolver : ISudokuSolver
    {
        private const int Dimension = 9;
        private const int SubGrid = 3;
        private readonly Solver _solver = new Solver("CpSimple");

        public SudokuGrid Solve(SudokuGrid inputGrid)
        {
            IntVar[][] grid = CreateModel(inputGrid);
            DecisionBuilder db = _solver.MakePhase(grid.Flatten(), Solver.INT_VAR_SIMPLE, Solver.INT_VALUE_SIMPLE);
            _solver.NewSearch(db);

            if (_solver.NextSolution())
            {
                return MakeSolution(grid);
            }

            throw new InvalidOperationException("Sudoku grid has no solution.");
        }

        private IntVar[][] CreateModel(SudokuGrid sudokuGrid)
        {
            IntVar[][] grid = new IntVar[Dimension][];

            for (int i = 0; i < Dimension; i++)
            {
                grid[i] = new IntVar[Dimension];
                for (int j = 0; j < Dimension; j++)
                {
                    int value = sudokuGrid.Cells[i, j];
                    grid[i][j] = _solver.MakeIntVar(value == 0 ? 1 : value, value == 0 ? Dimension : value, $"Cell({i},{j})");
                }
            }

            AddConstraints(grid);

            return grid;
        }

        private void AddConstraints(IntVar[][] grid)
        {
            for (int i = 0; i < Dimension; i++)
            {
                _solver.Add(_solver.MakeAllDifferent(grid[i]));
            }

            for (int j = 0; j < Dimension; j++)
            {
                IntVar[] column = new IntVar[Dimension];
                for (int i = 0; i < Dimension; i++)
                {
                    column[i] = grid[i][j];
                }
                _solver.Add(_solver.MakeAllDifferent(column));
            }

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
}
