using System;
using aima.core.search.csp;


namespace Sudoku.CSPwithAIMA
{
    using IBT = ImprovedBacktrackingStrategy;

    // Classe pour simplifier les tests de stratégie de résolution
    public class CSPStrategyInfo
    {
        public CSPStrategy StrategyType { get; set; }
        
        public IBT.Selection Selection { get; set; }

        public IBT.Inference Inference { get; set; }

        public bool EnableLCV { get; set; }

        public int MaxSteps { get; set; }
        
        public CSPStrategyInfo()
        {
            MaxSteps = 50;
        }

        public SolutionStrategy GetStrategy()
        {
            switch (StrategyType)
            {
                case CSPStrategy.BacktrackingStrategy:
                    return new BacktrackingStrategy();
                case CSPStrategy.ImprovedBacktrackingStrategy:
                    var improved = new ImprovedBacktrackingStrategy();
                    improved.enableLCV(EnableLCV);
                    improved.setVariableSelection(Selection);
                    improved.setInference(Inference);
                    return improved;
                case CSPStrategy.MinConflictsStrategy:
                    return new MinConflictsStrategy(MaxSteps);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}