using Python.Runtime;
using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.DeepLearning
{
	public class DeepLearningSolver : PythonSolverBase

	{
		public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
		{
			//System.Diagnostics.Debugger.Break();

			//For some reason, the Benchmark runner won't manage to get the mutex whereas individual execution doesn't cause issues
			//using (Py.GIL())
			//{
			// print the Python version
			Console.WriteLine($"Python Version: {PythonEngine.Version}");
			// create a Python scope
			using (PyModule scope = Py.CreateScope())
			{
				// convert the Cells array object to a PyObject
				PyObject pyCells = s.Cells.ToPython();

				// create a Python variable "instance"
				scope.Set("instance", pyCells);

				//Console.WriteLine("Solving Sudoku with DeepLearning 1");

				// run the Python script
				string code = Resources.DeepLearning_py;
				scope.Exec(code);

				// Print the result
				//Console.WriteLine(scope.Get("r"));
				//Console.WriteLine("Solving Sudoku with DeepLearning 2");


				//Retrieve solved Sudoku variable
				var result = scope.Get("r");

				// Clear the scope
				scope.Dispose();

				//Convert back to C# object
				var managedResult = result.As<int[][]>();
				//Console.WriteLine("Solving Sudoku with DeepLearning 3");

				//var convertesdResult = managedResult.Select(objList => objList.Select(o => (int)o).ToArray()).ToArray();
				return new Shared.SudokuGrid() { Cells = managedResult.To2D() };
			}
			//}

		}

		protected override void InitializePythonComponents()
		{
			//declare your pip packages here
			InstallPipModule("numpy");
			InstallPipModule("tensorflow");
			base.InitializePythonComponents();
		}

	}
}
