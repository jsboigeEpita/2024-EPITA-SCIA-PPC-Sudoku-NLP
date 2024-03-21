using Sudoku.Shared;

namespace Sudoku.Human;

public class HumanSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        Solver solver = new Solver(s);
		bool returnResult = solver.TrySolve();
        
        if (!returnResult)
        {
            // If Unsuccessful, do Backtracking
            solver.TryBacktrack();
        }
        
        return solver.Puzzle.toGrid(s);
    }
}
