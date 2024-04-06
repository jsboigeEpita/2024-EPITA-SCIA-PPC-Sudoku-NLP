using Sudoku.Shared;
using Python.Runtime;
using System.Runtime.Versioning;
using Sudoku.CspAima;

namespace Sudoku.CspAima
{
    public class CspAima : PythonSolverBase
    {
        public override SudokuGrid Solve(SudokuGrid s)
        {
            using (PyModule scope = Py.CreateScope())
            	{

            		// Injectez le script de conversion
            		AddNumpyConverterScript(scope);

            		// Convertissez le tableau .NET en tableau NumPy
            		var pyCells = AsNumpyArray(s.Cells, scope);

            		// create a Python variable "instance"
            		scope.Set("instance", pyCells);

            		// run the Python script
            		string code = Resources.Solver_py;
                    Console.WriteLine($"------\nCode : '{code}'\n-----");
            		scope.Exec(code);
                    
            		PyObject result = scope.Get("result");

            		// Convertissez le résultat NumPy en tableau .NET
            		var managedResult = AsManagedArray(scope, result);

            		return new SudokuGrid() { Cells = managedResult };
            	}
            s.Cells[0,0] = 9;
            s.Cells[0,1] = 9;
            s.Cells[0,2] = 9;
            return s;
        }
        
        protected override void InitializePythonComponents()
        {
	        //declare your pip packages here
	        InstallPipModule("numpy");
	        InstallPipModule("pycsp3");
	        base.InitializePythonComponents();
        }
    }
}

