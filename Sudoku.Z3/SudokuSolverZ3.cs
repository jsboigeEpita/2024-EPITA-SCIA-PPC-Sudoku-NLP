using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.Z3
{
    /// <summary>
    /// Definition of the abstract class Z3Solver
    /// </summary>
    public abstract class Z3Solver : ISudokuSolver
    {
        protected Z3Optimization _Solver;

        public Z3Solver()
        {
            _Solver = GetOptimization();
        }
        public SudokuGrid Solve(SudokuGrid s)
        {
            return _Solver.solve(s);
        }

        protected abstract Z3Optimization GetOptimization();
    }

    /// <summary>
    /// Definition of the Z3Solver classes, used to change the parameter and the used optimization to test
    /// </summary>

    public class Z3Solver_V1 : Z3Solver
    {
        protected override Z3Optimization GetOptimization()
        {
            return new Z3_V1_int_array();
        }
    }



    public class Z3Solver_V5 : Z3Solver
    {
        protected override Z3Optimization GetOptimization()
        {
            return new Z3Solver_V5_opti();
        }
    }
}