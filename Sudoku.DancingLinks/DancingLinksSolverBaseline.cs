using Sudoku.Shared;
using DlxLib;

namespace Sudoku.DancingLinks
{
    public class DancingLinksSolverBaseline : ISudokuSolver
    {
        /// <summary>
        /// Solves the given Sudoku grid using a dancing links algorithm.
        /// </summary>
        /// <param name="s">The Sudoku grid to be solved.</param>
        /// <returns>
        /// The solved Sudoku grid.
        /// </returns>
        public SudokuGrid Solve(SudokuGrid s)
        {
            //launch the solver
            byte[,] matrix = BuildDlxMatrix(s);
            IEnumerable<Solution> dlxSolution = new Dlx().Solve(matrix);
            SudokuGrid solution = DlxSolutionToSudokuGrid(dlxSolution.First(), matrix);
            return solution;
        }

        /// <summary>
        /// Given a [SudokuGrid] build the matrix that will be used in Dlx.Solve
        /// </summary>
        /// <param name="s">The sudoku grid to be solved</param>
        /// <returns>
        /// The sudoku to be solved ready to be fed into Dlx.Solve
        /// </returns>
        private byte[,] BuildDlxMatrix(SudokuGrid s)
        {
            int n = 9;
            int size = 3;
            int nConstraint = 4;

            byte[,] matrix = new byte[n * n * n, n * n * nConstraint];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int blockIndex = ((i / size) + ((j / size) * size));
                    int singleColumnIndex = n * j + i;

                    int value = s.Cells[i, j] - 1;
                    int rowIndex = n * n * j + n * i;
                    int rowNumberConstraintIndex = n * n + n * j;
                    int columnNumberConstraintIndex = n * n * 2 + n * i;
                    int boxNumberConstraintIndex = n * n * 3 + blockIndex * n;

                    if (value >= 0)
                    {
                        matrix[rowIndex, singleColumnIndex] = 1;
                        matrix[rowIndex, rowNumberConstraintIndex + value] = 1;
                        matrix[rowIndex, columnNumberConstraintIndex + value] = 1;
                        matrix[rowIndex, boxNumberConstraintIndex + value] = 1;
                    }
                    else
                    {
                        for (int d = 0; d < n; d++)
                        {
                            matrix[rowIndex, singleColumnIndex] = 1;
                            matrix[rowIndex, rowNumberConstraintIndex++] = 1;
                            matrix[rowIndex, columnNumberConstraintIndex++] = 1;
                            matrix[rowIndex++, boxNumberConstraintIndex++] = 1;
                        }
                    }
                }
            }

            return matrix;
        }

        /// <summary>
        /// Turn a Dlx solution back into a regular SudokuGrid
        /// </summary>
        /// <param name="dlxSolution">The solution to the sudoku grid</param>
        /// <returns>
        /// The solved [SudokuGrid] according to the Dlx Solution 
        /// </returns>
        private SudokuGrid DlxSolutionToSudokuGrid(Solution dlxSolution, byte[,] matrix)
        {
            var solution = new SudokuGrid();

            foreach (int row in dlxSolution.RowIndexes)
            {
                int x = 0, y = 0, nb = 0;
                for (int j = 0; j < 81; j++)
                {
                    if (matrix[row, j] == 1)
                    {
                        x = j % 9;
                        y = j / 9;
                        break;
                    }
                }

                for (int j = 81; j < 162; j++)
                {
                    if (matrix[row, j] == 1)
                    {
                        nb = (j - 81) % 9 + 1;
                        break;
                    }
                }

                solution.Cells[x, y] = nb;
            }

            return solution;
        }
    }
}