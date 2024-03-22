using Python.Runtime;
using Sudoku.Shared;

namespace Sudoku.NeuralNetworkPVSM
{
    
    public class NeuralNetworkPythonSolver : PythonSolverBase
    {
        public override SudokuGrid Solve(SudokuGrid s)
        {
            //System.Diagnostics.Debugger.Break();

            //For some reason, the Benchmark runner won't manage to get the mutex whereas individual execution doesn't cause issues
            //using (Py.GIL())
            //{
            // create a Python scope
            using (PyModule scope = Py.CreateScope())
            {

                // Injectez le script de conversion
                AddNumpyConverterScript(scope);

                // Convertissez le tableau .NET en tableau NumPy
                var pyCells = AsNumpyArray(s.Cells, scope);
                
                // PyObject pyCells = s.Cells.ToJaggedArray().ToPython();

                // create a Python variable "instance"
                scope.Set("instance", pyCells);

                // run the Python script
             
                string code = Resources.neuralnetwork_py;
                scope.Exec(code);

                PyObject result = scope.Get("result");

                // Convertissez le résultat NumPy en tableau .NET
                var managedResult = AsManagedArray(scope, result);

                return new SudokuGrid() { Cells = managedResult };
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