namespace Sudoku.GeneticAlgorithm;

public class GeneticSolver : ISudokuSolver
{
    public SudokuDancingLinks Solve(SudokuDancingLinks s)
    {
        return s.CloneSudoku();
    }
}