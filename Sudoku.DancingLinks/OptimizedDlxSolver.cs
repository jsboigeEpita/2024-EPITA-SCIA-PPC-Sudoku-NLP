using Sudoku.Shared;

namespace Sudoku.DancingLinks;

public class OptimizedDlxSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        CustomDlxLib.OptimizedDlx dlx = new CustomDlxLib.OptimizedDlx();
        dlx.Solve(s);
        return s;
    }
}