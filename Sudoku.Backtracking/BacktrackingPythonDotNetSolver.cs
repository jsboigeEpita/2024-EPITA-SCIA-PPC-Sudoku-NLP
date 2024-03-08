using Python.Runtime;
using Sudoku.Shared;

namespace Sudoku.Backtracking;

public class BacktrackingPythonDotNetSolver : PythonSolverBase
{

	public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
	{
		//System.Diagnostics.Debugger.Break();

		//For some reason, the Benchmark runner won't manage to get the mutex whereas individual execution doesn't cause issues
		//using (Py.GIL())
		//{
		// create a Python scope
		using (PyModule scope = Py.CreateScope())
		{
			// convert the Person object to a PyObject
			PyObject pySudoku = s.ToPython();

			// create a Python variable "person"
			scope.Set("sudoku", pySudoku);

			// the person object may now be used in Python
			string code = Resources.SelfCallSolver_py;
			scope.Exec(code);
			var result = scope.Get("solvedSudoku");
			var toReturn = result.As<SudokuGrid>();
			return toReturn;
		}
		//}

	}

}