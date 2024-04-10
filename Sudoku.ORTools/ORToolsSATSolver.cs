using Sudoku.Shared;
using Google.OrTools.Sat;
using System;

namespace Sudoku.ORTools
{
    public class OrToolsSatSolver : ISudokuSolver
    {
        private const int Dimension = 9;
        private const int SubGrid = 3;
        private readonly CpSolver _solver = new CpSolver();
        
        public SudokuGrid Solve(SudokuGrid inputGrid)
        {
            (CpModel model, IntVar[,] grid) = CreateModel(inputGrid);
            CpSolverStatus status = _solver.Solve(model);

            if (status is CpSolverStatus.Feasible or CpSolverStatus.Optimal)
            {
                return MakeSolution(_solver, grid);
            }
            else
            {
                throw new InvalidOperationException("Sudoku grid has no solution.");
            }
        }

        private SudokuGrid MakeSolution(CpSolver solver, IntVar[,] grid)
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

        private (CpModel model, IntVar[,]) CreateModel(SudokuGrid sudokuGrid)
        {
            CpModel model = new CpModel();
            IntVar[,] grid = new IntVar[Dimension, Dimension];

            CreateVariables(model, grid, sudokuGrid);
            AddConstraints(model, grid);

            return (model, grid);
        }

        private void AddConstraints(CpModel model, IntVar[,] grid)
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

        private void AddCellConstraint(CpModel model, IntVar[,] grid, int row, int col)
        {
            IntVar[] cellVariables = new IntVar[SubGrid * SubGrid];

            for (int i = 0; i < SubGrid; i++)
            {
                for (int j = 0; j < SubGrid; j++)
                {
                    cellVariables[i * SubGrid + j] = grid[row + i, col + j];
                }
            }

            model.AddAllDifferent(cellVariables);
        }

        private void AddColumnConstraint(CpModel model, IntVar[,] grid, int col)
        {
            IntVar[] colVariables = new IntVar[Dimension];

            for (int row = 0; row < Dimension; row++)
            {
                colVariables[row] = grid[row, col];
            }

            model.AddAllDifferent(colVariables);
        }

        private void AddRowConstraint(CpModel model, IntVar[,] grid, int row)
        {
            IntVar[] rowVariables = new IntVar[Dimension];

            for (int col = 0; col < Dimension; col++)
            {
                rowVariables[col] = grid[row, col];
            }

            model.AddAllDifferent(rowVariables);
        }

        private void CreateVariables(CpModel model, IntVar[,] grid, SudokuGrid sudokuGrid)
        {
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    int value = sudokuGrid.Cells[i, j];
                    grid[i, j] = model.NewIntVar(value == 0 ? 1 : value, value == 0 ? Dimension : value, $"Cell({i},{j})");
                }
            }
        }
    }
}
