using Python.Runtime;
using System;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.ChocoSolverEngine
{
    public class ChocoSolverMethod : PythonSolverBase
    {
        public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
        {
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

                //Convert back to C# object
                var managedResult = result.As<int[][]>();
                //var convertesdResult = managedResult.Select(objList => objList.Select(o => (int)o).ToArray()).ToArray();
                return new Shared.SudokuGrid() { Cells = managedResult.To2D() };
            }
        }

        private PyObject ConvertToPythonList(SudokuGrid s)
        {
            using (Py.GIL())
            {
                var pythonList = new PyList();
                for (int i = 0; i < 9; i++)
                {
                    var rowList = new PyList();
                    for (int j = 0; j < 9; j++)
                    {
                        rowList.Append(new PyInt(s.Cells[i, j]));
                    }
                    pythonList.Append(rowList);
                }
                return pythonList;
            }
        }

        private SudokuGrid ConvertFromPythonList(PyObject solvedGridPython)
        {
            SudokuGrid result = new SudokuGrid();
            for (int i = 0; i < 9; i++)
            {
                var pythonRow = solvedGridPython[i];
                for (int j = 0; j < 9; j++)
                {
                    result.Cells[i, j] = pythonRow[j].As<int>();
                }
            }
            return result;
        }
    }
}
