using Sudoku.Shared;
using Google.OrTools.ConstraintSolver;
using System.Text;

namespace Sudoku.ORTools
{
    public class OrToolsCpSolver : ISudokuSolver
    {
        private const int Dimension = 9;
        private const int SubGrid = 3;
        private readonly Solver _solver = new Solver("CpSimple");
        
        public SudokuGrid Solve(SudokuGrid s)
        {
            int[,] grid = s.Cells;
            
            IntVar[,] matrix = AddConstraints(_solver, grid);
            DecisionBuilder db = _solver.MakePhase(matrix.Flatten(), Solver.INT_VAR_SIMPLE, Solver.INT_VALUE_SIMPLE);
            
            _solver.NewSearch(db);

            while (_solver.NextSolution())
            {
                SudokuGrid res = new SudokuGrid();
                for (int i = 0; i < Dimension; i++)
                {
                    for (int j = 0; j < Dimension; j++)
                    {
                        res.Cells[i, j] = (int) matrix[i, j].Value();
                    }
                }
                _solver.EndSearch();

                return res;
            }

            throw new Exception("Sudoku grid has no solution.");
        }

        private static IntVar[,] AddConstraints(Solver solver, int[,] grid)
        {
            IntVar[,] g = solver.MakeIntVarMatrix(Dimension, Dimension, 1, 9, "g");

            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    if (grid[i, j] != 0)
                    {
                        solver.Add(g[i, j] == grid[i, j]);
                    }
                }
            }
            for (int i = 0; i < Dimension; i++)
            {
                var rowVariables = new IntVar[Dimension];
                var colVariables = new IntVar[Dimension];

                for (int j = 0; j < Dimension; j++)
                {
                    rowVariables[j] = g[i, j];
                    colVariables[j] = g[j, i];
                }

                solver.Add(solver.MakeAllDifferent(rowVariables));
                solver.Add(solver.MakeAllDifferent(colVariables));
            }

            for (int row = 0; row < Dimension; row += SubGrid)
            {
                for (int col = 0; col < Dimension; col += SubGrid)
                {
                    IntVar[] squareVariables = new IntVar[SubGrid * SubGrid];
                    for (int i = 0; i < SubGrid; i++)
                    {
                        for (int j = 0; j < SubGrid; j++)
                        {
                            squareVariables[i * SubGrid + j] = g[row + i, col + j];
                        }
                    }
                    solver.Add(solver.MakeAllDifferent(squareVariables));
                }
            }

            return g;
        }
    }
}