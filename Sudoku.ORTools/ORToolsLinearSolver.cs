using System;
using System.Linq;
using Google.OrTools.LinearSolver;
using Sudoku.Shared;

namespace Sudoku.ORTools
{
    public class SudokuSolver : ISudokuSolver
    {
        private const int GridSize = 9;
        private const int CellSize = 3;

        public SudokuGrid Solve(SudokuGrid s)
        {
            int[,] inputGrid = s.Cells;

            Solver solver = Solver.CreateSolver("SCIP");
            if (solver == null)
            {
                throw new InvalidOperationException("Solver initialization failed.");
            }

            Variable[,,] x = new Variable[GridSize, GridSize, GridSize];

            // Step 2: Create variables
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    for (int k = 0; k < GridSize; k++)
                    {
                        x[i, j, k] = solver.MakeBoolVar($"x[{i},{j},{k}]");
                    }
                }
            }

            // Step 3: Initialize variables in case of known values
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    int value = inputGrid[i, j];
                    bool defined = value != 0;
                    if (defined)
                    {
                        solver.Add(x[i, j, value - 1] == 1);
                    }
                }
            }

            // Step 4: All bins of a cell must have sum equals to 1
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    solver.Add(solver.Sum(from k in Enumerable.Range(0, GridSize) select x[i, j, k]) == 1);
                }
            }

            // Step 5: AllDifferent on rows, columns, and regions
            for (int k = 0; k < GridSize; k++)
            {
                // AllDifferent on rows
                for (int i = 0; i < GridSize; i++)
                {
                    solver.Add(solver.Sum(from j in Enumerable.Range(0, GridSize) select x[i, j, k]) == 1);
                }

                // AllDifferent on columns
                for (int j = 0; j < GridSize; j++)
                {
                    solver.Add(solver.Sum(from i in Enumerable.Range(0, GridSize) select x[i, j, k]) == 1);
                }

                // AllDifferent on regions
                for (int rowIdx = 0; rowIdx < GridSize; rowIdx += CellSize)
                {
                    for (int colIdx = 0; colIdx < GridSize; colIdx += CellSize)
                    {
                        solver.Add((from i in Enumerable.Range(rowIdx, CellSize)
                            from j in Enumerable.Range(colIdx, CellSize)
                            select x[i, j, k]).ToArray().Aggregate((a, b) => a + b) == 1);

                    }
                }
            }

            // Solve the problem
            solver.Solve();

            // Extract the solution
            int[,] result = new int[GridSize, GridSize];
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    for (int k = 0; k < GridSize; k++)
                    {
                        if (x[i, j, k].SolutionValue() == 1)
                        {
                            result[i, j] = k + 1;
                            break; // Only one value can be true
                        }
                    }
                }
            }

            return new SudokuGrid(result);
        }
    }
}
