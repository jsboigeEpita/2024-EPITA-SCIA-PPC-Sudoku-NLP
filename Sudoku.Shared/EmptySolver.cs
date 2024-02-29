namespace Sudoku.Shared
{
    public class EmptySolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            return s.CloneSudoku();
        }

    }
}