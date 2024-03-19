using Sudoku.Shared;

namespace Sudoku.Human;

public class HumanSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        Solver SolverTechnique = new Solver(s);
		SolverTechnique.TrySolve();
		return SolverTechnique.Puzzle.toGrid(s);
    }
}
