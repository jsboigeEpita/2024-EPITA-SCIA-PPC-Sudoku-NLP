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

            // var t = new TestSudoku();
            // t.testMain();

            return s;
        }

        protected abstract SolutionStrategy GetStrategy();

    }

    //----------------------------------------------------------------------------------------------------------------//
    //------------------------------------SOLVER CLASSES WITH DIFFERENT PARAMETERS------------------------------------//

    // A FEW PARAMETERS:

    /** L'attribut LCV (Least Constraining Value):
     * It is a value-level ordering heuristic that assigns the next value that yields the highest number of consistent
     * values of neighboring variables. Intuitively, this procedure chooses first the values that are most likely to
     * work.
     *
     * L'attribut Inference:
     * est une méthode utilisée pour développer des algorithmes informatisés afin de résoudre un
     * problème donné.Ici, la méthode "Forward Checking" est utilisée pour conserver et vérifier de nouvelles inférences
     * qui découlent des affectations.
     *
     * L'attribut Selection représente:
     * la façon dont la variable à affecter est choisie.
     * Dans ce cas, le "MRV-Deg" est utilisé, qui se compose du plus petit Reste à Affecter (MRV) couplé à la technique
     * de sommation des incidentes (Deg).
     *
     * L'attribut StrategyType:
     * détermine quelle stratégie de recherche backtrack sera utilisée.
     * Dans ce cas, l'algorithme à recherche arrière améliorée est utilisé.
     * Avec EnableLCV = false,
    **/

    //--------------------------------------------------BACKTRACKING--------------------------------------------------//

    public class CspAimaBacktracking : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                StrategyType = CSPStrategy.BacktrackingStrategy,
            };
            return objStrategyInfo.GetStrategy();
        }
    }

    //----------------------------------------------IMPROVED BACKTRACKING---------------------------------------------//
    // SELECTION set to: Selection.MRV_DEG

    public class CspAimaImprovedBacktracking_AC3_MRVDEG : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.AC3,
                Selection = IBT.Selection.MRV_DEG,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategyInfo.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_NONE_MRVDEG : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.NONE,
                Selection = IBT.Selection.MRV_DEG,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategyInfo.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_FORWARD_CHECKING_MRVDEG : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.FORWARD_CHECKING,
                Selection = IBT.Selection.MRV_DEG,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategyInfo.GetStrategy();
        }
    }

    // SELECTION set to: Selection.MRV

    public class CspAimaImprovedBacktracking_NONE_MRV : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.NONE,
                Selection = IBT.Selection.MRV,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategyInfo.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_AC3_MRV : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.AC3,
                Selection = IBT.Selection.MRV,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategyInfo.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_FORWARD_CHECKING_MRV : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.FORWARD_CHECKING,
                Selection = IBT.Selection.MRV,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategyInfo.GetStrategy();
        }
    }

    // SELECTION set to: Selection.DEFAULT_ORDER

    public class CspAimaImprovedBacktracking_NONE_DEFAULT_ORDER : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.NONE,
                Selection = IBT.Selection.DEFAULT_ORDER,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategyInfo.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_AC3_DEFAULT_ORDER : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.AC3,
                Selection = IBT.Selection.DEFAULT_ORDER,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategyInfo.GetStrategy();
        }
    }

    public class CspAimaImprovedBacktracking_FORWARD_CHECKING_DEFAULT_ORDER : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = IBT.Inference.FORWARD_CHECKING,
                Selection = IBT.Selection.DEFAULT_ORDER,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
            };
            return objStrategyInfo.GetStrategy();
        }
    }
}


/*L'attribut LCV (Least Constraining Value) permet de mettre l'accent sur les valeurs qui limitent le moins le nombre d'autres valeurs possibles pour les variables non assignées.

L'attribut Inference est une méthode utilisée pour développer des algorithmes informatisés afin de résoudre un problème donné.
Ici, la méthode "Forward Checking" est utilisée pour conserver et vérifier de nouvelles inférences qui découlent des affectations.

L'attribut Selection représente la façon dont la variable à affecter est choisie.
Dans ce cas, le "MRV-Deg" est utilisé, qui se compose du plus petit Reste à Affecter (MRV) couplé à la technique de sommation des incidentes (Deg).

L'attribut StrategyType détermine quelle stratégie de recherche backtrack sera utilisée.
Dans ce cas, l'algorithme à recherche arrière améliorée est utilisé.

Enfin, l’attribut MaxSteps détermine le nombre maximal d'étapes que l'algorithme effectuera avant d'interrompre la recherche et de retourner un échec en option.*/
    