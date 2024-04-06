using Sudoku.Shared;

namespace Sudoku.GeneticAlgorithm;

public class DancingLinkSolverWithCustomDlx : ISudokuSolver
{
    private SudokuGrid sudokuGrid;
    private bool stop = false;
    
    public SudokuGrid Solve(SudokuGrid sudoku)
    {
        DlxCustomized.DlxCustomized dlxCustomized = new DlxCustomized.DlxCustomized(sudoku);
        return dlxCustomized.Solve();
    }
}