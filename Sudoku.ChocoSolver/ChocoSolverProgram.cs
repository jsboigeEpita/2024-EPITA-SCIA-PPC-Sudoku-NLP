using Python.Runtime;
using Sudoku.Shared;

namespace Sudoku.ChocoSolver;

public class ChocoSolver : PythonSolverBase
{

    public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
    {
        //Runtime.PythonDLL = "/root/.pyenv/versions/3.12.0/lib/libpython3.12.so";

        using (PyModule scope = Py.CreateScope())
        {
            // convert the Cells array object to a PyObject
            PyObject pyCells = s.Cells.ToJaggedArray().ToPython();

            // create a Python variable "instance"
            scope.Set("instance", pyCells);

            // run the Python script
            string code = Resources.ChocoSolver_py;
            scope.Exec(code);

            //Retrieve solved Sudoku variable
            var result = scope.Get("r");

            // Clear the scope
            scope.Dispose();

            //Convert back to C# object
            var managedResult = result.As<int[][]>();

            //var convertesdResult = managedResult.Select(objList => objList.Select(o => (int)o).ToArray()).ToArray();
            return new Shared.SudokuGrid() { Cells = managedResult.To2D() };
        }
    }
    
    protected override void InitializePythonComponents()
    {
        //declare your pip packages here
        //InstallPipModule("pychoco");
        base.InitializePythonComponents();
    }
}
