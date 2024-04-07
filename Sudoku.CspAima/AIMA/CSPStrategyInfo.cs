using System;
using aima.core.search.csp;

namespace Sudoku.CSPwithAIMA
{
    // Classe pour simplifier les tests de stratégie de résolution
    public class CSPStrategyInfo
    {
        public CSPStrategyInfo()
        {
            MaxSteps = 50;
        }

        //[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public CSPStrategy StrategyType { get; set; }

        //[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public CSPSelection Selection { get; set; }

        //[JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        //[JsonConverter(typeof(StringEnumConverter))]
        public CSPInference Inference { get; set; }

        public bool EnableLCV { get; set; }

        public int MaxSteps { get; set; }

        public SolutionStrategy GetStrategy()
        {
            SolutionStrategy toReturn;
            switch (StrategyType)
            {
                case CSPStrategy.BacktrackingStrategy:
                    toReturn = new BacktrackingStrategy();
                    break;
                case CSPStrategy.ImprovedBacktrackingStrategy:
                    var improved = new ImprovedBacktrackingStrategy();
                    toReturn = improved;
                    improved.enableLCV(EnableLCV);
                    switch (Selection)
                    {
                        case CSPSelection.DefaultOrder:
                            break;
                        case CSPSelection.MRV:
                            improved.setVariableSelection(ImprovedBacktrackingStrategy.Selection.MRV);
                            break;
                        case CSPSelection.MRVDeg:
                            improved.setVariableSelection(ImprovedBacktrackingStrategy.Selection.MRV_DEG);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    switch (Inference)
                    {
                        case CSPInference.None:
                            break;
                        case CSPInference.ForwardChecking:
                            improved.setInference(ImprovedBacktrackingStrategy.Inference.FORWARD_CHECKING);
                            break;
                        case CSPInference.AC3:
                            improved.setInference(ImprovedBacktrackingStrategy.Inference.AC3);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case CSPStrategy.MinConflictsStrategy:
                    toReturn = new MinConflictsStrategy(MaxSteps);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return toReturn;
        }
    }
}