using Google.OrTools.Sat;

namespace Sudoku.ORTools;

public class VarArraySolutionPrinter : CpSolverSolutionCallback
{
    private int _solutionCount;
    private readonly IntVar[] _variables;
    
    public VarArraySolutionPrinter(IntVar[] variables)
    {
        _variables = variables;
    }
    
    public override void OnSolutionCallback()
    {
        Console.WriteLine(String.Format(@"Solution #{0}: time = {1:F2} s", _solutionCount, WallTime()));
        _solutionCount++;
    }

    public int SolutionCount()
    {
        return _solutionCount;
    }
}