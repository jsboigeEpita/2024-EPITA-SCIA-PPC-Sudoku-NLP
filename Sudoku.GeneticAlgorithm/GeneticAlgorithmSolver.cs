namespace Sudoku.Shared
{
    public class GeneticAlgorithmSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            Console.WriteLine("ICIIIIIIIIIIIIIIIII");
            return s.CloneSudoku();
        }

    }
}