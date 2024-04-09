using Python.Runtime;
using Sudoku.Shared;

namespace Sudoku.ChocoSolver
{
    public abstract class ChocoSolverBase : PythonSolverBase
    {
        protected string searchParameter;

        protected ChocoSolverBase(string searchParameter)
        {
            this.searchParameter = searchParameter;
        }

        public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
        {
            using (PyModule scope = Py.CreateScope())
            {
                // convert the Cells array object to a PyObject
                PyObject pyCells = s.Cells.ToJaggedArray().ToPython();

                // create a Python variable "instance"
                scope.Set("search_type", searchParameter);
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

                return new Shared.SudokuGrid() { Cells = managedResult.To2D() };
            }
        }

        protected override void InitializePythonComponents()
        {
            // Initialize any Python components if needed
            InstallPipModule("pychoco");
            base.InitializePythonComponents();
        }
    }

    public class ChocoSolverDefault : ChocoSolverBase
    {
        public ChocoSolverDefault() : base("") { }
    }

    public class ChocoSolverDomOverWDeg : ChocoSolverBase
    {
        public ChocoSolverDomOverWDeg() : base("dom_over_w_deg") { }
    }

    public class ChocoSolverDomOverWDegRef : ChocoSolverBase
    {
        public ChocoSolverDomOverWDegRef() : base("dom_over_w_deg_ref") { }
    }

    public class ChocoSolverActivityBased : ChocoSolverBase
    {
        public ChocoSolverActivityBased() : base("activity_based") { }
    }

    public class ChocoSolverMinDomLB : ChocoSolverBase
    {
        public ChocoSolverMinDomLB() : base("min_dom_lb") { }
    }

    public class ChocoSolverMinDomUB : ChocoSolverBase
    {
        public ChocoSolverMinDomUB() : base("min_dom_ub") { }
    }

    public class ChocoSolverRandom : ChocoSolverBase
    {
        public ChocoSolverRandom() : base("random") { }
    }

    public class ChocoSolverConflictHistory : ChocoSolverBase
    {
        public ChocoSolverConflictHistory() : base("conflict_history") { }
    }

    public class ChocoSolverInputOrderLB : ChocoSolverBase
    {
        public ChocoSolverInputOrderLB() : base("input_order_lb") { }
    }

    public class ChocoSolverInputOrderUB : ChocoSolverBase
    {
        public ChocoSolverInputOrderUB() : base("input_order_ub") { }
    }

    public class ChocoSolverFailureLengthBased : ChocoSolverBase
    {
        public ChocoSolverFailureLengthBased() : base("failure_length_based") { }
    }

    public class ChocoSolverFailureRateBased : ChocoSolverBase
    {
        public ChocoSolverFailureRateBased() : base("failure_rate_based") { }
    }
}


