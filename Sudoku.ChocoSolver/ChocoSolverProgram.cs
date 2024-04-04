using Python.Runtime;
using System;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.ChocoSolverEngine
{
    public class ChocoSolverMethod : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            Runtime.PythonDLL = "/root/.pyenv/versions/3.12.0/lib/libpython3.12.so";
            PythonEngine.Initialize();

            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                string codeBasePath = AppDomain.CurrentDomain.BaseDirectory;
                string resourcesPath = Path.Combine(codeBasePath, "Resources");
                sys.path.append(resourcesPath);

                dynamic solver = Py.Import("test");

                PyObject pyInitialGrid = ConvertToPythonList(s);
                PyObject pySolvedGrid = solver.solve_sudoku(pyInitialGrid);

                return ConvertFromPythonList(pySolvedGrid);
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
