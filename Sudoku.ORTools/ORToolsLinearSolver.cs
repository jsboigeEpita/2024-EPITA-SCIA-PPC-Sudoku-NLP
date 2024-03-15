using System;
using Sudoku.Shared;
using Google.OrTools.LinearSolver;
using Solver = Google.OrTools.LinearSolver.Solver;

namespace Sudoku.ORTools
{
    public class ORToolsLinearSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            // Create a new solver instance
            Solver solver = Solver.CreateSolver("GLOP");
            if (solver is null) return s; // Return the input grid if the solver creation fails

            // Create a 9x9 matrix of integer variables representing the Sudoku grid
            Variable[,] grid = solver.MakeIntVarMatrix(9, 9, 1, 9, "grid");

            // Add constraints based on the initial Sudoku grid
            AddInitialConstraints(s, solver, grid);

            // Add constraints for distinct values in rows, columns, and subgrids
            AddDistinctValueConstraints(solver, grid);

            // Solve the Sudoku problem
            Solver.ResultStatus resultStatus = solver.Solve();

            // Update the Sudoku grid with the solved values if a feasible solution is found
            if (resultStatus == Solver.ResultStatus.OPTIMAL)
            {
                UpdateSudokuGrid(s, grid);
            }

            return s; // Return the solved or partially solved Sudoku grid
        }

        private void AddInitialConstraints(SudokuGrid s, Solver solver, Variable[,] grid)
        {
            // Add constraints based on the initial Sudoku grid values
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (s.Cells[i, j] != 0)
                    {
                        solver.Add(s.Cells[i, j] == grid[i, j]); // Add constraint for fixed cell value
                    }
                }
            }
        }

        private void AddDistinctValueConstraints(Solver solver, Variable[,] grid)
        {
            // Add constraints to ensure each cell has a value between 1 and 9
            foreach (var cellVariable in grid)
            {
                solver.Add(cellVariable >= 1);
                solver.Add(cellVariable <= 9);
            }

            // Add constraints to ensure each row, column, and subgrid has distinct values
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = j + 1; k < 9; k++)
                    {
                        solver.Add(grid[i, j] != grid[i, k]); // Row constraints
                        solver.Add(grid[j, i] != grid[k, i]); // Column constraints
                    }
                }
            }

            // Add constraints for each 3x3 subgrid
            for (int i = 0; i < 9; i += 3)
            {
                for (int j = 0; j < 9; j += 3)
                {
                    AddSubgridConstraints(solver, grid, i, j);
                }
            }
        }

        private void AddSubgridConstraints(Solver solver, Variable[,] grid, int rowStart, int colStart)
        {
            for (int i = rowStart; i < rowStart + 3; i++)
            {
                for (int j = colStart; j < colStart + 3; j++)
                {
                    for (int k = i; k < rowStart + 3; k++)
                    {
                        for (int l = j + 1; l < colStart + 3; l++)
                        {
                            solver.Add(grid[i, j] != grid[k, l]); // Subgrid constraints
                        }
                    }
                }
            }
        }

        private void UpdateSudokuGrid(SudokuGrid s, Variable[,] grid)
        {
            // Update the Sudoku grid with the solved values
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    s.Cells[i, j] = (int)grid[i, j].SolutionValue();
                }
            }
        }
    }
}
