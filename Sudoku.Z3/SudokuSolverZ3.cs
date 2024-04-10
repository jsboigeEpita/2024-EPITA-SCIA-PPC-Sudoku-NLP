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

    public class Z3Solver_IntArray_V1 : Z3Solver
    {
        protected override Z3Optimization GetOptimization()
        {
            return new Z3_V1_IntArray();
        }
    }

    public class Z3Solver_BitVect_V2 : Z3Solver
    {
        protected override Z3Optimization GetOptimization()
        {
            return new Z3_V2_BitVect();
        }
    }

    public class Z3Solver_BitVect_Mask_V3 : Z3Solver
    {
        protected override Z3Optimization GetOptimization()
        {
            return new Z3_V3_BitVect_Mask();
        }
    }

    public class Z3Solver_BitVect_Mask_Tactics_V4 : Z3Solver
    {
        protected override Z3Optimization GetOptimization()
        {
            return new Z3_V4_BitVect_Mask_Tactics();
        }
    }

    public class Z3Solver_BitVect_Mask_Tactics_Subs_V5 : Z3Solver
    {
        protected override Z3Optimization GetOptimization()
        {
            return new Z3_V5_BitVect_Mask_Tactics_Subs();
        }
    }
}