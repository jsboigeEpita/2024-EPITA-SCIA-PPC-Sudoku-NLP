// using Google.OrTools.LinearSolver;
// using Sudoku.Shared;
//
// namespace Sudoku.ORTools;
//
// public class OrToolsCbcSolver : ISudokuSolver
// {
//     private const int Dimension = 9;
//     private const int SubGrid = 3;
//     private readonly Solver _solver = new Solver("CBC", Solver.OptimizationProblemType.CBC_MIXED_INTEGER_PROGRAMMING);
//
//     public SudokuGrid Solve(SudokuGrid s)
//     {
//         if (_solver == null)
//         {
//             throw new InvalidOperationException("Solver initialization failed.");
//         }
//
//         Variable[,,] cells = new Variable[Dimension, Dimension, Dimension];
//         for (int i = 0; i < Dimension; i++)
//         {
//             for (int j = 0; j < Dimension; j++)
//             {
//                 for (int k = 0; k < Dimension; k++)
//                 {
//                     cells[i, j, k] = _solver.MakeIntVar(0, 1, $"Cell({i},{j},{k})");
//                 }
//
//                 bool isDefined = s.Cells[i, j] != 0;
//                 if (isDefined)
//                 {
//                     _solver.Add(cells[i, j, s.Cells[i, j] - 1] == 1);
//                 }
//             }
//         }
//
//         // Each cell must have exactly one value.
//         for (int i = 0; i < Dimension; i++)
//         {
//             for (int j = 0; j < Dimension; j++)
//             {
//                 LinearExpr cellExpr = cells[i, j, 0];
//                 for (int k = 1; k < Dimension; k++)
//                 {
//                     cellExpr += cells[i, j, k];
//                 }
//
//                 _solver.Add(cellExpr == 1);
//             }
//         }
//
//         // Each row, column, and 3x3 subgrid must have distinct values.
//         for (int i = 0; i < Dimension; i++)
//         {
//             for (int j = 0; j < Dimension; j++)
//             {
//                 LinearExpr rowExpr = cells[i, 0, j];
//                 LinearExpr colExpr = cells[0, i, j];
//                 LinearExpr subgridExpr = cells[(i / SubGrid) * SubGrid, (j / SubGrid) * SubGrid,
//                     i % SubGrid * SubGrid + j % SubGrid];
//                 for (int k = 1; k < Dimension; k++)
//                 {
//                     rowExpr += cells[i, k, j];
//                     colExpr += cells[k, i, j];
//                     subgridExpr += cells[(i / SubGrid) * SubGrid + k / SubGrid,
//                         (j / SubGrid) * SubGrid + k % SubGrid, i % SubGrid * SubGrid + j % SubGrid];
//                 }
//
//                 _solver.Add(rowExpr == 1);
//                 _solver.Add(colExpr == 1);
//                 _solver.Add(subgridExpr == 1);
//             }
//         }
//
//         // Solve the problem
//         Solver.ResultStatus resultStatus = _solver.Solve();
//
//         if (resultStatus != Solver.ResultStatus.OPTIMAL && resultStatus != Solver.ResultStatus.FEASIBLE)
//         {
//             throw new Exception("No solution found.");
//         }
//
//         SudokuGrid solution = new SudokuGrid();
//         for (int i = 0; i < Dimension; i++)
//         {
//             for (int j = 0; j < Dimension; j++)
//             {
//                 for (int k = 0; k < Dimension; k++)
//                 {
//                     if (cells[i, j, k].SolutionValue() == 1)
//                     {
//                         solution.Cells[i, j] = k + 1;
//                     }
//                 }
//             }
//         }
//
//         return solution;
//     }
// }