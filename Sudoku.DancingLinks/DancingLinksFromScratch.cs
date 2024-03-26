using Sudoku.Shared;

namespace Sudoku.DancingLinks;

public class DancingLinksFromScratch : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        CustomDlxLib.CustomDlx dlx = new CustomDlxLib.CustomDlx(s);
        dlx.Solve();
        return s;
    }
}