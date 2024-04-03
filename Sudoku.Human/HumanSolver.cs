using Sudoku.Shared;

namespace Sudoku.Human;

public class HumanSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        Solver solver = new Solver(s);
        
        //sans backtracking
        //solver.TrySolveWithoutBacktracking();

        //avec backtracking
		solver.TrySolve();
        
        return solver.Puzzle.toGrid(s);
    }
}
