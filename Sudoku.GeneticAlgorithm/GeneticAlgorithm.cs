using Python.Runtime;
using Sudoku.Shared;
using System.IO;

namespace Sudoku.GeneticAlgorithm
{
    public class GeneticAlgorithm : PythonSolverBase
    {
        public override SudokuGrid Solve(SudokuGrid s)
        {
            using (PyModule scope = Py.CreateScope())
            {
                // Injectez le script de conversion
                AddNumpyConverterScript(scope);

                // Convertissez le tableau .NET en tableau NumPy
                var pyCells = AsNumpyArray(s.Cells, scope);
                Console.WriteLine("LE TABLEAU AVANT CHOCO RESOLUTION");
                Console.WriteLine(pyCells);

                // create a Python variable "ourSudoku"
                scope.Set("ourSudoku", pyCells);


                // read the content of GeneticAlgorithm.py file
                //string pathToPyFile = @"..\..\..\..\Sudoku.GeneticAlgorithm\Resources\GeneticAlgorithm.py";
                //string code = File.ReadAllText(pathToPyFile);

                // Normalement on utilise Resources, à fix...
                string code = Resources.GeneticAlgorithm_py;

                // Console.WriteLine(code);

                // execute the Python script
                scope.Exec(code);
                PyObject result = scope.Get("result");

                // Convertissez le résultat NumPy en tableau .NET
                var managedResult = AsManagedArray(scope, result);

                Console.WriteLine("APRES LA CHOCO RESOLUTION !");
                
                return new SudokuGrid { Cells = managedResult };
            }
        }

        protected override void InitializePythonComponents()
        {
            // declare your pip packages here
            InstallPipModule("numpy");
            InstallPipModule("pulp");
            base.InitializePythonComponents();
        }
    }
}