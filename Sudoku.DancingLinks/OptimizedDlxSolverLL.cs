using Sudoku.Shared;

namespace Sudoku.DancingLinks;

public class OptimizedDlxSolverLL : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        CustomDlxLib.OptimizedDlxLL dlx = new CustomDlxLib.OptimizedDlxLL();
        dlx.Solve(s);
        return s;
    }
}