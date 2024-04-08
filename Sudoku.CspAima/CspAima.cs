using aima.core.search.csp;
using IBT = aima.core.search.csp.ImprovedBacktrackingStrategy;
using Sudoku.Shared;


namespace Sudoku.CSPwithAIMA
{
    //-------------------------------------------------ABSTRACT CLASS-------------------------------------------------//
    public abstract class CspAima : ISudokuSolver
    {
        public CspAima()
        {
            _Strategy = GetStrategy();
        }

        private readonly SolutionStrategy _Strategy;

        public SudokuGrid Solve(SudokuGrid s)
        {
            //Construction of CSP object using SudokuCSPHelper
            var objCSp = SudokuCSPHelper.GetSudokuCSP(s);

            // Using _Strategy to solve given CSP
            var assignment = _Strategy.solve(objCSp);

            // Set result of solve to given sudoku grid
            SudokuCSPHelper.SetValuesFromAssignment(assignment, s);
            
            // Runs all combination of improvedBacktracking CspSolver on all the SUdokus and writes outputs into CSV 
            // Uncomment following lines
            
            // var solverCsv = new TestSudoku();
            // solverCsv.RunCspOnSudokus();

            return s;
        }

        protected abstract SolutionStrategy GetStrategy();

    }

    //----------------------------------------------------------------------------------------------------------------//
    //------------------------------------SOLVER CLASSES WITH DIFFERENT PARAMETERS------------------------------------//

    // A FEW PARAMETERS:

    /** Attribut LCV (Least Constraining Value):
     * It is a value-level ordering heuristic that assigns the next value that yields the highest number of consistent
     * values of neighboring variables. Intuitively, this procedure chooses first the values that are most likely to
     * work.
     *
     * 'Inference':
     * Is a method used to develop computer algorithms to solve a given problem.
     * Here, the "Forward Checking" method is used to maintain and verify new inferences resulting from assignments.
     *
     * 'Selection':
     * How the variable to be assigned is chosen.
     * In this case, "MRV-Deg" is used, which consists of the Minimum Remaining Values (MRV) coupled with the Degree heuristic.
     * 
     * 'StrategyType':
     * Determines which backtracking search strategy will be used.
     **/

    //--------------------------------------------------BACKTRACKING--------------------------------------------------//

    public class CspAimaBacktracking : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategy = new CSPStrategyInfo
            {
                StrategyType = CSPStrategy.BacktrackingStrategy,
            };
            return objStrategy.GetStrategy();
        }
    }

    //----------------------------------------------IMPROVED BACKTRACKING---------------------------------------------//
    // SELECTION set to: Selection.MRV_DEG

    public class CspAimaImprovedBacktracking_AC3_MRVDEG : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategy = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.AC3,
                Selection = IBT.Selection.MRV_DEG,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategy.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_NONE_MRVDEG : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategy = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.NONE,
                Selection = IBT.Selection.MRV_DEG,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategy.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_FORWARD_CHECKING_MRVDEG : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategy = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.FORWARD_CHECKING,
                Selection = IBT.Selection.MRV_DEG,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategy.GetStrategy();
        }
    }

    // SELECTION set to: Selection.MRV

    public class CspAimaImprovedBacktracking_NONE_MRV : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategy = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.NONE,
                Selection = IBT.Selection.MRV,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategy.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_AC3_MRV : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategy = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.AC3,
                Selection = IBT.Selection.MRV,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategy.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_FORWARD_CHECKING_MRV : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategy = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.FORWARD_CHECKING,
                Selection = IBT.Selection.MRV,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategy.GetStrategy();
        }
    }

    // SELECTION set to: Selection.DEFAULT_ORDER

    public class CspAimaImprovedBacktracking_NONE_DEFAULT_ORDER : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategy = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.NONE,
                Selection = IBT.Selection.DEFAULT_ORDER,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategy.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_AC3_DEFAULT_ORDER : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategy = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.AC3,
                Selection = IBT.Selection.DEFAULT_ORDER,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategy.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_FORWARD_CHECKING_DEFAULT_ORDER : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategy = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.FORWARD_CHECKING,
                Selection = IBT.Selection.DEFAULT_ORDER,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategy.GetStrategy();
        }
    }
}