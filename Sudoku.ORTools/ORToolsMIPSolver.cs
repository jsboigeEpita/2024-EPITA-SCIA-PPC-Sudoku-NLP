using Google.OrTools.LinearSolver;
using Sudoku.Shared;

namespace Sudoku.ORTools
{
    public class OrToolsMipSolver : ISudokuSolver
    {
        private const int Dimension = 9;
        private const int SubGrid = 3;

        public SudokuGrid Solve(SudokuGrid s)
        {
            Solver solver = Solver.CreateSolver("SCIP");
            if (solver == null)
            {
                throw new InvalidOperationException("Solver initialization failed.");
            }

            Variable[,,] cells = new Variable[Dimension, Dimension, Dimension];
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    for (int k = 0; k < Dimension; k++)
                    {
                        cells[i, j, k] = solver.MakeIntVar(0, 1, $"Cell({i},{j},{k})");
                    }
                }
            }

            // Each cell must have exactly one value.
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    solver.Add(cells[i, j, 0] + cells[i, j, 1] + cells[i, j, 2] + cells[i, j, 3] +
                        cells[i, j, 4] + cells[i, j, 5] + cells[i, j, 6] + cells[i, j, 7] +
                        cells[i, j, 8] == 1);
                }
            }

            // Each row must have distinct values.
            for (int i = 0; i < Dimension; i++)
            {
                for (int k = 0; k < Dimension; k++)
                {
                    solver.Add(cells[i, 0, k] + cells[i, 1, k] + cells[i, 2, k] + cells[i, 3, k] +
                        cells[i, 4, k] + cells[i, 5, k] + cells[i, 6, k] + cells[i, 7, k] +
                        cells[i, 8, k] == 1);
                }
            }

            // Each column must have distinct values.
            for (int j = 0; j < Dimension; j++)
            {
                for (int k = 0; k < Dimension; k++)
                {
                    solver.Add(cells[0, j, k] + cells[1, j, k] + cells[2, j, k] + cells[3, j, k] +
                        cells[4, j, k] + cells[5, j, k] + cells[6, j, k] + cells[7, j, k] +
                        cells[8, j, k] == 1);
                }
            }

            // Each 3x3 subgrid must have distinct values.
            for (int i = 0; i < Dimension; i += 3)
            {
                for (int j = 0; j < Dimension; j += 3)
                {
                    for (int k = 0; k < Dimension; k++)
                    {
                        solver.Add(cells[i, j, k] + cells[i, j + 1, k] + cells[i, j + 2, k] +
                            cells[i + 1, j, k] + cells[i + 1, j + 1, k] + cells[i + 1, j + 2, k] +
                            cells[i + 2, j, k] + cells[i + 2, j + 1, k] + cells[i + 2, j + 2, k] == 1);
                    }
                }
            }

            // Add constraints based on the given puzzle.
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    int value = s.Cells[i, j];
                    if (value > 0)
                    {
                        solver.Add(cells[i, j, value - 1] == 1);
                    }
                }
            }

            // Solve the problem.
            Solver.ResultStatus resultStatus = solver.Solve();

            if (resultStatus != Solver.ResultStatus.OPTIMAL && resultStatus != Solver.ResultStatus.FEASIBLE)
            {
                throw new Exception("No solution found.");
            }

            SudokuGrid solution = new SudokuGrid();
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    for (int k = 0; k < Dimension; k++)
                    {
                        if (cells[i, j, k].SolutionValue() == 1)
                        {
                            solution.Cells[i, j] = k + 1;
                        }
                    }
                }
            }

            return solution;
        }
    }
}