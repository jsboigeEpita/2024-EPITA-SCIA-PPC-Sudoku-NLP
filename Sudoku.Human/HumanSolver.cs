using Sudoku.Shared;

namespace Sudoku.Human;

public class HumanSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        Solver SolverTechnique = new Solver(s);
		bool returnResult = SolverTechnique.TrySolve();
        if (!returnResult)
        {
            // If Unsuccessful, do Backtracking
            SolverTechnique.TryBacktrack();
        }
        
        return SolverTechnique.Puzzle.toGrid(s);
    }
}
