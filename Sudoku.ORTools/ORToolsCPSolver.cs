using Sudoku.Shared;
using Google.OrTools.ConstraintSolver;
using System.Text;

namespace Sudoku.ORTools
{
    public class OrToolsCpSolver : ISudokuSolver
    {
        private const int Dimension = 9;
        private const int SubGrid = 3;
        
        public SudokuGrid Solve(SudokuGrid s)
        {
            int[,] grid = s.Cells;
            Solver solver = new Solver("CpSimple");
            IntVar[,] matrix = CreateConstraints(solver, grid);
            DecisionBuilder db = solver.MakePhase(matrix.Flatten(), Solver.INT_VAR_SIMPLE, Solver.INT_VALUE_SIMPLE);
            
            solver.NewSearch(db);

            while (solver.NextSolution())
            {
                SudokuGrid res = MakeSolution(matrix);
                solver.EndSearch();

                return res;
            }

            throw new Exception("Unfeasible Sudoku");
        }

        private static IntVar[,] CreateConstraints(Solver solver, int[,] grid)
        {
            IntVar[,] matrix = solver.MakeIntVarMatrix(Dimension, Dimension, 1, 9, "matrix");

            // Add constraints for pre-filled cells
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    if (grid[i, j] != 0)
                    {
                        solver.Add(matrix[i, j] == grid[i, j]);
                    }
                }
            }

            // Add constraints for rows and columns to have distinct values
            for (int i = 0; i < Dimension; i++)
            {
                solver.Add(solver.MakeAllDifferent((from j in Enumerable.Range(0, Dimension) select matrix[i, j]).ToArray()));
                solver.Add(solver.MakeAllDifferent((from j in Enumerable.Range(0, Dimension) select matrix[j, i]).ToArray()));
            }

            // Add constraints for each region to have distinct values
            for (int row = 0; row < Dimension; row += SubGrid)
            {
                for (int col = 0; col < Dimension; col += SubGrid)
                {
                    IntVar[] regionVars = new IntVar[SubGrid * SubGrid];
                    for (int r = 0; r < SubGrid; r++)
                    {
                        for (int c = 0; c < SubGrid; c++)
                        {
                            regionVars[r * SubGrid + c] = matrix[row + r, col + c];
                        }
                    }
                    solver.Add(solver.MakeAllDifferent(regionVars));
                }
            }

            return matrix;
        }
        
        private SudokuGrid MakeSolution(IntVar[,] grid)
        {
            SudokuGrid result = new SudokuGrid();
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    result.Cells[i, j] = (int)grid[i, j].Value();
                }
            }

            return result;
        }
    }
}