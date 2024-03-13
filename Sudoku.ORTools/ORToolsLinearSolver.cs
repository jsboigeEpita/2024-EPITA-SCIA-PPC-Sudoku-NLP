using Sudoku.Shared;
using Google;
using Google.OrTools.LinearSolver;

namespace Sudoku.ORTools;

public class ORToolsLinearSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        Solver solver = Solver.CreateSolver("GLOP");
        if (solver is null)
        {
            return s; // Error loading the solver
        }

        return s;
    }
}