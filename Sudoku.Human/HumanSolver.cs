using Sudoku.Shared;

namespace Sudoku.Human;

public class HumanSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        SolverTest SolverTechnique = new SolverTest(s);
		SolverTechnique.TrySolve();
		return SolverTechnique.Puzzle.toGrid(s);
    }
}
